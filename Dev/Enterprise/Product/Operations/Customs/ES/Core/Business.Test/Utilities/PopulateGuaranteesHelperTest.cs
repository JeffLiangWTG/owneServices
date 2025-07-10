using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class PopulateGuaranteesHelperTest : TestCaseWithFactory
	{
		public void TestImportPopulateAEATGuaranteesPotentialAndPopulateOffice()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeaderWithRuleCUS(Factory, org1.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			declaration.JE_OH_Importer = org1.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				declaration.Guarantees.RemoveAndDeleteAll();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
				invLine1.JI_FormattedProcedure = "4400";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert the office of declaration exist", 1, declaration.Guarantees.Count);
				AssertEquals("Assert the office of declaration exist and is 'Office'", 1, declaration.Guarantees.Find(x => x.PW_BondFiledPort == "Office").Count());
			});
		}

		public void TestImportNotPopulateGuaranteesWithEntryStatusPDA_CDA_CLP_CLR()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			declaration.JE_OH_Importer = org1.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "4000";
			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("To assert that PDA_CDA_CLP_CLR is not filled, there must first be one (PDI).", 1, declaration.Guarantees.Count);

				declaration.Guarantees.RemoveAndDeleteAll();
				entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("You should not have guarantee with Entry Status PDA.", 0, declaration.Guarantees.Count);

				entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("You should not have guarantee with Entry Status CDA.", 0, declaration.Guarantees.Count);

				entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("You should not have guarantee with Entry Status CLP.", 0, declaration.Guarantees.Count);

				entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("You should not have guarantee with Entry Status CLR.", 0, declaration.Guarantees.Count);
			});
		}

		public void TestImportNotPopulateGuaranteesWithEntryInstructionT2C_T2L()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			declaration.JE_OH_Importer = org1.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "4000";

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("To assert that T2C_T2L is not filled, there must first be one (PDI).", 1, declaration.Guarantees.Count);

				declaration.Guarantees.RemoveAndDeleteAll();
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("You should not have guarantee with Entry Instruction T2C.", 0, declaration.Guarantees.Count);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("You should not have guarantee with Entry Instruction T2L.", 0, declaration.Guarantees.Count);
			});
		}

		public void TestImportIfExistGuarantees()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			declaration.JE_OH_Importer = org1.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "4000";

			GuaranteesTestHelper.CreateMultipleGuaranteesForEntryInstruction(declaration, entryInstruction.PK, new CargoWise.Types.ZString[] { "22ESAGQ9990000096", "22ESXGL9990000095" });

			Factory.Save();
			populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("By default you must have two warranties created initially", 2, declaration.Guarantees.Count);

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("You should have two guarantees since you do not have to recalculate", 2, declaration.Guarantees.Count);

				declaration.Guarantees.RemoveAndDeleteAll();
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Once the old ones have been deleted, you have to calculate a single guarantee.", 1, declaration.Guarantees.Count);
			});
		}

		public void TestImportNotPopulateGuaranteesWihtMethodOfPaymentNotEqualR()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			declaration.JE_OH_Importer = org1.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "4000";

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert with Method Of Payment equal to R.", 1, declaration.Guarantees.Count);

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.A;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert with Method Of Payment not equal to R.", 0, declaration.Guarantees.Count);
			});
		}

		public void TestImportPopulateAEATClearanceGuarantees()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			declaration.JE_OH_Importer = org1.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "4000";

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that A has in the fifth position and L has in the seventh position there is only one", 1, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and L in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000096").Count());
			});
		}

		public void TestImportPopulateAEATGuaranteesPotentialWihtCPCStart_44_48_49_51_53_And7CharacterOfGuarantee_P_Q_L_R_S()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			declaration.JE_OH_Importer = org1.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				declaration.Guarantees.RemoveAndDeleteAll();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
				invLine1.JI_FormattedProcedure = "4400";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that A has in the fifth position and P has in the seventh position plus the L", 2, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_FormattedProcedure = "4800";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that A has in the fifth position and Q has in the seventh position plus the L", 2, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and Q in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGQ9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_FormattedProcedure = "4900";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that A has in the fifth position and D has in the seventh position plus the L", 2, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and D in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGD9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_FormattedProcedure = "5100";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that A has in the fifth position and R has in the seventh position plus the L", 2, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and R in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGR9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_FormattedProcedure = "5300";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that A has in the fifth position and S has in the seventh position plus the L", 2, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and S in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGS9990000096").Count());
			});
		}

		public void TestImportPopulateAEATGuaranteesPotentialWihtCPCStart_40AndPreferenceX15_X23_X40_X84_X86_X96_7CharacterOfGuarantee_P()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			declaration.JE_OH_Importer = org1.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "4000";

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for verification CPC = 40 and without preference there must be one garantee.", 1, declaration.Guarantees.Count);
				AssertEquals("Assert that, for verification CPC = 40 and without preference there must be one garantee with L in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_PrimaryPreference = "015";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for verification CPC = 40 and preference = X15 there must be one garantee.", 2, declaration.Guarantees.Count);
				AssertEquals("Assert that, for verification CPC = 40 and preference = X15 there must be one garantee with P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_PrimaryPreference = "023";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for verification CPC = 40 and preference = X23 there must be one garantee.", 2, declaration.Guarantees.Count);
				AssertEquals("Assert that, for verification CPC = 40 and preference = X23 there must be one garantee with P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_PrimaryPreference = "040";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for verification CPC = 40 and preference = X40 there must be one garantee.", 2, declaration.Guarantees.Count);
				AssertEquals("Assert that, for verification CPC = 40 and preference = X40 there must be one garantee with P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_PrimaryPreference = "084";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for verification CPC = 40 and preference = X84 there must be one garantee.", 2, declaration.Guarantees.Count);
				AssertEquals("Assert that, for verification CPC = 40 and preference = X84 there must be one garantee with P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_PrimaryPreference = "086";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for verification CPC = 40 and preference = X86 there must be one garantee.", 2, declaration.Guarantees.Count);
				AssertEquals("Assert that, for verification CPC = 40 and preference = X86 there must be one garantee with P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_PrimaryPreference = "096";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for verification CPC = 40 and preference = X96 there must be one garantee.", 2, declaration.Guarantees.Count);
				AssertEquals("Assert that, for verification CPC = 40 and preference = X96 there must be one garantee with P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());
			});
		}

		public void TestImportPopulateAEATGuaranteesGRNImporter()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var eoriCode2 = "123456789000";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode2, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "4000";

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

				declaration.JE_OH_Importer = org2.PK;
				declaration.Declarant.OA_OH = org2.PK;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for importer and declarant without garantee not populate anything.", 0, declaration.Guarantees.Count);

				declaration.Guarantees.RemoveAndDeleteAll();
				declaration.JE_OH_Importer = org1.PK;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for importer with garantee must have one.", 1, declaration.Guarantees.Count);

				declaration.Guarantees.RemoveAndDeleteAll();
				declaration.JE_OH_Importer = org2.PK;
				declaration.Declarant.OA_OH = org1.PK;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for declarant with garantee must have one.", 1, declaration.Guarantees.Count);
			});
		}

		public void TestImportPopulateAEATGuaranteesGRNDeclarant()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var eoriCode2 = "123456789000";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode2, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org2.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "4000";

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

				declaration.JE_OH_Importer = org1.PK;
				declaration.Declarant.OA_OH = org2.PK;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, without the importer's guarantee, it finds the declarant's guarantee.", 1, declaration.Guarantees.Count);
			});
		}

		public void TestImportPopulateAEATGuaranteesGRNImporterAndDeclarant()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var eoriCode2 = "123456789000";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode2, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, org1.PK, "22ESAGL9990000050", false);
			GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, org2.PK, "22ESAGR9990000050", false);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "5100";

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

				declaration.Guarantees.RemoveAndDeleteAll();
				declaration.JE_OH_Importer = org1.PK;
				declaration.Declarant.OA_OH = org2.PK;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);

				AssertEquals("Assert that, with the importer's guarantee L, it finds the declarant's guarantee R.", 2, declaration.Guarantees.Count);
				AssertEquals("Assert that, the importer's L guarantee exists", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000050").Count());
				AssertEquals("Assert that, the declarant's R guarantee exists", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGR9990000050").Count());
			});
		}

		public void TestImportPopulateAEATSeveralEntryInstructions()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var eoriCode2 = "123456789000";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode2, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, org1.PK, "22ESAGL9990000051", false);
			GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, org2.PK, "22ESAGL9990000050", false);
			GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, org1.PK, "22ESAGR9990000051", false);
			GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, org2.PK, "22ESAGQ9990000050", false);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "5100";

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine2 = invoice2.InvoiceLines.AddNew();
			invLine2.JI_CEI = entryInstruction1.PK;
			invLine2.JI_Procedure = "B";
			invLine2.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine2.JI_FormattedProcedure = "4800";

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

				declaration.Guarantees.RemoveAndDeleteAll();
				declaration.JE_OH_Importer = org1.PK;
				declaration.Declarant.OA_OH = org2.PK;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);

				AssertEquals("Assert that, with the importer's guarantee L, it finds the declarant's guarantee R.", 4, declaration.Guarantees.Count);

				AssertEquals("Assert that, the importer's L guarantee exists", 2, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000051").Count());
				AssertEquals("Assert that, the declarant's L guarantee exists", 0, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000050").Count());
				AssertEquals("Assert that, the importer's R guarantee exists", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGR9990000051").Count());
				AssertEquals("Assert that, the declarant's Q guarantee exists", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGQ9990000050").Count());

				AssertEquals("Assert that, the importer's L guarantee exists with Entry Instruction 1", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000051" && x.EntryInstructionID == invLine1.JI_CEI).Count());
				AssertEquals("Assert that, the importer's L guarantee exists with Entry Instruction 2", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000051" && x.EntryInstructionID == invLine2.JI_CEI).Count());
				AssertEquals("Assert that, the declarant's L guarantee exists with Entry Instruction 1", 0, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000050" && x.EntryInstructionID == invLine1.JI_CEI).Count());
				AssertEquals("Assert that, the declarant's L guarantee exists with Entry Instruction 2", 0, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000050" && x.EntryInstructionID == invLine2.JI_CEI).Count());
				AssertEquals("Assert that, the importer's R guarantee exists with Entry Instruction 1", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGR9990000051" && x.EntryInstructionID == invLine1.JI_CEI).Count());
				AssertEquals("Assert that, the declarant's Q guarantee exists with Entry Instruction 2", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGQ9990000050" && x.EntryInstructionID == invLine2.JI_CEI).Count());
			});
		}

		public void TestImportPopulateAEATSeveralEntryInstructionsWhenPreviousExistOne()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var eoriCode2 = "123456789000";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode2, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, org1.PK, "22ESAGL9990000051", false);
			GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, org2.PK, "22ESAGL9990000050", false);
			GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, org1.PK, "22ESAGR9990000051", false);
			GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, org2.PK, "22ESAGQ9990000050", false);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "5100";

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

				declaration.Guarantees.RemoveAndDeleteAll();
				declaration.JE_OH_Importer = org1.PK;
				declaration.Declarant.OA_OH = org2.PK;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);

				AssertEquals("With one invoice assert that, with the importer's guarantee L, it finds the declarant's guarantee R.", 2, declaration.Guarantees.Count);

				AssertEquals("With one invoice assert that, the importer's L guarantee exists", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000051").Count());
				AssertEquals("With one invoice assert that, the declarant's L guarantee exists", 0, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000050").Count());
				AssertEquals("With one invoice assert that, the importer's R guarantee exists", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGR9990000051").Count());
				AssertEquals("With one invoice assert that, the declarant's Q guarantee exists", 0, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGQ9990000050").Count());

				AssertEquals("With one invoice assert that, the importer's L guarantee exists with Entry Instruction 1", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000051" && x.EntryInstructionID == invLine1.JI_CEI).Count());
				AssertEquals("With one invoice assert that, the declarant's L guarantee exists with Entry Instruction 1", 0, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000050" && x.EntryInstructionID == invLine1.JI_CEI).Count());
				AssertEquals("With one invoice assert that, the importer's R guarantee exists with Entry Instruction 1", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGR9990000051" && x.EntryInstructionID == invLine1.JI_CEI).Count());
			});

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.B;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine2 = invoice2.InvoiceLines.AddNew();
			invLine2.JI_CEI = entryInstruction1.PK;
			invLine2.JI_Procedure = "B";
			invLine2.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine2.JI_FormattedProcedure = "4800";

			Factory.Save();
			declaration.Guarantees.RemoveAndDeleteAll();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

				declaration.JE_OH_Importer = org1.PK;
				declaration.Declarant.OA_OH = org2.PK;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);

				AssertEquals("when adding a second invoice assert that, with the importer's guarantee L, it finds the declarant's guarantee R.", 4, declaration.Guarantees.Count);

				AssertEquals("when adding a second invoice assert that, the importer's L guarantee exists", 2, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000051").Count());
				AssertEquals("when adding a second invoice assert that, the declarant's L guarantee exists", 0, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000050").Count());
				AssertEquals("when adding a second invoice assert that, the importer's R guarantee exists", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGR9990000051").Count());
				AssertEquals("when adding a second invoice assert that, the declarant's Q guarantee exists", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGQ9990000050").Count());

				AssertEquals("when adding a second invoice assert that, the importer's L guarantee exists with Entry Instruction 1", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000051" && x.EntryInstructionID == invLine1.JI_CEI).Count());
				AssertEquals("when adding a second invoice assert that, the importer's L guarantee exists with Entry Instruction 2", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000051" && x.EntryInstructionID == invLine2.JI_CEI).Count());
				AssertEquals("when adding a second invoice assert that, the declarant's L guarantee exists with Entry Instruction 1", 0, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000050" && x.EntryInstructionID == invLine1.JI_CEI).Count());
				AssertEquals("when adding a second invoice assert that, the declarant's L guarantee exists with Entry Instruction 2", 0, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000050" && x.EntryInstructionID == invLine2.JI_CEI).Count());
				AssertEquals("when adding a second invoice assert that, the importer's R guarantee exists with Entry Instruction 1", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGR9990000051" && x.EntryInstructionID == invLine1.JI_CEI).Count());
				AssertEquals("when adding a second invoice assert that, the declarant's Q guarantee exists with Entry Instruction 2", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGQ9990000050" && x.EntryInstructionID == invLine2.JI_CEI).Count());
			});
		}

		public void TestImportPopulateATCGuaranteesWihtIslandCanary_61_62_63_64_65_66_67()
		{
			var helper = CanaryIslandSetUp();

			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			declaration.JE_OH_Importer = org1.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.ZG_MethodOfPayment2 = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "4800";

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
				declaration.Guarantees.RemoveAndDeleteAll();
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Without country of destination the fifth guarantee position is A.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				declaration.ZG_DestinationState = "61";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("With country of destination = 61 the fifth guarantee position is C.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				declaration.ZG_DestinationState = "62";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("With country of destination = 62 the fifth guarantee position is C.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				declaration.ZG_DestinationState = "63";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("With country of destination = 63 the fifth guarantee position is C.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				declaration.ZG_DestinationState = "64";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("With country of destination = 64 the fifth guarantee position is C.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				declaration.ZG_DestinationState = "65";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("With country of destination = 65 the fifth guarantee position is C.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				declaration.ZG_DestinationState = "66";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("With country of destination = 66 the fifth guarantee position is C.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				declaration.ZG_DestinationState = "67";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("With country of destination = 67 the fifth guarantee position is C.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());
			});
		}

		public void TestImportPopulateATCClearanceGuarantees()
		{
			var helper = CanaryIslandSetUp();

			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			declaration.JE_OH_Importer = org1.PK;
			declaration.ZG_DestinationState = "61";

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.ZG_MethodOfPayment2 = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "4000";

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
				declaration.Guarantees.RemoveAndDeleteAll();
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that C has in the fifth position and L has in the seventh position there is only two", 2, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have C in fifth position and L in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());
				AssertEquals("Assert the guarantee number have A in fifth position and L in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000096").Count());
			});
		}

		public void TestImportPopulateATCGuaranteesPotentialWihtCPCStart_44_48_49_51_53_And7CharacterOfGuarantee_P_Q_L_R_S()
		{
			var helper = CanaryIslandSetUp();

			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			declaration.JE_OH_Importer = org1.PK;
			declaration.ZG_DestinationState = "61";

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.ZG_MethodOfPayment2 = MethodOfPaymentList.Codes.R;

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				declaration.Guarantees.RemoveAndDeleteAll();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
				invLine1.JI_FormattedProcedure = "4400";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that C has in the fifth position and P has in the seventh position plus the L", 4, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());
				AssertEquals("Assert the guarantee number have C in fifth position and P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGP9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_FormattedProcedure = "4800";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that C has in the fifth position and Q has in the seventh position plus the L", 4, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and Q in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGQ9990000096").Count());
				AssertEquals("Assert the guarantee number have C in fifth position and Q in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGQ9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_FormattedProcedure = "4900";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that C has in the fifth position and D has in the seventh position plus the L", 4, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and D in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGD9990000096").Count());
				AssertEquals("Assert the guarantee number have C in fifth position and D in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGD9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_FormattedProcedure = "5100";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that C has in the fifth position and R has in the seventh position plus the L", 4, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and R in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGR9990000096").Count());
				AssertEquals("Assert the guarantee number have C in fifth position and R in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGR9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_FormattedProcedure = "5300";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that C has in the fifth position and S has in the seventh position plus the L", 4, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and S in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGS9990000096").Count());
				AssertEquals("Assert the guarantee number have C in fifth position and S in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGS9990000096").Count());
			});
		}

		public void TestImportPopulateATCGuaranteesPotentialWihtCPCStart_44_48_49_51_53_And7CharacterOfGuarantee_P_Q_L_R_S_WithMethodOfPayment_Can_A()
		{
			var helper = CanaryIslandSetUp();

			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			declaration.JE_OH_Importer = org1.PK;
			declaration.ZG_DestinationState = "61";

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.ZG_MethodOfPayment2 = MethodOfPaymentList.Codes.A;

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				declaration.Guarantees.RemoveAndDeleteAll();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
				invLine1.JI_FormattedProcedure = "4400";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that C has in the fifth position and P has in the seventh position plus the L", 2, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());
				AssertEquals("Assert the guarantee number have C in fifth position and P in seventh position.", 0, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGP9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_FormattedProcedure = "4800";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that C has in the fifth position and Q has in the seventh position plus the L", 2, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and Q in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGQ9990000096").Count());
				AssertEquals("Assert the guarantee number have C in fifth position and Q in seventh position.", 0, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGQ9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_FormattedProcedure = "4900";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that C has in the fifth position and D has in the seventh position plus the L", 2, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and D in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGD9990000096").Count());
				AssertEquals("Assert the guarantee number have C in fifth position and D in seventh position.", 0, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGD9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_FormattedProcedure = "5100";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that C has in the fifth position and R has in the seventh position plus the L", 2, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and R in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGR9990000096").Count());
				AssertEquals("Assert the guarantee number have C in fifth position and R in seventh position.", 0, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGR9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_FormattedProcedure = "5300";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for the verification of the guarantee number that C has in the fifth position and S has in the seventh position plus the L", 2, declaration.Guarantees.Count);
				AssertEquals("Assert the guarantee number have A in fifth position and S in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGS9990000096").Count());
				AssertEquals("Assert the guarantee number have C in fifth position and S in seventh position.", 0, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGS9990000096").Count());
			});
		}

		public void TestImportPopulateATCGuaranteesPotentialWihtCPCStart_40AndPreferenceX15_X23_X40_X84_X86_X96_7CharacterOfGuarantee_P()
		{
			var helper = CanaryIslandSetUp();

			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			declaration.JE_OH_Importer = org1.PK;
			declaration.ZG_DestinationState = "61";

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.ZG_MethodOfPayment2 = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "4000";

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, Canarian for verification CPC = 40 and without preference there must be one garantee.", 2, declaration.Guarantees.Count);
				AssertEquals("Assert that, Canarian for verification CPC = 40 and without preference there must be one garantee with L in seventh position.(AEAT)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000096").Count());
				AssertEquals("Assert that, Canarian for verification CPC = 40 and without preference there must be one garantee with L in seventh position.(ATC)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_PrimaryPreference = "015";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X15 there must be one garantee.", 4, declaration.Guarantees.Count);
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X15 there must be one garantee with P in seventh position.(AEAT)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X15 there must be one garantee with P in seventh position.(ATC)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGP9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_PrimaryPreference = "023";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X23 there must be one garantee.", 4, declaration.Guarantees.Count);
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X23 there must be one garantee with P in seventh position.(AEAT)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X23 there must be one garantee with P in seventh position.(ATC)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGP9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_PrimaryPreference = "040";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X40 there must be one garantee.", 4, declaration.Guarantees.Count);
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X40 there must be one garantee with P in seventh position.(AEAT)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X40 there must be one garantee with P in seventh position.(ATC)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGP9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_PrimaryPreference = "084";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X84 there must be one garantee.", 4, declaration.Guarantees.Count);
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X84 there must be one garantee with P in seventh position.(AEAT)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X84 there must be one garantee with P in seventh position.(ATC)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGP9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_PrimaryPreference = "086";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X86 there must be one garantee.", 4, declaration.Guarantees.Count);
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X86 there must be one garantee with P in seventh position.(AEAT)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X86 there must be one garantee with P in seventh position.(ATC)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGP9990000096").Count());

				declaration.Guarantees.RemoveAndDeleteAll();
				invLine1.JI_PrimaryPreference = "096";
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X96 there must be one garantee.", 4, declaration.Guarantees.Count);
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X96 there must be one garantee with P in seventh position.(AEAT)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());
				AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X96 there must be one garantee with P in seventh position.(ATC)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGP9990000096").Count());
			});
		}

		public void TestImportPopulateATCGuaranteesGRNImport()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var eoriCode2 = "123456789000";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode2, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.ZG_MethodOfPayment2 = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "4000";

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

				declaration.JE_OH_Importer = org2.PK;
				declaration.Declarant.OA_OH = org2.PK;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for Canary for importer and declarant without garantee not populate anything.", 0, declaration.Guarantees.Count);

				declaration.Guarantees.RemoveAndDeleteAll();
				declaration.JE_OH_Importer = org1.PK;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for Canary for importer with garantee must have two.", 2, declaration.Guarantees.Count);

				declaration.Guarantees.RemoveAndDeleteAll();
				declaration.JE_OH_Importer = org2.PK;
				declaration.Declarant.OA_OH = org1.PK;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for Canary for declarant with garantee must have two.", 2, declaration.Guarantees.Count);
			});
		}

		public void TestImportPopulateATCGuaranteesGRNDeclarant()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var eoriCode2 = "123456789000";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode2, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org2.PK);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
			invLine1.ZG_MethodOfPayment2 = MethodOfPaymentList.Codes.R;
			invLine1.JI_FormattedProcedure = "4000";

			Factory.Save();

			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

				declaration.JE_OH_Importer = org1.PK;
				declaration.Declarant.OA_OH = org2.PK;
				populateGuaranteesHelper.PopulateGuaranteesIfApplicable(declaration);
				AssertEquals("Assert that, for Canary without the importer's guarantee, it finds the declarant's guarantee.", 2, declaration.Guarantees.Count);
				AssertEquals("Assert that, for Canary without the importer's guarantee, it finds the declarant's guarantee.(AEAT)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000096").Count());
				AssertEquals("Assert that, for Canary without the importer's guarantee, it finds the declarant's guarantee.(ATC)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());
			});
		}

		protected ESUniversalReferenceTestDataHelper CanaryIslandSetUp()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "62", "Test 62");
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "63", "Test 63");
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "64", "Test 64");
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "65", "Test 65");
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "66", "Test 66");
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "67", "Test 67");
			return helper;
		}
		protected override void SetUp()
		{
			populateGuaranteesHelper = new PopulateGuaranteesHelper();
		}
		PopulateGuaranteesHelper populateGuaranteesHelper;
	}
}
