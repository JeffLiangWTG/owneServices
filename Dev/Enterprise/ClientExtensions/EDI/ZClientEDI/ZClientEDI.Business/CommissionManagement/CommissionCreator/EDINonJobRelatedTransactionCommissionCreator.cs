using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public class EDINonJobRelatedTransactionCommissionCreator : NonJobRelatedTransactionCommissionCreator
	{
		#region New

		public static EDINonJobRelatedTransactionCommissionCreator New(ICommissionableTransaction transaction)
		{
			return new EDINonJobRelatedTransactionCommissionCreator(transaction);
		}

		#endregion

		#region Register/Unregister SubType Override

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = New;
		}

		#endregion

		#region Constructor

		protected EDINonJobRelatedTransactionCommissionCreator(ICommissionableTransaction transaction)
			: base(transaction)
		{
		}

		#endregion

		#region Create Commissions

		protected override CommissionHeaderBuilder GetNewCommissionHeaderBuilder(CreateCommissionContext context)
		{
			return new EDINonJobRelatedTransactionCommissionHeaderBuilder(Transaction, context);
		}

		protected override CommissionHeaderBuildItem GetNewCommissionHeaderBuildItem(CreateCommissionContext context, CommissionHeaderBuildItemArgs args)
		{
			return new EDINonJobRelatedTransactionCommissionHeaderBuildItem(context, args, GetCreatePercentageCommissionLineGroupsDelegate(Transaction.Lines.Cast<TransactionLine>(), args.CommissionDateByChargeDictionary));
		}

		#endregion

		#region Classes

		class EDINonJobRelatedTransactionCommissionHeaderBuildItem : CommissionHeaderBuildItem
		{
			public EDINonJobRelatedTransactionCommissionHeaderBuildItem(CreateCommissionContext context, CommissionHeaderBuildItemArgs args, Action<AccCommissionHeader, ICommissionAgreementAndRates> createPercentageCommissionLineGroupsDelegate)
				: base(context, args, createPercentageCommissionLineGroupsDelegate)
			{
			}

			#region AgreementAndRatesProvider

			protected override IMultipleCommissionAgreementAndRatesProvider GetNewAgreementAndRatesProvider()
			{
				// the default will be to fallback to looking at agreements responsbile for the whole invoice (instead of looking at transaction line level)
				var result = base.GetNewAgreementAndRatesProvider();

				OverrideProviderAgreementAndRatesWithCommissionAgreementResponsibleForAllLineChargeCodes(result);
				OverrideProviderAgreementAndRatesWithExistingAmbiguousCommissionResolutions(result);

				return result;
			}

			void OverrideProviderAgreementAndRatesWithExistingAmbiguousCommissionResolutions(IMultipleCommissionAgreementAndRatesProvider provider)
			{
				var existingAmbiguousCommissionQuery = new ZQuery(AccAmbiguousCommissionSchema.AC0_AH_Source, Args.Source.PK);
				var existingAmbiguousCommissions = Factory.Load<AccAmbiguousCommission>(existingAmbiguousCommissionQuery);
				foreach (var existingAmbiguousCommission in existingAmbiguousCommissions)
				{
					if (existingAmbiguousCommission != null
						&& existingAmbiguousCommission.SelectedAgreement != null
						&& existingAmbiguousCommission.SelectedAgreement.GetStatus(Args.CommissionDate) == OrgCommissionAgreementStatusList.Codes.Active)
					{
						// if an agreement has previously been selected for ambiguous commission, keep using same agreement if it is still active
						var stream = existingAmbiguousCommission.SelectedAgreement.CA0_CommissionStream;
						provider.ByStream[stream] = new CommissionAgreementAndRates(existingAmbiguousCommission.SelectedAgreement, Args.CommissionDate);
						CommissionStreamsWithMultipleCommissionAgreementsResponsibleForLineChargeCodes.Remove(stream);
					}
				}
			}

			void OverrideProviderAgreementAndRatesWithCommissionAgreementResponsibleForAllLineChargeCodes(IMultipleCommissionAgreementAndRatesProvider provider)
			{
				CommissionStreamsWithMultipleCommissionAgreementsResponsibleForLineChargeCodes = new HashSet<ZString>();

				var responsibleAgreementByCommissionStream = new Dictionary<ZString, ZGuid>();

				foreach (var possibleOverallItem in ResponsibleOverallItemsForInvoiceLineChargeCodesFinder.GetResponsibleOverallItems(Args.Source))
				{
					var possibleAgreement = possibleOverallItem.CommissionAgreement;
					var stream = possibleAgreement.CA0_CommissionStream;

					ZGuid responsibleAgreeement;
					if (!responsibleAgreementByCommissionStream.TryGetValue(stream, out responsibleAgreeement))
					{
						// single agreement found (so far) - use this for commission calculations
						provider.ByStream[stream] = new CommissionAgreementAndRates(possibleAgreement, Args.CommissionDate);
						responsibleAgreementByCommissionStream[stream] = possibleAgreement.PK;
					}
					else if (responsibleAgreeement != possibleAgreement.PK)
					{
						// multiple agreements were found - return no agreement (this means that this commission will be flagged as ambiguous)
						provider.ByStream.Remove(stream);
						CommissionStreamsWithMultipleCommissionAgreementsResponsibleForLineChargeCodes.Add(stream);
					}
				}
			}

			public HashSet<ZString> CommissionStreamsWithMultipleCommissionAgreementsResponsibleForLineChargeCodes { get; private set; }

			#endregion
		}

		class EDINonJobRelatedTransactionCommissionHeaderBuilder : CommissionHeaderBuilder
		{
			public EDINonJobRelatedTransactionCommissionHeaderBuilder(ICommissionableTransaction source, CreateCommissionContext context)
				: base(source.Factory, context)
			{
				this.source = source;
			}

			readonly ICommissionableTransaction source;

			protected override bool ShouldMarkOverriden(AccCommissionHeader commissionHeader)
			{
				return true;
			}

			#region Create

			protected override AccCommissionHeader[] CreateCore(ICommissionHeaderBuildItem buildItem)
			{
				var result = base.CreateCore(buildItem);

				var ediBuildItem = (EDINonJobRelatedTransactionCommissionHeaderBuildItem)buildItem;
				if (ediBuildItem.CommissionStreamsWithMultipleCommissionAgreementsResponsibleForLineChargeCodes != null)
				{
					foreach (var commissionStream in ediBuildItem.CommissionStreamsWithMultipleCommissionAgreementsResponsibleForLineChargeCodes)
					{
						var ambigiousCommissionsQuery = new ZQuery(AccAmbiguousCommissionSchema.AC0_AH_Source, source.PK);
						ambigiousCommissionsQuery.AddToFilter(AccAmbiguousCommissionSchema.AC0_CommissionStream, commissionStream);
						if (Factory.LoadTop1<AccAmbiguousCommission>(ambigiousCommissionsQuery) == null
							&& buildItem.GetExistingCommissionHeaders(new ZQuery(AccCommissionHeaderSchema.CH0_CommissionStream, commissionStream)).Length == 0)
						{
							var ambigiousTransaction = Factory.New<AccAmbiguousCommission>();
							ambigiousTransaction.AC0_AH_Source = source.PK;
							ambigiousTransaction.AC0_CommissionStream = commissionStream;
							ambigiousTransaction.AC0_CA0_SelectedAgreement = ZGuid.Empty;
						}
					}
				}

				foreach (var header in result)
				{
					var commissionStream = header.CH0_CommissionStream;
					if (Context.AgreementAndRatesOverride != null)
					{
						var ambigiousCommissionsQuery = new ZQuery(AccAmbiguousCommissionSchema.AC0_AH_Source, source.PK);
						ambigiousCommissionsQuery.AddToFilter(AccAmbiguousCommissionSchema.AC0_CommissionStream, commissionStream);
						var ambigiousCommissions = Factory.Load<AccAmbiguousCommission>(ambigiousCommissionsQuery);
						foreach (var ambiguousCommission in ambigiousCommissions)
						{
							ambiguousCommission.AC0_CA0_SelectedAgreement = header.CH0_CA0;
						}
					}
					else
					{
						var unresolvedAmbigiousCommissionsQuery = new ZQuery(AccAmbiguousCommissionSchema.AC0_AH_Source, source.PK);
						unresolvedAmbigiousCommissionsQuery.AddToFilter(AccAmbiguousCommissionSchema.AC0_CommissionStream, commissionStream);
						unresolvedAmbigiousCommissionsQuery.AddToFilter(AccAmbiguousCommissionSchema.AC0_CA0_SelectedAgreement, null);
						var unresolvedAmbiguousCommissions = Factory.Load<AccAmbiguousCommission>(unresolvedAmbigiousCommissionsQuery);
						foreach (var unresolvedAmbiguousCommission in unresolvedAmbiguousCommissions)
						{
							unresolvedAmbiguousCommission.Delete();
						}
					}
				}

				return result;
			}

			#endregion
		}

		#endregion
	}
}
