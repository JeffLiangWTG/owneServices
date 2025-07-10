using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class Test : TestCaseWithFactory
	{
		public void TestGetVoucherNumberForAP()
		{
			APInvoice aPInvoice = Factory.NewWithValidTestData<APInvoice>();
			aPInvoice.AH_ConsolidatedInvoiceRef = "123456";
			aPInvoice.AH_TransactionReference = "666666";
			aPInvoice.AH_TransactionNum = "789987";
			VoucherNumberLookUp lookUp = new VoucherNumberLookUp(aPInvoice);
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			AssertEquals("123456", lookUp.GetVoucherNumber());
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			AssertEquals("123456", lookUp.GetVoucherNumber());
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertEquals("123456", lookUp.GetVoucherNumber());
		}
	}
}