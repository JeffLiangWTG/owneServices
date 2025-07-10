using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Billing.Collectors;
using CargoWise.Common;
using Enterprise.Integration.Billing;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	sealed class RefStlScriptRetriever : BaseStlScript
	{
		public RefStlScriptRetriever(IRefStlScript bizo)
			: this(bizo, new TransactionFactoryProvider())
		{
		}

		public RefStlScriptRetriever(IRefStlScript bizo, TransactionFactoryProvider transactionFactoryProvider)
		{
			Bizo = bizo;
			transactionFactory = transactionFactoryProvider.GetTransactionFactory(bizo);
		}

		readonly IStlTransactionFactory transactionFactory;

		public bool IsSupersededBy(RefStlScriptRetriever otherScript) => otherScript.ActiveOnRanking < ActiveOnRanking;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "developer exception report")]
		public override bool IsActive
		{
			get
			{
				try
				{
					return IsCollectionActive;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					string message = "Error Loading Dynamic STL Collection Point: " + ex.Message;
					ExceptionReporter.Instance.ReportDeveloperException(message, ex);
					return false;
				}
			}
		}

		public StlDateType DateType
		{
			get
			{
				switch (Bizo.DateType)
				{
					case RefStlDateType.DateTime:
						return StlDateType.DateTime;
					case RefStlDateType.DateTimeOffset:
						return StlDateType.DateTimeOffset;
					case RefStlDateType.SmallDateTime:
						return StlDateType.SmallDateTime;
					default:
						throw new ArgumentOutOfRangeException(Bizo.DateType);
				}
			}
		}

		public override IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange) =>
			ScriptRunner.Run(this, dateTimeRange, transactionFactory);

		public bool IsCollectionActive => IsCollectionActiveAccordingToActiveOn && SatisfiesMinimumVersion && SatisfiesMaximumVersion;
		protected override bool IncludeNonserializablePropertiesInAdditionalRefs => transactionFactory is UsageTransactionFactory;
		public override string Name => Bizo.GetType().Name;
		internal IRefStlScript Bizo { get; private set; }

		protected override string FeatureCode => Bizo.FeatureCode;
		protected override string RoleName => Bizo.RoleName;
		protected override string ModuleName => Bizo.ModuleName;
		protected override string FunctionName => Bizo.FunctionName;
		protected override string FeatureName => Bizo.FeatureName;
		protected override string TransactionCompanyExpression => Bizo.CompanyCode;
		protected override string BranchCodeExpression => Bizo.BranchCode;
		protected override string TransactionDateUtcExpression => Bizo.TransactionDateUtc;
		protected override string TransactionGuidReference => Bizo.GuidReference;
		protected override string TransactionReference01 => Bizo.BillingReference1;
		protected override string FromClause => Bizo.FromClause;
		protected override string WhereClause => Bizo.WhereClause;
		protected override string CreateUserCodeExpression => Bizo.CreatingUserCode;
		protected override string TransactionReference02 => Bizo.BillingReference2;
		protected override string TransactionReference03 => Bizo.BillingReference3;
		protected override string TransactionReference04 => Bizo.BillingReference4;
		protected override string AdditionalRefs => Bizo.AdditionalRefs;
		protected override string TransactionCountExpression => Bizo.TransactionCount;
		protected override string PreparationScript => Bizo.PreparationScript;
		protected override bool WithOptionRecompile => Bizo.WithOptionRecompile;
		public override bool IsMandatoryForMilestones => Bizo.UsedInBilling;
		public override StlCollectorType CollectorType => StlCollectorType.Dynamic;

		protected override StlDateType ScriptDateType => DateType;

		protected override StlDataGrain StlItemGrain
		{
			get
			{
				switch (Bizo.DataGranularity)
				{
					case RefStlItemGrain.Transactional:
						return StlDataGrain.Transactional;
					case RefStlItemGrain.MonthlyAllowHistoricalData:
						return StlDataGrain.MonthlyAllowHistoricalData;
					case RefStlItemGrain.MonthlyCurrentDataOnly:
						return StlDataGrain.MonthlyCurrentDataOnly;
					case RefStlItemGrain.Daily:
						return StlDataGrain.Daily;
					case RefStlItemGrain.Snapshot:
						return StlDataGrain.Snapshot;
					default:
						throw new ArgumentOutOfRangeException(Bizo.DataGranularity);
				}
			}
		}

		protected override string ActiveOn => Bizo.ActiveOn;
		protected override string MinVersion => Bizo.MinCW1Version;
		protected override string MaxVersion => Bizo.MaxCW1Version;
		protected override DateTime StartDateUtc => Bizo.CollectionStartDateUtc;

		bool IsCollectionActiveAccordingToActiveOn
		{
			get
			{
				switch (Bizo.ActiveOn)
				{
					case RefActiveOn.All:
						return true;
					case RefActiveOn.None:
						return false;
					case RefActiveOn.ProductionOnly:
						return IsProductionSystem;
					case RefActiveOn.TestOnly:
						return !IsProductionSystem;
					default:
						throw new ArgumentOutOfRangeException(Bizo.ActiveOn);
				}
			}
		}

		bool IsProductionSystem
		{
			get
			{
				var productRegistration = ObjectFactory.Get<IProductRegistration>();
				return productRegistration.Key.DatabaseType == DatabaseTypes.Codes.Production && !productRegistration.IsWiseTechGlobalInternalSystem();
			}
		}

		bool SatisfiesMinimumVersion
		{
			get
			{
				if (string.IsNullOrEmpty(Bizo.MinCW1Version))
				{
					return true;
				}

				var minVersion = new VersionNumber(Bizo.MinCW1Version);
				return minVersion <= CurrentVersion;
			}
		}

		bool SatisfiesMaximumVersion
		{
			get
			{
				if (string.IsNullOrEmpty(Bizo.MaxCW1Version))
				{
					return true;
				}

				var maxVersion = new VersionNumber(Bizo.MaxCW1Version);
				return maxVersion >= CurrentVersion;
			}
		}

		static VersionNumber CurrentVersion
		{
			get
			{
#if DEBUG
				return Globals.IsTest ? ReleaseInfo.Instance.VersionNumber : ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow().VersionNumber;
#else
				return ReleaseInfo.Instance.VersionNumber;
#endif
			}
		}

		int ActiveOnRanking
		{
			get
			{
				switch (Bizo.ActiveOn)
				{
					case RefActiveOn.ProductionOnly:
						return 1;
					case RefActiveOn.TestOnly:
						return 2;
					case RefActiveOn.All:
						return 3;
					case RefActiveOn.None:
						return 4;
					default:
						throw new ArgumentOutOfRangeException(Bizo.ActiveOn);
				}
			}
		}
	}
}
