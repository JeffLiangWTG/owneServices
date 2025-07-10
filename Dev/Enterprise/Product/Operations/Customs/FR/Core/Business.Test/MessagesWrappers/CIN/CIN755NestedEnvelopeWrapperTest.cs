using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN.Testing
{
	class CIN755NestedEnvelopeWrapperTest : TestCaseWithFactory
	{
		public void TestPackagesCount()
		{
			var cTO = Factory.New<OrgHeader>();
			cTO.FillWithValidTestData();
			var addressCTO = cTO.Addresses.AddNew();

			var customCodeCTO = cTO.CustomsCodes.AddNew();
			customCodeCTO.OK_CodeType = CusEntryHeader.Schema.CINOACICode;
			customCodeCTO.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			customCodeCTO.OK_CustomsRegNo = "FR123456800";
			customCodeCTO.OK_OA_PremisesAddress = addressCTO.PK;

			var depot = Factory.New<OrgHeader>();
			depot.FillWithValidTestData();
			var addressDepot = depot.Addresses.AddNew();

			var customCodedepot = depot.CustomsCodes.AddNew();
			customCodedepot.OK_CodeType = CusEntryHeader.Schema.CINOACICode;
			customCodedepot.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			customCodedepot.OK_CustomsRegNo = "FR123456799";
			customCodedepot.OK_OA_PremisesAddress = addressDepot.PK;

			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.Add(cusEntryHeader);
			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackType = "PK";

			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 3;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var packing2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
			packing2.IsLinked = true;
			packing2.PackQty = 7;

			var entryLine1 = cusEntryHeader.MergedLines.AddNew();
			var entryLine2 = cusEntryHeader.MergedLines.AddNew();

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			AssertEquals(10, cusEntryHeader.PackagesCount);

			var nestedEnvelopeWrapper = new CIN755NestedEnvelopeWrapper(cusEntryHeader);
			Assert(nestedEnvelopeWrapper.CIN.Contains("QTY+156:10:COL"));
		}

		public void TestMagasin()
		{
			var cTO = Factory.New<OrgHeader>();
			cTO.FillWithValidTestData();
			var addressCTO = cTO.Addresses.AddNew();

			var customCodeCTO = cTO.CustomsCodes.AddNew();
			customCodeCTO.OK_CodeType = CusEntryHeader.Schema.CINOACICode;
			customCodeCTO.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			customCodeCTO.OK_CustomsRegNo = "FR123456800";
			customCodeCTO.OK_OA_PremisesAddress = addressCTO.PK;

			var depot = Factory.New<OrgHeader>();
			depot.FillWithValidTestData();
			var addressDepot = depot.Addresses.AddNew();

			var customCodedepot = depot.CustomsCodes.AddNew();
			customCodedepot.OK_CodeType = CusEntryHeader.Schema.CINOACICode;
			customCodedepot.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			customCodedepot.OK_CustomsRegNo = "FR123456799";
			customCodedepot.OK_OA_PremisesAddress = addressDepot.PK;

			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.Add(cusEntryHeader);
			var nestedEnvelopeWrapper = new CIN755NestedEnvelopeWrapper(cusEntryHeader);
			AssertEquals(ZString.Empty, nestedEnvelopeWrapper.MAGASIN);

			cusEntryHeader.Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = addressCTO.PK;
			AssertEquals("FR123456800", nestedEnvelopeWrapper.MAGASIN);

			cusEntryHeader.Declaration.DepotDocAddress.E2_OA_Address = addressDepot.PK;
			AssertEquals("FR123456799", nestedEnvelopeWrapper.MAGASIN);

			Assert(nestedEnvelopeWrapper.CIN.Contains("LOC+FR123456799"));
		}
	}
}
