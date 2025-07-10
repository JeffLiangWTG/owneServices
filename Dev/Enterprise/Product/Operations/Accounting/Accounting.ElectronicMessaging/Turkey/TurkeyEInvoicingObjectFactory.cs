using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	public class TurkeyEInvoicingObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.Turkey;
		protected override string ApTransactionListRequestBatchId => AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeatures ? "TR:AP:GetInboxInvoiceListBatch" : null;
		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting() => new PopulateOptionalXUTFieldsSetting(populateShipments: true);
		protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider
			=> new TurkeyGlobalXUEFunctionalityProvider(this);
	}
}
