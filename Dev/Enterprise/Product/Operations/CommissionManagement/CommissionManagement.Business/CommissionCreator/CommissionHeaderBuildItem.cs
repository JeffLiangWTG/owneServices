using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public interface ICommissionHeaderBuildItem
	{
		ICommissionableTransaction Source { get; }
		BusinessObject GroupingSource { get; }
		IMultipleCommissionAgreementAndRatesProvider TargetAgreementAndRates { get; }
		AccCommissionHeader[] GetExistingCommissionHeaders(ZQuery filter = null);
		AccCommissionHeader NewCommissionHeaderWithLineGroups(KeyValuePair<ZString, ICommissionAgreementAndRates> commissionTarget);
	}

	public class CommissionHeaderBuildItem : ICommissionHeaderBuildItem
	{
		public CommissionHeaderBuildItem(CreateCommissionContext context, CommissionHeaderBuildItemArgs args, Action<AccCommissionHeader, ICommissionAgreementAndRates> createPercentageCommissionLineGroupsDelegate, DataTable effectiveDateCacheTable = null)
		{
			Argument.NotNull(context, "context");
			Argument.NotNull(args, "args");
			Argument.NotNull(createPercentageCommissionLineGroupsDelegate, "createPercentageCommissionLineGroupsDelegate");

			this.Context = context;
			this.Args = args;
			this.CreatePercentageCommissionLineGroupsDelegate = createPercentageCommissionLineGroupsDelegate;
			this.effectiveDateCacheTable = effectiveDateCacheTable;
		}

		protected readonly CreateCommissionContext Context;
		protected readonly CommissionHeaderBuildItemArgs Args;
		readonly Action<AccCommissionHeader, ICommissionAgreementAndRates> CreatePercentageCommissionLineGroupsDelegate;
		protected DataTable effectiveDateCacheTable;

		protected BusinessObjectFactory Factory => Context.OverrideFactory ?? Args.Source.Factory;

		public ICommissionableTransaction Source
		{
			get { return Args.Source; }
		}

		public BusinessObject GroupingSource => Args.GroupingSource;

		#region AgreementAndRatesToUse

		public IMultipleCommissionAgreementAndRatesProvider TargetAgreementAndRates
		{
			get
			{
				if (agreementAndRatesProvider == null)
				{
					agreementAndRatesProvider = GetNewAgreementAndRatesProvider();
				}

				return agreementAndRatesProvider;
			}
		}
		IMultipleCommissionAgreementAndRatesProvider agreementAndRatesProvider;

		protected virtual IMultipleCommissionAgreementAndRatesProvider GetNewAgreementAndRatesProvider()
		{
			return EffectiveCommissionAgreementProvider.New(Factory, Args, effectiveDateCacheTable);
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

			if (!Args.Mode.IsEmpty && Args.Mode != OrgCommissionAgreementItemLookups.AllModesCode)
			{
				existingCommissionHeadersQuery.AddToFilter(AccCommissionHeaderSchema.CH0_Mode, new ZString[] { Args.Mode, OrgCommissionAgreementItemLookups.AllModesCode });
			}

			if (!Args.Origin.IsEmpty)
			{
				existingCommissionHeadersQuery.AddToFilter(AccCommissionHeaderSchema.CH0_NKOrigin, new[] { Args.Origin, Args.Origin.SubstringSafe(0, 2), ZString.Empty });
			}

			if (!Args.Destination.IsEmpty)
			{
				existingCommissionHeadersQuery.AddToFilter(AccCommissionHeaderSchema.CH0_NKDestination, new[] { Args.Destination, Args.Destination.SubstringSafe(0, 2), ZString.Empty });
			}

			existingCommissionHeadersQuery.AddToFilter(AccCommissionHeaderSchema.CH0_OverridenDateTimeUtc, null);

			if (filter != null)
			{
				existingCommissionHeadersQuery.AddToFilter(filter);
			}

			return Factory.Load<AccCommissionHeader>(existingCommissionHeadersQuery);
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
			var commissionHeader = Factory.New<AccCommissionHeader>();
			commissionHeader.CH0_GC = Args.Source.AH_GC;
			commissionHeader.CH0_AH_Source = Args.Source.PK;
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
			commissionHeader.CH0_Mode = Args.Mode;
			commissionHeader.CH0_NKOrigin = Args.Origin;
			commissionHeader.CH0_NKDestination = Args.Destination;
			if (Args.GroupingSource.TablePrefix == JobHeaderSchema.Constants.Prefix)
			{
				commissionHeader.CH0_JobNumber = (ZString)Args.GroupingSource[JobHeaderSchema.JH_JobNum];
			}

			return commissionHeader;
		}

		#endregion
	}

	public class EffectiveCommissionItemArgs : CommissionItemArgs
	{
		public EffectiveCommissionItemArgs(ZGuid customerPk,
			ZString product, ZString service, ZString subModule,
			Dictionary<ZGuid, ZDate> commissionDateByChargeDictionary, ZString mode, ZString origin, ZString destination)
			: base(customerPk, product, service, subModule, GetSingleCommissionDate(commissionDateByChargeDictionary), mode, origin, destination)
		{
			CommissionDateByChargeDictionary = commissionDateByChargeDictionary;
		}

		public EffectiveCommissionItemArgs(ZGuid customerPk,
			ZString product, ZString service, ZString subModule,
			ZDate commissionDate, ZString mode, ZString origin, ZString destination)
			: base(customerPk, product, service, subModule, commissionDate, mode, origin, destination)
		{
			CommissionDateByChargeDictionary = new Dictionary<ZGuid, ZDate>();
			CommissionDateByChargeDictionary[ZGuid.Empty] = commissionDate;
		}

		public readonly Dictionary<ZGuid, ZDate> CommissionDateByChargeDictionary;

		static ZDate GetSingleCommissionDate(Dictionary<ZGuid, ZDate> commissionDateByChargeDictionary)
		{
			if (commissionDateByChargeDictionary.ContainsKey(ZGuid.Empty))
			{
				return commissionDateByChargeDictionary[ZGuid.Empty];
			}
			else
			{
				return commissionDateByChargeDictionary.Where(d => !d.Key.IsEmpty).Min(d => d.Value);
			}
		}
	}

	public class CommissionHeaderBuildItemArgs : EffectiveCommissionItemArgs
	{
		public CommissionHeaderBuildItemArgs(ICommissionableTransaction source, BusinessObject groupingSource, ZGuid customerPk,
			ZString product, ZString service, ZString subModule,
			Dictionary<ZGuid, ZDate> commissionDateByChargeDictionary, ZDateTime snapshotDateTime, ZString snapshotEventCode,
			ZString mode, ZString origin, ZString destination)
			: base(customerPk, product, service, subModule, commissionDateByChargeDictionary, mode, origin, destination)
		{
			Source = source;
			GroupingSource = groupingSource;
			SnapshotDateTime = snapshotDateTime;
			SnapshotEventCode = snapshotEventCode;
		}

		public CommissionHeaderBuildItemArgs(ICommissionableTransaction source, BusinessObject groupingSource, ZGuid customerPk,
			ZString product, ZString service, ZString subModule,
			ZDate commissionDate, ZDateTime snapshotDateTime, ZString snapshotEventCode,
			ZString mode, ZString origin, ZString destination)
			: base(customerPk, product, service, subModule, BuildChargeDictionary(commissionDate), mode, origin, destination)
		{
			Source = source;
			GroupingSource = groupingSource;
			SnapshotDateTime = snapshotDateTime;
			SnapshotEventCode = snapshotEventCode;
		}

		static Dictionary<ZGuid, ZDate> BuildChargeDictionary(ZDate date)
		{
			var commissionDateByChargeDictionary = new Dictionary<ZGuid, ZDate>();
			commissionDateByChargeDictionary[ZGuid.Empty] = date;
			return commissionDateByChargeDictionary;
		}

		public readonly ICommissionableTransaction Source;
		public readonly BusinessObject GroupingSource;
		public readonly ZDateTime SnapshotDateTime;
		public readonly ZString SnapshotEventCode;
	}
}
