using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class EMCSJobDeclarationDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCloneEMCSJobDeclaration()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var officeOfDispatch = declaration.CustomsOffices.AddNew(OfficeCodes_EMCS.Codes.OfficeOfDispatch, "GB11111");
			var officeOfDelivery = declaration.CustomsOffices.AddNew(OfficeCodes_EMCS.Codes.OfficeOfDelivery, "GB22222");
			var competentAuthorityOfDispatch = declaration.CustomsOffices.AddNew(OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch, "GB33333");
			var competentAuthorityOfArrival = declaration.CustomsOffices.AddNew(OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival, "GB44444");

			var package1 = declaration.EMCSPackages.AddNew();
			package1.B5_UnitCount = 11;
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.ZG_IsMainPack = true;
			var packagePivot1 = invoiceLine1.EMCSPackagePivots[0];
			packagePivot1.IsForInvoiceLine = true;

			var clonedDec = (EMCSJobDeclaration)(new EMCSJobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone());

			AssertEquals("Cloned 4 offices", 4, clonedDec.CustomsOffices.Count);
			AssertEquals("Cloned OfficeOfDispatch", true, clonedDec.CustomsOffices.Cast<OfficeCode>().Any(x => x.CY_Code == OfficeCodes_EMCS.Codes.OfficeOfDispatch && x.CY_Data == "GB11111"));
			AssertEquals("Cloned OfficeOfDelivery", true, clonedDec.CustomsOffices.Cast<OfficeCode>().Any(x => x.CY_Code == OfficeCodes_EMCS.Codes.OfficeOfDelivery && x.CY_Data == "GB22222"));
			AssertEquals("Cloned CompetentAuthorityOfDispatch", true, clonedDec.CustomsOffices.Cast<OfficeCode>().Any(x => x.CY_Code == OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch && x.CY_Data == "GB33333"));
			AssertEquals("Cloned CompetentAuthorityOfArrival", true, clonedDec.CustomsOffices.Cast<OfficeCode>().Any(x => x.CY_Code == OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival && x.CY_Data == "GB44444"));

			AssertEquals("Cloned package pivot IsForInvoiceLine setting", true, clonedDec.InvoiceLines[0].EMCSPackagePivots[0].IsForInvoiceLine);
			AssertEquals("Cloned package count", "11", clonedDec.EMCSPackages[0].B5_UnitCount.ToString());
		}
	}
}
