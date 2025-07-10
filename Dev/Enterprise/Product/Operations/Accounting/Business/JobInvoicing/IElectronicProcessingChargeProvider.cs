using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IElectronicProcessingChargeProvider
	{
		void CreateElectronicProcessingCharge(Job job);
		void InsertAndSetElectronicProcessingChargeRegistry();
		bool HasElectronicProcessingChargeCurrencyExchangeRate(Job job);
		bool ShouldCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting(ZGuid chargeCodePK, ZGuid companyPK);
	}
}
