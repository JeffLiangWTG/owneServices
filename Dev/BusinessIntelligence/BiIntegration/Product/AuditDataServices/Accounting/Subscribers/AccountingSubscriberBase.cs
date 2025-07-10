using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.AuditDataServices.Accounting.Subscribers
{
	public abstract class AccountingSubscriberBase : ActualDataChangesAuditSubscriber
	{
		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => false;

		public override bool NotifyDelete => false;

		public override IEnumerable<SchemaColumn> SpecificColumns => null;

		public override bool IsRequired()
		{
			return AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value;
		}

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			logger.Log(LogType.Debug, $"{Table.TableName} Received DataRows: {changeTable.Rows.Count}");
			changeTable.TableName = Table.TableName;
			var changeRows = changeTable.Rows.Cast<DataRow>().Where(CheckGeneralLedgerDataRow).ToArray();
			if (changeRows.Length > 0)
			{
				GeneralLedgerDataProcessor.ProcessData(changeRows);
			}
		}

		protected bool CheckGeneralLedgerDataRow(DataRow changeRow)
		{
			var companyPK = GetCompanyPKForIsCDCEnabledForChangeRow(changeRow);
			var changeDate = GetCreateDateForIsCDCEnabledForChangeRow(changeRow);

			return IsCDCEnabledForChangeRow(companyPK, changeDate);
		}

		protected T GetDataRowValue<T>(object data)
		{
			var result = default(T);

			if (data is T tData)
			{
				result = tData;
			}

			return result;
		}

		protected abstract Guid GetCompanyPKForIsCDCEnabledForChangeRow(DataRow changeRow);

		protected abstract DateTime GetCreateDateForIsCDCEnabledForChangeRow(DataRow changeRow);

		bool IsCDCEnabledForChangeRow(Guid companyPK, DateTime changeDate)
		{
			var dateInCache = CDCStartDateCache.TryGetValue(companyPK, out var cdcStartDate);
			if (!dateInCache)
			{
				var dateBytes = RegistryDataAccessor.DisposableInstance.GetBinaryValue("GenerateJournalEntriesCDCStartDate", companyPK, Guid.Empty);
				cdcStartDate = dateBytes == null ? DateTime.MinValue : new DateTimeRegistryDataType().Deserialise(dateBytes);
				CDCStartDateCache.Add(companyPK, cdcStartDate);
			}

			if (cdcStartDate == DateTime.MinValue || (changeDate != DateTime.MinValue && cdcStartDate >= changeDate))
			{
				return false;
			}
			return true;
		}

		readonly Dictionary<Guid, DateTime> CDCStartDateCache = new Dictionary<Guid, DateTime>();

		IGeneralLedgerDataProcessor GeneralLedgerDataProcessor => generalLedgerDataProcessor ?? (generalLedgerDataProcessor = ObjectFactory.Get<IGeneralLedgerDataProcessor>());
		IGeneralLedgerDataProcessor generalLedgerDataProcessor;

		protected ReadOnlyBusinessObjectFactory DataFactory => dataFactory ?? (dataFactory = new ReadOnlyBusinessObjectFactory());
		ReadOnlyBusinessObjectFactory dataFactory;
	}
}
