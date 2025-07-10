using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Testing
{
	public static class TestDataHelper
	{
		public static void CreateInstructionsForWarehouseAndGoodsLocationTest(BusinessObjectFactory factory, out OrgHeader warehouseOUTOF, out JobDeclaration dec, out CusEntryInstruction cei4071, out CusEntryInstruction cei4000, out CusEntryInstruction cei7100, out CusEntryInstruction cei7171, out CusEntryInstruction cei7200)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var procedure4071 = helper.CreateRefCusProcedure(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "A", "40", "71", "000", "whatever", "IMP", group: "H1", outOfWarehouse: true);
			var procedure4000 = helper.CreateRefCusProcedure(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "A", "40", "00", "000", "whatever", "IMP", group: "H1");
			procedure4000.ZZ6_OutOfInwardProcessing = "Y";
			var procedure7100 = helper.CreateRefCusProcedure(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "A", "71", "00", "000", "whatever", "IMP", group: "H2", intoWarehouse: true);
			var procedure7171 = helper.CreateRefCusProcedure(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "A", "71", "71", "000", "whatever", "IMP", group: "H2", intoWarehouse: true, outOfWarehouse: true);
			var procedure7200 = helper.CreateRefCusProcedure(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "A", "72", "00", "000", "whatever", "IMP", group: "H2");
			procedure7200.ZZ6_IntoInwardProcessing = "Y";
			factory.Save();

			var warehouseINTO = factory.NewWithValidTestData<OrgHeader>();
			warehouseINTO.CompanyData.OB_IMUsedBondedWhs = true;
			warehouseOUTOF = factory.NewWithValidTestData<OrgHeader>();
			warehouseOUTOF.CompanyData.OB_IMUsedBondedWhs = true;
			warehouseINTO.MainAddress.OA_RN_NKCountryCode = "GB";
			warehouseOUTOF.MainAddress.OA_RN_NKCountryCode = "GB";
			warehouseINTO.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U1234567INN", "GB");
			warehouseOUTOF.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U7654321OUT", "GB");

			dec = factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_Calc_LocationOtherInformationCountry = "AA";
			dec.JE_Calc_LocationOtherInformationType = "BB";
			dec.JE_LocationQualifier = "CC";
			dec.JE_GoodsLocation = "DDDDDDDDD";

			cei4071 = (CusEntryInstruction)dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			cei4000 = (CusEntryInstruction)dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			cei7100 = (CusEntryInstruction)dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			cei7171 = (CusEntryInstruction)dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			cei7200 = (CusEntryInstruction)dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			cei4071.CEI_Style = "H1";
			cei4000.CEI_Style = "H1";
			cei7100.CEI_Style = "H2";
			cei7171.CEI_Style = "H2";
			cei7200.CEI_Style = "H2";
			cei4071.CEI_OA_Warehouse = warehouseOUTOF.MainAddress.PK;
			cei4071.CEI_OA_Warehouse2 = warehouseINTO.MainAddress.PK;
			cei4000.CEI_OA_Warehouse = warehouseOUTOF.MainAddress.PK;
			cei4000.CEI_OA_Warehouse2 = warehouseINTO.MainAddress.PK;
			cei7100.CEI_OA_Warehouse = warehouseOUTOF.MainAddress.PK;
			cei7100.CEI_OA_Warehouse2 = warehouseINTO.MainAddress.PK;
			cei7171.CEI_OA_Warehouse = warehouseOUTOF.MainAddress.PK;
			cei7171.CEI_OA_Warehouse2 = warehouseINTO.MainAddress.PK;
			cei7200.CEI_OA_Warehouse = warehouseOUTOF.MainAddress.PK;
			cei7200.CEI_OA_Warehouse2 = warehouseINTO.MainAddress.PK;

			var invoice = dec.Invoices.AddNew();
			var inLine4071 = invoice.InvoiceLines.AddNew();
			var inLine4000 = invoice.InvoiceLines.AddNew();
			var inLine7100 = invoice.InvoiceLines.AddNew();
			var inLine7171 = invoice.InvoiceLines.AddNew();
			var inLine7200 = invoice.InvoiceLines.AddNew();
			inLine4071.JI_CEI = cei4071.PK;
			inLine4000.JI_CEI = cei4000.PK;
			inLine7100.JI_CEI = cei7100.PK;
			inLine7171.JI_CEI = cei7171.PK;
			inLine7200.JI_CEI = cei7200.PK;
			inLine4071.JI_Procedure = procedure4071.FullCodeCurrentPlusPreviousPlusConcession;
			inLine4000.JI_Procedure = procedure4000.FullCodeCurrentPlusPreviousPlusConcession;
			inLine7100.JI_Procedure = procedure7100.FullCodeCurrentPlusPreviousPlusConcession;
			inLine7171.JI_Procedure = procedure7171.FullCodeCurrentPlusPreviousPlusConcession;
			inLine7200.JI_Procedure = procedure7200.FullCodeCurrentPlusPreviousPlusConcession;
		}

		public static GlbCompany CreateCompanyAndBranch(BusinessObjectFactory factory, string identifier)
		{
			var comp = factory.New<GlbCompany>();
			comp.GC_Code = "GC" + identifier;
			comp.CompanyName = "Test Company " + identifier;
			comp.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			var gbBranch = factory.New<GlbBranch>();
			gbBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			gbBranch.GB_Code = "BR" + identifier;
			gbBranch.GB_GC = comp.PK;

			factory.Save();

			return comp;
		}

		public static void CreateCredentials(BusinessObjectFactory factory, ZGuid companyPK, ZString badge, ZString eori, ZString passwordType)
		{
			var extPassword = factory.New<GlbExternalPassword_GB>();

			extPassword.GP_PasswordType = passwordType;
			extPassword.EORI = eori;
			extPassword.Badge = badge;
			extPassword.GP_GC = companyPK;

			factory.Save();
		}

		public static void CreateCredentials(BusinessObjectFactory factory, ZGuid companyPK, ZString badge, ZString eori, ZString passwordType, ZDateTime expiryDate)
		{
			var extPassword = factory.New<GlbExternalPassword_GB>();

			extPassword.GP_PasswordType = passwordType;
			extPassword.EORI = eori;
			extPassword.Badge = badge;
			extPassword.GP_ExpiryDate = expiryDate;
			extPassword.GP_GC = companyPK;
			extPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			factory.Save();
		}

		public static CDSResponseStatusTestDataHelper CreateCDSCustomsStatuses(BusinessObjectFactory factory)
		{
			var testHelper = new CDSResponseStatusTestDataHelper(factory);
			testHelper.CreateCDSCustomsStatuses();
			return testHelper;
		}
	}
}
