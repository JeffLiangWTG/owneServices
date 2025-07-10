using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.CommissionManagement.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BillingCommissionHeaderBuildItem : ICommissionHeaderBuildItem
	{
		public BillingCommissionHeaderBuildItem(CreateCommissionContext context, BillingCommissionHeaderBuildItemArgs args, Dictionary<(string Stream, ZGuid PivotPk), EdiCommissionAgreement> responsibleAgreements, Action<AccCommissionHeader, ICommissionAgreementAndRates> createPercentageCommissionLineGroupsDelegate)
		{
			Argument.NotNull(context, "context");
			Argument.NotNull(args, "args");
			Argument.NotNull(responsibleAgreements, "responsibleAgreements");
			Argument.NotNull(createPercentageCommissionLineGroupsDelegate, "createPercentageCommissionLineGroupsDelegate");

			this.Context = context;
			this.Args = args;
			this.responsibleAgreements = responsibleAgreements;
			this.CreatePercentageCommissionLineGroupsDelegate = createPercentageCommissionLineGroupsDelegate;
		}

		protected readonly CreateCommissionContext Context;
		protected readonly BillingCommissionHeaderBuildItemArgs Args;
		protected readonly Dictionary<(string Stream, ZGuid PivotPk), EdiCommissionAgreement> responsibleAgreements;
		readonly Action<AccCommissionHeader, ICommissionAgreementAndRates> CreatePercentageCommissionLineGroupsDelegate;

		protected BusinessObjectFactory Factory
		{
			get { return Args.Source.Factory; }
		}

		public ICommissionableTransaction Source
		{
			get { return Args.Source; }
		}

		public BusinessObject GroupingSource => Args.GroupingSource;

		#region TargetAgreementAndRates

		public IMultipleCommissionAgreementAndRatesProvider TargetAgreementAndRates
		{
			get
			{
				if (targetAgreementAndRates == null)
				{
					targetAgreementAndRates = GetNewAgreementAndRatesProvider();
				}

				return targetAgreementAndRates;
			}
		}

		IMultipleCommissionAgreementAndRatesProvider targetAgreementAndRates;

		protected IMultipleCommissionAgreementAndRatesProvider GetNewAgreementAndRatesProvider()
		{
			return BillingEffectiveCommissionAgreementProvider.New(Args.ClientCompanyPk, Args.LicenceDatabasePk, Args.CommissionDate, responsibleAgreements);
		}

		#endregion

		#region GetExistingCommissionHeaders

		public AccCommissionHeader[] GetExistingCommissionHeaders(ZQuery filter = null)
		{
			var existingCommissionHeadersQuery = new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, Args.Source.PK);
			existingCommissionHeadersQuery.AddToFilter(AccCommissionHeaderSchema.CH0_GroupingSourceID, Args.GroupingSource.PK);
			existingCommissionHeadersQuery.AddToFilter(AccCommissionHeaderSchema.CH0_GroupingSourceTableCode, Args.GroupingSource.TablePrefix);
			existingCommissionHeadersQuery.AddToFilter(AccCommissionHeaderSchema.CH0_OH_Customer, Args.CustomerPk);
			existingCommissionHeadersQuery.AddToFilter(AccCommissionHeaderSchema.CH0_Product, Args.Product);
			existingCommissionHeadersQuery.AddToFilter(AccCommissionHeaderSchema.CH0_Service, Args.Service);
			existingCommissionHeadersQuery.AddToFilter(AccCommissionHeaderSchema.CH0_SubModule, Args.SubModule);
			existingCommissionHeadersQuery.AddToFilter(AccCommissionHeaderSchema.CH0_OverridenDateTimeUtc, null);

			if (filter != null)
			{
				existingCommissionHeadersQuery.AddToFilter(filter);
			}

			var existingCommissionHeaders = Factory.Load<EdiCommissionHeader>(existingCommissionHeadersQuery);
			return existingCommissionHeaders.Where(x => MatchesArgs(x)).ToArray();
		}

		bool MatchesArgs(EdiCommissionHeader commissionHeader)
		{
			if (commissionHeader.AdditionalInfo == null)
			{
				return Args.ClientCompanyPk.IsEmpty && Args.LicenceDatabasePk.IsEmpty;
			}
			else
			{
				return
					commissionHeader.AdditionalInfo.ECH_LCC == Args.ClientCompanyPk &&
					commissionHeader.AdditionalInfo.ECH_LD == Args.LicenceDatabasePk;
			}
		}

		#endregion

		#region NewCommissionHeaderWithLineGroups

		public AccCommissionHeader NewCommissionHeaderWithLineGroups(KeyValuePair<ZString, ICommissionAgreementAndRates> commissionTarget)
		{
			var result = NewCommissionHeader(commissionTarget.Key, commissionTarget.Value.CommissionAgreement);
			CreatePercentageCommissionLineGroupsDelegate(result, commissionTarget.Value);
			return result;
		}

		AccCommissionHeader NewCommissionHeader(ZString commissionStream, OrgCommissionAgreement commissionAgreement)
		{
			var commissionHeader = Factory.New<EdiCommissionHeader>();
			commissionHeader.CH0_AH_Source = Args.Source.PK;
			commissionHeader.CH0_GC = Args.Source.AH_GC;
			commissionHeader.CH0_GroupingSourceID = Args.GroupingSource.PK;
			commissionHeader.CH0_GroupingSourceTableCode = Args.GroupingSource.TablePrefix;
			commissionHeader.CH0_CommissionStream = commissionStream;
			commissionHeader.CH0_CA0 = commissionAgreement.PK;
			commissionHeader.CH0_OH_Debtor = Args.Source.AH_OH;
			commissionHeader.CH0_OH_Customer = Args.CustomerPk;
			commissionHeader.CH0_Product = Args.Product;
			commissionHeader.CH0_Service = Args.Service;
			commissionHeader.CH0_SubModule = Args.SubModule;
			commissionHeader.CH0_CommissionDate = Args.CommissionDate;
			commissionHeader.CH0_SnapshotDateTime = Args.SnapshotDateTime;
			commissionHeader.CH0_SnapshotEventCode = Args.SnapshotEventCode;

			if (!Args.ClientCompanyPk.IsEmpty || !Args.LicenceDatabasePk.IsEmpty)
			{
				var additionalInfo = commissionHeader.GetOrCreateAdditionalInfo();
				additionalInfo.ECH_LCC = Args.ClientCompanyPk;
				additionalInfo.ECH_LD = Args.LicenceDatabasePk;
			}

			return commissionHeader;
		}

		#endregion
	}

	public class BillingCommissionHeaderBuildItemArgs
	{
		public BillingCommissionHeaderBuildItemArgs(ICommissionableTransaction source, BusinessObject groupingSource, ZGuid customerPk, ZGuid clientCompanyPk, ZGuid licenceDatabasePk, ZString product, ZString service, ZString subModule, ZDate commissionDate, ZDateTime snapshotDateTime, ZString snapshotEventCode)
		{
			this.Source = source;
			this.GroupingSource = groupingSource;
			this.CustomerPk = customerPk;
			this.ClientCompanyPk = clientCompanyPk;
			this.LicenceDatabasePk = licenceDatabasePk;
			this.Product = product;
			this.Service = service;
			this.SubModule = subModule;
			this.CommissionDate = commissionDate;
			this.SnapshotDateTime = snapshotDateTime;
			this.SnapshotEventCode = snapshotEventCode;
		}

		public readonly ICommissionableTransaction Source;
		public readonly BusinessObject GroupingSource;
		public readonly ZGuid CustomerPk;
		public readonly ZGuid ClientCompanyPk;
		public readonly ZGuid LicenceDatabasePk;
		public readonly ZString Product;
		public readonly ZString Service;
		public readonly ZString SubModule;
		public readonly ZDate CommissionDate;
		public readonly ZDateTime SnapshotDateTime;
		public readonly ZString SnapshotEventCode;
	}
}

