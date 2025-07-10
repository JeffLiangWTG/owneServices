using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using Constants = Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

sealed class LineMergerTest : EU.Business.Testing.LineMergerTest
{
	public void TestImportNotPopulateGuaranteesWithEntryStatusPDA_CDA_CLP_CLR()
	{
		var eoriCode1 = "123456789000";
		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		declaration.JE_OH_Importer = org1.PK;
		var (invoice1, invLine1, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.JI_FormattedProcedure = "4000";

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertGuaranteesCount("To assert that PDA_CDA_CLP_CLR is not filled, there must first be one (PDI).", 1, EntryStatusCodes.IncompletePreDeclaration, entryHeader);

			declaration.Guarantees.RemoveAndDeleteAll();
			AssertGuaranteesCount("After the merger, you should not have guarantee with Entry Status PDA.", 0, EntryStatusCodes.PreDeclarationAccepted, entryHeader);

			AssertGuaranteesCount("After the merger, you should not have guarantee with Entry Status CDA.", 0, EntryStatusCodes.CustomsDeclarationAccepted, entryHeader);

			AssertGuaranteesCount("After the merger, you should not have guarantee with Entry Status CLP.", 0, EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, entryHeader);

			AssertGuaranteesCount("After the merger, you should not have guarantee with Entry Status CLR.", 0, EntryStatusCodes.Cleared, entryHeader);
		});
	}

	void AssertGuaranteesCount(string message, int expected, string entryStatus, CusEntryHeader entryHeader)
	{
		entryHeader.CH_EntryStatus = entryStatus;
		declaration.DoMerge();
		AssertEquals(message, expected, declaration.Guarantees.Count);
	}

	public void TestImportNotPopulateGuaranteesWithEntryInstructionT2C_T2L()
	{
		var eoriCode1 = "123456789000";
		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		declaration.JE_OH_Importer = org1.PK;
		var (invoice1, invLine1, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.JI_FormattedProcedure = "4000";

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];

			AssertGuaranteesCount("To assert that T2C_T2L is not filled, there must first be one (PDI).", 1, EntryStatusCodes.IncompletePreDeclaration, entryHeader);

			declaration.Guarantees.RemoveAndDeleteAll();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertGuaranteesCount("After the merger, you should not have guarantee with Entry Instruction T2C.", 0, EntryStatusCodes.IncompletePreDeclaration, entryHeader);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertGuaranteesCount("After the merger, you should not have guarantee with Entry Instruction T2L.", 0, EntryStatusCodes.IncompletePreDeclaration, entryHeader);
		});
	}

	public void TestImportIfExistGuarantees()
	{
		var eoriCode1 = "123456789000";
		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		declaration.JE_OH_Importer = org1.PK;
		var (invoice1, invLine1, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.JI_FormattedProcedure = "4000";

		GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, "22ESAGQ9990000096"), (entryInstruction.PK, "22ESAGL9990000095"));

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			AssertEquals("By default you must have two warranties created initially", 2, declaration.Guarantees.Count);

			var entryHeader = declaration.CustomsEntryHeaders[0];

			AssertGuaranteesCount("After the merge you should have two guarantees since you do not have to recalculate", 2, EntryStatusCodes.IncompletePreDeclaration, entryHeader);

			declaration.Guarantees.RemoveAndDeleteAll();
			AssertGuaranteesCount("Once the old ones have been deleted, you have to calculate a single guarantee.", 1, EntryStatusCodes.IncompletePreDeclaration, entryHeader);
		});
	}

	public void TestImportNotPopulateGuaranteesWihtMethodOfPaymentNotEqualR()
	{
		var eoriCode1 = "123456789000";
		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		declaration.JE_OH_Importer = org1.PK;
		var (invoice1, invLine1, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.JI_FormattedProcedure = "4000";

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertGuaranteesCount("Assert with Method Of Payment equal to R.", 1, EntryStatusCodes.IncompletePreDeclaration, entryHeader);

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.A;
			AssertGuaranteesCount("Assert with Method Of Payment not equal to R.", 0, EntryStatusCodes.IncompletePreDeclaration, entryHeader);
		});
	}

	public void TestImportPopulateAEATClearanceGuarantees()
	{
		var eoriCode1 = "123456789000";
		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		declaration.JE_OH_Importer = org1.PK;
		var (invoice1, invLine1, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.JI_FormattedProcedure = "4000";

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertGuaranteesCount("For the verification of the guarantee number that A has in the fifth position and L has in the seventh position there is only one", 1, EntryStatusCodes.IncompletePreDeclaration, entryHeader);
			AssertEquals("Assert the guarantee number have A in fifth position and L in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000096").Count());
		});
	}

	public void TestImportPopulateAEATGuaranteesPotentialWihtCPCStart_44_48_49_51_53_And7CharacterOfGuarantee_P_Q_L_R_S()
	{
		var eoriCode1 = "123456789000";
		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		declaration.JE_OH_Importer = org1.PK;
		var (invoice1, invLine1, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			declaration.Guarantees.RemoveAndDeleteAll();
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			invLine1.JI_FormattedProcedure = "4400";
			declaration.DoMerge();
			AssertEquals("Assert that, for the verification of the guarantee number that A has in the fifth position and P has in the seventh position plus the L", 2, declaration.Guarantees.Count);
			AssertEquals("Assert the guarantee number have A in fifth position and P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_FormattedProcedure = "4800";
			declaration.DoMerge();
			AssertEquals("Assert that, for the verification of the guarantee number that A has in the fifth position and Q has in the seventh position plus the L", 2, declaration.Guarantees.Count);
			AssertEquals("Assert the guarantee number have A in fifth position and Q in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGQ9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_FormattedProcedure = "4900";
			declaration.DoMerge();
			AssertEquals("Assert that, for the verification of the guarantee number that A has in the fifth position and D has in the seventh position plus the L", 2, declaration.Guarantees.Count);
			AssertEquals("Assert the guarantee number have A in fifth position and D in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGD9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_FormattedProcedure = "5100";
			declaration.DoMerge();
			AssertEquals("Assert that, for the verification of the guarantee number that A has in the fifth position and R has in the seventh position plus the L", 2, declaration.Guarantees.Count);
			AssertEquals("Assert the guarantee number have A in fifth position and R in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGR9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_FormattedProcedure = "5300";
			declaration.DoMerge();
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

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		declaration.JE_OH_Importer = org1.PK;
		var (invoice1, invLine1, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.JI_FormattedProcedure = "4000";

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			declaration.DoMerge();
			AssertEquals("Assert that, for verification CPC = 40 and without preference there must be one garantee.", 1, declaration.Guarantees.Count);
			AssertEquals("Assert that, for verification CPC = 40 and without preference there must be one garantee with L in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_PrimaryPreference = "015";
			declaration.DoMerge();
			AssertEquals("Assert that, for verification CPC = 40 and preference = X15 there must be one garantee.", 2, declaration.Guarantees.Count);
			AssertEquals("Assert that, for verification CPC = 40 and preference = X15 there must be one garantee with P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_PrimaryPreference = "023";
			declaration.DoMerge();
			AssertEquals("Assert that, for verification CPC = 40 and preference = X23 there must be one garantee.", 2, declaration.Guarantees.Count);
			AssertEquals("Assert that, for verification CPC = 40 and preference = X23 there must be one garantee with P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_PrimaryPreference = "040";
			declaration.DoMerge();
			AssertEquals("Assert that, for verification CPC = 40 and preference = X40 there must be one garantee.", 2, declaration.Guarantees.Count);
			AssertEquals("Assert that, for verification CPC = 40 and preference = X40 there must be one garantee with P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_PrimaryPreference = "084";
			declaration.DoMerge();
			AssertEquals("Assert that, for verification CPC = 40 and preference = X84 there must be one garantee.", 2, declaration.Guarantees.Count);
			AssertEquals("Assert that, for verification CPC = 40 and preference = X84 there must be one garantee with P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_PrimaryPreference = "086";
			declaration.DoMerge();
			AssertEquals("Assert that, for verification CPC = 40 and preference = X86 there must be one garantee.", 2, declaration.Guarantees.Count);
			AssertEquals("Assert that, for verification CPC = 40 and preference = X86 there must be one garantee with P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_PrimaryPreference = "096";
			declaration.DoMerge();
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

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		var (invoice1, invLine1, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.JI_FormattedProcedure = "4000";

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			declaration.JE_OH_Importer = org2.PK;
			declaration.Declarant.OA_OH = org2.PK;
			declaration.DoMerge();
			AssertEquals("Assert that, for importer and declarant without garantee not populate anything.", 0, declaration.Guarantees.Count);

			declaration.Guarantees.RemoveAndDeleteAll();
			declaration.JE_OH_Importer = org1.PK;
			declaration.DoMerge();
			AssertEquals("Assert that, for importer with garantee must have one.", 1, declaration.Guarantees.Count);

			declaration.Guarantees.RemoveAndDeleteAll();
			declaration.JE_OH_Importer = org2.PK;
			declaration.Declarant.OA_OH = org1.PK;
			declaration.DoMerge();
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

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		var (invoice1, invLine1, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.JI_FormattedProcedure = "4000";

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			declaration.JE_OH_Importer = org1.PK;
			declaration.Declarant.OA_OH = org2.PK;
			declaration.DoMerge();
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

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		var (invoice1, invLine1, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.JI_FormattedProcedure = "5100";

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			declaration.Guarantees.RemoveAndDeleteAll();
			declaration.JE_OH_Importer = org1.PK;
			declaration.Declarant.OA_OH = org2.PK;
			declaration.DoMerge();

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

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

		var (invoice1, invLine1, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.JI_FormattedProcedure = "5100";

		var (invoice2, invLine2, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.B);

		invoice2.JZ_RX_NKInvoice_Currency = "EUR";
		invLine2.JI_Procedure = "B";
		invLine2.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine2.JI_FormattedProcedure = "4800";

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			declaration.Guarantees.RemoveAndDeleteAll();
			declaration.JE_OH_Importer = org1.PK;
			declaration.Declarant.OA_OH = org2.PK;
			declaration.DoMerge();

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

	public void TestImportPopulateATCGuaranteesWihtIslandCanary_61_62_63_64_65_66_67()
	{
		var helper = SetUpCanaryIslandsCusCodeLists();

		var eoriCode1 = "123456789000";
		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		declaration.JE_OH_Importer = org1.PK;

		var (invoice1, invLine1, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.ZG_MethodOfPayment2 = MethodOfPaymentList.Codes.R;
		invLine1.JI_FormattedProcedure = "4800";

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			declaration.Guarantees.RemoveAndDeleteAll();
			declaration.DoMerge();
			AssertEquals("Without country of destination the fifth guarantee position is A.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			declaration.ZG_DestinationState = "61";
			declaration.DoMerge();
			AssertEquals("With country of destination = 61 the fifth guarantee position is C.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			declaration.ZG_DestinationState = "62";
			declaration.DoMerge();
			AssertEquals("With country of destination = 62 the fifth guarantee position is C.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			declaration.ZG_DestinationState = "63";
			declaration.DoMerge();
			AssertEquals("With country of destination = 63 the fifth guarantee position is C.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			declaration.ZG_DestinationState = "64";
			declaration.DoMerge();
			AssertEquals("With country of destination = 64 the fifth guarantee position is C.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			declaration.ZG_DestinationState = "65";
			declaration.DoMerge();
			AssertEquals("With country of destination = 65 the fifth guarantee position is C.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			declaration.ZG_DestinationState = "66";
			declaration.DoMerge();
			AssertEquals("With country of destination = 66 the fifth guarantee position is C.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			declaration.ZG_DestinationState = "67";
			declaration.DoMerge();
			AssertEquals("With country of destination = 67 the fifth guarantee position is C.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());
		});
	}

	public void TestImportPopulateATCClearanceGuarantees()
	{
		var helper = SetUpCanaryIslandsCusCodeLists();

		var eoriCode1 = "123456789000";
		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		declaration.JE_OH_Importer = org1.PK;
		declaration.ZG_DestinationState = "61";

		var (invoice1, invLine1, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.ZG_MethodOfPayment2 = MethodOfPaymentList.Codes.R;
		invLine1.JI_FormattedProcedure = "4000";

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			declaration.Guarantees.RemoveAndDeleteAll();
			declaration.DoMerge();
			AssertEquals("Assert that, for the verification of the guarantee number that C has in the fifth position and L has in the seventh position there is only two", 2, declaration.Guarantees.Count);
			AssertEquals("Assert the guarantee number have C in fifth position and L in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());
			AssertEquals("Assert the guarantee number have A in fifth position and L in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000096").Count());
		});
	}

	public void TestImportPopulateATCGuaranteesPotentialWihtCPCStart_44_48_49_51_53_And7CharacterOfGuarantee_P_Q_L_R_S()
	{
		var helper = SetUpCanaryIslandsCusCodeLists();

		var eoriCode1 = "123456789000";
		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		declaration.JE_OH_Importer = org1.PK;
		declaration.ZG_DestinationState = "61";

		var (invoice1, invLine1, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.ZG_MethodOfPayment2 = MethodOfPaymentList.Codes.R;

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			declaration.Guarantees.RemoveAndDeleteAll();
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			invLine1.JI_FormattedProcedure = "4400";
			declaration.DoMerge();
			AssertEquals("Assert that, for the verification of the guarantee number that C or A has in the fifth position and P has in the seventh position plus the L", 4, declaration.Guarantees.Count);
			AssertEquals("Assert the guarantee number have C in fifth position and P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGP9990000096").Count());
			AssertEquals("Assert the guarantee number have A in fifth position and P in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_FormattedProcedure = "4800";
			declaration.DoMerge();
			AssertEquals("Assert that, for the verification of the guarantee number that C or A has in the fifth position and Q has in the seventh position plus the L", 4, declaration.Guarantees.Count);
			AssertEquals("Assert the guarantee number have C in fifth position and Q in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGQ9990000096").Count());
			AssertEquals("Assert the guarantee number have A in fifth position and Q in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGQ9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_FormattedProcedure = "4900";
			declaration.DoMerge();
			AssertEquals("Assert that, for the verification of the guarantee number that C or A has in the fifth position and D has in the seventh position plus the L", 4, declaration.Guarantees.Count);
			AssertEquals("Assert the guarantee number have C in fifth position and D in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGD9990000096").Count());
			AssertEquals("Assert the guarantee number have A in fifth position and D in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGD9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_FormattedProcedure = "5100";
			declaration.DoMerge();
			AssertEquals("Assert that, for the verification of the guarantee number that C or A has in the fifth position and R has in the seventh position plus the L", 4, declaration.Guarantees.Count);
			AssertEquals("Assert the guarantee number have C in fifth position and R in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGR9990000096").Count());
			AssertEquals("Assert the guarantee number have A in fifth position and R in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGR9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_FormattedProcedure = "5300";
			declaration.DoMerge();
			AssertEquals("Assert that, for the verification of the guarantee number that C or A has in the fifth position and S has in the seventh position plus the L", 4, declaration.Guarantees.Count);
			AssertEquals("Assert the guarantee number have C in fifth position and S in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGS9990000096").Count());
			AssertEquals("Assert the guarantee number have A in fifth position and S in seventh position.", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGS9990000096").Count());
		});
	}

	public void TestImportPopulateATCGuaranteesPotentialWihtCPCStart_40AndPreferenceX15_X23_X40_X84_X86_X96_7CharacterOfGuarantee_P()
	{
		var helper = SetUpCanaryIslandsCusCodeLists();

		var eoriCode1 = "123456789000";
		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		GuaranteesTestHelper.CreateGuaranteesHeader(Factory, org1.PK);

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		declaration.JE_OH_Importer = org1.PK;
		declaration.ZG_DestinationState = "61";

		var (invoice1, invLine1, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.ZG_MethodOfPayment2 = MethodOfPaymentList.Codes.R;
		invLine1.JI_FormattedProcedure = "4000";

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			declaration.DoMerge();
			AssertEquals("Assert that, Canarian for verification CPC = 40 and without preference there must be one garantee.", 2, declaration.Guarantees.Count);
			AssertEquals("Assert that, Canarian for verification CPC = 40 and without preference there must be one garantee with L in seventh position.(AEAT)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGL9990000096").Count());
			AssertEquals("Assert that, Canarian for verification CPC = 40 and without preference there must be one garantee with L in seventh position.(ATC)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGL9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_PrimaryPreference = "015";
			declaration.DoMerge();
			AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X15 there must be one garantee.", 4, declaration.Guarantees.Count);
			AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X15 there must be one garantee with P in seventh position.(AEAT)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());
			AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X15 there must be one garantee with P in seventh position.(ATC)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGP9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_PrimaryPreference = "023";
			declaration.DoMerge();
			AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X23 there must be one garantee.", 4, declaration.Guarantees.Count);
			AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X23 there must be one garantee with P in seventh position.(AEAT)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());
			AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X23 there must be one garantee with P in seventh position.(ATC)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGP9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_PrimaryPreference = "040";
			declaration.DoMerge();
			AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X40 there must be one garantee.", 4, declaration.Guarantees.Count);
			AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X40 there must be one garantee with P in seventh position.(AEAT)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());
			AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X40 there must be one garantee with P in seventh position.(ATC)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGP9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_PrimaryPreference = "084";
			declaration.DoMerge();
			AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X84 there must be one garantee.", 4, declaration.Guarantees.Count);
			AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X84 there must be one garantee with P in seventh position.(AEAT)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());
			AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X84 there must be one garantee with P in seventh position.(ATC)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGP9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_PrimaryPreference = "086";
			declaration.DoMerge();
			AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X86 there must be one garantee.", 4, declaration.Guarantees.Count);
			AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X86 there must be one garantee with P in seventh position.(AEAT)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESAGP9990000096").Count());
			AssertEquals("Assert that, for Canary for verification CPC = 40 and preference = X86 there must be one garantee with P in seventh position.(ATC)", 1, declaration.Guarantees.Find(x => x.PW_BondNumber == "22ESCGP9990000096").Count());

			declaration.Guarantees.RemoveAndDeleteAll();
			invLine1.JI_PrimaryPreference = "096";
			declaration.DoMerge();
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

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		SetUpCanaryIslandsCusCodeLists(CanaryIslandsForTest.First());
		declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];

		var (invoice1, invLine1, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.ZG_MethodOfPayment2 = MethodOfPaymentList.Codes.R;
		invLine1.JI_FormattedProcedure = "4000";

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			declaration.JE_OH_Importer = org2.PK;
			declaration.Declarant.OA_OH = org2.PK;
			declaration.DoMerge();
			AssertEquals("Assert that, for Canary for importer and declarant without garantee not populate anything.", 0, declaration.Guarantees.Count);

			declaration.Guarantees.RemoveAndDeleteAll();
			declaration.JE_OH_Importer = org1.PK;
			declaration.DoMerge();
			AssertEquals("Assert that, for Canary for importer with garantee must have two.", 2, declaration.Guarantees.Count);

			declaration.Guarantees.RemoveAndDeleteAll();
			declaration.JE_OH_Importer = org2.PK;
			declaration.Declarant.OA_OH = org1.PK;
			declaration.DoMerge();
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

		declaration.JE_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		declaration.ZG_DestinationState = "61";

		var (invoice1, invLine1, _) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.ZG_MethodOfPayment = MethodOfPaymentList.Codes.R;
		invLine1.JI_FormattedProcedure = "4000";

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			declaration.JE_OH_Importer = org1.PK;
			declaration.Declarant.OA_OH = org2.PK;
			declaration.DoMerge();
			AssertEquals("Assert that, for Canary without the importer's guarantee, it finds the declarant's guarantee.", 1, declaration.Guarantees.Count);
		});
	}

	public void TestCreateImport7002CLSupportingDocument()
	{
		var (invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4, entryHeader) = SetUpDataForImport7002And7003Documents();

		CombineAssertions(() =>
		{
			AssertEquals("VAT_Additions is 0 for invoiceLine1", 0m, invoiceLine1.JI_VAT_Additions);
			AssertEquals("VAT_Additions is 0 for invoiceLine2", 0m, invoiceLine2.JI_VAT_Additions);
			AssertEquals("VAT_Additions is 0 for invoiceLine3", 0m, invoiceLine3.JI_VAT_Additions);
			AssertEquals("VAT_Additions is 0 for invoiceLine4", 0m, invoiceLine4.JI_VAT_Additions);

			AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
			AssertEquals("First entry has no supporting document for 7002 when vat additions is 0", false, firstEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002"));

			var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
			AssertEquals("Second entry has no supporting document for 7002 when vat additions is 0", false, secondEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002"));

			var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");
			AssertEquals("Third entry has no supporting document for 7002 when vat additions is 0", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002"));

			SetUpChargesForImport7002Document(invoiceLine1, invoiceLine2, invoiceLine3);

			declaration.ResumeApportionment();

			AssertEquals("VAT_Additions is 30 for invoiceLine1", 30m, invoiceLine1.JI_VAT_Additions);
			AssertEquals("VAT_Additions is 40 for invoiceLine2", 40m, invoiceLine2.JI_VAT_Additions);
			AssertEquals("VAT_Additions is 50 for invoiceLine3", 50m, invoiceLine3.JI_VAT_Additions);
			AssertEquals("VAT_Additions is 0 for invoiceLine4", 0m, invoiceLine4.JI_VAT_Additions);

			firstEntry.AddEntryLineDocument<SupportingDocument>("7002", "10,00", subType: "LIQ", status: "ACC");

			Factory.Save();
			declaration.DoMerge();
			Factory.Save();

			AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			AssertEquals("First entry has 2 supporting documents for 7002 (one accepted one new)", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7002"));
			AssertEntryLineTypeSupDoc("First entry", firstEntry, "7002", "30,00", dataModel: Core.Constants.CountryCodes.Spain);

			AssertEntryLineTypeSupDoc("Second entry", secondEntry, "7002", "90,00", dataModel: Core.Constants.CountryCodes.Spain);

			AssertEquals("Third entry has no supporting document for 7002", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002"));
		});
	}

	public void TestCreateImport7002CLSupportingDocumentH1()
	{
		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
		{
			var (invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4, entryHeader) = SetUpDataForImport7002And7003Documents(true);

			CombineAssertions(() =>
			{
				SetUpChargesForImport7002Document(invoiceLine1, invoiceLine2, invoiceLine3);

				declaration.ResumeApportionment();

				AssertEquals("VAT_Additions is 30 for invoiceLine1", 30m, invoiceLine1.JI_VAT_Additions);
				AssertEquals("VAT_Additions is 40 for invoiceLine2", 40m, invoiceLine2.JI_VAT_Additions);
				AssertEquals("VAT_Additions is 50 for invoiceLine3", 50m, invoiceLine3.JI_VAT_Additions);
				AssertEquals("VAT_Additions is 0 for invoiceLine4", 0m, invoiceLine4.JI_VAT_Additions);

				var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
				var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
				var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");

				firstEntry.AddEntryLineDocument<SupportingDocument>("7002", "10,00", subType: "LIQ", status: "ACC", amount: 10.0m);

				Factory.Save();
				declaration.DoMerge();
				Factory.Save();

				AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);

				AssertEquals("First entry has 2 supporting documents for 7002 (one accepted one new)", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7002"));
				AssertEntryLineTypeSupDoc("First entry", firstEntry, "7002", "30,00", amount: 30.0m, dataModel: Core.Constants.CountryCodes.Spain);

				AssertEntryLineTypeSupDoc("Second entry", secondEntry, "7002", "90,00", amount: 90.0m, dataModel: Core.Constants.CountryCodes.Spain);

				AssertEquals("Third entry has no supporting document for 7002", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002"));
			});
		}
	}

	public void TestUpdateImport7002CLSupportingDocument()
	{
		var (invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4, entryHeader) = SetUpDataForImport7002And7003Documents();

		CombineAssertions(() =>
		{
			AssertEquals("VAT_Additions is 0 for invoiceLine1", 0m, invoiceLine1.JI_VAT_Additions);
			AssertEquals("VAT_Additions is 0 for invoiceLine2", 0m, invoiceLine2.JI_VAT_Additions);
			AssertEquals("VAT_Additions is 0 for invoiceLine3", 0m, invoiceLine3.JI_VAT_Additions);
			AssertEquals("VAT_Additions is 0 for invoiceLine4", 0m, invoiceLine4.JI_VAT_Additions);

			AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
			AssertEquals("First entry has no supporting document for 7002 when vat additions is 0", false, firstEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002"));

			var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
			AssertEquals("Second entry has no supporting document for 7002 when vat additions is 0", false, secondEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002"));

			var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");
			AssertEquals("Third entry has no supporting document for 7002 when vat additions is 0", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002"));

			SetUpChargesForImport7002Document(invoiceLine1, invoiceLine2, invoiceLine3);

			declaration.ResumeApportionment();

			AssertEquals("VAT_Additions is 30 for invoiceLine1", 30m, invoiceLine1.JI_VAT_Additions);
			AssertEquals("VAT_Additions is 40 for invoiceLine2", 40m, invoiceLine2.JI_VAT_Additions);
			AssertEquals("VAT_Additions is 50 for invoiceLine3", 50m, invoiceLine3.JI_VAT_Additions);
			AssertEquals(0m, invoiceLine4.JI_VAT_Additions);

			firstEntry.AddEntryLineDocument<SupportingDocument>("7002", "10,00", subType: "LIQ", status: "ACC");
			firstEntry.AddEntryLineDocument<SupportingDocument>("7002", "10,00", subType: "LIQ");
			AssertEquals("First entry has 2 supporting documents for 7002 (one accepted one not) before second merge", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7002"));
			AssertEntryLineTypeSupDoc("First entry before second merge", firstEntry, "7002", "10,00");

			secondEntry.AddEntryLineDocument<SupportingDocument>("7002", "20,00", subType: "LIQ");
			AssertEntryLineTypeSupDoc("Second entry before second merge", secondEntry, "7002", "20,00");

			Factory.Save();
			declaration.DoMerge();
			Factory.Save();

			AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			AssertEquals("First entry has 2 supporting documents for 7002 (one accepted one updated) after second merge", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7002"));
			AssertEntryLineTypeSupDoc("First entry after second merge", firstEntry, "7002", "30,00", dataModel: Core.Constants.CountryCodes.Spain);

			AssertEntryLineTypeSupDoc("Second entry after second merge", secondEntry, "7002", "90,00", dataModel: Core.Constants.CountryCodes.Spain);

			AssertEquals("Third entry has no supporting document for 7002", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002"));
		});
	}

	public void TestUpdateImport7002CLSupportingDocumentH1()
	{
		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
		{
			var (invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4, entryHeader) = SetUpDataForImport7002And7003Documents(true);

			CombineAssertions(() =>
			{
				SetUpChargesForImport7002Document(invoiceLine1, invoiceLine2, invoiceLine3);

				declaration.ResumeApportionment();

				AssertEquals("VAT_Additions is 30 for invoiceLine1", 30m, invoiceLine1.JI_VAT_Additions);
				AssertEquals("VAT_Additions is 40 for invoiceLine2", 40m, invoiceLine2.JI_VAT_Additions);
				AssertEquals("VAT_Additions is 50 for invoiceLine3", 50m, invoiceLine3.JI_VAT_Additions);
				AssertEquals(0m, invoiceLine4.JI_VAT_Additions);

				var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
				var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
				var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");

				firstEntry.AddEntryLineDocument<SupportingDocument>("7002", "10,00", subType: "LIQ", status: "ACC", amount: 10.0m, currency: "EUR");
				firstEntry.AddEntryLineDocument<SupportingDocument>("7002", "10,00", subType: "LIQ", amount: 10.0m, currency: "EUR");
				AssertEquals("First entry has 2 supporting documents for 7002 (one accepted one not) before second merge", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7002"));
				AssertEntryLineTypeSupDoc("First entry before second merge", firstEntry, "7002", "10,00", amount: 10.0m);

				secondEntry.AddEntryLineDocument<SupportingDocument>("7002", "20,00", subType: "LIQ", amount: 20.0m, currency: "EUR");
				AssertEntryLineTypeSupDoc("Second entry before second merge", secondEntry, "7002", "20,00", amount: 20.0m);

				Factory.Save();
				declaration.DoMerge();
				Factory.Save();

				AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);

				AssertEquals("First entry has 2 supporting documents for 7002 (one accepted one updated) after second merge", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7002"));
				AssertEntryLineTypeSupDoc("First entry after second merge", firstEntry, "7002", "30,00", amount: 30.0m, dataModel: Core.Constants.CountryCodes.Spain);

				AssertEntryLineTypeSupDoc("Second entry after second merge", secondEntry, "7002", "90,00", amount: 90.0m, dataModel: Core.Constants.CountryCodes.Spain);

				AssertEquals("Third entry has no supporting document for 7002", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002"));
			});
		}
	}

	public void TestRemoveImport7002CLSupportingDocument()
	{
		declaration.SupportingDocuments.Add(Factory.CreateSupportingDocument("7002", "declaration"));
		declaration.SupportingDocuments.Add(Factory.CreateSupportingDocument("AAA", "declaration"));

		var (invoice, invoiceLine1, entryInstruction1) = SetUpInvHeaderInvLineAndEntryInstruction(ZString.Empty);
		entryInstruction1.SupportingDocuments.Add(Factory.CreateSupportingDocument("7002", "entryInstruction1"));
		entryInstruction1.SupportingDocuments.Add(Factory.CreateSupportingDocument("BBB", "entryInstruction1"));

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.SupportingDocuments.Add(Factory.CreateSupportingDocument("7002", "entryInstruction2"));

		var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction3.SupportingDocuments.Add(Factory.CreateSupportingDocument("CCC", "entryInstruction3"));

		invoice.JZ_IncoTerm = "DDP";
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		invoice.JZ_InvoiceAmount = 1000m;
		invoice.SupportingDocuments.Add(Factory.CreateSupportingDocument("7002", "invoice"));
		invoice.SupportingDocuments.Add(Factory.CreateSupportingDocument("DDD", "invoice"));

		invoiceLine1.JI_Tariff = "2208905400";
		invoiceLine1.JI_LinePrice = 100m;
		invoiceLine1.SupportingDocuments.Add(Factory.CreateSupportingDocument("7002", "invoiceLine1"));
		invoiceLine1.SupportingDocuments.Add(Factory.CreateSupportingDocument("EEE", "invoiceLine1"));

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_Tariff = "2208905401";
		invoiceLine2.JI_LinePrice = 100m;
		invoiceLine2.SupportingDocuments.Add(Factory.CreateSupportingDocument("7002", "invoiceLine2"));

		var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstruction1.PK;
		invoiceLine3.JI_Tariff = "2208905402";
		invoiceLine3.JI_LinePrice = 100m;
		invoiceLine3.SupportingDocuments.Add(Factory.CreateSupportingDocument("FFF", "invoiceLine4"));

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Declaration has 2 supporting documents", 2, declaration.SupportingDocuments.Count);
			AssertEquals("EntryInstruction1 has 2 supporting documents", 2, entryInstruction1.SupportingDocuments.Count);
			AssertEquals("EntryInstruction2 has 1 supporting document", 1, entryInstruction2.SupportingDocuments.Count);
			AssertEquals("EntryInstruction3 has 1 supporting document", 1, entryInstruction3.SupportingDocuments.Count);
			AssertEquals("Invoice has 2 supporting documents", 2, invoice.SupportingDocuments.Count);
			AssertEquals("InvoiceLine1 has 2 supporting documents", 2, invoiceLine1.SupportingDocuments.Count);
			AssertEquals("InvoiceLine2 has 1 supporting document", 1, invoiceLine2.SupportingDocuments.Count);
			AssertEquals("InvoiceLine3 has 1 supporting document", 1, invoiceLine3.SupportingDocuments.Count);

			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			AssertEquals("VAT_Additions is 0 for invoiceLine1", 0m, invoiceLine1.JI_VAT_Additions);
			AssertEquals("VAT_Additions is 0 for invoiceLine2", 0m, invoiceLine2.JI_VAT_Additions);
			AssertEquals("VAT_Additions is 0 for invoiceLine3", 0m, invoiceLine3.JI_VAT_Additions);

			AssertEquals("Supporting document 7002 was removed from declaration, now it only has 1 sup doc", 1, declaration.SupportingDocuments.Count);
			AssertEquals("Supporting document 7002 was removed from entryInstruction1, now it only has 1 sup doc", 1, entryInstruction1.SupportingDocuments.Count);
			AssertEquals("Supporting document 7002 was removed from entryInstruction2, now it has 0 sup doc", 0, entryInstruction2.SupportingDocuments.Count);
			AssertEquals("Supporting document 7002 was not declared for entryInstruction3, it still has 1 sup doc", 1, entryInstruction3.SupportingDocuments.Count);
			AssertEquals("Supporting document 7002 was removed from invoice, now it only has 1 sup doc", 1, invoice.SupportingDocuments.Count);
			AssertEquals("Supporting document 7002 was removed from invoiceLine1, now it only has 1 sup doc", 1, invoiceLine1.SupportingDocuments.Count);
			AssertEquals("Supporting document 7002 was removed from invoiceLine2, now it has 0 sup doc", 0, invoiceLine2.SupportingDocuments.Count);
			AssertEquals("Supporting document 7002 was not declared for invoiceLine3, it still has 1 sup doc", 1, invoiceLine3.SupportingDocuments.Count);

			AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
			AssertEquals("First entry has no supporting document for 7002 when vat additions is 0", false, firstEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002"));

			var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
			AssertEquals("Second entry has no supporting document for 7002 when vat additions is 0", false, secondEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002"));

			var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");
			AssertEquals("Third entry has no supporting document for 7002 when vat additions is 0", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002"));

			firstEntry.AddEntryLineDocument<SupportingDocument>("7002", "10,00", subType: "LIQ", status: "ACC");
			firstEntry.AddEntryLineDocument<SupportingDocument>("7002", "10,00", subType: "LIQ");
			AssertEquals("First entry has 2 supporting document for 7002 (one accepted one not) before second merge", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7002"));
			AssertEntryLineTypeSupDoc("First entry before second merge", firstEntry, "7002", "10,00");

			secondEntry.AddEntryLineDocument<SupportingDocument>("7002", "90,00", subType: "LIQ");
			AssertEntryLineTypeSupDoc("Second entry before second merge", secondEntry, "7002", "90,00");

			thirdEntry.AddEntryLineDocument<SupportingDocument>("7002", "50,00", subType: "LIQ");
			AssertEntryLineTypeSupDoc("Third entry before second merge", thirdEntry, "7002", "50,00");

			Factory.Save();
			declaration.DoMerge();
			Factory.Save();

			AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			AssertEquals("First entry has 1 supporting document for 7002 (accepted) after second merge", 1, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7002"));
			AssertEquals("First entry has no supporting document for 7002 without accepted status", false, firstEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002" && x.CSI_Status != "ACC"));

			AssertEquals("Second entry has no supporting document for 7002", false, secondEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002"));

			AssertEquals("Third entry has no supporting document for 7002", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7002"));
		});
	}

	public void TestCreateImport7003CLSupportingDocument()
	{
		SetREAData();

		var (invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4, entryHeader) = SetUpDataForImport7002And7003Documents();

		CombineAssertions(() =>
		{
			AssertEquals("REA charge amount is 0 for invoiceLine1", 0m, invoiceLine1.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 0 for invoiceLine2", 0m, invoiceLine2.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 0 for invoiceLine3", 0m, invoiceLine3.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 0 for invoiceLine4", 0m, invoiceLine4.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));

			AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
			AssertEquals("First entry has no supporting document for 7003 when there are no REA charges in the invoice lines", false, firstEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003"));

			var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
			AssertEquals("Second entry has no supporting document for 7003 when there are no REA charges in the invoice lines", false, secondEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003"));

			var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");
			AssertEquals("Third entry has no supporting document for 7003 when there are no REA charges in the invoice lines", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003"));

			SetUpREAChargesForImport7003Document(invoiceLine1, invoiceLine2, invoiceLine3);

			AssertEquals("REA charge amount is 0 for invoiceLine1 before merge", 0m, invoiceLine1.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 0 for invoiceLine2 before merge", 0m, invoiceLine2.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 0 for invoiceLine3 before merge", 0m, invoiceLine3.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 0 for invoiceLine4", 0m, invoiceLine4.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));

			firstEntry.AddEntryLineDocument<SupportingDocument>("7003", "10,00", subType: "LIQ", status: "ACC");

			Factory.Save();
			declaration.DoMerge();
			Factory.Save();

			AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);
			AssertEquals("REA charge amount is 1721,69 for invoiceLine1 afer merge", 1721.69m, invoiceLine1.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 5000,10 for invoiceLine2 afer merge", 5000.10m, invoiceLine2.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 10000,10 for invoiceLine3 afer merge", 10000.10m, invoiceLine3.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));

			AssertEquals("First entry has 2 supporting documents for 7003 (one accepted one new)", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7003"));
			AssertEntryLineTypeSupDoc("First entry", firstEntry, "7003", "1721,69", dataModel: Core.Constants.CountryCodes.Spain);

			AssertEntryLineTypeSupDoc("Second entry", secondEntry, "7003", "15000,20", dataModel: Core.Constants.CountryCodes.Spain);

			AssertEquals("Third entry has no supporting document for 7003", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003"));
		});
	}

	public void TestCreateImport7003CLSupportingDocumentH1()
	{
		SetREAData();

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
		{
			var (invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4, entryHeader) = SetUpDataForImport7002And7003Documents(true);

			CombineAssertions(() =>
			{
				var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
				var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
				var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");

				SetUpREAChargesForImport7003Document(invoiceLine1, invoiceLine2, invoiceLine3);

				firstEntry.AddEntryLineDocument<SupportingDocument>("7003", "10,00", subType: "LIQ", status: "ACC", amount: 10.0m);

				Factory.Save();
				declaration.DoMerge();
				Factory.Save();

				AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);
				AssertEquals("REA charge amount is 1721,69 for invoiceLine1 afer merge", 1721.69m, invoiceLine1.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
				AssertEquals("REA charge amount is 5000,10 for invoiceLine2 afer merge", 5000.10m, invoiceLine2.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
				AssertEquals("REA charge amount is 10000,10 for invoiceLine3 afer merge", 10000.10m, invoiceLine3.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));

				AssertEquals("First entry has 2 supporting documents for 7003 (one accepted one new)", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7003"));
				AssertEntryLineTypeSupDoc("First entry", firstEntry, "7003", "1721,69", amount: 1721.69m, dataModel: Core.Constants.CountryCodes.Spain);

				AssertEntryLineTypeSupDoc("Second entry", secondEntry, "7003", "15000,20", amount: 15000.20m, dataModel: Core.Constants.CountryCodes.Spain);

				AssertEquals("Third entry has no supporting document for 7003", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003"));
			});
		}
	}

	public void TestUpdateImport7003CLSupportingDocument()
	{
		SetREAData();

		var (invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4, entryHeader) = SetUpDataForImport7002And7003Documents();

		CombineAssertions(() =>
		{
			AssertEquals("REA charge amount is 0 for invoiceLine1", 0m, invoiceLine1.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 0 for invoiceLine2", 0m, invoiceLine2.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 0 for invoiceLine3", 0m, invoiceLine3.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 0 for invoiceLine4", 0m, invoiceLine4.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));

			AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
			AssertEquals("First entry has no supporting document for 7003 when there are no REA charges in the invoice lines", false, firstEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003"));

			var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
			AssertEquals("Second entry has no supporting document for 7003 when there are no REA charges in the invoice lines", false, secondEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003"));

			var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");
			AssertEquals("Third entry has no supporting document for 7003 when there are no REA charges in the invoice lines", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003"));

			SetUpREAChargesForImport7003Document(invoiceLine1, invoiceLine2, invoiceLine3);

			AssertEquals("REA charge amount is 0 for invoiceLine1 before merge", 0m, invoiceLine1.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 0 for invoiceLine2 before merge", 0m, invoiceLine2.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 0 for invoiceLine3 before merge", 0m, invoiceLine3.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 0 for invoiceLine4", 0m, invoiceLine4.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));

			firstEntry.AddEntryLineDocument<SupportingDocument>("7003", "10,00", subType: "LIQ", status: "ACC");
			firstEntry.AddEntryLineDocument<SupportingDocument>("7003", "10,00", subType: "LIQ");
			AssertEquals("First entry has 2 supporting documents for 7003 (one accepted one not) before second merge", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7003"));
			AssertEntryLineTypeSupDoc("First entry before second merge", firstEntry, "7003", "10,00");

			secondEntry.AddEntryLineDocument<SupportingDocument>("7003", "90,00", subType: "LIQ");
			AssertEntryLineTypeSupDoc("Second entry before second merge", secondEntry, "7003", "90,00");

			Factory.Save();
			declaration.DoMerge();
			Factory.Save();

			AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);
			AssertEquals("REA charge amount is 1721,69 for invoiceLine1 afer merge", 1721.69m, invoiceLine1.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 5000,10 for invoiceLine2 afer merge", 5000.10m, invoiceLine2.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 10000,10 for invoiceLine3 afer merge", 10000.10m, invoiceLine3.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));

			AssertEquals("First entry has 2 supporting documents for 7003 (one accepted one updated) after second merge", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7003"));
			AssertEntryLineTypeSupDoc("First entry after second merge", firstEntry, "7003", "1721,69", dataModel: Core.Constants.CountryCodes.Spain);

			AssertEntryLineTypeSupDoc("Second entry after second merge", secondEntry, "7003", "15000,20", dataModel: Core.Constants.CountryCodes.Spain);

			AssertEquals("Third entry has no supporting document for 7003", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003"));
		});
	}

	public void TestUpdateImport7003CLSupportingDocumentH1()
	{
		SetREAData();

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
		{
			var (invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4, entryHeader) = SetUpDataForImport7002And7003Documents(true);

			CombineAssertions(() =>
			{
				var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
				var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
				var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");

				SetUpREAChargesForImport7003Document(invoiceLine1, invoiceLine2, invoiceLine3);

				firstEntry.AddEntryLineDocument<SupportingDocument>("7003", "10,00", subType: "LIQ", status: "ACC", amount: 10.0m, currency: "EUR");
				firstEntry.AddEntryLineDocument<SupportingDocument>("7003", "10,00", subType: "LIQ", amount: 10.0m, currency: "EUR");
				AssertEquals("First entry has 2 supporting documents for 7003 (one accepted one not) before second merge", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7003"));
				AssertEntryLineTypeSupDoc("First entry before second merge", firstEntry, "7003", "10,00", amount: 10.0m);

				secondEntry.AddEntryLineDocument<SupportingDocument>("7003", "90,00", subType: "LIQ", amount: 90.0m, currency: "EUR");
				AssertEntryLineTypeSupDoc("Second entry before second merge", secondEntry, "7003", "90,00", amount: 90.0m);

				Factory.Save();
				declaration.DoMerge();
				Factory.Save();

				AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);
				AssertEquals("REA charge amount is 1721,69 for invoiceLine1 afer merge", 1721.69m, invoiceLine1.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
				AssertEquals("REA charge amount is 5000,10 for invoiceLine2 afer merge", 5000.10m, invoiceLine2.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
				AssertEquals("REA charge amount is 10000,10 for invoiceLine3 afer merge", 10000.10m, invoiceLine3.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));

				AssertEquals("First entry has 2 supporting documents for 7003 (one accepted one updated) after second merge", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7003"));
				AssertEntryLineTypeSupDoc("First entry after second merge", firstEntry, "7003", "1721,69", amount: 1721.69m, dataModel: Core.Constants.CountryCodes.Spain);

				AssertEntryLineTypeSupDoc("Second entry after second merge", secondEntry, "7003", "15000,20", amount: 15000.20m, dataModel: Core.Constants.CountryCodes.Spain);

				AssertEquals("Third entry has no supporting document for 7003", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003"));
			});
		}
	}

	(JobComInvoiceLine, JobComInvoiceLine, JobComInvoiceLine, JobComInvoiceLine, CusEntryHeader) SetUpDataForImport7002And7003Documents(bool shouldAddEntryInstruction = false)
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_IncoTerm = "DDP";
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		invoice.JZ_InvoiceAmount = 1000m;

		var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_Tariff = "2208905400";
		invoiceLine1.JI_LinePrice = 100m;

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_Tariff = "2208905401";
		invoiceLine2.JI_LinePrice = 100m;

		var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine3.JI_Tariff = "2208905401";
		invoiceLine3.JI_LinePrice = 100m;

		var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine4.JI_Tariff = "2208905402";
		invoiceLine4.JI_LinePrice = 100m;

		if (shouldAddEntryInstruction)
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine4.JI_CEI = entryInstruction.PK;
		}

		Factory.Save();
		declaration.DoMerge();
		var entryHeader = declaration.CustomsEntryHeaders[0];

		return (invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4, entryHeader);
	}

	void SetUpChargesForImport7002Document(JobComInvoiceLine invoiceLine1, JobComInvoiceLine invoiceLine2, JobComInvoiceLine invoiceLine3)
	{
		var tDM1 = invoiceLine1.Charges.AddNew(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 30m, declaration.LocalCurrencyCode);
		tDM1.J7_IsNotIncludedInInvoice = false;
		tDM1.J7_IsDutiable = false;
		tDM1.J7_IsGSTApplicable = true;
		var oFT1 = invoiceLine1.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 20m, declaration.LocalCurrencyCode);

		var tDM2 = invoiceLine2.Charges.AddNew(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 40m, declaration.LocalCurrencyCode);
		tDM2.J7_IsNotIncludedInInvoice = false;
		tDM2.J7_IsDutiable = false;
		tDM2.J7_IsGSTApplicable = true;
		var oFT2 = invoiceLine2.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 20m, declaration.LocalCurrencyCode);

		var tDM3 = invoiceLine3.Charges.AddNew(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 50m, declaration.LocalCurrencyCode);
		tDM3.J7_IsNotIncludedInInvoice = false;
		tDM3.J7_IsDutiable = false;
		tDM3.J7_IsGSTApplicable = true;
		var oFT3 = invoiceLine3.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 20m, declaration.LocalCurrencyCode);
	}

	void SetUpREAChargesForImport7003Document(JobComInvoiceLine invoiceLine1, JobComInvoiceLine invoiceLine2, JobComInvoiceLine invoiceLine3)
	{
		invoiceLine1.JI_PrimaryPreference = "085";
		invoiceLine1.ZG_REAProductCode = "T001";
		invoiceLine1.ZG_IsREADirectConsumption = true;
		invoiceLine1.JI_CustomsQuantity = 20255.2;
		invoiceLine1.JI_CustomsUnitQty = "KGM";

		invoiceLine2.JI_PrimaryPreference = "085";
		invoiceLine2.ZG_REAProductCode = "T002";
		invoiceLine2.ZG_IsREADirectConsumption = true;
		invoiceLine2.JI_CustomsQuantity = 10000.2;
		invoiceLine2.JI_CustomsUnitQty = "KGM";
		var oFT = invoiceLine2.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 20m, declaration.LocalCurrencyCode);

		invoiceLine3.JI_PrimaryPreference = "085";
		invoiceLine3.ZG_REAProductCode = "T002";
		invoiceLine3.ZG_IsREADirectConsumption = true;
		invoiceLine3.JI_CustomsQuantity = 20000.2;
		invoiceLine3.JI_CustomsUnitQty = "KGM";
		var oFT2 = invoiceLine3.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 20m, declaration.LocalCurrencyCode);

		declaration.ResumeApportionment();
	}

	public void TestRemoveImport7003CLSupportingDocument()
	{
		declaration.SupportingDocuments.Add(Factory.CreateSupportingDocument("7003", "declaration"));
		declaration.SupportingDocuments.Add(Factory.CreateSupportingDocument("AAA", "declaration"));

		var (invoice, invoiceLine1, entryInstruction1) = SetUpInvHeaderInvLineAndEntryInstruction(ZString.Empty);
		entryInstruction1.SupportingDocuments.Add(Factory.CreateSupportingDocument("7002", "entryInstruction1"));
		entryInstruction1.SupportingDocuments.Add(Factory.CreateSupportingDocument("BBB", "entryInstruction1"));

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.SupportingDocuments.Add(Factory.CreateSupportingDocument("7002", "entryInstruction2"));

		var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction3.SupportingDocuments.Add(Factory.CreateSupportingDocument("CCC", "entryInstruction3"));

		invoice.JZ_IncoTerm = "DDP";
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		invoice.JZ_InvoiceAmount = 1000m;
		invoice.SupportingDocuments.Add(Factory.CreateSupportingDocument("7003", "invoice"));
		invoice.SupportingDocuments.Add(Factory.CreateSupportingDocument("BBB", "invoice"));

		invoiceLine1.JI_Tariff = "2208905400";
		invoiceLine1.JI_LinePrice = 100m;
		invoiceLine1.SupportingDocuments.Add(Factory.CreateSupportingDocument("7003", "invoiceLine1"));
		invoiceLine1.SupportingDocuments.Add(Factory.CreateSupportingDocument("CCC", "invoiceLine1"));

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_Tariff = "2208905401";
		invoiceLine2.JI_LinePrice = 100m;
		invoiceLine2.SupportingDocuments.Add(Factory.CreateSupportingDocument("7003", "invoiceLine2"));

		var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstruction1.PK;
		invoiceLine3.JI_Tariff = "2208905402";
		invoiceLine3.JI_LinePrice = 100m;
		invoiceLine3.SupportingDocuments.Add(Factory.CreateSupportingDocument("EEE", "invoiceLine4"));

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Declaration has 2 supporting documents", 2, declaration.SupportingDocuments.Count);
			AssertEquals("EntryInstruction1 has 2 supporting documents", 2, entryInstruction1.SupportingDocuments.Count);
			AssertEquals("EntryInstruction2 has 1 supporting document", 1, entryInstruction2.SupportingDocuments.Count);
			AssertEquals("EntryInstruction3 has 1 supporting document", 1, entryInstruction3.SupportingDocuments.Count);
			AssertEquals("Invoice has 2 supporting documents", 2, invoice.SupportingDocuments.Count);
			AssertEquals("InvoiceLine1 has 2 supporting documents", 2, invoiceLine1.SupportingDocuments.Count);
			AssertEquals("InvoiceLine2 has 1 supporting document", 1, invoiceLine2.SupportingDocuments.Count);
			AssertEquals("InvoiceLine3 has 1 supporting document", 1, invoiceLine3.SupportingDocuments.Count);

			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			AssertEquals("REA charge amount is 0 for invoiceLine1", 0m, invoiceLine1.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 0 for invoiceLine2", 0m, invoiceLine2.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));
			AssertEquals("REA charge amount is 0 for invoiceLine3", 0m, invoiceLine3.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, declaration.LocalCurrency));

			AssertEquals("Supporting document 7003 was removed from declaration, now it only has 1 sup doc", 1, declaration.SupportingDocuments.Count);
			AssertEquals("Supporting document 7002 was removed from entryInstruction1, now it only has 1 sup doc", 1, entryInstruction1.SupportingDocuments.Count);
			AssertEquals("Supporting document 7002 was removed from entryInstruction2, now it has 0 sup doc", 0, entryInstruction2.SupportingDocuments.Count);
			AssertEquals("Supporting document 7002 was not declared for entryInstruction3, it still has 1 sup doc", 1, entryInstruction3.SupportingDocuments.Count);
			AssertEquals("Supporting document 7003 was removed from invoice, now it only has 1 sup doc", 1, invoice.SupportingDocuments.Count);
			AssertEquals("Supporting document 7003 was removed from invoiceLine1, now it only has 1 sup doc", 1, invoiceLine1.SupportingDocuments.Count);
			AssertEquals("Supporting document 7003 was removed from invoiceLine2, now it has 0 sup doc", 0, invoiceLine2.SupportingDocuments.Count);
			AssertEquals("Supporting document 7003 was not declared for invoiceLine3, it still has 1 sup doc", 1, invoiceLine3.SupportingDocuments.Count);

			AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
			AssertEquals("First entry has no supporting document for 7003 when REA charge amount is 0", false, firstEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003"));

			var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
			AssertEquals("Second entry has no supporting document for 7003 when REA charge amount is 0", false, secondEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003"));

			var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");
			AssertEquals("Third entry has no supporting document for 7003 when REA charge amount is 0", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003"));

			firstEntry.AddEntryLineDocument<SupportingDocument>("7003", "10,00", subType: "LIQ", status: "ACC");
			firstEntry.AddEntryLineDocument<SupportingDocument>("7003", "10,00", subType: "LIQ");
			AssertEquals("First entry has 2 supporting document for 7003 (one accepted one not) before second merge", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7003"));
			AssertEntryLineTypeSupDoc("First entry before second merge", firstEntry, "7003", "10,00");

			secondEntry.AddEntryLineDocument<SupportingDocument>("7003", "90,00", subType: "LIQ");
			AssertEntryLineTypeSupDoc("Second entry before second merge", secondEntry, "7003", "90,00");

			thirdEntry.AddEntryLineDocument<SupportingDocument>("7003", "50,00", subType: "LIQ");
			AssertEntryLineTypeSupDoc("Third entry before second merge", thirdEntry, "7003", "50,00");

			Factory.Save();
			declaration.DoMerge();
			Factory.Save();

			AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			AssertEquals("First entry has 1 supporting document for 7003 (accepted) after second merge", 1, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == "7003"));
			AssertEquals("First entry has no supporting document for 7003 without accepted status", false, firstEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003" && x.CSI_Status != "ACC"));

			AssertEquals("Second entry has no supporting document for 7003", false, secondEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003"));

			AssertEquals("Third entry has no supporting document for 7003", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == "7003"));
		});
	}

	public void TestCreateImport9015CLSupportingDocument()
	{
		var docType = SupportingDocumentType.VATReductionCode;

		declaration.JE_DeclarantType = "DIR";
		SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
		SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, "AAA");

		var (invoice, invoiceLine1, entryInstruction1) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

		var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction3.CEI_SubStyle = EntrySubStyleList.Codes.C;

		var declarant = Factory.New<OrgHeader>();
		declarant.OH_Code = "AA";
		declaration.Declarant.OA_OH = declarant.PK;
		var authorisation = SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);

		GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, declarant.PK, "Guarantee", false);

		const string bondNumber = "Guarantee";
		GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction1.PK, bondNumber), (entryInstruction2.PK, bondNumber), (entryInstruction3.PK, bondNumber));

		SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType);
		SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, "BBB");

		invoiceLine1.JI_Tariff = "2208905400";
		invoiceLine1.ZG_MethodOfPayment = "R";
		SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine1, docType);
		SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine1, "CCC");

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_Tariff = "2208905401";
		invoiceLine2.JI_CEI = entryInstruction2.PK;
		invoiceLine2.ZG_MethodOfPayment = "R";
		SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine2, docType);

		var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine3.JI_Tariff = "2208905402";
		invoiceLine3.JI_CEI = entryInstruction2.PK;
		invoiceLine3.ZG_MethodOfPayment = "R";
		SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine3, "DDD");

		var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine4.JI_Tariff = "2208905403";
		invoiceLine4.JI_CEI = entryInstruction3.PK;
		invoiceLine4.ZG_MethodOfPayment = "A";
		SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine4, docType);

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Declaration has 2 supporting documents", 2, declaration.SupportingDocuments.Count);
			AssertEquals("Invoice has 2 supporting documents", 2, invoice.SupportingDocuments.Count);
			AssertEquals("InvoiceLine1 has 2 supporting documents", 2, invoiceLine1.SupportingDocuments.Count);
			AssertEquals("InvoiceLine2 has 1 supporting document", 1, invoiceLine2.SupportingDocuments.Count);
			AssertEquals("InvoiceLine3 has 1 supporting document", 1, invoiceLine3.SupportingDocuments.Count);
			AssertEquals("InvoiceLine4 has 1 supporting document", 1, invoiceLine4.SupportingDocuments.Count);

			declaration.DoMerge();
			Factory.Save();

			AssertEquals("Supporting document 9015 was removed from declaration, now it only has 1 sup doc", 1, declaration.SupportingDocuments.Count);
			AssertEquals("Supporting document 9015 was removed from invoice, now it only has 1 sup doc", 1, invoice.SupportingDocuments.Count);
			AssertEquals("Supporting document 9015 was removed from invoiceLine1, now it only has 1 sup doc", 1, invoiceLine1.SupportingDocuments.Count);
			AssertEquals("Supporting document 9015 was removed from invoiceLine2, now it has 0 sup doc", 0, invoiceLine2.SupportingDocuments.Count);
			AssertEquals("Supporting document 9015 was not declared for invoiceLine3, it still has 1 sup doc", 1, invoiceLine3.SupportingDocuments.Count);
			AssertEquals("Supporting document 9015 was not removed from invoiceLine4 because since it does not meet the requirements, it still has 1 sup doc", 1, invoiceLine4.SupportingDocuments.Count);

			AssertEquals("There are 3 entry headers", 3, declaration.CustomsEntryHeaders.Count);

			var firstEntryHeader = declaration.CustomsEntryHeaders.FirstOrDefault(x => x.CH_CEI_Instruction == entryInstruction1.PK);
			AssertEquals("There is 1 entry line in the first entry header", 1, firstEntryHeader.MergedLines.Count);
			var firstEntryLine = firstEntryHeader.MergedLines[0];
			AssertEquals("First entry header has new supporting document for 9015 in the only entry line it has", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(firstEntryLine, docType));
			AssertEntryLineTypeSupDoc("First entry's only line", firstEntryLine, "9015", authorisation.CPH_Number, dataModel: Core.Constants.CountryCodes.Spain);

			var secondEntryHeader = declaration.CustomsEntryHeaders.FirstOrDefault(x => x.CH_CEI_Instruction == entryInstruction2.PK);
			AssertEquals("There are 2 entry lines in the second entry header", 2, secondEntryHeader.MergedLines.Count);

			var secondEntryLine1 = secondEntryHeader.MergedLines.FirstOrDefault(x => x.CL_LineNumber == 1);
			AssertEquals("Second entry header has new supporting document for 9015 in the first entry line it has", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(secondEntryLine1, docType));
			AssertEntryLineTypeSupDoc("Second entry's first line", secondEntryLine1, "9015", authorisation.CPH_Number, dataModel: Core.Constants.CountryCodes.Spain);
			var secondEntryLine2 = secondEntryHeader.MergedLines.FirstOrDefault(x => x.CL_LineNumber == 2);
			AssertEquals("Second entry header has no supporting document for 9015 in the second entry line it has", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(secondEntryLine2, docType));

			var thirdEntryHeader = declaration.CustomsEntryHeaders.FirstOrDefault(x => x.CH_CEI_Instruction == entryInstruction3.PK);
			AssertEquals("There is 1 entry line in the third entry header", 1, thirdEntryHeader.MergedLines.Count);
			AssertEquals("Third entry header has no supporting document for 9015 in the only entry line it has since it does not meet the requirements", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(thirdEntryHeader.MergedLines[0], docType));
		});
	}

	public void TestUpdateImport9015CLSupportingDocument()
	{
		var docType = SupportingDocumentType.VATReductionCode;
		var initialReference = "AAA";

		declaration.JE_DeclarantType = "DIR";
		var (invoice, invoiceLine1, entryInstruction1) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

		var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction3.CEI_SubStyle = EntrySubStyleList.Codes.C;

		var declarant = Factory.New<OrgHeader>();
		declarant.OH_Code = "AA";
		declaration.Declarant.OA_OH = declarant.PK;
		var authorisation = SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);

		GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, declarant.PK, "Guarantee", false);

		const string bondNumber = "Guarantee";
		GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction1.PK, bondNumber), (entryInstruction2.PK, bondNumber), (entryInstruction3.PK, bondNumber));

		invoiceLine1.JI_Tariff = "2208905400";
		invoiceLine1.ZG_MethodOfPayment = "R";

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_Tariff = "2208905401";
		invoiceLine2.JI_CEI = entryInstruction2.PK;
		invoiceLine2.ZG_MethodOfPayment = "R";

		var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine3.JI_Tariff = "2208905402";
		invoiceLine3.JI_CEI = entryInstruction2.PK;
		invoiceLine3.ZG_MethodOfPayment = "R";

		var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine4.JI_Tariff = "2208905403";
		invoiceLine4.JI_CEI = entryInstruction3.PK;
		invoiceLine4.ZG_MethodOfPayment = "R";

		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
		var entryLine1 = entryHeader1.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		entryLine1.AddEntryLineDocument<SupportingDocument>(docType, initialReference, subType: "LIQ", status: "ACC");
		entryLine1.AddEntryLineDocument<SupportingDocument>(docType, initialReference, subType: "LIQ");

		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
		var entryLine2 = entryHeader2.MergedLines.AddNew();
		entryLine2.CL_LineNumber = 1;
		invoiceLine2.JI_CL = entryLine2.PK;
		entryLine2.AddEntryLineDocument<SupportingDocument>(docType, initialReference, subType: "LIQ");
		var entryLine3 = entryHeader2.MergedLines.AddNew();
		entryLine3.CL_LineNumber = 2;
		invoiceLine3.JI_CL = entryLine3.PK;
		entryLine3.AddEntryLineDocument<SupportingDocument>(docType, initialReference, subType: "LIQ");

		var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader3.CH_CEI_Instruction = entryInstruction3.PK;
		var entryLine4 = entryHeader3.MergedLines.AddNew();
		invoiceLine4.JI_CL = entryLine4.PK;
		entryLine4.AddEntryLineDocument<SupportingDocument>(docType, initialReference, subType: "LIQ", status: "ACC");

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("First entry header's entry line has 2 supporting documents for 9015 (one accepted one not) before merge", 2, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine1, docType));
			AssertEntryLineTypeSupDoc("First entry's only line before merge", entryLine1, docType, initialReference, dataModel: Core.Constants.CountryCodes.Spain);

			AssertEquals("Second entry header's first entry line has 1 supporting document for 9015 (not accepted) before merge", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2, docType));
			AssertEntryLineTypeSupDoc("Second entry's first line before merge", entryLine2, docType, initialReference, dataModel: Core.Constants.CountryCodes.Spain);
			AssertEquals("Second entry header's second entry line has 1 supporting document for 9015 (not accepted) before merge", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine3, docType));
			AssertEntryLineTypeSupDoc("Second entry's second line before merge", entryLine3, docType, initialReference, dataModel: Core.Constants.CountryCodes.Spain);

			AssertEquals("Third entry header's entry line has 1 supporting document for 9015 (accepted) before merge", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine4, docType));

			declaration.DoMerge();
			Factory.Save();

			AssertEquals("First entry header's entry line has 1 supporting document for 9015 (accepted and not updated) after merge", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine1, docType));
			AssertEquals("First entry header's entry line has no supporting document for 9015 without accepted status", false, entryLine1.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType && x.CSI_Status != "ACC"));

			AssertEquals("Second entry header's first entry line has 1 supporting documents for 9015 (updated) after merge", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2, docType));
			AssertEntryLineTypeSupDoc("Second entry's first line after merge", entryLine2, docType, authorisation.CPH_Number, dataModel: Core.Constants.CountryCodes.Spain);
			AssertEquals("Second entry header's second entry line has no supporting documents for 9015 after merge", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine3, docType));

			AssertEquals("Third entry header's entry line has 1 supporting document for 9015 (accepted and not updated) after merge", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine4, docType));
			AssertEquals("Third entry header's entry line has no supporting document for 9015 without accepted status", false, entryLine4.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType && x.CSI_Status != "ACC"));
		});
	}

	public void TestRemoveImport9015CLSupportingDocument()
	{
		var docType = SupportingDocumentType.VATReductionCode;
		var initialReference = "AAA";

		var (invoice, invoiceLine1, entryInstruction1) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

		var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction3.CEI_SubStyle = EntrySubStyleList.Codes.C;

		invoiceLine1.JI_Tariff = "2208905400";
		invoiceLine1.ZG_MethodOfPayment = "R";

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_Tariff = "2208905401";
		invoiceLine2.JI_CEI = entryInstruction2.PK;
		invoiceLine2.ZG_MethodOfPayment = "R";

		var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine3.JI_Tariff = "2208905402";
		invoiceLine3.JI_CEI = entryInstruction2.PK;
		invoiceLine3.ZG_MethodOfPayment = "R";

		var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine4.JI_Tariff = "2208905403";
		invoiceLine4.JI_CEI = entryInstruction3.PK;
		invoiceLine4.ZG_MethodOfPayment = "R";

		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
		var entryLine1 = entryHeader1.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		entryLine1.AddEntryLineDocument<SupportingDocument>(docType, initialReference, subType: "LIQ", status: "ACC");
		entryLine1.AddEntryLineDocument<SupportingDocument>(docType, initialReference, subType: "LIQ");

		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
		var entryLine2 = entryHeader2.MergedLines.AddNew();
		entryLine2.CL_LineNumber = 1;
		invoiceLine2.JI_CL = entryLine2.PK;
		entryLine2.AddEntryLineDocument<SupportingDocument>(docType, initialReference, subType: "LIQ");
		var entryLine3 = entryHeader2.MergedLines.AddNew();
		entryLine3.CL_LineNumber = 2;
		invoiceLine3.JI_CL = entryLine3.PK;
		entryLine3.AddEntryLineDocument<SupportingDocument>(docType, initialReference, subType: "LIQ");

		var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader3.CH_CEI_Instruction = entryInstruction3.PK;
		var entryLine4 = entryHeader3.MergedLines.AddNew();
		invoiceLine4.JI_CL = entryLine4.PK;
		entryLine4.AddEntryLineDocument<SupportingDocument>(docType, initialReference, subType: "LIQ", status: "ACC");

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("First entry header's entry line has 2 supporting documents for 9015 (one accepted one not) before merge", 2, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine1, docType));
			AssertEntryLineTypeSupDoc("First entry's only line before merge", entryLine1, docType, initialReference, dataModel: Core.Constants.CountryCodes.Spain);

			AssertEquals("Second entry header's first entry line has 1 supporting document for 9015 (not accepted) before merge", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2, docType));
			AssertEntryLineTypeSupDoc("Second entry's first line before merge", entryLine2, docType, initialReference, dataModel: Core.Constants.CountryCodes.Spain);
			AssertEquals("Second entry header's second entry line has 1 supporting document for 9015 (not accepted) before merge", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine3, docType));
			AssertEntryLineTypeSupDoc("Second entry's second line before merge", entryLine3, docType, initialReference, dataModel: Core.Constants.CountryCodes.Spain);

			AssertEquals("Third entry header's entry line has 1 supporting document for 9015 (accepted) before merge", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine4, docType));

			Factory.Save();
			declaration.DoMerge();
			Factory.Save();

			AssertEquals("First entry header's entry line has 1 supporting document for 9015 (the accepted one) after merge when conditions are not met", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine1, docType));
			AssertEquals("First entry header's entry line has no supporting document for 9015 without accepted status when conditions are not met", false, entryLine1.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType && x.CSI_Status != "ACC"));

			AssertEquals("Second entry header's first entry line has no supporting documents for 9015 after merge when conditions are not met", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2, docType));
			AssertEquals("Second entry header's second entry line has no supporting documents for 9015 after merge", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine3, docType));

			AssertEquals("Third entry header's entry line has 1 supporting document for 9015 (the accepted one) after merge when conditions are not met", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine4, docType));
			AssertEquals("Third entry header's entry line has no supporting document for 9015 without accepted status when conditions are not met", false, entryLine4.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType && x.CSI_Status != "ACC"));
		});
	}

	public void TestCreateImportH25018CLSupportingDocument()
	{
		var docType = "5018";
		var docReference = "invoiceDoc";

		var (invoice, invoiceLine1, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);
		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;

		invoiceLine1.JI_Tariff = "2208905400";

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Tariff = "2208905401";

		var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstruction.PK;
		invoiceLine3.JI_Tariff = "2208905401";

		var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine4.JI_CEI = entryInstruction.PK;
		invoiceLine4.JI_Tariff = "2208905402";

		var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine5.JI_CEI = entryInstruction.PK;
		invoiceLine5.JI_Tariff = "2208905403";

		var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine6.JI_CEI = entryInstruction.PK;
		invoiceLine6.JI_Tariff = "2208905404";

		Factory.Save();
		declaration.DoMerge();
		var entryHeader = declaration.CustomsEntryHeaders[0];

		CombineAssertions(() =>
		{
			AssertEquals("JI_CustomsThirdQuantity is 0 for invoiceLine1", 0m, invoiceLine1.JI_CustomsThirdQuantity);
			AssertEquals("JI_CustomsThirdQuantity is 0 for invoiceLine2", 0m, invoiceLine2.JI_CustomsThirdQuantity);
			AssertEquals("JI_CustomsThirdQuantity is 0 for invoiceLine3", 0m, invoiceLine3.JI_CustomsThirdQuantity);
			AssertEquals("JI_CustomsThirdQuantity is 0 for invoiceLine4", 0m, invoiceLine4.JI_CustomsThirdQuantity);
			AssertEquals("JI_CustomsThirdQuantity is 0 for invoiceLine5", 0m, invoiceLine5.JI_CustomsThirdQuantity);
			AssertEquals("JI_CustomsThirdQuantity is 0 for invoiceLine6", 0m, invoiceLine6.JI_CustomsThirdQuantity);

			AssertEquals("There are 5 entry lines", 5, entryHeader.MergedLines.Count);

			var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
			AssertEquals("First entry has no supporting document for 5018 when third quantity is 0", false, firstEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
			AssertEquals("Second entry has no supporting document for 5018 when third quantity is 0", false, secondEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");
			AssertEquals("Third entry has no supporting document for 5018 when third quantity is 0", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			var fourthEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905403");
			AssertEquals("Fourth entry has no supporting document for 5018 when third quantity is 0", false, fourthEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			var fifthEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905404");
			AssertEquals("Fifth entry has no supporting document for 5018 when third quantity is 0", false, fifthEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			invoiceLine1.JI_CustomsThirdQuantity = 10m;
			invoiceLine1.JI_CustomsThirdUnitQty = "LT";
			invoiceLine2.JI_CustomsThirdQuantity = 20m;
			invoiceLine2.JI_CustomsThirdUnitQty = "KGM";
			invoiceLine3.JI_CustomsThirdQuantity = 30m;
			invoiceLine3.JI_CustomsThirdUnitQty = "KGM";
			invoiceLine4.JI_CustomsThirdQuantity = 40m;
			invoiceLine4.JI_CustomsThirdUnitQty = ESConstants.UOM.PK;
			invoiceLine5.JI_CustomsThirdQuantity = ZDecimal.Zero;
			invoiceLine5.JI_CustomsThirdUnitQty = "HG";
			invoiceLine6.JI_CustomsThirdQuantity = 40m;
			invoiceLine6.JI_CustomsThirdUnitQty = ZString.Empty;

			Factory.Save();
			declaration.DoMerge();
			Factory.Save();

			AssertEquals("There are 5 entry lines", 5, entryHeader.MergedLines.Count);

			AssertEquals("First entry has no supporting document for 5018 when third quantity is not 0 but there is no 5018 document in invoice header", false, firstEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			AssertEquals("Second entry has no supporting document for 5018 when third quantity is not 0 but there is no 5018 document in invoice header", false, secondEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			AssertEquals("Third entry has no supporting document for 5018 when third quantity is not 0 but unit is PK or GF", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			AssertEquals("Fourth entry has no supporting document for 5018 when third quantity is 0", false, fourthEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			AssertEquals("Fifth entry has no supporting document for 5018 when third quantity unit is empty", false, fifthEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			entryInstruction.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, docReference));
			firstEntry.AddEntryLineDocument<SupportingDocument>(docType, "50,00", subType: "LIQ", status: "ACC");

			Factory.Save();
			declaration.DoMerge();
			Factory.Save();

			AssertEquals("There are 5 entry lines", 5, entryHeader.MergedLines.Count);

			AssertEquals("First entry has 1 supporting document for 5018 (only the accepted one)", 1, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == docType));

			AssertEntryLineTypeSupDoc("Second entry", secondEntry, docType, docReference, 50m, "KGM", dataModel: Core.Constants.CountryCodes.Spain);

			AssertEquals("Third entry has no supporting document for 5018 because third quantity unit is PK or GF", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));
			AssertEquals("Fourth entry has no supporting document for 5018 because third quantity is 0", false, fourthEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));
			AssertEquals("Fifth entry has no supporting document for 5018 because third quantity unit is empty", false, fifthEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));
		});
	}

	public void TestUpdateImportH25018CLSupportingDocument()
	{
		var docType = "5018";
		var docReference = "invoiceDoc";

		var (invoice, invoiceLine1, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);
		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		entryInstruction.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, docReference));

		invoiceLine1.JI_Tariff = "2208905400";

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Tariff = "2208905401";

		var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstruction.PK;
		invoiceLine3.JI_Tariff = "2208905401";

		var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine4.JI_CEI = entryInstruction.PK;
		invoiceLine4.JI_Tariff = "2208905402";

		var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine5.JI_CEI = entryInstruction.PK;
		invoiceLine5.JI_Tariff = "2208905403";

		var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine6.JI_CEI = entryInstruction.PK;
		invoiceLine6.JI_Tariff = "2208905404";

		Factory.Save();
		declaration.DoMerge();
		var entryHeader = declaration.CustomsEntryHeaders[0];

		CombineAssertions(() =>
		{
			AssertEquals("JI_CustomsThirdQuantity is 0 for invoiceLine1", 0m, invoiceLine1.JI_CustomsThirdQuantity);
			AssertEquals("JI_CustomsThirdQuantity is 0 for invoiceLine2", 0m, invoiceLine2.JI_CustomsThirdQuantity);
			AssertEquals("JI_CustomsThirdQuantity is 0 for invoiceLine3", 0m, invoiceLine3.JI_CustomsThirdQuantity);
			AssertEquals("JI_CustomsThirdQuantity is 0 for invoiceLine4", 0m, invoiceLine4.JI_CustomsThirdQuantity);
			AssertEquals("JI_CustomsThirdQuantity is 0 for invoiceLine5", 0m, invoiceLine5.JI_CustomsThirdQuantity);
			AssertEquals("JI_CustomsThirdQuantity is 0 for invoiceLine6", 0m, invoiceLine6.JI_CustomsThirdQuantity);

			AssertEquals("There are 5 entry lines", 5, entryHeader.MergedLines.Count);

			var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
			AssertEquals("First entry has no supporting document for 5018 when third quantity is 0", false, firstEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
			AssertEquals("Second entry has no supporting document for 5018 when third quantity is 0", false, secondEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");
			AssertEquals("Third entry has no supporting document for 5018 when third quantity is 0", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			var fourthEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905403");
			AssertEquals("Fourth entry has no supporting document for 5018 when third quantity is 0", false, fourthEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			var fifthEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905404");
			AssertEquals("Fifth entry has no supporting document for 5018 when third quantity is 0", false, fifthEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			invoiceLine1.JI_CustomsThirdQuantity = 10m;
			invoiceLine1.JI_CustomsThirdUnitQty = "LT";
			invoiceLine2.JI_CustomsThirdQuantity = 20m;
			invoiceLine2.JI_CustomsThirdUnitQty = "KGM";
			invoiceLine3.JI_CustomsThirdQuantity = 30m;
			invoiceLine3.JI_CustomsThirdUnitQty = "KGM";
			invoiceLine4.JI_CustomsThirdQuantity = 40m;
			invoiceLine4.JI_CustomsThirdUnitQty = ESConstants.UOM.PK;
			invoiceLine5.JI_CustomsThirdQuantity = ZDecimal.Zero;
			invoiceLine5.JI_CustomsThirdUnitQty = "HG";
			invoiceLine6.JI_CustomsThirdQuantity = 40m;
			invoiceLine6.JI_CustomsThirdUnitQty = ZString.Empty;

			firstEntry.AddEntryLineDocument<SupportingDocument>(docType, "50,00", subType: "LIQ", status: "ACC");
			firstEntry.AddEntryLineDocument<SupportingDocument>(docType, "50,00", subType: "LIQ");
			AssertEquals("First entry has 2 supporting documents for 5018 (one accepted one not) before second merge", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == docType));
			AssertEntryLineTypeSupDoc("First entry before second merge", firstEntry, docType, "50,00");

			secondEntry.AddEntryLineDocument<SupportingDocument>(docType, "90,00", subType: "LIQ");
			AssertEntryLineTypeSupDoc("Second entry before second merge", secondEntry, docType, "90,00");

			Factory.Save();
			declaration.DoMerge();
			Factory.Save();

			AssertEquals("There are 5 entry lines", 5, entryHeader.MergedLines.Count);

			AssertEquals("First entry has 1 supporting document for 5018 (only the accepted one) after second merge", 1, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == docType));

			AssertEntryLineTypeSupDoc("Second entry after second merge", secondEntry, docType, docReference, 50m, "KGM", dataModel: Core.Constants.CountryCodes.Spain);

			AssertEquals("Third entry has no supporting document for 5018 because third quantity unit is PK or GF", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));
			AssertEquals("Fourth entry has no supporting document for 5018 because third quantity is 0", false, fourthEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));
			AssertEquals("Fifth entry has no supporting document for 5018 because third quantity unit is empty", false, fifthEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));
		});
	}

	public void TestRemoveImportH25018CLSupportingDocument()
	{
		var docType = "5018";

		declaration.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, "declaration"));
		declaration.SupportingDocuments.Add(Factory.CreateSupportingDocument("AAA", "declaration"));

		var (invoice, invoiceLine1, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);
		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		entryInstruction.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, "invoice"));
		entryInstruction.SupportingDocuments.Add(Factory.CreateSupportingDocument("BBB", "invoice"));

		invoiceLine1.JI_Tariff = "2208905400";
		invoiceLine1.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, "invoiceLine1"));
		invoiceLine1.SupportingDocuments.Add(Factory.CreateSupportingDocument("CCC", "invoiceLine1"));

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Tariff = "2208905401";
		invoiceLine2.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, "invoiceLine2"));

		var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstruction.PK;
		invoiceLine3.JI_Tariff = "2208905402";
		invoiceLine3.SupportingDocuments.Add(Factory.CreateSupportingDocument("EEE", "invoiceLine3"));

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Declaration has 2 supporting documents", 2, declaration.SupportingDocuments.Count);
			AssertEquals("EntryInstruction has 2 supporting documents", 2, entryInstruction.SupportingDocuments.Count);
			AssertEquals("InvoiceLine1 has 2 supporting documents", 2, invoiceLine1.SupportingDocuments.Count);
			AssertEquals("InvoiceLine2 has 1 supporting document", 1, invoiceLine2.SupportingDocuments.Count);
			AssertEquals("InvoiceLine3 has 1 supporting document", 1, invoiceLine3.SupportingDocuments.Count);

			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			AssertEquals("JI_CustomsThirdQuantity is 0 for invoiceLine1", 0m, invoiceLine1.JI_VAT_Additions);
			AssertEquals("JI_CustomsThirdQuantity is 0 for invoiceLine2", 0m, invoiceLine2.JI_VAT_Additions);
			AssertEquals("JI_CustomsThirdQuantity is 0 for invoiceLine3", 0m, invoiceLine3.JI_VAT_Additions);

			AssertEquals("Supporting document 5018 was removed from declaration, now it only has 1 sup doc", 1, declaration.SupportingDocuments.Count);
			AssertEquals("Supporting document 5018 was NOT removed from entryInstruction, it stll has 2 sup doc", 2, entryInstruction.SupportingDocuments.Count);
			AssertEquals("Supporting document 5018 was removed from invoiceLine1, now it only has 1 sup doc", 1, invoiceLine1.SupportingDocuments.Count);
			AssertEquals("Supporting document 5018 was removed from invoiceLine2, now it has 0 sup doc", 0, invoiceLine2.SupportingDocuments.Count);
			AssertEquals("Supporting document 5018 was not declared for invoiceLine3, it still has 1 sup doc", 1, invoiceLine3.SupportingDocuments.Count);

			AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
			AssertEquals("First entry has no supporting document for 5018 when vat additions is 0", false, firstEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
			AssertEquals("Second entry has no supporting document for 5018 when vat additions is 0", false, secondEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");
			AssertEquals("Third entry has no supporting document for 5018 when vat additions is 0", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			firstEntry.AddEntryLineDocument<SupportingDocument>(docType, "10,00", subType: "LIQ", status: "ACC");
			firstEntry.AddEntryLineDocument<SupportingDocument>(docType, "10,00", subType: "LIQ");
			AssertEquals("First entry has 2 supporting document for 5018 (one accepted one not) before second merge", 2, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == docType));
			AssertEntryLineTypeSupDoc("First entry before second merge", firstEntry, docType, "10,00");

			secondEntry.AddEntryLineDocument<SupportingDocument>(docType, "90,00", subType: "LIQ");
			AssertEntryLineTypeSupDoc("Second entry before second merge", secondEntry, docType, "90,00");

			thirdEntry.AddEntryLineDocument<SupportingDocument>(docType, "50,00", subType: "LIQ");
			AssertEntryLineTypeSupDoc("Third entry before second merge", thirdEntry, docType, "50,00");

			Factory.Save();
			declaration.DoMerge();
			Factory.Save();

			AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			AssertEquals("First entry has 1 supporting document for 5018 (accepted) after second merge", 1, firstEntry.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == docType));
			AssertEquals("First entry has no supporting document for 5018 without accepted status", false, firstEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType && x.CSI_Status != "ACC"));

			AssertEquals("Second entry has no supporting document for 5018", false, secondEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));

			AssertEquals("Third entry has no supporting document for 5018", false, thirdEntry.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType));
		});
	}

	public void TestCreateImport7015CLSupportingDocument()
	{
		SetUpTariffsWithFormulaPCA();

		var docType = "7015";
		var docReference = "1";

		var (invoice, invoiceLine1, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(ZString.Empty);

		invoiceLine1.ZG_ExciseCode = "1CF";
		invoiceLine1.JI_Tariff = "2208905400";

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.ZG_ExciseCode = "1CF";
		invoiceLine2.ZG_GlobalWarmingPotential = 1;
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Tariff = "2208905401";

		var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine3.ZG_ExciseCode = "A00";
		invoiceLine3.JI_CEI = entryInstruction.PK;
		invoiceLine3.JI_Tariff = "2208905402";

		CombineAssertions(() =>
		{
			var isDeclarationUCC6 = declaration.IsUCC6;
			AssertEquals("Pre-requisite: Non-UCC6 declaration", false, isDeclarationUCC6);

			Factory.Save();
			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			AssertEquals("1st Merge: There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
			AssertEntryLineTypeSupDocCount($"1st Merge: First entry has no supporting document for {docType} because PCA = 0", firstEntry, docType, 0);

			var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
			AssertEntryLineTypeSupDocCount($"1st Merge: Second entry has 1 supporting document for {docType}", secondEntry, docType, 1);
			AssertEntryLine7015SupDoc($"1st Merge: Second entry supporting document for {docType}", secondEntry, docType, 1m, isDeclarationUCC6, dataModel: Core.Constants.CountryCodes.Spain);

			var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");
			AssertEntryLineTypeSupDocCount($"1st Merge: Third entry has no supporting document for {docType} because PCA is not applicable", thirdEntry, docType, 0);

			entryInstruction.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, docReference));
			firstEntry.AddEntryLineDocument<SupportingDocument>(docType, "50,00", quantity: 50m, unitOfQuantity: "NAR", subType: "LIQ", status: "ACC");

			Factory.Save();
			declaration.DoMerge();

			AssertEquals("2nd Merge: There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			AssertEntryLineTypeSupDocCount($"2nd Merge: First entry has 1 supporting documents for {docType} (1 accepted)", firstEntry, docType, 1);

			AssertEntryLineTypeSupDocCount($"2nd Merge: Second entry has 1 supporting document for {docType}", secondEntry, docType, 1);
			AssertEntryLine7015SupDoc($"2nd Merge: Second entry supporting document for {docType}", secondEntry, docType, 1m, isDeclarationUCC6, dataModel: Core.Constants.CountryCodes.Spain);

			AssertEntryLineTypeSupDocCount($"2nd Merge: Third entry has no supporting document for {docType} because PCA is not applicable", thirdEntry, docType, 0);

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
			{
				isDeclarationUCC6 = declaration.IsUCC6;
				AssertEquals("Pre-requisite: UCC6 declaration", true, isDeclarationUCC6);

				Factory.Save();
				declaration.DoMerge();

				AssertEntryLineTypeSupDocCount($"3rd Merge (UCC6): First entry has 1 supporting document for {docType} (1 accepted)", firstEntry, docType, 1);
				AssertEntryLineTypeSupDocCount($"3rd Merge (UCC6): Second entry has 1 supporting document for {docType}", secondEntry, docType, 1);
				AssertEntryLine7015SupDoc($"3rd Merge (UCC6): Second entry supporting document for {docType}", secondEntry, docType, 1m, isDeclarationUCC6, dataModel: Core.Constants.CountryCodes.Spain);
				AssertEntryLineTypeSupDocCount($"3rd Merge (UCC6): Third entry has no supporting document for {docType} because PCA is not applicable", thirdEntry, docType, 0);
			}
		});
	}

	public void TestUpdateImport7015CLSupportingDocument()
	{
		SetUpTariffsWithFormulaPCA();

		var docType = "7015";
		var docReference = "1";

		var (invoice, invoiceLine1, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(ZString.Empty);
		entryInstruction.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, docReference));

		invoiceLine1.ZG_ExciseCode = "1CF";
		invoiceLine1.JI_Tariff = "2208905400";

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.ZG_ExciseCode = "1CF";
		invoiceLine2.ZG_GlobalWarmingPotential = 1;
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Tariff = "2208905401";

		var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine3.ZG_ExciseCode = "A00";
		invoiceLine3.JI_CEI = entryInstruction.PK;
		invoiceLine3.JI_Tariff = "2208905402";

		var isDeclarationUCC6 = declaration.IsUCC6;

		Factory.Save();
		declaration.DoMerge();
		var entryHeader = declaration.CustomsEntryHeaders[0];

		CombineAssertions(() =>
		{
			AssertEquals("1st Merge: There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
			AssertEntryLineTypeSupDocCount($"1st Merge: First entry has no supporting document for {docType} because PCA = 0", firstEntry, docType, 0);

			var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
			AssertEntryLineTypeSupDocCount($"1st Merge: Second entry has 1 supporting document for {docType}", secondEntry, docType, 1);
			AssertEntryLine7015SupDoc($"1st Merge: Second entry supporting document for {docType}", secondEntry, docType, 1m, isDeclarationUCC6, dataModel: Core.Constants.CountryCodes.Spain);

			var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");
			AssertEntryLineTypeSupDocCount($"1st Merge: Third entry has no supporting document for {docType} because PCA is not applicable", thirdEntry, docType, 0);

			firstEntry.AddEntryLineDocument<SupportingDocument>(docType, "50,00", quantity: 50m, unitOfQuantity: "NAR", subType: "LIQ", status: "ACC");
			firstEntry.AddEntryLineDocument<SupportingDocument>(docType, "50,00", quantity: 50m, unitOfQuantity: "NAR", subType: "LIQ");
			AssertEntryLineTypeSupDocCount($"First entry has 2 supporting documents for {docType} (1 accepted, 1 not accepted) before second merge", firstEntry, docType, 2);

			secondEntry.AddEntryLineDocument<SupportingDocument>(docType, "90,00", subType: "LIQ");

			Factory.Save();
			declaration.DoMerge();

			AssertEquals("2nd Merge: There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			AssertEntryLineTypeSupDocCount($"2nd Merge: First entry has 1 supporting document for {docType} (1 accepted)", firstEntry, docType, 1);

			AssertEntryLineTypeSupDocCount($"2nd Merge: Second entry has 1 supporting document for {docType}", secondEntry, docType, 1);
			AssertEntryLine7015SupDoc($"2nd Merge: Second entry supporting document for {docType}", secondEntry, docType, 1m, isDeclarationUCC6, dataModel: Core.Constants.CountryCodes.Spain);

			AssertEntryLineTypeSupDocCount($"2nd Merge: Third entry has no supporting document for {docType} because PCA is not applicable", thirdEntry, docType, 0);
		});
	}

	public void TestRemoveImport7015CLSupportingDocument()
	{
		SetUpTariffsWithFormulaPCA();
		var docType = "7015";

		declaration.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, "declaration"));
		declaration.SupportingDocuments.Add(Factory.CreateSupportingDocument("AAA", "declaration"));

		var (invoice, invoiceLine1, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(ZString.Empty);
		entryInstruction.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, "invoice"));
		entryInstruction.SupportingDocuments.Add(Factory.CreateSupportingDocument("BBB", "invoice"));

		invoiceLine1.ZG_ExciseCode = "1CF";
		invoiceLine1.JI_Tariff = "2208905400";
		invoiceLine1.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, "invoiceLine1"));
		invoiceLine1.SupportingDocuments.Add(Factory.CreateSupportingDocument("CCC", "invoiceLine1"));

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.ZG_ExciseCode = "1CF";
		invoiceLine2.ZG_GlobalWarmingPotential = 1;
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Tariff = "2208905401";
		invoiceLine2.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, "invoiceLine2"));

		var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine3.ZG_ExciseCode = "A00";
		invoiceLine3.JI_CEI = entryInstruction.PK;
		invoiceLine3.JI_Tariff = "2208905402";
		invoiceLine3.SupportingDocuments.Add(Factory.CreateSupportingDocument("EEE", "invoiceLine3"));

		var isDeclarationUCC6 = declaration.IsUCC6;

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Pre-requisite: Declaration has 2 supporting documents", 2, declaration.SupportingDocuments.Count);
			AssertEquals("Pre-requisite: EntryInstruction has 2 supporting documents", 2, entryInstruction.SupportingDocuments.Count);
			AssertEquals("Pre-requisite: InvoiceLine1 has 2 supporting documents", 2, invoiceLine1.SupportingDocuments.Count);
			AssertEquals("Pre-requisite: InvoiceLine2 has 1 supporting document", 1, invoiceLine2.SupportingDocuments.Count);
			AssertEquals("Pre-requisite: InvoiceLine3 has 1 supporting document", 1, invoiceLine3.SupportingDocuments.Count);

			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			AssertEquals($"1st Merge: Supporting document {docType} was removed from declaration, now it only has 1 sup doc", 1, declaration.SupportingDocuments.Count);
			AssertEquals($"1st Merge: Supporting document {docType} was NOT removed from entryInstruction, it stll has 2 sup doc", 2, entryInstruction.SupportingDocuments.Count);
			AssertEquals($"1st Merge: Supporting document {docType} was removed from invoiceLine1, now it only has 1 sup doc", 1, invoiceLine1.SupportingDocuments.Count);
			AssertEquals($"1st Merge: Supporting document {docType} was removed from invoiceLine2, now it has 0 sup doc", 0, invoiceLine2.SupportingDocuments.Count);
			AssertEquals($"1st Merge: Supporting document {docType} was not declared for invoiceLine3, it still has 1 sup doc", 1, invoiceLine3.SupportingDocuments.Count);

			AssertEquals("There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			var firstEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
			AssertEntryLineTypeSupDocCount($"1st Merge: First entry has no supporting document for {docType} because PCA = 0", firstEntry, docType, 0);

			var secondEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
			AssertEntryLineTypeSupDocCount($"1st Merge: Second entry has 1 supporting document for {docType}", secondEntry, docType, 1);
			AssertEntryLine7015SupDoc($"1st Merge: Second entry supporting document for {docType}", secondEntry, docType, 1m, isDeclarationUCC6, dataModel: Core.Constants.CountryCodes.Spain);

			var thirdEntry = entryHeader.MergedLines.FirstOrDefault(x => x.Tariff == "2208905402");
			AssertEntryLineTypeSupDocCount($"1st Merge: Third entry has no supporting document for {docType} because vat additions is 0", thirdEntry, docType, 0);

			firstEntry.AddEntryLineDocument<SupportingDocument>(docType, "10,00", quantity: 10m, unitOfQuantity: "NAR", subType: "LIQ", status: "ACC");
			firstEntry.AddEntryLineDocument<SupportingDocument>(docType, "10,00", quantity: 10m, unitOfQuantity: "NAR", subType: "LIQ");
			AssertEntryLineTypeSupDocCount($"First entry has 2 supporting documents for {docType} (1 accepted, 1 not accepted) before second merge", firstEntry, docType, 2);

			secondEntry.AddEntryLineDocument<SupportingDocument>(docType, "90,00", quantity: 90m, unitOfQuantity: "NAR", subType: "LIQ");
			AssertEntryLineTypeSupDocCount($"Second entry has 2 supporting documents for {docType} (2 not accepted) before second merge", secondEntry, docType, 2);

			thirdEntry.AddEntryLineDocument<SupportingDocument>(docType, "50,00", quantity: 50m, unitOfQuantity: "NAR", subType: "LIQ");
			AssertEntryLineTypeSupDocCount($"Second entry has 1 supporting documents for {docType} (1 not accepted) before second merge", thirdEntry, docType, 1);

			Factory.Save();
			declaration.DoMerge();

			AssertEquals("2nd Merge: There are 3 entry lines", 3, entryHeader.MergedLines.Count);

			AssertEntryLineTypeSupDocCount($"2nd Merge: First entry has 1 supporting document for {docType} (1 accepted)", firstEntry, docType, 1);

			AssertEntryLineTypeSupDocCount($"2nd Merge: Second entry has 1 supporting document for {docType}", secondEntry, docType, 1);
			AssertEntryLine7015SupDoc($"2nd Merge: Second entry supporting document for {docType}", secondEntry, docType, 1m, isDeclarationUCC6, dataModel: Core.Constants.CountryCodes.Spain);

			AssertEntryLineTypeSupDocCount($"2nd Merge: Third entry has no supporting document for {docType} because PCA is not applicable", thirdEntry, docType, 0);
		});
	}

	void TestCreateH1SSupportingDocument_NotH1OrNotApplicableStyle(string docType, params (string rateCode, string formula)[] duties)
	{
		const string tariffWithDuty = "2208905400";
		const string tariffWithoutDuty = "2208905410";

		SetUpImportTariffWithDuties(tariffWithDuty, duties);
		var (invoice, invoiceLine1, invoiceLine2, entryInstruction) = SetUpH1SImportDeclaration();

		invoiceLine1.JI_Tariff = tariffWithDuty;
		invoiceLine1.JI_LinePrice = 100;

		invoiceLine2.JI_Tariff = tariffWithoutDuty;
		invoiceLine2.JI_LinePrice = 200;

		Factory.Save();

		CombineAssertions(() =>
		{
			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(enabled: false))
			{
				var declarationDescription = "Non-H1 Simplified Import Declaration";
				entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
				AssertEquals($"Pre-requisite: {declarationDescription}", true, declaration.IsImport && !declaration.IsUCC6 && entryInstruction.CEI_Style == "IM" && entryInstruction.CEI_SubStyle == "B");

				AssertNoSupportingDocumentsAfterMerge(declarationDescription);
			}

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
			{
				var declarationDescription = "H1 Import Declaration SubStyle A";
				entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertEquals($"Pre-requisite: {declarationDescription}", true, declaration.IsImport && declaration.IsUCC6 && entryInstruction.CEI_Style == "IM" && entryInstruction.CEI_SubStyle == "A");

				AssertNoSupportingDocumentsAfterMerge(declarationDescription);

				declarationDescription = "H1 Import Declaration Style H2";
				entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
				AssertEquals($"Pre-requisite: {declarationDescription}", true, declaration.IsImport && declaration.IsUCC6 && entryInstruction.CEI_Style == "H2" && entryInstruction.CEI_SubStyle == "B");

				AssertNoSupportingDocumentsAfterMerge(declarationDescription);
			}
		});

		void AssertNoSupportingDocumentsAfterMerge(string declarationDescription)
		{
			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			AssertEquals($"There are 2 entry lines ({declarationDescription})", 2, entryHeader.MergedLines.Count);

			var firstEntry = entryHeader.MergedLines.Single(x => x.Tariff == tariffWithDuty);
			AssertNoEntryLineTypeSupDoc($"First entry has no supporting documents for {docType} ({declarationDescription})", firstEntry, docType);

			var secondEntry = entryHeader.MergedLines.Single(x => x.Tariff == tariffWithoutDuty);
			AssertNoEntryLineTypeSupDoc($"Second entry has no supporting documents for {docType} ({declarationDescription})", secondEntry, docType);
		}
	}

	public void TestCreateImport7018SupportingDocument_NotH1OrNotApplicableStyle()
	{
		TestCreateH1SSupportingDocument_NotH1OrNotApplicableStyle("7018", ("A00", "0.1 * VFD"), ("A01", "0.05 * VFD"));
	}

	public void TestCreateImportH1_7018SupportingDocument()
	{
		var docType = "7018";
		const string tariffWithDuty = "2208905400";
		const string tariffWithoutDuty = "2208905410";

		SetUpImportTariffWithDuties(tariffWithDuty, new[] { ("A00", "0.1 * VFD"), ("A01", "0.05 * VFD") } );
		var (invoice, invoiceLine1, invoiceLine2, entryInstruction) = SetUpH1SImportDeclaration();

		invoiceLine1.JI_Tariff = tariffWithDuty;
		invoiceLine1.JI_LinePrice = 100;

		invoiceLine2.JI_Tariff = tariffWithoutDuty;
		invoiceLine2.JI_LinePrice = 200;

		Factory.Save();

		CombineAssertions(() =>
		{
			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
			{
				AssertEquals("Pre-requisite: H1 Simplified Import Declaration", true, declaration.IsUCC6AndIsImport && entryInstruction.CEI_SubStyle == "B");

				declaration.DoMerge();
				var entryHeader = declaration.CustomsEntryHeaders[0];

				AssertEquals("There are 2 entry lines", 2, entryHeader.MergedLines.Count);

				var firstEntry = entryHeader.MergedLines.Single(x => x.Tariff == tariffWithDuty);
				AssertEntryLineH1SSupDoc($"First entry supporting document for {docType}", firstEntry, docType, "Amount to be guaranteed (AEAT)", 15m, "EUR", dataModel: Core.Constants.CountryCodes.Spain);

				var secondEntry = entryHeader.MergedLines.Single(x => x.Tariff == tariffWithoutDuty);
				AssertEntryLineH1SSupDoc($"Second entry supporting document for {docType} (no applicable fees)", secondEntry, docType, "Amount to be guaranteed (AEAT)", 0m, "EUR", dataModel: Core.Constants.CountryCodes.Spain);
			}
		});
	}

	public void TestUpdateImportH1_7018SupportingDocument()
	{
		var docType = "7018";

		const string tariffWithDuty = "2208905400";
		const string tariffWithoutDuty = "2208905410";

		SetUpImportTariffWithDuties(tariffWithDuty, new[] { ("A00", "0.1 * VFD"), ("A01", "0.05 * VFD") });
		var (invoice, invoiceLine1, invoiceLine2, entryInstruction) = SetUpH1SImportDeclaration();

		invoiceLine1.JI_Tariff = tariffWithDuty;
		invoiceLine1.JI_LinePrice = 100;

		invoiceLine2.JI_Tariff = tariffWithoutDuty;
		invoiceLine2.JI_LinePrice = 200;

		Factory.Save();

		CombineAssertions(() =>
		{
			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
			{
				AssertEquals("Pre-requisite: H1 Simplified Import Declaration", true, declaration.IsUCC6AndIsImport && entryInstruction.CEI_SubStyle == "B");

				declaration.DoMerge();
				var entryHeader = declaration.CustomsEntryHeaders[0];

				AssertEquals("There are 2 entry lines", 2, entryHeader.MergedLines.Count);

				var firstEntry = entryHeader.MergedLines.Single(x => x.Tariff == tariffWithDuty);
				AssertEntryLineH1SSupDoc($"First entry supporting document for {docType}", firstEntry, docType, "Amount to be guaranteed (AEAT)", 15m, "EUR", dataModel: Core.Constants.CountryCodes.Spain);

				var secondEntry = entryHeader.MergedLines.Single(x => x.Tariff == tariffWithoutDuty);
				AssertEntryLineH1SSupDoc($"Second entry supporting document for {docType} (no applicable fees)", secondEntry, docType, "Amount to be guaranteed (AEAT)", 0m, "EUR", dataModel: Core.Constants.CountryCodes.Spain);

				invoiceLine1.JI_LinePrice = 200;

				Factory.Save();
				declaration.DoMerge();

				AssertEntryLineH1SSupDoc($"2nd Merge: First entry supporting document for {docType}", firstEntry, docType, "Amount to be guaranteed (AEAT)", 30m, "EUR", dataModel: Core.Constants.CountryCodes.Spain);
				AssertEntryLineH1SSupDoc($"2nd Merge: Second entry supporting document for {docType} (no applicable fees)", secondEntry, docType, "Amount to be guaranteed (AEAT)", 0m, "EUR", dataModel: Core.Constants.CountryCodes.Spain);
			}
		});
	}

	public void TestRemoveImportH1_7018SupportingDocument()
	{
		var docType = "7018";

		const string tariffWithDuty = "2208905400";
		const string tariffWithoutDuty = "2208905410";

		SetUpImportTariffWithDuties(tariffWithDuty, new[] { ("A00", "0.1 * VFD"), ("A01", "0.05 * VFD") });
		var (invoice, invoiceLine1, invoiceLine2, entryInstruction) = SetUpH1SImportDeclaration();

		invoiceLine1.JI_Tariff = tariffWithDuty;
		invoiceLine1.JI_LinePrice = 100;

		invoiceLine2.JI_Tariff = tariffWithoutDuty;
		invoiceLine2.JI_LinePrice = 200;

		CombineAssertions(() =>
		{
			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(enabled: false))
			{
				AssertEquals("Pre-requisite: Non-H1 Simplified Import Declaration", false, IsH1SimplifiedImport());
				AssertNewH1SImportSupportingDocumentsRemovedAfterMerge(entryInstruction, invoice, invoiceLine1, invoiceLine2, docType, false);
			}

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
			{
				AssertEquals("Pre-requisite: H1 Simplified Import Declaration", true, IsH1SimplifiedImport());
				AssertNewH1SImportSupportingDocumentsRemovedAfterMerge(entryInstruction, invoice, invoiceLine1, invoiceLine2, docType, true);

				entryInstruction.CEI_SubStyle = "A";
				AssertEquals("Pre-requisite: H1 Import Declaration SubStyle A", false, IsH1SimplifiedImport());
				AssertNewH1SImportSupportingDocumentsRemovedAfterMerge(entryInstruction, invoice, invoiceLine1, invoiceLine2, docType, false);
			}
		});

		bool IsH1SimplifiedImport() => declaration.IsUCC6AndIsImport && entryInstruction.CEI_SubStyle == "B" || entryInstruction.CEI_SubStyle == "C";
	}

	public void TestCreateImport7019SupportingDocument_NotH1OrNotApplicableStyle()
	{
		TestCreateH1SSupportingDocument_NotH1OrNotApplicableStyle("7019", ("321", "0.1 * VFD"), ("432", "0.05 * VFD"));
	}

	public void TestCreateImportH1_7019SupportingDocument()
	{
		var docType = "7019";
		const string tariffWithDuty = "2208905400";
		const string tariffWithoutDuty = "2208905410";

		SetUpCanaryIslandsCusCodeLists(CanaryIslandsForTest.First());
		SetUpImportTariffWithDuties(tariffWithDuty, new[] { ("321", "0.1 * VFD"), ("432", "0.05 * VFD") });
		var (invoice, invoiceLine1, invoiceLine2, entryInstruction) = SetUpH1SImportDeclaration();

		invoiceLine1.JI_Tariff = tariffWithDuty;
		invoiceLine1.JI_LinePrice = 100;

		invoiceLine2.JI_Tariff = tariffWithoutDuty;
		invoiceLine2.JI_LinePrice = 200;

		declaration.JE_CustomsOffice = "ES009998";

		Factory.Save();

		CombineAssertions(() =>
		{
			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
			{
				AssertEquals("Pre-requisite: H1 Simplified Import Declaration", true, declaration.IsUCC6AndIsImport && entryInstruction.CEI_SubStyle == "B");

				declaration.DoMerge();
				var entryHeader = declaration.CustomsEntryHeaders[0];

				AssertEquals("There are 2 entry lines", 2, entryHeader.MergedLines.Count);

				var firstEntry = entryHeader.MergedLines.Single(x => x.Tariff == tariffWithDuty);
				AssertEntryLineH1SSupDoc($"First entry supporting document for {docType}", firstEntry, docType, "Amount to be guaranteed (ATC)", 15m, "EUR", dataModel: Core.Constants.CountryCodes.Spain);

				var secondEntry = entryHeader.MergedLines.Single(x => x.Tariff == tariffWithoutDuty);
				AssertEntryLineH1SSupDoc($"Second entry supporting document for {docType} (no applicable fees)", secondEntry, docType, "Amount to be guaranteed (ATC)", 0m, "EUR", dataModel: Core.Constants.CountryCodes.Spain);

				declaration.JE_CustomsOffice = "ES003712";
				Factory.Save();
				declaration.DoMerge();

				AssertNoEntryLineTypeSupDoc($"First entry has no supporting documents for {docType} (destination not in Canary Islands)", firstEntry, docType);
				AssertNoEntryLineTypeSupDoc($"Second entry has no supporting documents for {docType} (destination not in Canary Islands)", secondEntry, docType);
			}
		});
	}

	public void TestUpdateImportH1_7019SupportingDocument()
	{
		var docType = "7019";

		const string tariffWithDuty = "2208905400";
		const string tariffWithoutDuty = "2208905410";

		SetUpCanaryIslandsCusCodeLists(CanaryIslandsForTest.First());
		SetUpImportTariffWithDuties(tariffWithDuty, new[] { ("321", "0.1 * VFD"), ("432", "0.05 * VFD") });
		var (invoice, invoiceLine1, invoiceLine2, entryInstruction) = SetUpH1SImportDeclaration();

		invoiceLine1.JI_Tariff = tariffWithDuty;
		invoiceLine1.JI_LinePrice = 100;

		invoiceLine2.JI_Tariff = tariffWithoutDuty;
		invoiceLine2.JI_LinePrice = 200;

		declaration.JE_CustomsOffice = "ES003861";

		Factory.Save();

		CombineAssertions(() =>
		{
			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
			{
				AssertEquals("Pre-requisite: H1 Simplified Import Declaration", true, declaration.IsUCC6AndIsImport && entryInstruction.CEI_SubStyle == "B");

				declaration.DoMerge();
				var entryHeader = declaration.CustomsEntryHeaders[0];

				AssertEquals("There are 2 entry lines", 2, entryHeader.MergedLines.Count);

				var firstEntry = entryHeader.MergedLines.Single(x => x.Tariff == tariffWithDuty);
				AssertEntryLineH1SSupDoc($"First entry supporting document for {docType}", firstEntry, docType, "Amount to be guaranteed (ATC)", 15m, "EUR", dataModel: Core.Constants.CountryCodes.Spain);

				var secondEntry = entryHeader.MergedLines.Single(x => x.Tariff == tariffWithoutDuty);
				AssertEntryLineH1SSupDoc($"Second entry supporting document for {docType} (no applicable fees)", secondEntry, docType, "Amount to be guaranteed (ATC)", 0m, "EUR", dataModel: Core.Constants.CountryCodes.Spain);

				invoiceLine1.JI_LinePrice = 200;

				Factory.Save();
				declaration.DoMerge();

				AssertEntryLineH1SSupDoc($"2nd Merge: First entry supporting document for {docType}", firstEntry, docType, "Amount to be guaranteed (ATC)", 30m, "EUR", dataModel: Core.Constants.CountryCodes.Spain);
				AssertEntryLineH1SSupDoc($"2nd Merge: Second entry supporting document for {docType} (no applicable fees)", secondEntry, docType, "Amount to be guaranteed (ATC)", 0m, "EUR", dataModel: Core.Constants.CountryCodes.Spain);
			}
		});
	}

	public void TestRemoveImportH1_7019SupportingDocument()
	{
		var docType = "7019";
		const string tariffWithDuty = "2208905400";
		const string tariffWithoutDuty = "2208905410";

		SetUpCanaryIslandsCusCodeLists(CanaryIslandsForTest.First());
		SetUpImportTariffWithDuties(tariffWithDuty, new[] { ("A00", "0.1 * VFD"), ("A01", "0.05 * VFD") });
		var (invoice, invoiceLine1, invoiceLine2, entryInstruction) = SetUpH1SImportDeclaration();

		invoiceLine1.JI_Tariff = tariffWithDuty;
		invoiceLine1.JI_LinePrice = 100;

		invoiceLine2.JI_Tariff = tariffWithoutDuty;
		invoiceLine2.JI_LinePrice = 200;

		declaration.JE_CustomsOffice = "ES003861";

		CombineAssertions(() =>
		{
			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(enabled: false))
			{
				AssertEquals("Pre-requisite: Non-H1 Simplified Import Declaration", false, IsH1SimplifiedImport());
				AssertNewH1SImportSupportingDocumentsRemovedAfterMerge(entryInstruction, invoice, invoiceLine1, invoiceLine2, docType, false);
			}

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
			{
				AssertEquals("Pre-requisite: H1 Simplified Import Declaration", true, IsH1SimplifiedImport());
				AssertNewH1SImportSupportingDocumentsRemovedAfterMerge(entryInstruction, invoice, invoiceLine1, invoiceLine2, docType, true);

				entryInstruction.CEI_SubStyle = "A";
				AssertEquals("Pre-requisite: H1 Import Declaration SubStyle A", false, IsH1SimplifiedImport());
				AssertNewH1SImportSupportingDocumentsRemovedAfterMerge(entryInstruction, invoice, invoiceLine1, invoiceLine2, docType, false);
			}
		});

		bool IsH1SimplifiedImport() => declaration.IsUCC6AndIsImport && entryInstruction.CEI_SubStyle == "B" || entryInstruction.CEI_SubStyle == "C";
	}

	public void TestCreateImportH1_7018_7019_B00ChargeType()
	{
		var docType7018 = "7018";
		var docType7019 = "7019";

		const string tariffWithDuty = "2208905400";

		SetUpCanaryIslandsCusCodeLists(CanaryIslandsForTest.First());
		SetUpImportTariffWithDuties(tariffWithDuty, new[] { ("B00", "0.1 * VFD") });
		var (invoice, invoiceLine1, invoiceLine2, entryInstruction) = SetUpH1SImportDeclaration();

		invoiceLine1.JI_Tariff = tariffWithDuty;
		invoiceLine1.JI_LinePrice = 100;

		CombineAssertions(() =>
		{
			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
			{
				AssertEquals("Pre-requisite: H1 Simplified Import Declaration", true, declaration.IsUCC6AndIsImport && entryInstruction.CEI_SubStyle == "B");

				declaration.JE_CustomsOffice = "ES003541";
				Factory.Save();
				declaration.DoMerge();
				var entryHeader = declaration.CustomsEntryHeaders[0];

				AssertEquals("There are 2 entry lines", 2, entryHeader.MergedLines.Count);

				var firstEntry = entryHeader.MergedLines.Single(x => x.Tariff == tariffWithDuty);
				AssertEntryLineH1SSupDoc("B00 charge is not included in AEAT (destination is in Canary Islands)", firstEntry, docType7018, "Amount to be guaranteed (AEAT)", 0m, "EUR", dataModel: Core.Constants.CountryCodes.Spain);
				AssertEntryLineH1SSupDoc("B00 charge is included in ATC (destination is in Canary Islands)", firstEntry, docType7019, "Amount to be guaranteed (ATC)", 10m, "EUR", dataModel: Core.Constants.CountryCodes.Spain);

				declaration.JE_CustomsOffice = "ES003712";
				Factory.Save();
				declaration.DoMerge();

				AssertEntryLineH1SSupDoc("B00 charge is included in AEAT (destination is not in Canary Islands)", firstEntry, docType7018, "Amount to be guaranteed (AEAT)", 10m, "EUR", dataModel: Core.Constants.CountryCodes.Spain);
				AssertNoEntryLineTypeSupDoc($"No supporting documents for {docType7019} (destination is not in Canary Islands)", firstEntry, docType7019);
			}
		});
	}

	void AssertEntryLineTypeSupDocCount(ZString message, CusEntryLine entryLine, ZString docType, int expectedCount)
	{
		AssertEquals(message, expectedCount, entryLine.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == docType));
	}

	void AssertNoEntryLineTypeSupDoc(ZString message, CusEntryLine entryLine, ZString docType) =>
		AssertEntryLineTypeSupDocCount(message, entryLine, docType, 0);

	SupportingDocument AssertEntryLineTypeSupDoc(ZString message, CusEntryLine entryLine, ZString docType, ZString referenceNumber, decimal quantity = 0m, string qtyUnit = "", string subType = "LIQ", decimal amount = 0, string dataModel = "")
	{
		var supDocs = entryLine.GetPreviouslySentSupportingDocuments().Where(x => x.CSI_Code == docType && x.CSI_Status != "ACC").ToArray();
		AssertEquals(message + ", only one not accepted " + docType + " document in the entry line", 1, supDocs.Length);

		var document = supDocs[0];
		AssertEquals(message + ", CSI_Type", "SUP", document.CSI_Type);
		AssertEquals(message + ", CSI_SubType", subType, document.CSI_SubType);
		AssertEquals(message + ", CSI_Code", docType, document.CSI_Code);
		AssertEquals(message + ", CSI_ReferenceNumber", referenceNumber, document.CSI_ReferenceNumber);
		AssertEquals(message + ", CSI_Status", ZString.Empty, document.CSI_Status);
		AssertEquals(message + ", CSI_Quantity", quantity, document.CSI_Quantity);
		AssertEquals(message + ", CSI_UnitOfQuantity", qtyUnit, document.CSI_UnitOfQuantity);
		AssertEquals(message + ", CSI_DataModel", dataModel, document.CSI_DataModel);

		if (amount != ZDecimal.Zero)
		{
			AssertEquals(message + ", CSI_Value", amount, document.CSI_Value);
			AssertEquals(message + ", CSI_RX_NKCurrency", "EUR", document.CSI_RX_NKCurrency);
		}

		return document;
	}

	void AssertEntryLineH1SSupDoc(ZString message, CusEntryLine entryLine, ZString docType, ZString referenceNumber, decimal valueAmount, string valueCurrency, string dataModel = "")
	{
		var document = AssertEntryLineTypeSupDoc(message, entryLine, docType, referenceNumber, subType: "H1S", dataModel: dataModel);
		AssertEquals(message + ", CSI_Value", valueAmount, document.CSI_Value);
		AssertEquals(message + ", CSI_RX_NKCurrency", valueCurrency, document.CSI_RX_NKCurrency);
	}

	void AssertEntryLine7015SupDoc(ZString message, CusEntryLine entryLine, string docType, decimal pca, bool isUCC6Declaration, string dataModel = "")
	{
		var pcaStr = ((ZDecimal)pca).ToString(2, useCommas: true);
		var (expectedReference, expectedQuantity, expectedQuantityUnit) = isUCC6Declaration
			? (pcaStr, pca, "NAR")
			: (pcaStr, 0m, "");
		AssertEntryLineTypeSupDoc(message, entryLine, docType, expectedReference, expectedQuantity, expectedQuantityUnit, dataModel: dataModel);
	}

	void AssertNewH1SImportSupportingDocumentsRemovedAfterMerge(
		CusEntryInstruction entryInstruction,
		JobComInvoiceHeader invoice,
		JobComInvoiceLine invoiceLine1,
		JobComInvoiceLine invoiceLine2,
		string docType,
		bool documentsRequired)
	{
		foreach (CusEntryHeader x in declaration.CustomsEntryHeaders)
		{
			x.RemoveSupportingDocuments(docType);
		}

		declaration.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, "declaration"));
		entryInstruction.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, "entryInstruction"));
		invoice.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, "invoice"));
		invoiceLine1.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, "invoiceLine1"));
		invoiceLine2.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, "invoiceLine1"));

		Factory.Save();
		declaration.DoMerge();

		var messagePrefix = $"Supporting documents {docType} removed";
		AssertEquals($"{messagePrefix} from declaration ({declaration.TablePrefix})", 0, declaration.SupportingDocuments.Count);
		AssertEquals($"{messagePrefix} from entryInstruction ({entryInstruction.TablePrefix})", 0, entryInstruction.SupportingDocuments.Count);
		AssertEquals($"{messagePrefix} from invoice ({invoice.TablePrefix})", 0, invoice.SupportingDocuments.Count);
		AssertEquals($"{messagePrefix} from invoiceLine1 ({invoiceLine1.TablePrefix})", 0, invoiceLine1.SupportingDocuments.Count);
		AssertEquals($"{messagePrefix} from invoiceLine2 ({invoiceLine2.TablePrefix})", 0, invoiceLine2.SupportingDocuments.Count);

		var entryHeader = declaration.CustomsEntryHeaders[0];
		AssertEquals("There are 2 entry lines", 2, entryHeader.MergedLines.Count);

		var firstEntry = entryHeader.MergedLines.Single(x => x.Tariff == invoiceLine1.JI_Tariff);
		firstEntry.GetPreviouslySentSupportingDocuments().Where(x => x.CSI_Code == docType).ForEach(x => x.Delete());
		firstEntry.AddEntryLineDocument<SupportingDocument>(docType, $"Accepted {docType}", subType: "H1S", status: "ACC");
		firstEntry.AddEntryLineDocument<SupportingDocument>(docType, $"New {docType} 1", subType: "H1S");
		firstEntry.AddEntryLineDocument<SupportingDocument>(docType, $"New {docType} 2", subType: "H1S");
		AssertEntryLineTypeSupDocCount($"First entry has 3 supporting documents for {docType} (1 accepted, 2 new)", firstEntry, docType, 3);

		var secondEntry = entryHeader.MergedLines.Single(x => x.Tariff == invoiceLine2.JI_Tariff);
		secondEntry.GetPreviouslySentSupportingDocuments().Where(x => x.CSI_Code == docType).ForEach(x => x.Delete());
		secondEntry.AddEntryLineDocument<SupportingDocument>(docType, $"New {docType} 1", subType: "H1S");
		secondEntry.AddEntryLineDocument<SupportingDocument>(docType, $"New {docType} 2", subType: "H1S");
		AssertEntryLineTypeSupDocCount($"Second entry has 2 supporting documents for {docType}", secondEntry, docType, 2);

		Factory.Save();
		declaration.DoMerge();

		if (documentsRequired)
		{
			AssertEntryLineTypeSupDocCount($"First entry has 2 supporting documents for {docType} (1 accepted, 1 new)", firstEntry, docType, 2);
			AssertEntryLineTypeSupDocCount($"Second entry has 1 supporting document for {docType}", secondEntry, docType, 1);
		}
		else
		{
			AssertEntryLineTypeSupDocCount($"First entry has 1 supporting document for {docType} (1 accepted)", firstEntry, docType, 1);
			AssertEntryLineTypeSupDocCount($"Second entry has no supporting documents for {docType}", secondEntry, docType, 0);
		}
	}

	public void TestCalculateAidAmountREA()
	{
		SetREAData();

		var header = (JobComInvoiceHeader)TestJobDeclaration.Invoices.AddNew();
		var invoiceLine = header.InvoiceLines.AddNew();
		invoiceLine.JI_PrimaryPreference = "085";
		invoiceLine.ZG_REAProductCode = "T001";
		invoiceLine.JI_Tariff = "2208905400";
		invoiceLine.ZG_IsREADirectConsumption = true;
		invoiceLine.JI_CustomsQuantity = 20255.2;
		invoiceLine.JI_CustomsUnitQty = "KGM";

		CombineAssertions(() =>
		{
			var merger = GetNewLineMerger(TestJobDeclaration);
			merger.DoMerge();
			AssertEquals("No supporting document is added", 0, invoiceLine.SupportingDocuments.Count);
			AssertEquals("New charge added", 1, invoiceLine.Charges.Count);

			var charge = invoiceLine.Charges[0];
			AssertEquals("Charge amount", 1721.69m, charge.J7_Amount);
			AssertEquals("Charge type", "REA", charge.J7_ChargeType);

			invoiceLine.JI_CustomsQuantity = 0;
			merger = GetNewLineMerger(TestJobDeclaration);
			merger.DoMerge();

			AssertEquals("No supporting document is added when customs quantity is 0", 0, invoiceLine.SupportingDocuments.Count);
			AssertEquals("No charge is added when customs quantity is 0", 0, invoiceLine.Charges.Count);

			invoiceLine.JI_CustomsQuantity = 20255.2;
			merger = GetNewLineMerger(TestJobDeclaration);
			merger.DoMerge();

			AssertEquals("No supporting document is added when customs quantity is not 0", 0, invoiceLine.SupportingDocuments.Count);
			AssertEquals("New charge added when customs quantity is not 0", 1, invoiceLine.Charges.Count);

			invoiceLine.JI_PrimaryPreference = "XX";
			merger = GetNewLineMerger(TestJobDeclaration);
			merger.DoMerge();

			AssertEquals("No supporting document is added when primary preference is not correct", 0, invoiceLine.SupportingDocuments.Count);
			AssertEquals("No charge is added when primary preference is not correct", 0, invoiceLine.Charges.Count);
		});
	}

	public override void TestLineMergerCreateOneEntryHeader()
	{
		TestJobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		var header1 = TestJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		header1.JZ_OH_Supplier = GetValidSupplier();
		header1.JZ_IncoTerm = "FOB";
		var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
		header1.JZ_RX_NKInvoice_Currency = audCurrency.RX_Code;
		header1.JobComInvoiceLines.AddNew();
		header1.JobComInvoiceLines.AddNew();
		header1.JobComInvoiceLines.AddNew();
		SetDefaultValuesToInvoiceLines(header1);

		var header2 = TestJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		header2.JZ_OH_Supplier = GetValidSupplier();
		header2.JZ_IncoTerm = "CIF";
		header2.JZ_RX_NKInvoice_Currency = "NZD";
		header2.JobComInvoiceLines.AddNew();
		header2.JobComInvoiceLines.AddNew();
		header2.JobComInvoiceLines.AddNew();
		SetDefaultValuesToInvoiceLines(header2);

		var merger = GetNewLineMerger(TestJobDeclaration);
		merger.DoMerge();
		AssertEquals("One cus entry header", 2, TestJobDeclaration.CustomsEntryHeaders.Count);
		AssertEquals("Six cus entry lines", 3, TestJobDeclaration.CustomsEntryHeaders[0].MergedLines.Count);
	}

	public void TestDefaultMethodOfPaymentWhenImporterIsVatDeferred()
	{
		var dtyTariffCode = "222";
		DutyReferenceDataConfigurationBuilder.New(Factory)
			.AddTariffType("IMP")
			.AddTariff(dtyTariffCode)
			.AddRateCode(RateTypeEnum.Duty, "A00", "0.12 * [KGM] + 0.2 * VFD", preference: "")
			.AddRateCode(RateTypeEnum.Duty, "B00", "0.23 * [VFD]", preference: "")
			.Configure();
		Factory.Save();
		var (invoice1, invLine1, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(ZString.Empty);
		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.JI_Tariff = dtyTariffCode;

		var invoice2 = declaration.Invoices.AddNew();
		invoice2.JZ_RX_NKInvoice_Currency = "EUR";
		var invLine2 = invoice2.InvoiceLines.AddNew();
		invLine2.JI_CEI = entryInstruction.PK;
		invLine2.JI_Procedure = "A";
		invLine2.JI_Tariff = dtyTariffCode;

		declaration.DoMerge();
		AssertEquals("[PRE-CONDITION] Entry Headers", 1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders[0];
		AssertEquals("[PRE-CONDITION] Entry Lines", 1, entryHeader.MergedLines.Count);

		var importer = Factory.New<OrgHeader>();
		var addInfoCusImp = ESOrgImpAddInfo.Get(importer);
		declaration.JE_OH_Importer = importer.PK;
		addInfoCusImp.ZO_VATDeferment = false;

		var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().Single();
		var fees = entryLine.Fees.OfType<CusEntryLineFee>();

		declaration.DoMerge();
		var vatFees = fees.Where(x => x.CF_ChargeType == "B00");
		AssertEquals("Importer Not Vat Deferred, VAT type - MoP Should be Empty", true, vatFees.All(x => x.CF_MethodOfPayment == ""));

		addInfoCusImp.ZO_VATDeferment = true;
		declaration.DoMerge();
		vatFees = fees.Where(x => x.CF_ChargeType == "B00");
		AssertEquals("Importer Vat Deferred, VAT Type - MoP Should be DEF", true, vatFees.All(x => x.CF_MethodOfPayment == "DEF"));

		var dutyFees = fees.Where(x => x.CF_ChargeType == "A00");
		AssertEquals("Not VAT type - MoP Should be Empty", true, dutyFees.All(x => x.CF_MethodOfPayment == ""));
	}

	public void TestOnMerged_PDI()
	{
		var errorText = "Cannot remove all Entry lines from this entry";

		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		var (invoice, invoiceLine, instruction) = SetUpInvHeaderInvLineAndEntryInstruction(ZString.Empty);
		invoiceLine.JI_Tariff = "100101";

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = instruction.PK;
		invoiceLine2.JI_Tariff = "100102";

		CombineAssertions(() =>
		{
			new LineMerger(declaration).DoMerge();
			AssertNoRowErrorContaining(instruction, errorText);

			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			new LineMerger(declaration).DoMerge();
			AssertNoRowErrorContaining(instruction, errorText);

			invoiceLine.Delete();
			new LineMerger(declaration).DoMerge();
			AssertNoRowErrorContaining(instruction, errorText);

			invoiceLine2.Delete();
			new LineMerger(declaration).DoMerge();
			AssertHasRowErrorContaining(instruction, errorText);
		});
	}

	public void TestUOMAllocationAfterDutyCalculation_NoUnitToAllocate()
	{
		var dtyTariffCode = "11112222";
		SetRateDataForFees(dtyTariffCode);

		declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
		var (invoice, invLine, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(ZString.Empty);
		invoice.JZ_RX_NKInvoice_Currency = "EUR";
		invLine.JI_Procedure = "A";
		invLine.JI_Tariff = dtyTariffCode;
		invLine.JI_CustomsQuantity = 20;
		invLine.JI_CustomsUnitQty = "TNE";
		invLine.JI_LinePrice = 50;
		invLine.ZG_ExciseCode = "0A7";

		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
			AssertEquals("Entry Header count", 1, entryHeaders.Count());
			var entryHeader = entryHeaders.Single();
			AssertEquals("Entry Line count", 1, entryHeader.MergedLines.Count);
			var entryLine = entryHeader.MergedLines.Single();
			var fees = entryLine.Fees.OfType<CusEntryLineFee>();

			AssertEquals("There is 1 fee", 1, fees.Count());
			AssertEquals("0A7 fee has PVP as Method of Calculation", "PVP", fees.First(x => x.CF_ChargeType == "0A7").CF_MethodOfCalculation);
			AssertEquals("ThirdQtyUnit is not set to the fee's method of calculation when it is PVP", ZString.Empty, invLine.JI_CustomsThirdUnitQty);
			AssertEquals("FourthQtyUnit is not set to the fee's method of calculation when it is PVP", ZString.Empty, invLine.JI_CustomsFourthUnitQty);

			invLine.ZG_ExciseCode = "0A1";
			declaration.DoMerge();
			fees = entryLine.Fees.OfType<CusEntryLineFee>();

			AssertEquals("There is 1 fee", 1, fees.Count());
			AssertEquals("0A1 fee has PVP as Method of Calculation", "%", fees.First(x => x.CF_ChargeType == "0A1").CF_MethodOfCalculation);
			AssertEquals("ThirdQtyUnit is not set to the fee's method of calculation when it is %", ZString.Empty, invLine.JI_CustomsThirdUnitQty);
			AssertEquals("FourthQtyUnit is not set to the fee's method of calculation when it is %", ZString.Empty, invLine.JI_CustomsFourthUnitQty);
		});
	}

	public void TestUOMAllocationAfterDutyCalculation_OneInvoiceLine()
	{
		var dtyTariffCode = "11112222";
		SetRateDataForFees(dtyTariffCode);

		declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
		var (invoice, invLine, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(ZString.Empty);
		invoice.JZ_RX_NKInvoice_Currency = "EUR";
		invLine.JI_Procedure = "A";
		invLine.JI_Tariff = dtyTariffCode;
		invLine.JI_CustomsQuantity = 20;
		invLine.JI_CustomsUnitQty = "TNE";
		invLine.JI_LinePrice = 50;
		invLine.ZG_ExciseCode = "0A0";

		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
			AssertEquals("Entry Header count", 1, entryHeaders.Count());
			var entryHeader = entryHeaders.Single();
			AssertEquals("Entry Line count", 1, entryHeader.MergedLines.Count);
			var entryLine = entryHeader.MergedLines.Single();
			var fees = entryLine.Fees.OfType<CusEntryLineFee>();

			AssertEquals("There are 2 fees", 2, fees.Count());
			AssertEquals("0A0 fee has HG as Method of Calculation", "HG", fees.First(x => x.CF_ChargeType == "0A0").CF_MethodOfCalculation);
			AssertEquals("ThirdQtyUnit is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine.JI_CustomsThirdUnitQty);

			AssertEquals("5A0 fee has ASVX as Method of Calculation", "ASVX", fees.First(x => x.CF_ChargeType == "5A0").CF_MethodOfCalculation);
			AssertEquals("FourthQtyUnit is set to the second fee's method of calculation when that unit or it's conversion is not present in any qty unit", "ASVX", invLine.JI_CustomsFourthUnitQty);

			invLine.JI_CustomsUnitQty = "LPA";
			invLine.JI_CustomsThirdUnitQty = ZString.Empty;
			invLine.JI_CustomsFourthUnitQty = ZString.Empty;
			declaration.DoMerge();
			fees = entryLine.Fees.OfType<CusEntryLineFee>();

			AssertEquals("There are 2 fees", 2, fees.Count());
			AssertEquals("0A0 fee has HG as Method of Calculation", "HG", fees.First(x => x.CF_ChargeType == "0A0").CF_MethodOfCalculation);
			AssertEquals("ThirdQtyUnit is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine.JI_CustomsThirdUnitQty);

			AssertEquals("5A0 fee has ASVX as Method of Calculation", "ASVX", fees.First(x => x.CF_ChargeType == "5A0").CF_MethodOfCalculation);
			AssertEquals("FourthQtyUnit is not set to the second fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine.JI_CustomsFourthUnitQty);

			invLine.JI_CustomsUnitQty = "LPA";
			invLine.JI_CustomsSecondUnitQty = "HG";
			invLine.JI_CustomsThirdUnitQty = ZString.Empty;
			invLine.JI_CustomsFourthUnitQty = ZString.Empty;
			declaration.DoMerge();
			fees = entryLine.Fees.OfType<CusEntryLineFee>();

			AssertEquals("There are 2 fees", 2, fees.Count());
			AssertEquals("0A0 fee has HG as Method of Calculation", "HG", fees.First(x => x.CF_ChargeType == "0A0").CF_MethodOfCalculation);
			AssertEquals("ThirdQtyUnit is not set to the first fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine.JI_CustomsThirdUnitQty);

			AssertEquals("5A0 fee has ASVX as Method of Calculation", "ASVX", fees.First(x => x.CF_ChargeType == "5A0").CF_MethodOfCalculation);
			AssertEquals("FourthQtyUnit is not set to the second fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine.JI_CustomsFourthUnitQty);

			invLine.JI_CustomsUnitQty = "TNE";
			invLine.JI_CustomsSecondUnitQty = ZString.Empty;
			invLine.JI_CustomsThirdUnitQty = "TNE";
			invLine.JI_CustomsFourthUnitQty = ZString.Empty;
			declaration.DoMerge();
			fees = entryLine.Fees.OfType<CusEntryLineFee>();

			AssertEquals("There are 2 fees", 2, fees.Count());
			AssertEquals("0A0 fee has HG as Method of Calculation", "HG", fees.First(x => x.CF_ChargeType == "0A0").CF_MethodOfCalculation);
			AssertEquals("ThirdQtyUnit is filled so it is not set to the first fee's method of calculation", "TNE", invLine.JI_CustomsThirdUnitQty);
			AssertEquals("FourthQtyUnit is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine.JI_CustomsFourthUnitQty);

			AssertEquals("5A0 fee has ASVX as Method of Calculation", "ASVX", fees.First(x => x.CF_ChargeType == "5A0").CF_MethodOfCalculation);
		});
	}

	public void TestUOMAllocationAfterDutyCalculation_MultipleInvoiceLines()
	{
		var dtyTariffCode = "11112222";
		SetRateDataForFees(dtyTariffCode);

		declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
		var (invoice, invLine1, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(ZString.Empty);
		invoice.JZ_RX_NKInvoice_Currency = "EUR";
		invLine1.JI_Procedure = "A";
		invLine1.JI_Tariff = dtyTariffCode;
		invLine1.JI_CustomsQuantity = 20;
		invLine1.JI_CustomsUnitQty = "TNE";
		invLine1.JI_LinePrice = 50;
		invLine1.ZG_ExciseCode = "0A0";

		var invLine2 = invoice.InvoiceLines.AddNew();
		invLine2.JI_CEI = entryInstruction.PK;
		invLine2.JI_Procedure = "A";
		invLine2.JI_Tariff = dtyTariffCode;
		invLine2.JI_CustomsQuantity = 30;
		invLine2.JI_CustomsUnitQty = "TNE";
		invLine2.JI_LinePrice = 50;
		invLine2.ZG_ExciseCode = "0A0";

		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
			AssertEquals("Entry Header count", 1, entryHeaders.Count());
			var entryHeader = entryHeaders.Single();
			AssertEquals("Entry Line count", 1, entryHeader.MergedLines.Count);
			var entryLine = entryHeader.MergedLines.Single();
			var fees = entryLine.Fees.Cast<CusEntryLineFee>();

			AssertEquals("There are 2 fees", 2, fees.Count());
			AssertEquals("0A0 fee has HG as Method of Calculation", "HG", fees.First(x => x.CF_ChargeType == "0A0").CF_MethodOfCalculation);
			AssertEquals("ThirdQtyUnit in first invoice line is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine1.JI_CustomsThirdUnitQty);
			AssertEquals("ThirdQtyUnit in second invoice line is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine2.JI_CustomsThirdUnitQty);

			AssertEquals("5A0 fee has ASVX as Method of Calculation", "ASVX", fees.First(x => x.CF_ChargeType == "5A0").CF_MethodOfCalculation);
			AssertEquals("FourthQtyUnit in first invoice line is set to the second fee's method of calculation when that unit or it's conversion is not present in any qty unit", "ASVX", invLine1.JI_CustomsFourthUnitQty);
			AssertEquals("FourthQtyUnit in second invoice line is set to the second fee's method of calculation when that unit or it's conversion is not present in any qty unit", "ASVX", invLine2.JI_CustomsFourthUnitQty);

			invLine1.JI_CustomsUnitQty = "LPA";
			invLine1.JI_CustomsThirdUnitQty = ZString.Empty;
			invLine1.JI_CustomsFourthUnitQty = ZString.Empty;

			invLine2.JI_CustomsUnitQty = "LPA";
			invLine2.JI_CustomsThirdUnitQty = ZString.Empty;
			invLine2.JI_CustomsFourthUnitQty = ZString.Empty;
			declaration.DoMerge();
			fees = entryLine.Fees.Cast<CusEntryLineFee>();

			AssertEquals("There are 2 fees", 2, fees.Count());
			AssertEquals("0A0 fee has HG as Method of Calculation", "HG", fees.First(x => x.CF_ChargeType == "0A0").CF_MethodOfCalculation);
			AssertEquals("ThirdQtyUnit in first invoice is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine1.JI_CustomsThirdUnitQty);
			AssertEquals("ThirdQtyUnit in second invoice is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine2.JI_CustomsThirdUnitQty);

			AssertEquals("5A0 fee has ASVX as Method of Calculation", "ASVX", fees.First(x => x.CF_ChargeType == "5A0").CF_MethodOfCalculation);
			AssertEquals("FourthQtyUnit in first invoice is not set to the second fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine1.JI_CustomsFourthUnitQty);
			AssertEquals("FourthQtyUnit in second invoice is not set to the second fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine2.JI_CustomsFourthUnitQty);

			invLine1.JI_CustomsUnitQty = "LPA";
			invLine1.JI_CustomsSecondUnitQty = "HG";
			invLine1.JI_CustomsThirdUnitQty = ZString.Empty;
			invLine1.JI_CustomsFourthUnitQty = ZString.Empty;

			invLine2.JI_CustomsUnitQty = "LPA";
			invLine2.JI_CustomsSecondUnitQty = "HG";
			invLine2.JI_CustomsThirdUnitQty = ZString.Empty;
			invLine2.JI_CustomsFourthUnitQty = ZString.Empty;
			declaration.DoMerge();
			fees = entryLine.Fees.OfType<CusEntryLineFee>();

			AssertEquals("There are 2 fees", 2, fees.Count());
			AssertEquals("0A0 fee has HG as Method of Calculation", "HG", fees.First(x => x.CF_ChargeType == "0A0").CF_MethodOfCalculation);
			AssertEquals("ThirdQtyUnit in first invoice is not set to the first fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine1.JI_CustomsThirdUnitQty);
			AssertEquals("ThirdQtyUnit in second invoice is not set to the first fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine2.JI_CustomsThirdUnitQty);

			AssertEquals("5A0 fee has ASVX as Method of Calculation", "ASVX", fees.First(x => x.CF_ChargeType == "5A0").CF_MethodOfCalculation);
			AssertEquals("FourthQtyUnit in first invoice is not set to the second fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine1.JI_CustomsFourthUnitQty);
			AssertEquals("FourthQtyUnit in second invoice is not set to the second fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine2.JI_CustomsFourthUnitQty);

			invLine1.JI_CustomsUnitQty = "TNE";
			invLine1.JI_CustomsSecondUnitQty = ZString.Empty;
			invLine1.JI_CustomsThirdUnitQty = "TNE";
			invLine1.JI_CustomsFourthUnitQty = ZString.Empty;

			invLine2.JI_CustomsUnitQty = "TNE";
			invLine2.JI_CustomsSecondUnitQty = ZString.Empty;
			invLine2.JI_CustomsThirdUnitQty = "TNE";
			invLine2.JI_CustomsFourthUnitQty = ZString.Empty;
			declaration.DoMerge();
			fees = entryLine.Fees.OfType<CusEntryLineFee>();

			AssertEquals("There are 2 fees", 2, fees.Count());
			AssertEquals("0A0 fee has HG as Method of Calculation", "HG", fees.First(x => x.CF_ChargeType == "0A0").CF_MethodOfCalculation);
			AssertEquals("ThirdQtyUnit in first invoice is filled so it is not set to the first fee's method of calculation", "TNE", invLine1.JI_CustomsThirdUnitQty);
			AssertEquals("ThirdQtyUnit in second invoice is filled so it is not set to the first fee's method of calculation", "TNE", invLine2.JI_CustomsThirdUnitQty);
			AssertEquals("FourthQtyUnit in first invoice line is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine1.JI_CustomsFourthUnitQty);
			AssertEquals("FourthQtyUnit in second invoice line is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine2.JI_CustomsFourthUnitQty);

			AssertEquals("5A0 fee has ASVX as Method of Calculation", "ASVX", fees.First(x => x.CF_ChargeType == "5A0").CF_MethodOfCalculation);
		});
	}

	public void TestOnMergingResetReadOnlyPreviousDocuments()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

		var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection;
		var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection;
		var attributeNameValuePairs = new Dictionary<string, string[]> { { "Level", new string[] { "ITEM" } } };
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
													   new string[] { importCodeType, exportCodeType },
													   "1234",
													   "1234",
													   attributeNameValuePairs,
													   ZDateTime.MinSmallDateTimeValue,
													   ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;

		CombineAssertions(() =>
		{
			AssertEquals("ReadOnly Previous Documents count", 0, entryLine.ReadOnlyPreviousDocuments.Count);

			invoiceLine1.PreviousDocuments.Add(GetPreviousDoc(1, "REF111", "Y"));
			invoiceLine1.PreviousDocuments.Add(GetPreviousDoc(2, "REF222", "Z"));

			AssertEquals("ReadOnly Previous Documents count", 0, entryLine.ReadOnlyPreviousDocuments.Count);
			declaration.DoMerge();

			AssertEquals("ReadOnly Previous Documents count", 2, entryLine.ReadOnlyPreviousDocuments.Count);
		});
	}

	public void TestOnMergingResetReadOnlyAdditionalInfos()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;

		CombineAssertions(() =>
		{
			AssertEquals("ReadOnly Additional Infos count", 0, entryLine.ReadOnlyAdditionalInfos.Count);

			invoiceLine1.AdditionalInfos.Add(GetAdditionalInfo("REF111", "TRA"));
			invoiceLine1.AdditionalInfos.Add(GetAdditionalInfo("REF222", "INF"));

			AssertEquals("ReadOnly Additional Infos count", 0, entryLine.ReadOnlyAdditionalInfos.Count);
			declaration.DoMerge();

			AssertEquals("ReadOnly Additional Infos count", 2, entryLine.ReadOnlyAdditionalInfos.Count);
		});
	}

	void SetRateDataForFees(ZString dtyTariffCode)
	{
		var countryCode = Core.Constants.CountryCodes.Spain;

		var helper = SetUpCanaryIslandsCusCodeLists(CanaryIslandsForTest.First());

		var impTariffType = helper.CreateTariffType(countryCode, "IMP");
		var canexcTariffType = helper.CreateTariffType(countryCode, "CANEX");
		var rateType = helper.CreateCusRateType(countryCode, "EXC");
		Factory.Save();
		var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, dtyTariffCode, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		CreateNewFormula(helper, canexcTariffType.PK, "0A0", "750.36*[HG]", rateType.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		CreateNewFormula(helper, canexcTariffType.PK, "5A0", "24*[ASVX]", rateType.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		CreateNewFormula(helper, canexcTariffType.PK, "0A7", "0.2*PVP", rateType.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		CreateNewFormula(helper, canexcTariffType.PK, "0A1", "0.5*VFD", rateType.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		Factory.Save();
	}

	void CreateNewFormula(UniversalReferenceTestDataHelper helper, ZGuid tariffTypePK, string rateCode, string formula, ZGuid rateTypePK, ZGuid impTariffTypePK, ZString tariffCode)
	{
		var tariffExcise = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Spain, tariffTypePK, rateCode, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc2");
		var rate = helper.LoadOrCreateNewCusRateCode(Factory, rateCode, rateTypePK);
		helper.CreateRate(tariffExcise, rate.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: formula);

		helper.CreateTariffRelationship(tariffExcise.PK, impTariffTypePK, tariffCode);
	}

	public void TestSetUCC6VersionExport()
	{
		var (_, invoiceLine, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("When declaraton is import UCC6Version is not changed", 0, entryHeader.ZG_UCC6Version);

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.DoMerge();
				AssertEquals("When declaraton is export and entry instruction is (A, B, C, X, Y or Z) and IsUCC6 flag is true (messageVersion is AES), UCC6Version is changed to 1", 1, entryHeader.ZG_UCC6Version);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				declaration.DoMerge();
				AssertEquals("When declaraton is export and IsUCC6 flag is true (messageVersion is AES) but entry instruction is not (A, B, C, X, Y or Z), UCC6Version is changed to 0", 0, entryHeader.ZG_UCC6Version);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
				entryHeader.CH_Status = "REJ";
				declaration.DoMerge();
				AssertEquals("When declaraton is export and entry instruction is (A, B, C, X, Y or Z) and IsUCC6 flag is true (messageVersion is AES) and entry status is empty and message status is not AWR, UCC6Version is changed to 1", 1, entryHeader.ZG_UCC6Version);

				entryHeader.ZG_UCC6Version = 4;
				entryHeader.CH_EntryStatus = "CLR";
				declaration.DoMerge();
				AssertEquals("When declaraton is export and entry instruction is (A, B, C, X, Y or Z) and IsUCC6 flag is true (messageVersion is AES) but entry status is not empty, UCC6Version is not changed", 4, entryHeader.ZG_UCC6Version);

				entryHeader.ZG_UCC6Version = 3;
				entryHeader.CH_EntryStatus = ZString.Empty;
				entryHeader.CH_Status = "AWR";
				declaration.DoMerge();
				AssertEquals("When declaraton is export and entry instruction is (A, B, C, X, Y or Z) and IsUCC6 flag is true (messageVersion is AES) and entry status is empty but message status is AWR, UCC6Version is not changed", 3, entryHeader.ZG_UCC6Version);
			}

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				entryHeader.ZG_UCC6Version = 0;
				entryHeader.CH_Status = ZString.Empty;
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.DoMerge();
				AssertEquals("When declaraton is export and entry instruction is (A, B, C, X, Y or Z) and IsUCC6 flag is true (messageVersion is AES1.1), UCC6Version is changed to 1", 1, entryHeader.ZG_UCC6Version);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				declaration.DoMerge();
				AssertEquals("When declaraton is export and IsUCC6 flag is true (messageVersion is AES1.1) but entry instruction is not (A, B, C, X, Y or Z), UCC6Version is changed to 0", 0, entryHeader.ZG_UCC6Version);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
				entryHeader.CH_Status = "REJ";
				declaration.DoMerge();
				AssertEquals("When declaraton is export and entry instruction is (A, B, C, X, Y or Z) and IsUCC6 flag is true (messageVersion is AES1.1) and entry status is empty and message status is not AWR, UCC6Version is changed to 1", 1, entryHeader.ZG_UCC6Version);

				entryHeader.ZG_UCC6Version = 4;
				entryHeader.CH_EntryStatus = "CLR";
				declaration.DoMerge();
				AssertEquals("When declaraton is export and entry instruction is (A, B, C, X, Y or Z) and IsUCC6 flag is true (messageVersion is AES1.1) but entry status is not empty, UCC6Version is not changed", 4, entryHeader.ZG_UCC6Version);

				entryHeader.ZG_UCC6Version = 3;
				entryHeader.CH_EntryStatus = ZString.Empty;
				entryHeader.CH_Status = "AWR";
				declaration.DoMerge();
				AssertEquals("When declaraton is export and entry instruction is (A, B, C, X, Y or Z) and IsUCC6 flag is true (messageVersion is AES1.1) and entry status is empty but message status is AWR, UCC6Version is not changed", 3, entryHeader.ZG_UCC6Version);
			}
		});
	}

	public void TestSetUCC6VersionImport()
	{
		var (_, invoiceLine, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A, style: IMPDeclarationTypeList.Codes.H2);
		Factory.Save();

		CombineAssertions(() =>
		{
			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("When declaraton is import UCC6Version (messageVersion is ICS)", 0, entryHeader.ZG_UCC6Version);

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
			{
				declaration.DoMerge();
				AssertEquals("When declaraton is import and entry instruction is A and IsUCC6 flag is true (FUNCS ImportMessageVersionUCC6 enabled) and Style H2, UCC6Version is not changed", 0, entryHeader.ZG_UCC6Version);

				entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;
				declaration.DoMerge();
				AssertEquals("When declaraton is import and entry instruction is A and IsUCC6 flag is true (FUNCS ImportMessageVersionUCC6 enabled) and Style IM, UCC6Version is changed to 1", 1, entryHeader.ZG_UCC6Version);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				declaration.DoMerge();
				AssertEquals("When declaraton is import and entry instruction is T2l and IsUCC6 flag is true (FUNCS ImportMessageVersionUCC6 enabled) and Style IM, UCC6Version is changed to 0", 0, entryHeader.ZG_UCC6Version);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
				entryHeader.CH_Status = "REJ";
				declaration.DoMerge();
				AssertEquals("When declaraton is import and entry instruction is B and IsUCC6 flag is true (FUNCS ImportMessageVersionUCC6 enabled) and entry status is empty and message status is not AWR, UCC6Version is changed", 1, entryHeader.ZG_UCC6Version);

				entryHeader.ZG_UCC6Version = 4;
				entryHeader.CH_EntryStatus = "CLR";
				declaration.DoMerge();
				AssertEquals("When declaraton is import and entry instruction is B and IsUCC6 flag is true (FUNCS ImportMessageVersionUCC6 enabled) but entry status is not empty, UCC6Version is not changed", 4, entryHeader.ZG_UCC6Version);

				entryHeader.ZG_UCC6Version = 3;
				entryHeader.CH_EntryStatus = ZString.Empty;
				entryHeader.CH_Status = "AWR";
				declaration.DoMerge();
				AssertEquals("When declaraton is import and entry instruction is B and IsUCC6 flag is true (FUNCS ImportMessageVersionUCC6 enabled) and entry status is empty but message status is AWR, UCC6Version is not changed", 3, entryHeader.ZG_UCC6Version);
			}

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
			{
				entryHeader.CH_Status = ZString.Empty;
				declaration.DoMerge();
				AssertEquals("When declaraton is import and entry instruction is A and IsUCC6 flag is false (FUNCS ImportMessageVersionUCC6 disabled) and Style IM, UCC6Version is changed", 0, entryHeader.ZG_UCC6Version);

				entryHeader.ZG_UCC6Version = 3;
				entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
				declaration.DoMerge();
				AssertEquals("When declaraton is import and entry instruction is A and IsUCC6 flag is false (FUNCS ImportMessageVersionUCC6 disabled) and Style H2, UCC6Version is not changed", 3, entryHeader.ZG_UCC6Version);

				entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;
				declaration.DoMerge();
				AssertEquals("When declaraton is import and entry instruction is T2l and IsUCC6 flag is false (FUNCS ImportMessageVersionUCC6 disabled) and Style IM, UCC6Version is changed", 0, entryHeader.ZG_UCC6Version);

				entryHeader.ZG_UCC6Version = 2;
				entryHeader.CH_Status = "REJ";
				declaration.DoMerge();
				AssertEquals("When declaraton is import and entry instruction is B and IsUCC6 flag is false (FUNCS ImportMessageVersionUCC6 disabled) and entry status is empty and message status is not AWR, UCC6Version is changed", 0, entryHeader.ZG_UCC6Version);

				entryHeader.ZG_UCC6Version = 4;
				entryHeader.CH_EntryStatus = "CLR";
				declaration.DoMerge();
				AssertEquals("When declaraton is import and entry instruction is B and IsUCC6 flag is false (FUNCS ImportMessageVersionUCC6 disabled) but entry status is not empty, UCC6Version is not changed", 4, entryHeader.ZG_UCC6Version);

				entryHeader.ZG_UCC6Version = 3;
				entryHeader.CH_EntryStatus = ZString.Empty;
				entryHeader.CH_Status = "AWR";
				declaration.DoMerge();
				AssertEquals("When declaraton is import and entry instruction is B and IsUCC6 flag is false (FUNCS ImportMessageVersionUCC6 disabled) and entry status is empty but message status is AWR, UCC6Version is not changed", 3, entryHeader.ZG_UCC6Version);
			}
		});
	}

	public void TestSetPOUSVersion()
	{
		var (_, invoiceLine, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(ZString.Empty);

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.ProofOfUnionStatus))
			{
				entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				declaration.DoMerge();
				AssertEquals("When messageVersion is Pous and entry instruction is T2L, POUSVersion is changed to 1", 1, entryHeader.ZG_POUSVersion);

				entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				declaration.DoMerge();
				AssertEquals("When messageVersion is Pous and entry instruction is T2C, POUSVersion is changed to 1", 1, entryHeader.ZG_POUSVersion);

				entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				declaration.DoMerge();
				AssertEquals("When messageVersion is NoPous and entry instruction is A, POUSVersion is not changed", 0, entryHeader.ZG_POUSVersion);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.NoProofOfUnionStatus))
			{
				entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				declaration.DoMerge();
				AssertEquals("When messageVersion is NoPous and entry instruction is T2L, POUSVersion is not changed", 0, entryHeader.ZG_POUSVersion);

				entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				declaration.DoMerge();
				AssertEquals("When messageVersion is NoPous and entry instruction is T2C, POUSVersion is not changed", 0, entryHeader.ZG_POUSVersion);
			}

			using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.RequestJecAndReceptionPous))
			{
				entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				declaration.DoMerge();
				AssertEquals("When messageVersion is Pous2 and entry instruction is T2L, POUSVersion is changed to 2", 2, entryHeader.ZG_POUSVersion);

				entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				declaration.DoMerge();
				AssertEquals("When messageVersion is Pous2 and entry instruction is T2C, POUSVersion is changed to 2", 2, entryHeader.ZG_POUSVersion);

				entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				declaration.DoMerge();
				AssertEquals("When messageVersion is NoPous2 and entry instruction is A, POUSVersion is not changed", 0, entryHeader.ZG_POUSVersion);
			}
		});
	}

	public void TestCreateExportUCC6T2LSupportingDocument_OneInvoiceLine_FinalPeriod()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var (_, invoiceLine, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		Factory.Save();
		declaration.DoMerge();
		var entryHeader = declaration.CustomsEntryHeaders[0];

		CombineAssertions(() =>
		{
			const string docType = "N825";
			const string docReference = "T2L";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				AssertEquals("entryInstruction has no T2L supporting document", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));

				declaration.ZG_CTStatusID = docReference;

				var entryLine = entryHeader.MergedLines.FirstOrDefault();
				entryLine.AddEntryLineDocument<SupportingDocument>(docType, "AAA");
				AssertEquals("entryLine has T2L supporting document", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine, docType));

				declaration.DoMerge();
				AssertEquals("entryInstruction has no T2L supporting document when entryLine already has the document", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));

				entryLine.GetPreviouslySentSupportingDocuments().ForEach(x => x.Delete());
				var docT2L = invoiceLine.SupportingDocuments.AddNew();
				docT2L.CSI_ReferenceNumber = "BBB";
				docT2L.CSI_Code = docType;
				declaration.DoMerge();
				AssertEquals("entryInstruction has no new T2L supporting document when entryLine has no document but there is an old T2L document in the line", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));
				AssertEquals("invoiceLine has old T2L supporting document", true, invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == "BBB"));

				docT2L.Delete();
				entryHeader.MovementReferenceNumber = "AAAAAAA";
				declaration.DoMerge();
				AssertEquals("entryInstruction has no T2L supporting document when there is not document in entryLine and no T2L document in the line but entry has MRN", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));

				entryHeader.MovementReferenceNumber = ZString.Empty;
				declaration.DoMerge();
				AssertEquals("entryInstruction has new T2L supporting document when there is not document in entryLine, no T2L document in the line and entry has empty MRN", true, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));
			}
		});
	}

	public void TestCreateExportUCC6T2LSupportingDocument_OneInvoiceLine_TransitionPeriod()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var (_, invoiceLine, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		Factory.Save();
		declaration.DoMerge();
		var entryHeader = declaration.CustomsEntryHeaders[0];

		CombineAssertions(() =>
		{
			const string docType = "N825";
			const string docReference = "T2L";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				AssertEquals("entryInstruction has no T2L supporting document", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));

				declaration.ZG_CTStatusID = docReference;

				var entryLine = entryHeader.MergedLines.FirstOrDefault();
				entryLine.AddEntryLineDocument<SupportingDocument>(docType, "AAA", subType: "LIQ");
				AssertEquals("entryLine has T2L supporting document", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine, docType, "AAA", "LIQ"));

				declaration.DoMerge();
				AssertEquals("entryInstruction has no T2L supporting document when entryLine already has the document", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));
				AssertEquals("entryLine has no extra T2L supporting document when entryLine already has the document (AAA)", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine, docType, "AAA", "LIQ"));
				AssertEquals("entryLine has no extra T2L supporting document when entryLine already has the document (T2L)", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine, docType, docReference, "LIQ"));

				entryLine.GetPreviouslySentSupportingDocuments().ForEach(x => x.Delete());
				var docT2L = invoiceLine.SupportingDocuments.AddNew();
				docT2L.CSI_ReferenceNumber = "BBB";
				docT2L.CSI_Code = docType;
				declaration.DoMerge();
				AssertEquals("entryInstruction has no new T2L supporting document when entryLine has no document but there is an old T2L document in the line", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));
				AssertEquals("invoiceLine has old T2L supporting document", true, invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == "BBB"));
				AssertEquals("entryLine has no extra T2L supporting document when invoiceLine already has the document (BBB)", true, entryLine.ReadOnlySupportingDocuments.Cast<ReadOnlySupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == "BBB"));
				AssertEquals("entryLine has no extra T2L supporting document when invoiceLine already has the document (T2L)", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine, docType, docReference, "LIQ"));

				docT2L.Delete();
				entryHeader.MovementReferenceNumber = "AAAAAAA";
				declaration.DoMerge();
				AssertEquals("entryInstruction has no T2L supporting document when there is not document in entryLine and no T2L document in the line but entry has MRN", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));
				AssertEquals("entryLine has no T2L supporting document when there is not document in entryLine and no T2L document in the line but entry has MRN", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine, docType));

				entryHeader.MovementReferenceNumber = ZString.Empty;
				declaration.DoMerge();
				AssertEquals("entryInstruction has new T2L supporting document when there is not document in entryLine, no T2L document in the line and entry has empty MRN", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));
				AssertEquals("entryLine has new T2L supporting document when there is not document in entryLine, no T2L document in the line and entry has empty MRN", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine, docType, docReference, "LIQ"));
			}
		});
	}

	public void TestCreateExportUCC6T2LSupportingDocument_MultipleInvoiceLines_FinalPeriod()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var (_, _, entryInstruction1) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		var (invoice2, invoiceLine2_1, entryInstruction2) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.B);
		invoiceLine2_1.JI_Tariff = "2208905400";
		var invoiceLine2_2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2_2.JI_Tariff = "2208905401";
		invoiceLine2_2.JI_CEI = entryInstruction2.PK;

		var (invoice3, _, entryInstruction3) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.C);
		var invoiceLine3_2 = invoice3.InvoiceLines.AddNew();
		invoiceLine3_2.JI_CEI = entryInstruction3.PK;

		var (_, _, entryInstruction4) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.X);

		var (_, _, entryInstruction5) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.Z);

		Factory.Save();
		declaration.DoMerge();
		var entryHeader1 = entryInstruction1.EntryHeader;
		var entryHeader2 = entryInstruction2.EntryHeader;
		var entryHeader3 = entryInstruction3.EntryHeader;
		var entryHeader4 = entryInstruction4.EntryHeader;
		entryHeader4.MovementReferenceNumber = "AAAAAAA";
		var entryHeader5 = declaration.CustomsEntryHeaders.FirstOrDefault(x => x.EntryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.Z);
		entryHeader5.CH_EntryStatus = "CLR";
		entryHeader5.ZG_UCC6Version = 0;

		CombineAssertions(() =>
		{
			AssertEquals("entryHeader1 has 1 entryLine", 1, entryHeader1.MergedLines.Count);
			AssertEquals("entryHeader2 has 2 entryLines", 2, entryHeader2.MergedLines.Count);
			AssertEquals("entryHeader3 has 1 entryLine", 1, entryHeader3.MergedLines.Count);
			AssertEquals("entryHeader4 has 1 entryLine", 1, entryHeader4.MergedLines.Count);
			AssertEquals("entryHeader5 has 1 entryLine", 1, entryHeader5.MergedLines.Count);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				const string docType = "N825";
				const string docReference = "T2L";
				bool checkCodeAndReferenceNumber(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument x)
					=> x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference;

				AssertEquals("entryInstruction1 has no T2L supporting document", false, entryInstruction1.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction2 has no T2L supporting document", false, entryInstruction2.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction3 has no T2L supporting document", false, entryInstruction3.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction4 has no T2L supporting document", false, entryInstruction4.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction5 has no T2L supporting document", false, entryInstruction5.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());

				declaration.ZG_CTStatusID = docReference;

				var entryLine2_1 = entryHeader2.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
				entryLine2_1.AddEntryLineDocument<SupportingDocument>(docType, "AAA");
				AssertEquals("entryLine2_1 has T2L supporting document", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_1, docType));

				declaration.DoMerge();

				AssertEquals("entryHeader3 has 1 entryLine, both invoice lines have the same document", 1, entryHeader3.MergedLines.Count);

				AssertEquals("entryInstruction1 has new T2L supporting document", true, entryInstruction1.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction2 has no T2L supporting document when entryLine2_1 has T2L document", false, entryInstruction2.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction3 has new T2L supporting document", true, entryInstruction3.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction4 has no T2L supporting document when entry has MRN", false, entryInstruction4.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction5 has no T2L supporting document when entry is not Ucc6", false, entryInstruction5.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
			}
		});
	}

	public void TestCreateExportUCC6T2LSupportingDocument_MultipleInvoiceLines_TransitionPeriod()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var (_, _, entryInstruction1) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		var (invoice2, invoiceLine2_1, entryInstruction2) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.B);
		invoiceLine2_1.JI_Tariff = "2208905400";
		var invoiceLine2_2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2_2.JI_Tariff = "2208905401";
		invoiceLine2_2.JI_CEI = entryInstruction2.PK;

		var (invoice3, _, entryInstruction3) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.C);
		var invoiceLine3_2 = invoice3.InvoiceLines.AddNew();
		invoiceLine3_2.JI_CEI = entryInstruction3.PK;

		var (_, _, entryInstruction4) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.X);

		var (_, _, entryInstruction5) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.Z);

		var (invoice6, invoiceLine6_1, entryInstruction6) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.Y);
		invoiceLine6_1.JI_Tariff = "2208905402";
		var invoiceLine6_2 = invoice6.InvoiceLines.AddNew();
		invoiceLine6_2.JI_Tariff = "2208905403";
		invoiceLine6_2.JI_CEI = entryInstruction6.PK;

		Factory.Save();
		declaration.DoMerge();
		var entryHeader1 = entryInstruction1.EntryHeader;
		var entryHeader2 = entryInstruction2.EntryHeader;
		var entryHeader3 = entryInstruction3.EntryHeader;
		var entryHeader4 = entryInstruction4.EntryHeader;
		entryHeader4.MovementReferenceNumber = "AAAAAAA";
		var entryHeader5 = declaration.CustomsEntryHeaders.FirstOrDefault(x => x.EntryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.Z);
		entryHeader5.CH_EntryStatus = "CLR";
		entryHeader5.ZG_UCC6Version = 0;
		var entryHeader6 = entryInstruction6.EntryHeader;

		CombineAssertions(() =>
		{
			AssertEquals("entryHeader1 has 1 entryLine", 1, entryHeader1.MergedLines.Count);
			AssertEquals("entryHeader2 has 2 entryLines", 2, entryHeader2.MergedLines.Count);
			AssertEquals("entryHeader3 has 1 entryLine", 1, entryHeader3.MergedLines.Count);
			AssertEquals("entryHeader4 has 1 entryLine", 1, entryHeader4.MergedLines.Count);
			AssertEquals("entryHeader5 has 1 entryLine", 1, entryHeader5.MergedLines.Count);
			AssertEquals("entryHeader6 has 2 entryLines", 2, entryHeader6.MergedLines.Count);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				const string docType = "N825";
				const string docReference = "T2L";
				bool checkCodeAndReferenceNumber(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument x)
					=> x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference;

				var entryLine1 = entryHeader1.MergedLines[0];
				var entryLine2_1 = entryHeader2.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
				var entryLine2_2 = entryHeader2.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
				var entryLine3 = entryHeader3.MergedLines[0];
				var entryLine4 = entryHeader4.MergedLines[0];
				var entryLine5 = entryHeader5.MergedLines[0];
				var entryLine6_1 = entryHeader6.MergedLines.FirstOrDefault(x => x.CL_LineNumber == 1);
				var entryLine6_2 = entryHeader6.MergedLines.FirstOrDefault(x => x.CL_LineNumber == 2);

				AssertEquals("entryInstruction1 has no T2L supporting document", false, entryInstruction1.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine1 has no T2L supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine1, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction2 has no T2L supporting document", false, entryInstruction2.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine2_1 has no T2L supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_1, docType, docReference, "LIQ"));
				AssertEquals("entryLine2_2 has no T2L supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_2, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction3 has no T2L supporting document", false, entryInstruction3.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine3 has no T2L supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine3, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction4 has no T2L supporting document", false, entryInstruction4.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine4 has no T2L supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine4, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction5 has no T2L supporting document", false, entryInstruction5.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine5 has no T2L supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine5, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction6 has no T2L supporting document", false, entryInstruction6.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine6_1 has no T2L supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine6_1, docType, docReference, "LIQ"));
				AssertEquals("entryLine6_2 has no T2L supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine6_2, docType, docReference, "LIQ"));

				declaration.ZG_CTStatusID = docReference;

				entryLine2_2.AddEntryLineDocument<SupportingDocument>(docType, "AAA");
				AssertEquals("entryLine2_2 has T2L supporting document", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_2, docType));

				declaration.DoMerge();

				AssertEquals("entryInstruction1 has no T2L supporting document after merge", false, entryInstruction1.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine1 has new T2L supporting document after merge", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine1, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction2 has no T2L supporting document after merge", false, entryInstruction2.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine2_1 has no T2L supporting document after merge when entryLine2_1 has T2L document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_1, docType, docReference, "LIQ"));
				AssertEquals("entryLine2_2 has T2L supporting document after merge (added manually before merge)", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_2, docType));
				AssertEquals("entryInstruction3 has no T2L supporting document after merge", false, entryInstruction3.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine3 has new T2L supporting document after merge", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine3, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction4 has no T2L supporting document after merge", false, entryInstruction4.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine4 has no T2L supporting document after merge when entry has MRN", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine4, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction5 has no T2L supporting document after merge", false, entryInstruction5.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine5 has no T2L supporting document after merge when entry is not Ucc6", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine5, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction6 has no T2L supporting document after merge", false, entryInstruction6.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine6_1 has new T2L supporting document after merge (it's the first entry line)", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine6_1, docType, docReference, "LIQ"));
				AssertEquals("entryLine6_2 has no T2L supporting document after merge (it's not the first entry line)", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine6_2, docType, docReference, "LIQ"));
			}
		});
	}

	public void TestRemoveExportUCC6T2LSupportingDocumentWhenNotT2L()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var (_, _, entryInstruction1) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		var (invoice2, invoiceLine2_1, entryInstruction2) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.B);
		invoiceLine2_1.JI_Tariff = "2208905400";
		var invoiceLine2_2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2_2.JI_Tariff = "2208905401";
		invoiceLine2_2.JI_CEI = entryInstruction2.PK;

		Factory.Save();
		declaration.DoMerge();
		var entryHeader1 = entryInstruction1.EntryHeader;
		var entryHeader2 = entryInstruction2.EntryHeader;

		CombineAssertions(() =>
		{
			AssertEquals("entryHeader1 has 1 entryLine", 1, entryHeader1.MergedLines.Count);
			AssertEquals("entryHeader2 has 2 entryLines", 2, entryHeader2.MergedLines.Count);

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				const string docType = "N825";

				var entryLine1 = entryHeader1.MergedLines[0];
				var entryLine2_1 = entryHeader2.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
				var entryLine2_2 = entryHeader2.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");

				entryLine1.AddEntryLineDocument<SupportingDocument>(docType, "AAA");
				entryLine2_1.AddEntryLineDocument<SupportingDocument>(docType, "BBB");
				entryLine2_2.AddEntryLineDocument<SupportingDocument>(docType, "CCC", status: "ACC");

				AssertEquals("entryLine1 has T2L supporting document", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine1, docType));
				AssertEquals("entryLine2_1 has T2L supporting document", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_1, docType));
				AssertEquals("entryLine2_2 has T2L supporting document", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_2, docType));

				declaration.ZG_CTStatusID = ZString.Empty;

				declaration.DoMerge();

				AssertEquals("entryLine1 has no T2L supporting document after merge", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine1, docType));
				AssertEquals("entryLine2_1 has no T2L supporting document after merge", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_1, docType));
				AssertEquals("entryLine2_2 has T2L supporting document after merge (not removed because it has status ACC)", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_2, docType));
			}
		});
	}

	public void TestCreateExportUCC6T2LFSupportingDocument_OneInvoiceLine_FinalPeriod()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var (_, invoiceLine, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		Factory.Save();
		declaration.DoMerge();
		var entryHeader = declaration.CustomsEntryHeaders[0];

		CombineAssertions(() =>
		{
			const string docType = "C620";
			const string docReference = "T2LF";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				entryInstruction.SupportingDocuments.RemoveAndDeleteAll();
				declaration.ZG_CTStatusID = ZString.Empty;
				declaration.DoMerge();
				AssertEquals("invoiceLine has no T2LF supporting document", false, invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));

				declaration.ZG_CTStatusID = docReference;

				var entryLine = entryHeader.MergedLines.FirstOrDefault();
				entryLine.AddEntryLineDocument<SupportingDocument>(docType, "AAA");
				AssertEquals("entryLine has T2LF supporting document", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine, docType));

				declaration.DoMerge();
				AssertEquals("entryInstruction has no T2LF supporting document when entryLine already has the document", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));

				entryLine.GetPreviouslySentSupportingDocuments().ForEach(x => x.Delete());
				var docT2LF = invoiceLine.SupportingDocuments.AddNew();
				docT2LF.CSI_ReferenceNumber = "BBB";
				docT2LF.CSI_Code = docType;
				declaration.DoMerge();
				AssertEquals("entryInstruction has no new T2LF supporting document when entryLine has no document but there is an old T2LF document in the line", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));
				AssertEquals("invoiceLine has old T2LF supporting document ", true, invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == "BBB"));

				docT2LF.Delete();
				entryHeader.MovementReferenceNumber = "AAAAAAA";
				declaration.DoMerge();
				AssertEquals("entryInstruction has no T2LF supporting document when there is not document in entryLine and no T2LF document in the line but entry has MRN", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));

				entryHeader.MovementReferenceNumber = ZString.Empty;
				declaration.DoMerge();
				AssertEquals("entryInstruction has new T2LF supporting document when there is not document in entryLine, no T2LF document in the line and entry has empty MRN", true, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));
			}
		});
	}

	public void TestCreateExportUCC6T2LFSupportingDocument_OneInvoiceLine_TransitionPeriod()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var (_, invoiceLine, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		Factory.Save();
		declaration.DoMerge();
		var entryHeader = declaration.CustomsEntryHeaders[0];

		CombineAssertions(() =>
		{
			const string docType = "C620";
			const string docReference = "T2LF";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				AssertEquals("entryInstruction has no T2LF supporting document", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));

				declaration.ZG_CTStatusID = docReference;

				var entryLine = entryHeader.MergedLines.FirstOrDefault();
				entryLine.AddEntryLineDocument<SupportingDocument>(docType, "AAA", subType: "LIQ");
				AssertEquals("entryLine has T2LF supporting document", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine, docType, "AAA", "LIQ"));

				declaration.DoMerge();
				AssertEquals("entryInstruction has no T2LF supporting document when entryLine already has the document", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));
				AssertEquals("entryLine has no extra T2LF supporting document when entryLine already has the document (AAA)", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine, docType, "AAA", "LIQ"));
				AssertEquals("entryLine has no extra T2LF supporting document when entryLine already has the document (T2LF)", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine, docType, docReference, "LIQ"));

				entryLine.GetPreviouslySentSupportingDocuments().ForEach(x => x.Delete());
				var docT2LF = invoiceLine.SupportingDocuments.AddNew();
				docT2LF.CSI_ReferenceNumber = "BBB";
				docT2LF.CSI_Code = docType;
				declaration.DoMerge();
				AssertEquals("entryInstruction has no new T2LF supporting document when entryLine has no document but there is an old T2LF document in the line", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));
				AssertEquals("invoiceLine has old T2LF supporting document", true, invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == "BBB"));
				AssertEquals("entryLine has no extra T2L supporting document when invoiceLine already has the document (BBB)", true, entryLine.ReadOnlySupportingDocuments.Cast<ReadOnlySupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == "BBB"));
				AssertEquals("entryLine has no extra T2LF supporting document when invoiceLine already has the document (T2LF)", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine, docType, docReference, "LIQ"));

				docT2LF.Delete();
				entryHeader.MovementReferenceNumber = "AAAAAAA";
				declaration.DoMerge();
				AssertEquals("entryInstruction has no T2LF supporting document when there is not document in entryLine and no T2LF document in the line but entry has MRN", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));
				AssertEquals("entryLine has no T2LF supporting document when there is not document in entryLine and no T2L document in the line but entry has MRN", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine, docType));

				entryHeader.MovementReferenceNumber = ZString.Empty;
				declaration.DoMerge();
				AssertEquals("entryInstruction has new T2LF supporting document when there is not document in entryLine, no T2LF document in the line and entry has empty MRN", false, entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference));
				AssertEquals("entryLine has new T2LF supporting document when there is not document in entryLine, no T2LF document in the line and entry has empty MRN", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine, docType, docReference, "LIQ"));
			}
		});
	}

	public void TestCreateExportUCC6T2LFSupportingDocument_MultipleInvoiceLines_FinalPeriod()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		var (_, _, entryInstruction1) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		var (invoice2, invoiceLine2_1, entryInstruction2) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.B);
		invoiceLine2_1.JI_Tariff = "2208905400";
		var invoiceLine2_2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2_2.JI_Tariff = "2208905401";
		invoiceLine2_2.JI_CEI = entryInstruction2.PK;

		var (invoice3, _, entryInstruction3) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.C);
		var invoiceLine3_2 = invoice3.InvoiceLines.AddNew();
		invoiceLine3_2.JI_CEI = entryInstruction3.PK;

		var (_, _, entryInstruction4) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.X);
		var (_, _, entryInstruction5) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.Z);

		Factory.Save();
		declaration.DoMerge();
		var entryHeader1 = entryInstruction1.EntryHeader;
		var entryHeader2 = entryInstruction2.EntryHeader;
		var entryHeader3 = entryInstruction3.EntryHeader;
		var entryHeader4 = entryInstruction4.EntryHeader;
		entryHeader4.MovementReferenceNumber = "AAAAAAA";
		var entryHeader5 = declaration.CustomsEntryHeaders.FirstOrDefault(x => x.EntryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.Z);
		entryHeader5.CH_EntryStatus = "CLR";
		entryHeader5.ZG_UCC6Version = 0;

		CombineAssertions(() =>
		{
			AssertEquals("entryHeader1 has 1 entryLine", 1, entryHeader1.MergedLines.Count);
			AssertEquals("entryHeader2 has 2 entryLines", 2, entryHeader2.MergedLines.Count);
			AssertEquals("entryHeader3 has 1 entryLine", 1, entryHeader3.MergedLines.Count);
			AssertEquals("entryHeader4 has 1 entryLine", 1, entryHeader4.MergedLines.Count);
			AssertEquals("entryHeader5 has 1 entryLine", 1, entryHeader5.MergedLines.Count);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				const string docType = "C620";
				const string docReference = "T2LF";
				bool checkCodeAndReferenceNumber(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument x)
					=> x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference;

				AssertEquals("entryInstruction1 has no T2LF supporting document", false, entryInstruction1.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction2 has no T2LF supporting document", false, entryInstruction2.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction3 has no T2LF supporting document", false, entryInstruction3.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction4 has no T2LF supporting document", false, entryInstruction4.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction5 has no T2LF supporting document", false, entryInstruction5.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());

				declaration.ZG_CTStatusID = docReference;

				var entryLine2_1 = entryHeader2.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
				entryLine2_1.AddEntryLineDocument<SupportingDocument>(docType, "AAA");
				AssertEquals("entryLine2_1 has T2LF supporting document", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_1, docType));

				declaration.DoMerge();

				AssertEquals("entryHeader3 has 1 entryLine, both invocie lines have the same document", 1, entryHeader3.MergedLines.Count);

				AssertEquals("entryInstruction1 has new T2LF supporting document", true, entryInstruction1.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction2 has no T2LF supporting document when entryLine2_1 has T2L document", false, entryInstruction2.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction3 has new T2LF supporting document", true, entryInstruction3.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction4 has no T2LF supporting document when entry has MRN", false, entryInstruction4.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryInstruction5 has no T2LF supporting document when entry is not Ucc6", false, entryInstruction5.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
			}
		});
	}

	public void TestCreateExportUCC6T2LFSupportingDocument_MultipleInvoiceLines_TransitionPeriod()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var (_, _, entryInstruction1) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		var (invoice2, invoiceLine2_1, entryInstruction2) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.B);
		invoiceLine2_1.JI_Tariff = "2208905400";
		var invoiceLine2_2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2_2.JI_Tariff = "2208905401";
		invoiceLine2_2.JI_CEI = entryInstruction2.PK;

		var (invoice3, _, entryInstruction3) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.C);
		var invoiceLine3_2 = invoice3.InvoiceLines.AddNew();
		invoiceLine3_2.JI_CEI = entryInstruction3.PK;

		var (_, _, entryInstruction4) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.X);

		var (_, _, entryInstruction5) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.Z);

		var (invoice6, invoiceLine6_1, entryInstruction6) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.Y);
		invoiceLine6_1.JI_Tariff = "2208905402";
		var invoiceLine6_2 = invoice6.InvoiceLines.AddNew();
		invoiceLine6_2.JI_Tariff = "2208905403";
		invoiceLine6_2.JI_CEI = entryInstruction6.PK;

		Factory.Save();
		declaration.DoMerge();
		var entryHeader1 = entryInstruction1.EntryHeader;
		var entryHeader2 = entryInstruction2.EntryHeader;
		var entryHeader3 = entryInstruction3.EntryHeader;
		var entryHeader4 = entryInstruction4.EntryHeader;
		entryHeader4.MovementReferenceNumber = "AAAAAAA";
		var entryHeader5 = declaration.CustomsEntryHeaders.FirstOrDefault(x => x.EntryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.Z);
		entryHeader5.CH_EntryStatus = "CLR";
		entryHeader5.ZG_UCC6Version = 0;
		var entryHeader6 = entryInstruction6.EntryHeader;

		CombineAssertions(() =>
		{
			AssertEquals("entryHeader1 has 1 entryLine", 1, entryHeader1.MergedLines.Count);
			AssertEquals("entryHeader2 has 2 entryLines", 2, entryHeader2.MergedLines.Count);
			AssertEquals("entryHeader3 has 1 entryLine", 1, entryHeader3.MergedLines.Count);
			AssertEquals("entryHeader4 has 1 entryLine", 1, entryHeader4.MergedLines.Count);
			AssertEquals("entryHeader5 has 1 entryLine", 1, entryHeader5.MergedLines.Count);
			AssertEquals("entryHeader6 has 2 entryLines", 2, entryHeader6.MergedLines.Count);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				const string docType = "C620";
				const string docReference = "T2LF";
				bool checkCodeAndReferenceNumber(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument x)
					=> x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference;

				var entryLine1 = entryHeader1.MergedLines[0];
				var entryLine2_1 = entryHeader2.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
				var entryLine2_2 = entryHeader2.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");
				var entryLine3 = entryHeader3.MergedLines[0];
				var entryLine4 = entryHeader4.MergedLines[0];
				var entryLine5 = entryHeader5.MergedLines[0];
				var entryLine6_1 = entryHeader6.MergedLines.FirstOrDefault(x => x.CL_LineNumber == 1);
				var entryLine6_2 = entryHeader6.MergedLines.FirstOrDefault(x => x.CL_LineNumber == 2);

				AssertEquals("entryInstruction1 has no T2LF supporting document", false, entryInstruction1.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine1 has no T2LF supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine1, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction2 has no T2LF supporting document", false, entryInstruction2.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine2_1 has no T2LF supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_1, docType, docReference, "LIQ"));
				AssertEquals("entryLine2_2 has no T2LF supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_2, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction3 has no T2LF supporting document", false, entryInstruction3.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine3 has no T2LF supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine3, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction4 has no T2LF supporting document", false, entryInstruction4.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine4 has no T2LF supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine4, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction5 has no T2LF supporting document", false, entryInstruction5.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine5 has no T2LF supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine5, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction6 has no T2LF supporting document", false, entryInstruction6.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine6_1 has no T2LF supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine6_1, docType, docReference, "LIQ"));
				AssertEquals("entryLine6_2 has no T2LF supporting document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine6_2, docType, docReference, "LIQ"));

				declaration.ZG_CTStatusID = docReference;

				entryLine2_2.AddEntryLineDocument<SupportingDocument>(docType, "AAA");
				AssertEquals("entryLine2_2 has T2LF supporting document", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_2, docType));

				declaration.DoMerge();

				AssertEquals("entryInstruction1 has no T2LF supporting document after merge", false, entryInstruction1.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine1 has new T2LF supporting document after merge", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine1, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction2 has no T2LF supporting document after merge", false, entryInstruction2.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine2_1 has no T2LF supporting document after merge when entryLine2_1 has T2LF document", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_1, docType, docReference, "LIQ"));
				AssertEquals("entryLine2_2 has T2LF supporting document after merge (added manually before merge)", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_2, docType));
				AssertEquals("entryInstruction3 has no T2LF supporting document after merge", false, entryInstruction3.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine3 has new T2LF supporting document after merge", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine3, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction4 has no T2LF supporting document after merge", false, entryInstruction4.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine4 has no T2LF supporting document after merge when entry has MRN", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine4, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction5 has no T2LF supporting document after merge", false, entryInstruction5.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine5 has no T2LF supporting document after merge when entry is not Ucc6", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine5, docType, docReference, "LIQ"));
				AssertEquals("entryInstruction6 has no T2LF supporting document after merge", false, entryInstruction6.SupportingDocuments.Find(checkCodeAndReferenceNumber).Any());
				AssertEquals("entryLine6_1 has new T2LF supporting document after merge (it's the first entry line)", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine6_1, docType, docReference, "LIQ"));
				AssertEquals("entryLine6_2 has no T2LF supporting document after merge (it's not the first entry line)", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine6_2, docType, docReference, "LIQ"));
			}
		});
	}

	public void TestRemoveExportUCC6T2LFSupportingDocumentWhenNotT2LF()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var (_, _, entryInstruction1) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);

		var (invoice2, invoiceLine2_1, entryInstruction2) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.B);
		invoiceLine2_1.JI_Tariff = "2208905400";
		var invoiceLine2_2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2_2.JI_Tariff = "2208905401";
		invoiceLine2_2.JI_CEI = entryInstruction2.PK;

		Factory.Save();
		declaration.DoMerge();
		var entryHeader1 = entryInstruction1.EntryHeader;
		var entryHeader2 = entryInstruction2.EntryHeader;

		CombineAssertions(() =>
		{
			AssertEquals("entryHeader1 has 1 entryLine", 1, entryHeader1.MergedLines.Count);
			AssertEquals("entryHeader2 has 2 entryLines", 2, entryHeader2.MergedLines.Count);

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				const string docType = "C620";

				var entryLine1 = entryHeader1.MergedLines[0];
				var entryLine2_1 = entryHeader2.MergedLines.FirstOrDefault(x => x.Tariff == "2208905400");
				var entryLine2_2 = entryHeader2.MergedLines.FirstOrDefault(x => x.Tariff == "2208905401");

				entryLine1.AddEntryLineDocument<SupportingDocument>(docType, "AAA");
				entryLine2_1.AddEntryLineDocument<SupportingDocument>(docType, "BBB");
				entryLine2_2.AddEntryLineDocument<SupportingDocument>(docType, "CCC", status: "ACC");

				AssertEquals("entryLine1 has T2L supporting document", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine1, docType));
				AssertEquals("entryLine2_1 has T2L supporting document", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_1, docType));
				AssertEquals("entryLine2_2 has T2L supporting document", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_2, docType));

				declaration.ZG_CTStatusID = ZString.Empty;

				declaration.DoMerge();

				AssertEquals("entryLine1 has no T2L supporting document after merge", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine1, docType));
				AssertEquals("entryLine2_1 has no T2L supporting document after merge", 0, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_1, docType));
				AssertEquals("entryLine2_2 has T2L supporting document after merge (not removed because it has status ACC)", 1, SupDocTestHelper.GetTypeGetPreviouslySentSupportingDocumentsCount(entryLine2_2, docType));
			}
		});
	}

	public void TestPopulateLocationOfGoodsIfNeeded()
	{
		declaration.JE_LocationOfGoods = "AH30202";

		var (invoice1, _, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A, style: IMPDeclarationTypeList.Codes.H2);
		invoice1.JZ_RX_NKInvoice_Currency = "EUR";

		AssertEquals("PREREQ: GoodsLocation is Empty", ZString.Empty, entryInstruction.GoodsLocationDescription);

		Factory.Save();
		declaration.DoMerge();

		CombineAssertions(() =>
		{
			var entryHeader = declaration.CustomsEntryHeaders[0];
			declaration.JE_LocationOfGoods = ZString.Empty;
			declaration.DoMerge();
			AssertEquals("Empty GoodsLocation is not populated with JE_LocationOfGoods as it's empty", ZString.Empty, entryInstruction.GoodsLocationDescription);

			declaration.JE_LocationOfGoods = "ES30202";
			declaration.DoMerge();
			AssertEquals("Empty GoodsLocation is populated with JE_LocationOfGoods as it's not empty", "Y;B;ES30202", entryInstruction.GoodsLocationDescription);

			declaration.JE_LocationOfGoods = "ES99999";
			declaration.DoMerge();
			AssertEquals("GoodsLocation is not populated with new JE_LocationOfGoods as it's already populated", "Y;B;ES30202", entryInstruction.GoodsLocationDescription);
		});
	}

	public void TestPopulateLocationOfGoodsIfNeededForException()
	{
		declaration.JE_LocationOfGoods = "ES30202";

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		Factory.Save();

		AssertNoExceptionThrown("When there is no entry instruction, the code should continue without any exception", () => declaration.DoMerge());
	}

	public void TestCreateImportH2SupportingDocumentsIfNeededForC513()
	{
		DontCreateImportH2SupportingDocument_Existing(SupportingDocumentType.CentralizedClearance, authType: CusAuthorizationHeaderTypeList.Codes.CentralizedClearance);
		CreateImportH2SupportingDocument_NewDocument(SupportingDocumentType.CentralizedClearance, authType: CusAuthorizationHeaderTypeList.Codes.CentralizedClearance);
		CreateImportH2SupportingDocument_NewDocument_WithMultipleInvoiceHeaders(SupportingDocumentType.CentralizedClearance, authType: CusAuthorizationHeaderTypeList.Codes.CentralizedClearance);
	}

	public void TestCreateImportH2SupportingDocumentsIfNeededForC514()
	{
		DontCreateImportH2SupportingDocument_Existing(SupportingDocumentType.EntryOfDataInDeclarantsRecords, authType: CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);
		CreateImportH2SupportingDocument_NewDocument(SupportingDocumentType.EntryOfDataInDeclarantsRecords, authType: CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);
		CreateImportH2SupportingDocument_NewDocument_WithMultipleInvoiceHeaders(SupportingDocumentType.CentralizedClearance, authType: CusAuthorizationHeaderTypeList.Codes.CentralizedClearance);
	}

	public void TestCreateImportH2SupportingDocumentsIfNeededFor5018()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LocationsInAuthorisations, "LOCAT");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LocationsInAuthorisations, AuthValue1, "ES LOCAT 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LocationsInAuthorisations, AuthValue2, "ES LOCAT 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LocationsInAuthorisations, AuthValue3, "ES LOCAT 3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		DontCreateImportH2SupportingDocument_Existing(SupportingDocumentType.WHLOCAuthorisation, shouldCreateRule: true);
		CreateImportH2SupportingDocument_NewDocument(SupportingDocumentType.WHLOCAuthorisation, shouldCreateRule: true);
		CreateImportH2SupportingDocument_NewDocument_WithMultipleInvoiceHeaders(SupportingDocumentType.WHLOCAuthorisation, shouldCreateRule: true);
	}

	void DontCreateImportH2SupportingDocument_Existing(string docType, string authType = "CW1", bool shouldCreateRule = false)
	{
		var (_, _, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);
		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		SetValidAuthorization(entryInstruction, firstLOCValue: shouldCreateRule ? AuthValue1 : string.Empty, authType: authType, authNumber: !shouldCreateRule ? AuthValue1 : string.Empty);
		entryInstruction.SupportingDocuments.Add(Factory.CreateSupportingDocument(docType, "AAA"));

		CombineAssertions(() =>
		{
			var supDocsList = entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Where(x => x.CSI_Code == docType);
			AssertEquals("entryInstruction has one " + docType + " supporting document", 1, supDocsList.Count());
			AssertEquals("entryInstruction " + docType + " supporting document has the correct CSI_ReferenceNumber", "AAA", supDocsList.First().CSI_ReferenceNumber);

			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			supDocsList = entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Where(x => x.CSI_Code == docType);
			AssertEquals("entryInstruction still has one " + docType + " supporting document", 1, supDocsList.Count());
			AssertEquals("entryInstruction " + docType + " supporting document has the correct CSI_ReferenceNumber (not changed)", "AAA", supDocsList.First().CSI_ReferenceNumber);
		});
	}

	void CreateImportH2SupportingDocument_NewDocument(string docType, string authType = "CW1", bool shouldCreateRule = false)
	{
		var (_, _, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);
		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		SetValidAuthorization(entryInstruction, firstLOCValue: shouldCreateRule ? AuthValue1 : string.Empty, authType: authType, authNumber: !shouldCreateRule ? AuthValue1 : string.Empty);

		CombineAssertions(() =>
		{
			var supDocsList = entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Where(x => x.CSI_Code == docType);
			AssertEquals("entryInstruction has no " + docType + " supporting document", 0, supDocsList.Count());

			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			supDocsList = entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Where(x => x.CSI_Code == docType);
			AssertEquals("entryInstruction has one new " + docType + " supporting document", 1, supDocsList.Count());
			AssertEquals("entryInstruction " + docType + " supporting document has the correct CSI_ReferenceNumber", AuthValue1, supDocsList.First().CSI_ReferenceNumber);
		});
	}

	void CreateImportH2SupportingDocument_NewDocument_WithMultipleInvoiceHeaders(string docType, string authType = "CW1", bool shouldCreateRule = false)
	{
		var (_, _, entryInstruction1) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);
		entryInstruction1.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		SetValidAuthorization(entryInstruction1, firstLOCValue: shouldCreateRule ? AuthValue1 : string.Empty, authType: authType, authNumber: !shouldCreateRule ? AuthValue1 : string.Empty);

		var (_, _, entryInstruction2) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.B);
		entryInstruction2.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		SetValidAuthorization(entryInstruction2, firstLOCValue: shouldCreateRule ? AuthValue2 : string.Empty, authType: authType, authNumber: !shouldCreateRule ? AuthValue2 : string.Empty, holderCode: "BB");

		var (_, _, entryInstruction3) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.C);
		SetValidAuthorization(entryInstruction3, firstLOCValue: shouldCreateRule ? AuthValue3 : string.Empty, authType: authType, authNumber: !shouldCreateRule ? AuthValue3 : string.Empty, holderCode: "CC");

		CombineAssertions(() =>
		{
			AssertEquals("before merge entryInstruction1 has no " + docType + " supporting document", 0, entryInstruction1.SupportingDocuments.Find(x => x.CSI_Code == docType).Count());
			AssertEquals("before merge entryInstruction2 has no " + docType + " supporting document", 0, entryInstruction2.SupportingDocuments.Find(x => x.CSI_Code == docType).Count());
			AssertEquals("before merge entryInstruction3 has no " + docType + " supporting document", 0, entryInstruction3.SupportingDocuments.Find(x => x.CSI_Code == docType).Count());

			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			AssertEquals("After merge entryInstruction1 has " + docType + " supporting document wiht number" + AuthValue1, 1, entryInstruction1.SupportingDocuments.Find(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == AuthValue1).Count());
			AssertEquals("After merge entryInstruction2 has " + docType + " supporting document wiht number" + AuthValue2, 1, entryInstruction2.SupportingDocuments.Find(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == AuthValue2).Count());
			AssertEquals("After merge entryInstruction3 has no " + docType + " supporting document", 0, entryInstruction3.SupportingDocuments.Find(x => x.CSI_Code == docType).Count());
		});
	}

	public void TestDontCreateImportH25018SupportingDocument_MultipleRules()
	{
		var docType = "5018";
		var authRuleValue1 = "ES123456789";
		var authRuleValue2 = "ES987654321";

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LocationsInAuthorisations, "LOCAT");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LocationsInAuthorisations, authRuleValue1, "ES LOCAT 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LocationsInAuthorisations, authRuleValue2, "ES LOCAT 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var (_, _, entryInstruction) = SetUpInvHeaderInvLineAndEntryInstruction(EntrySubStyleList.Codes.A);
		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		SetValidAuthorization(entryInstruction, firstLOCValue: authRuleValue1, secondLOCValue: authRuleValue2);
		var supDoc = entryInstruction.SupportingDocuments.AddNew();
		supDoc.CSI_ReferenceNumber = "AAA";
		supDoc.CSI_Code = docType;

		Factory.Save();

		CombineAssertions(() =>
		{
			var supDocsList = entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Where(x => x.CSI_Code == docType);
			AssertEquals("entryInstruction has one 5018 supporting document", 1, supDocsList.Count());
			AssertEquals("entryInstruction 5018 supporting document has the correct CSI_ReferenceNumber", "AAA", supDocsList.First().CSI_ReferenceNumber);

			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			supDocsList = entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Where(x => x.CSI_Code == docType);
			AssertEquals("entryInstruction still has one 5018 supporting document", 1, supDocsList.Count());
			AssertEquals("entryInstruction 5018 supporting document has the correct CSI_ReferenceNumber (not changed)", "AAA", supDocsList.First().CSI_ReferenceNumber);
		});
	}

	void SetValidAuthorization(CusEntryInstruction entryInstruction, string firstLOCValue = "", string holderCode = "AA", string secondLOCValue = "", string authType = "CW1", string authNumber = "Number")
	{
		var holder = Factory.New<OrgHeader>();
		holder.OH_Code = holderCode;

		var authorisation = Factory.New<CusAuthorisationHeader>();
		authorisation.CPH_OH_PermitHolder = holder.PK;
		authorisation.CPH_Type = authType;
		authorisation.CPH_StartDate = new ZDate(2021, 11, 03);
		authorisation.CPH_Number = authNumber;

		if (!string.IsNullOrEmpty(firstLOCValue))
		{
			var authorisationRule = authorisation.CusAuthorisationRules.AddNew();
			authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			authorisationRule.CPR_ValueFrom = firstLOCValue;
		}

		if (!string.IsNullOrEmpty(secondLOCValue))
		{
			var authorisationRule2 = authorisation.CusAuthorisationRules.AddNew();
			authorisationRule2.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			authorisationRule2.CPR_ValueFrom = secondLOCValue;
		}

		var auth1 = entryInstruction.CusAuthorizationUsages.AddNew();
		auth1.AGC_Code = authType;
		auth1.AGC_Number = authNumber;
		auth1.AGC_OH_Owner = holder.PK;
	}

	void SetREAData()
	{
		var esCode = Core.Constants.CountryCodes.Spain;

		var helper = new UniversalReferenceTestDataHelper(Factory);

		var impTariffType = helper.CreateNewOrGetExistingTariffType(esCode, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
		Factory.Save();
		var tariff = helper.LoadOrCreateNewTariff(esCode, impTariffType.PK, "2208905400", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
		var reaRateType = helper.CreateNewOrGetExistingRateType(esCode, UniversalReferenceConstants.RateTypeList.REA, "REA - AY Tax Rebate");
		var rateCodeAYD = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RateCodeCodeList.AYD, reaRateType.PK);
		var rateAYD = helper.CreateRate(tariff, rateCodeAYD.PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), rateFormula: "85.0000 * [TNE]");
		helper.CreateCusApplicability(rateAYD, null, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "T001");

		var tariff2 = helper.LoadOrCreateNewTariff(esCode, impTariffType.PK, "2208905401", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
		var reaRateType2 = helper.CreateNewOrGetExistingRateType(esCode, UniversalReferenceConstants.RateTypeList.REA, "REA - AY Tax Rebate");
		var rateCodeAYD2 = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RateCodeCodeList.AYD, reaRateType2.PK);
		var rateAYD2 = helper.CreateRate(tariff2, rateCodeAYD2.PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), rateFormula: "50.0000 * [DTN]");
		helper.CreateCusApplicability(rateAYD2, null, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "T002");
	}

	PreviousDocument GetPreviousDoc(int i, ZString refNumber, ZString subType)
	{
		var prevDoc = Factory.New<PreviousDocument>();
		prevDoc.SuspendValidation();

		prevDoc.CSI_Code = "1234";
		prevDoc.CSI_SubType = subType;
		prevDoc.CSI_ReferenceNumber = refNumber;
		prevDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
		prevDoc.CSI_LineNo = i;
		prevDoc.CSI_Quantity = i * 10;
		prevDoc.CSI_UnitOfQuantity = "BAG";

		return prevDoc;
	}

	AdditionalInfo GetAdditionalInfo(ZString refNumber, ZString kind)
	{
		var addInf = Factory.New<AdditionalInfo>();
		addInf.SuspendValidation();

		addInf.CSI_Code = "1234";
		addInf.CSI_Description = "description";
		addInf.CSI_SubType = kind;
		addInf.CSI_ReferenceNumber = refNumber;
		addInf.CSI_ReferenceNumber2 = refNumber + "Extra";
		addInf.CSI_RX_NKCurrency = "EUR";
		addInf.CSI_Value = 20;
		addInf.CSI_Status = "QWE";

		return addInf;
	}

	protected override Type ExpectedDutyCalculatorStrategyType => typeof(DutyCalculatorStrategy);

	protected override Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(EntryCreationStrategy) };

	protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

	protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration)
		=> new LineMerger((JobDeclaration)declaration);

	protected override Type GetSupportingDocumentType() => typeof(SupportingDocument);

	protected override void ActivateLockEntryLines(EU.Business.Declaration.CusEntryHeader entryHeader) => entryHeader.CH_EntryStatus = MessageProcessorConstants.EntryStatusCodes.Cleared;

	ESUniversalReferenceTestDataHelper SetUpCanaryIslandsCusCodeLists(params (string Code, string Description)[] canaryIslands)
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		var testSetup = canaryIslands.Length == 0 ? CanaryIslandsForTest : canaryIslands;
		foreach (var island in testSetup)
		{
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, island.Code, island.Description);
		}

		return helper;
	}

	static readonly (string Code, string Description)[] CanaryIslandsForTest =
		Enumerable.Range(1, 7).Select(n => ($"6{n}", $"Test 6{n}")).ToArray();

	public void SetUpTariffsWithFormulaPCA()
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		var impTariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Spain, "IMP");
		var esexcTariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Spain, "ESEXC");
		var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.Spain, "EXC");
		Factory.Save();
		var tariff0 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Spain, impTariffType.PK, "2208905400", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Spain, impTariffType.PK, "2208905401", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Spain, impTariffType.PK, "2208905402", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		CreateNewFormula(helper, esexcTariffType.PK, "1CF", "0.5*PCA*[GF]", rateType.PK, impTariffType.PK, tariff0.ZZ1_TariffCode);
		CreateNewFormula(helper, esexcTariffType.PK, "1CF", "0.5*PCA*[GF]", rateType.PK, impTariffType.PK, tariff1.ZZ1_TariffCode);
		CreateNewFormula(helper, esexcTariffType.PK, "A00", "100*[GF]", rateType.PK, impTariffType.PK, tariff2.ZZ1_TariffCode);
		Factory.Save();
	}

	public void SetUpImportTariffWithDuties(string tariffCode, params (string rateCode, string formula)[] duties)
	{
		var builder = DutyReferenceDataConfigurationBuilder.New(Factory)
			.AddTariffType("IMP")
			.AddTariff(tariffCode, taxOrFeeCode: "DTY");

		foreach (var (rateCode, formula) in duties)
		{
			builder.AddRateCode(RateTypeEnum.Duty, rateCode, formula, preference: "100");
		}

		builder.Configure();

		Factory.Save();
	}

	(JobComInvoiceHeader, JobComInvoiceLine, CusEntryInstruction) SetUpInvHeaderInvLineAndEntryInstruction(string subStyle, string style = "")
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = subStyle;
		entryInstruction.CEI_Style = style;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		return (invoice, invoiceLine, entryInstruction);
	}

	(JobComInvoiceHeader, JobComInvoiceLine, JobComInvoiceLine, CusEntryInstruction) SetUpH1SImportDeclaration(string subStyle = EntrySubStyleList.Codes.B, string style = "")
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = style;
		entryInstruction.CEI_SubStyle = subStyle;

		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = "EUR";

		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_PrimaryPreference = "100";

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_PrimaryPreference = "100";

		return (invoice, invoiceLine1, invoiceLine2, entryInstruction);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
	}

	JobDeclaration declaration;

	const string AuthValue1 = "55885001";
	const string AuthValue2 = "55885002";
	const string AuthValue3 = "55885003";
}
