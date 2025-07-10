using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	public static class MessageBuilderTestHelper
	{
		public static (OrgHeader Proxy, OrgHeader Supplier) FillMandatoryFields(CNEntryHeaderTestData testData)
		{
			var factory = testData.JobDeclaration.Factory;
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusMapType("CNTRY", "OUT", "Country Code Mapping", true);
			helper.CreateCusMap("CNTRY", "US", "502", startDate, endDate, Core.Constants.CountryCodes.China);
			factory.Save();

			var proxy = factory.New<OrgHeader>();
			proxy.OH_Code = "Agent";
			proxy.OH_FullName = "Agent Company";
			var ccdCode = proxy.CustomsCodes.AddNew();
			ccdCode.OK_CodeType = "CCD";
			ccdCode.OK_CustomsRegNo = "CCD123";
			testData.JobDeclaration.Branch.GB_OH_OrgProxy = proxy.PK;

			testData.InvoiceLine.JI_NameOfGoods = "Car";
			testData.InvoiceLine.JI_Tariff = "3005109000";
			testData.EntryInstruction.CEI_Style = "AB";

			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testData.EntryHeader, false, EntryTypeList.Codes.CustomsEntry);
			testData.InvoiceLine.JI_CountryOfOrigin = "US";

			var supplier = CNCusEntryHeaderHelper.CreateNewAddress(factory, "Supplier Company", "CcdCode", "UscCode", "CiqCode").Header;
			testData.JobDeclaration.JE_OH_Supplier = supplier.PK;
			testData.InvoiceLine.JI_TradeQuantity = 11m;
			factory.Save();

			return (proxy, supplier);
		}
	}
}
