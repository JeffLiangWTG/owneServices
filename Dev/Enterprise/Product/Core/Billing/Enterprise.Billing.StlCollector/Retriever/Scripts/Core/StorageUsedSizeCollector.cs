using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using Enterprise.Integration.Billing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	public class StorageUsedSizeCollector : IStlItem
	{
		internal const string DbNameColumnName = "DbName";
		internal const string DbGuidColumnName = "DbGuid";
		internal const string UsedSizeMbColumnName = "UsedSizeMb";

		public StorageUsedSizeCollector(BillingTransactionFactory transactionFactory)
		{
			this.transactionFactory = transactionFactory;
		}

		public StorageUsedSizeCollector()
			: this(new BillingTransactionFactory())
		{
		}

		readonly BillingTransactionFactory transactionFactory;

		public IEnumerable<ZSqlParameter> GetInputParameters(IDateTimeRange dateTimeRange)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange)
		{
			var startDate = dateTimeRange.DailyRangeStartInclusive ?? dateTimeRange.StartDateTimeInclusive;
			var bizoCollection = new DynamicBusinessObjectCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			bizoCollection.Load("ep_DatabaseSetInfo");
			foreach (var bizo in bizoCollection.ToArray())
			{
				var dataRow = ((IBusinessObjectInternals)bizo).Row;
				var billingTransaction = transactionFactory.CreateTransaction(Code, (int)dataRow[UsedSizeMbColumnName], startDate, (string)dataRow[DbNameColumnName], reference5: (string)dataRow[DbGuidColumnName]);
				if (billingTransaction != null)
				{
					yield return billingTransaction;
				}
			}

			var persister = ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister(SystemDataRegistry.Instance.EDocsStorageProvider.Value);
			if (persister != null)
			{
				var billingTransaction = transactionFactory.CreateTransaction(Code, (int)persister.GetBucketSizeInMb(), startDate, SystemDataRegistry.Instance.DocManagerStorageBucketName.Value?.Trim());
				if (billingTransaction != null)
				{
					yield return billingTransaction;
				}
			}
		}

		public string Code => "STS";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "literal String is safe to use in this Context")]
		public string Role => "Hosting";

		public string Module => "WiseCloud";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "literal String is safe to use in this Context")]
		public string Function => "Storage including Database and S3 Bucket";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		public string Feature => "Storage including Database and S3 Bucket Used Size in Megabytes";

		public StlDataGrain StlGrain => StlDataGrain.Daily;

		public bool IsSystemLevel => true;

		public bool IsMandatoryForMilestones => true;

		public bool IsActive => EnvProxy.IsHostedWithCargowise || DataRegistry.Instance.EHubTesting;

		public Exception CollectionException { get; set; }

		public bool CollectionOccurred { get; set; }

		public StlDateType DateType => StlDateType.DateTime;

		public StlCollectorType CollectorType => StlCollectorType.Custom;

		public DateTime CollectionStartDateUtc => DateTime.MinValue;
	}
}
