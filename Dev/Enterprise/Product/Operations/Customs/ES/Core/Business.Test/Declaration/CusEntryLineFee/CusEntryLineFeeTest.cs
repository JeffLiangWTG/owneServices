using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLineFee))]
sealed class CusEntryLineFeeTest : EU.Business.Declaration.Testing.CusEntryLineFeeTest<JobDeclaration, CusEntryLine, CusEntryLineFee>
{
	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		var fee = entryLine.Fees.AddNew();

		return fee;
	}

	public void TestAllowZeroOrEmptyAmount()
	{
		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var entryLineFee = entryLine.Fees.AddNew();

			AssertEquals("Zero Amount is allowed for 0 entry instructions", true, entryLineFee.AllowZeroOrEmptyAmount);

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			AssertEquals("Zero Amount is not allowed for not T2C entry instruction", false, entryLineFee.AllowZeroOrEmptyAmount);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("Zero Amount is allowed for T2C entry instruction", true, entryLineFee.AllowZeroOrEmptyAmount);

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;

			AssertEquals("Zero Amount is not allowed for not all entry instructions equals T2C", false, entryLineFee.AllowZeroOrEmptyAmount);

			instruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("Zero Amount is allowed for all entry instructions equals T2C", true, entryLineFee.AllowZeroOrEmptyAmount);
		});
	}

	public void TestIsNationalIndirectTaxationFee()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();
		entryLineFee.CF_ChargeType = "768";
		AssertEquals(true, entryLineFee.IsNationalIndirectTaxationFee);
		entryLineFee.CF_ChargeType = "ABC";
		AssertEquals(false, entryLineFee.IsNationalIndirectTaxationFee);
	}

	public void TestDefaultMethodOfPaymentVATDeferredIfEmpty()
	{
		var declaration = Factory.New<JobDeclaration>();
		var importer = Factory.New<OrgHeader>();
		var addInfoCusImp = ESOrgImpAddInfo.Get(importer);
		declaration.JE_OH_Importer = importer.PK;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var entryLineFee = entryLine.Fees.AddNew();

		entryLineFee.CF_MethodOfPayment = "";
		addInfoCusImp.ZO_VATDeferment = false;
		entryLineFee.CF_ChargeType = "B00";
		entryLineFee.DefaultMethodOfPaymentIfEmpty();
		AssertEquals("Default MoP when charge type is VAT and Importer is not VAT deferred", ZString.Empty, entryLineFee.CF_MethodOfPayment);

		entryLineFee.CF_MethodOfPayment = "";
		addInfoCusImp.ZO_VATDeferment = true;
		entryLineFee.DefaultMethodOfPaymentIfEmpty();
		AssertEquals("Default MoP when charge type is VAT and Importer is VAT deferred", UniversalReferenceConstants.FeeMethodOfPayment.Deferred, entryLineFee.CF_MethodOfPayment);

		entryLineFee.CF_MethodOfPayment = "";
		entryLineFee.CF_ChargeType = "A00";
		entryLineFee.DefaultMethodOfPaymentIfEmpty();
		AssertEquals("Default MoP when charge type is not VAT and Importer is VAT deferred", ZString.Empty, entryLineFee.CF_MethodOfPayment);
	}

	public void TestDefaultMethodOfPaymentNBLIfEmpty()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		var entryLineFee = entryLine.Fees.AddNew();

		entryLineFee.CF_MethodOfPayment = ZString.Empty;
		entryLineFee.CF_ChargeType = "B00";
		entryLineFee.DefaultMethodOfPaymentIfEmpty();

		AssertEquals("Default MoP when Entry Style H2 is NBL by default", FeeMethodOfPayment.NonBillableTax, entryLineFee.CF_MethodOfPayment);

		entryInstruction.CEI_Style = ZString.Empty;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entryLineFee.CF_MethodOfPayment = ZString.Empty;
		entryLineFee.DefaultMethodOfPaymentIfEmpty();
		AssertEquals("Default MoP when Entry SubStyle is T2L is NBL by default", FeeMethodOfPayment.NonBillableTax, entryLineFee.CF_MethodOfPayment);

		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
		entryLineFee.CF_MethodOfPayment = ZString.Empty;
		entryLineFee.DefaultMethodOfPaymentIfEmpty();
		AssertEquals("Default MoP when Entry SubStyle is T2C is NBL by default", FeeMethodOfPayment.NonBillableTax, entryLineFee.CF_MethodOfPayment);

		entryInstruction.CEI_SubStyle = ZString.Empty;
		invoiceLine.ZG_MethodOfPayment = "A";
		entryLineFee.CF_MethodOfPayment = ZString.Empty;
		entryLineFee.DefaultMethodOfPaymentIfEmpty();
		AssertEquals("Default MoP when JobComInvoiceLine MethodOfPayment is A is NBL by default", FeeMethodOfPayment.NonBillableTax, entryLineFee.CF_MethodOfPayment);

		invoiceLine.ZG_MethodOfPayment = "R";
		entryLineFee.CF_MethodOfPayment = ZString.Empty;
		entryLineFee.DefaultMethodOfPaymentIfEmpty();
		AssertEquals("Default MoP when JobComInvoiceLine MethodOfPayment is not A is Empty", ZString.Empty, entryLineFee.CF_MethodOfPayment);

		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;
		entryLineFee.DefaultMethodOfPaymentIfEmpty();
		AssertEquals("Default MoP when Entry Style IM and JobComInvoiceLine MethodOfPayment is not A is Empty", ZString.Empty, entryLineFee.CF_MethodOfPayment);

		invoiceLine.ZG_MethodOfPayment = "A";
		entryLineFee.DefaultMethodOfPaymentIfEmpty();
		AssertEquals("Default MoP when Entry Style IM and JobComInvoiceLine MethodOfPayment is A is NBL by default", FeeMethodOfPayment.NonBillableTax, entryLineFee.CF_MethodOfPayment);
	}

	public void TestDefaultMethodOfPaymentForSimplifiedImportH1Declaration() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var lineFees = GetEntryLineFees(declaration, IMPDeclarationTypeList.Codes.IM, EntrySubStyleList.Codes.A);
		lineFees.ForEach(l => AssertEquals(string.Empty, l.CF_MethodOfPayment));

		lineFees = GetEntryLineFees(declaration, string.Empty, EntrySubStyleList.Codes.B);
		lineFees.ForEach(l => AssertEquals(FeeMethodOfPayment.NonBillableTax, l.CF_MethodOfPayment));

		lineFees = GetEntryLineFees(declaration, IMPDeclarationTypeList.Codes.IM, EntrySubStyleList.Codes.C);
		lineFees.ForEach(l => AssertEquals(FeeMethodOfPayment.NonBillableTax, l.CF_MethodOfPayment));

		lineFees = GetEntryLineFees(declaration, IMPDeclarationTypeList.Codes.IM, EntrySubStyleList.Codes.B, ucc6Version: 0);
		lineFees.ForEach(l => AssertEquals(string.Empty, l.CF_MethodOfPayment));
	});

	IEnumerable<CusEntryLineFee> GetEntryLineFees(JobDeclaration declaration, string ceiStyle, string ceiSubStyle, int ucc6Version = 1)
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = ceiStyle;
		entryInstruction.CEI_SubStyle = ceiSubStyle;

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_AddInfo = $"UCC6Version={ucc6Version}";
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var entryLine = entryHeader.MergedLines.AddNew();

		var entryLineFee1 = entryLine.Fees.AddNew();
		entryLineFee1.CF_ChargeType = "B00";
		entryLineFee1.DefaultMethodOfPaymentIfEmpty();

		var entryLineFee2 = entryLine.Fees.AddNew();
		entryLineFee2.CF_ChargeType = "A00";
		entryLineFee2.DefaultMethodOfPaymentIfEmpty();

		return entryLine.Fees.Select(f => f);
	}

	public void TestChargeAmountRounderType()
	{
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		AssertType<TwoDigitsChargeAmountRounder>($"{nameof(CusEntryLineFee.ChargeAmountRounder)} type", entryLineFee.ChargeAmountRounder);
	}

	public void TestRoundChangeAmount()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();

		entryLineFee.CF_ChargeAmount = 1.3542m;
		AssertEquals("ChargeAmount", 1.35m, entryLineFee.CF_ChargeAmount);

		entryLineFee.CF_ChargeAmount = 1.3555;
		AssertEquals("ChargeAmount", 1.36m, entryLineFee.CF_ChargeAmount);
	}

	public void TestRateDuty()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, MapDirectionList.Codes.BTH, "Description", false);
		helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGM", "KN", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), CountryCodes.Spain);
		Factory.Save();

		var fee = Factory.New<CusEntryLineFee>();
		fee.CF_ChargeType = "A00";
		fee.CF_MethodOfCalculation = ZString.Empty;
		var taxBoxSupporter = (IESDocSADHLineTaxBoxSupporter)fee;

		CombineAssertions(() =>
		{
			AssertEquals("Expected % if empty method of calculation", "%", taxBoxSupporter.RateDuty);

			fee.CF_MethodOfCalculation = "%";
			AssertEquals("Expected % if method of calculation is %", "%", taxBoxSupporter.RateDuty);

			fee.CF_MethodOfCalculation = "KGM";
			AssertEquals("Expected mapped value if method of calculation is not % or empty", "KN", taxBoxSupporter.RateDuty);

			fee.CF_MethodOfCalculation = "DTL";
			AssertEquals("Expected original value if method of calculation is not % or empty but the value is not mapped", "DTL", taxBoxSupporter.RateDuty);
		});
	}

	public void TestDestinationStateIsCanaryIsland() => CombineAssertions(() =>
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");

		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var fee = entryLine.Fees.AddNew();
		var taxBoxSupporter = (IESDocSADHLineTaxBoxSupporter)fee;

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		{
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			AssertEquals("Destination State is Canary Island and not UCC6", true, taxBoxSupporter.DestinationStateIsCanaryIsland);

			declaration.ZG_DestinationState = ZString.Empty;
			AssertEquals("Destination State is not Canary Island and not UCC6", false, taxBoxSupporter.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = "ES003861";
			AssertEquals("Destination State is not canary island, JE_CustomsOffice = 'ES003861', Import and not UCC6", false, taxBoxSupporter.DestinationStateIsCanaryIsland);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Destination State is not canary island, JE_CustomsOffice = 'ES003861', Export and not UCC6", false, taxBoxSupporter.DestinationStateIsCanaryIsland);
		}

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			AssertEquals("Destination State is not canary island, JE_CustomsOffice = 'ES003861', Export and UCC6", false, taxBoxSupporter.DestinationStateIsCanaryIsland);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Destination State is canary island, JE_CustomsOffice = 'ES003861', Import and UCC6", true, taxBoxSupporter.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = ZString.Empty;
			AssertEquals("Destination State is not canary island, JE_CustomsOffice = Empty, Import and UCC6", false, taxBoxSupporter.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = "ES003541";
			AssertEquals("Destination State is canary island, JE_CustomsOffice = 'ES003541', Import and UCC6", true, taxBoxSupporter.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = "ES003712";
			AssertEquals("Destination State is not canary island, JE_CustomsOffice = 'ES003712', Import and UCC6", false, taxBoxSupporter.DestinationStateIsCanaryIsland);

			declaration.JE_CustomsOffice = "ES009998";
			AssertEquals("Destination State is canary island, JE_CustomsOffice = 'ES009998', Import and UCC6", true, taxBoxSupporter.DestinationStateIsCanaryIsland);
		}
	});

	public void TestEntryLineMethodOfPayment()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.ZG_MethodOfPayment = "R";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		invoiceLine.JI_CEI = entryInstruction.PK;
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		var entryLine = entryHeader.MergedLines[0];
		var fee = entryLine.Fees.AddNew();
		var taxBoxSupporter = (IESDocSADHLineTaxBoxSupporter)fee;

		AssertEquals("Expected R as the Invoice Line Method of Payment", "R", taxBoxSupporter.EntryLineMethodOfPayment);
	}

	public void TestEntryLineMethodOfPayment2()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.ZG_MethodOfPayment2 = "A";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		invoiceLine.JI_CEI = entryInstruction.PK;
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders[0];
		var entryLine = entryHeader.MergedLines[0];
		var fee = entryLine.Fees.AddNew();
		var taxBoxSupporter = (IESDocSADHLineTaxBoxSupporter)fee;

		AssertEquals("Expected A as the Invoice Line Method of Payment2", "A", taxBoxSupporter.EntryLineMethodOfPayment2);
	}

	public void TestLookupsType()
	{
		var lineFee = Factory.New<CusEntryLineFee>();
		AssertType<CusEntryLineFeeLookups>("Type", lineFee.Lookups);
	}

	public void TestIsVAT()
	{
		CombineAssertions(() =>
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			lineFee.CF_ChargeType = "";
			AssertEquals("Is VAT", false, lineFee.IsVAT);

			lineFee.CF_ChargeType = "B00";
			AssertEquals("Is VAT", true, lineFee.IsVAT);
		});
	}

	public void TestGetTaxClass()
	{
		CombineAssertions(() =>
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			lineFee.CF_ChargeType = "A00";
			AssertEquals("Canary Islands: TaxClass = CF_ChargeType", "A00", lineFee.GetTaxClass(true));
			AssertEquals("Non Canary Islands: TaxClass = CF_ChargeType", "A00", lineFee.GetTaxClass(false));

			lineFee.CF_ChargeType = "B00";
			AssertEquals("Canary Islands: TaxClass = 3IG (special case for B00)", "3IG", lineFee.GetTaxClass(true));
			AssertEquals("Non Canary Islands: TaxClass = CF_ChargeType", "B00", lineFee.GetTaxClass(false));
		});
	}
}
