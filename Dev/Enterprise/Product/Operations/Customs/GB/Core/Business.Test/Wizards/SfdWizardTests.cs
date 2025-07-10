using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Wizards.CFSP.SFD.Testing
{
	class SfdWizardTests : TestCaseWithFactory
	{
		[TestDate(2010, 03, 31)]
		public void TestCreateSfd()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var decSfd = Factory.New<JobDeclaration>();
			decSfd.JE_TransportMode = TransportTypeList.Codes.Air;
			decSfd.JE_MessageType = "IMP";
			decSfd.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			decSfd.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
			decSfd.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var cei = decSfd.CustomsEntryInstructions.AddNew();
			SfdWizardManager wizardManager = new SfdWizardManager(decSfd);
			wizardManager.SfdWizard.Consignee = importer.PK;
			wizardManager.SfdWizard.LocationOfGoods = "LHRBAC";
			wizardManager.SfdWizard.NumberPackages = 69;
			wizardManager.SfdWizard.PackageType = "BG";
			wizardManager.SfdWizard.CPC = "1234567";
			wizardManager.SfdWizard.GoodsDesc = "Stuff and nonsense";
			wizardManager.SfdWizard.MarksAndNumbers = "Fragile";
			wizardManager.SfdWizard.InvoiceNumber = "INV123";

			wizardManager.CreateSfd();

			AssertEquals(importer.PK, decSfd.JE_OH_Importer);
			var lineCreated = decSfd.InvoiceLines[0];
			AssertEquals("IMP", decSfd.JE_MessageType);
			AssertEquals("IFD", decSfd.JE_DeclarationType);
			AssertEquals("IM", decSfd.JE_EntryStyle);
			AssertEquals("F", decSfd.JE_EntrySubStyle);
			AssertEquals(1, decSfd.CustomsEntryInstructions.Count);
			AssertEquals(69, decSfd.JE_TotalNoOfPacks);
			AssertEquals("PKG", decSfd.JE_TotalNoOfPacksPackType);
			AssertEquals("INV123", decSfd.Invoices[0].JZ_InvoiceNumber);
			AssertEquals("1234567", lineCreated.JI_Procedure);
			AssertEquals(0, lineCreated.Taxes.Count);
			AssertEquals("", decSfd.ZG_VATDeferNumber);
			AssertEquals("", decSfd.ZG_VATDeferType);
			AssertEquals("", decSfd.JE_DefermentAccountNumber);
			AssertEquals("", decSfd.JE_PaymentMethod);
			AssertEquals("Stuff and nonsense", decSfd.JE_GoodsDescription);
			AssertEquals("Stuff and nonsense", lineCreated.JI_Description);
			AssertEquals("LHR", decSfd.JE_LocationOfGoods);
			AssertEquals("LHRBAC", decSfd.JE_SubLocationOfGoods);
			AssertEquals("BAC", decSfd.SubLocation);
			AssertEquals("CFS", decSfd.JE_EidrType);
			var pivot = lineCreated.PackagesForInvoiceLinesForBindingOnly[1];
			AssertEquals(69, pivot.PackQty);
			AssertEquals("BG", pivot.Package.CW_PackType);
			AssertEquals("Fragile", pivot.Package.CW_MarksAndNos);
			AssertEquals(69, pivot.Package.CW_PackQty);
			AssertEquals(true, pivot.IsLinked);
		}
	}

	[TestedType(typeof(SfdWizard))]
	public class SfdWizardNPBOTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SfdWizard(this.Factory);
		}
	}
}
