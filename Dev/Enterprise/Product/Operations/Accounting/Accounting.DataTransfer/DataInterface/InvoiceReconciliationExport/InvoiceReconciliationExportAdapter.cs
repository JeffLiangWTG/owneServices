using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.DataInterface
{
	public class InvoiceReconciliationExportAdapter
	{
		public InvoiceReconciliationExportAdapter(BusinessObjectFactory factory, ChinaReconciliationExportWrapper wrapper)
		{
			Factory = factory;
			this.wrapper = wrapper;
		}

		readonly BusinessObjectFactory Factory;
		readonly ChinaReconciliationExportWrapper wrapper;
#if DEBUG
		public InvoiceReconciliationVoucherCollection ReconciliationVoucherCollectionExposedForTest => ReconciliationVoucherCollection;
#endif
		InvoiceReconciliationVoucherCollection ReconciliationVoucherCollection;

		public IEnumerable<BusinessObject> GetBusinessObjectsForExport()
		{
			List<InvoiceReconciliation> listReconciliations = new List<InvoiceReconciliation>();

			ReconciliationVoucherCollection = new InvoiceReconciliationVoucherCollection(Factory, wrapper.ComplianceSubType, wrapper.ExportStatus);
			ReconciliationVoucherCollection.AddElements(wrapper.PostDateFrom, wrapper.PostDateTo.AddDays(1), wrapper.BranchCode, false);

			foreach (VoucherKingDeeK3 voucher in ReconciliationVoucherCollection)
			{
				var reconciliation = new InvoiceReconciliation();
				reconciliation.SetReconciliationValue(voucher);
				listReconciliations.Add(reconciliation);
			}

			return listReconciliations;
		}

		public string Notification
		{
			get
			{
				IList<ZString> listToNotify = null;

				if (ReconciliationVoucherCollection != null && ReconciliationVoucherCollection.Notifications.Count > 0)
				{
					var list = ReconciliationVoucherCollection.Notifications;

					listToNotify = list.Where(x => x.Key == VoucherLineErrorType.LocalGLNotMapped).Select(x => x.Message).Distinct().ToList();	// For developer key usage only. Not show to client.
					if (listToNotify.Count > 0)
					{
						listToNotify.Insert(0, Res.GetString("9F8AC469-8998-4ba4-A465-56768B1D0876",
@"The Invoice Reconciliation Export cannot be completed as there are GL Accounts used in posted transactions that have not been properly mapped.

Please create  GL Mappings for the following Parent GL Accounts before running the export.

Parent GL Accounts without mapping:"));
					}
					else if (list.Count > 0)
					{
						listToNotify = list.Select(x => x.Message).ToList();
						listToNotify.Insert(0, Res.GetString("84220BAC-B05D-4b60-B380-773D66D92703", "Some voucher lines are invalid. Please fix them before running the export:"));
					}

					if (listToNotify != null && listToNotify.Count > 0)
					{
						return new ZStringBuilder(listToNotify).ToStringWithNewLineBetweenAppends();
					}
				}

				return null;
			}
		}

		public void AddEventForExportedTransaction()
		{
			if (ReconciliationVoucherCollection.ListTransactionHeader.Any())
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				var transactionHeaderCollection = newFactory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, ReconciliationVoucherCollection.ListTransactionHeader.Select(c => c.PK)));
				foreach (AccTransactionHeader transactionHeader in transactionHeaderCollection)
				{
					transactionHeader.Logs.AddNew(AutoEvents.DataExport, "Invoice Reconciliation Export");
				}
				newFactory.Save();
			}
		}

		public IExportCollectionInfo GetMultiTypeCollectionInfo(IEnumerable<BusinessObject> businessObjects)
		{
			var info = new ExportCollectionInfoImpl(Factory, businessObjects);
			AddProperties<InvoiceReconciliation>(info, ResString.GetMultilingualString("bc3a2532-47e1-4797-a13a-f6d59d0b4ca9", "Invoice Reconciliation"));
			return info;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Class Name does not need to be localised")]
		void AddProperties<T>(ExportCollectionInfoImpl info, MultilingualString name)
		{
			var fields = new ImportPropertyInfoCollection();
			var schemaClassName = "Schema";
			var schemaType = typeof(T).BaseType?.GetNestedType(schemaClassName) ?? typeof(T).BaseType?.BaseType?.GetNestedType(schemaClassName);
			if (schemaType != null)
			{
				foreach (var fieldInfo in schemaType.GetFields())
				{
					if (fieldInfo.FieldType.Name == "String" && !fieldInfo.Name.EndsWith("MaxLength", StringComparison.OrdinalIgnoreCase))
					{
						fields.Add(new ImportPropertyInfoImpl<T>(fieldInfo.Name));
					}
				}
			}
			info.Add(name, typeof(T), fields);
		}
	}
}
