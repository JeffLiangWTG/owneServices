using System;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.India
{
	public class IndiaAdditionalDataItemsProvider : IAdditionalDataItemsProvider
	{
		public GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection GetAdditionalHeaderDataItems(AccEInvoicingBatch batch, GlbBranch branch, TransactionBatch universalTransactionBatch, ICountryEInvoicingObjectFactory countryFactory, INotifications warnings)
		{
			var result = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection();
			var registryValue = AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.GetValueWithoutFallback(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty);
			if (registryValue != DateTime.MinValue)
			{
				var item = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem { Key = RegComplianceNumFrom, Value = registryValue.ToString("yyyy-MM-dd") };
				result.Add(item);
			}

			return result;
		}

		const string RegComplianceNumFrom = "Reg_ComplianceNumFrom";
	}
}
