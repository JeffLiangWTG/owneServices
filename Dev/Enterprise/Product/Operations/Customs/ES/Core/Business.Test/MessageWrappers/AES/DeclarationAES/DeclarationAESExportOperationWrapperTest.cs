using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationAESExportOperationWrapperTest : WrapperHelperTest<DeclarationAESExportOperationWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if entryHeader is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryHeader"), () => GetWrapper(null, "0", DeclarationMessageTypeList.Codes.ExportUcc6));

				AssertExceptionThrown("Constructor Throws Exception if jobDeclaration is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","Declaration"), () => GetWrapper(Factory.New<CusEntryHeader>(), ZString.Empty, DeclarationMessageTypeList.Codes.ExportUcc6));

				AssertExceptionThrown("Constructor Throws Exception if messageType is empty", typeof(ArgumentException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be empty string (\"\").","messageType"), () => GetWrapper(entryHeader, ZString.Empty, ZString.Empty));
			});
		}

		public void TestLRN()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled LRN with only entry ref num when no declarant with id is declared", "ES00001", wrapper.LRN);

				wrapper = GetWrapper(entryHeader, "2", DeclarationMessageTypeList.Codes.ExportUcc6, isComplementaryCWithMRN: true);
				AssertEquals("Expected filled LRN with entry ref num and _Y when isComplementaryCWithMRN is true", "ES00001_Y", wrapper.LRN);
			});
		}

		public void TestDeclarationType()
		{
			declaration.JE_MessageSubType = "CO";
			AssertEquals("Expected filled DeclarationType", "CO", wrapper.DeclarationType);
		}

		public void TestDeclarationSubType()
		{
			AssertEquals("Expected filled DeclarationSubType when is Declaration AES", "A", wrapper.DeclarationSubType);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			AssertEquals("Expected filled DeclarationSubType when is Declaration AES, C when isComplementaryCWithMRN is false", "C", wrapper.DeclarationSubType);

			wrapper = GetWrapper(entryHeader, "2", DeclarationMessageTypeList.Codes.ExportUcc6, isComplementaryCWithMRN: true);
			AssertEquals("Expected mapped DeclarationSubType when is C, isComplementaryCWithMRN is true and when is Declaration AES", "Y", wrapper.DeclarationSubType);

			wrapper = GetWrapper(entryHeader, "2", DeclarationMessageTypeList.Codes.ExportPreDeclaration);
			AssertEquals("Expected mapped DeclarationSubType when is C and when is PreDeclaration AES", "F", wrapper.DeclarationSubType);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("Expected mapped DeclarationSubType when is B and when is PreDeclaration AES", "E", wrapper.DeclarationSubType);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("Expected mapped DeclarationSubType when is A and when is PreDeclaration AES", "D", wrapper.DeclarationSubType);
		}

		public void TestRecapitulationDate()
		{
			CombineAssertions(() =>
			{
				declaration.ZG_LCPDepart = new ZDateTime(2022, 12, 05, 10, 50, 30);
				AssertEquals("Expected empty RecapitulationDate when declared", new DateTime(2022, 12, 05, 10, 50, 30), wrapper.RecapitulationDate);

				declaration.ZG_LCPDepart = ZDateTime.Empty;
				AssertEquals("Expected default RecapitulationDate when not declared", ZDateTime.Empty, wrapper.RecapitulationDate);
			});
		}

		public void TestRecapitulationDateSpecified()
		{
			CombineAssertions(() =>
			{
				declaration.ZG_LCPDepart = new ZDateTime(2022, 12, 05, 10, 50, 30);
				AssertEquals("Expected false RecapitulationDateSpecified when entryInstruction is not Y nor Z and date is declared", false, wrapper.RecapitulationDateSpecified);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;
				AssertEquals("Expected true RecapitulationDateSpecified when entryInstruction is Y or Z and date is declared", true, wrapper.RecapitulationDateSpecified);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
				AssertEquals("Expected filled RecapitulationDateSpecified when EntryInstruction is C and isComplementaryCWithMRN is false", false, wrapper.RecapitulationDateSpecified);

				wrapper = GetWrapper(entryHeader, "2", DeclarationMessageTypeList.Codes.ExportUcc6, isComplementaryCWithMRN: true);
				AssertEquals("Expected empty RecapitulationDateSpecified when EntryInstruction is C but isComplementaryCWithMRN is true", true, wrapper.RecapitulationDateSpecified);

				declaration.ZG_LCPDepart = ZDateTime.Empty;
				wrapper = GetWrapper(entryHeader, "2", DeclarationMessageTypeList.Codes.ExportUcc6);
				AssertEquals("Expected false RecapitulationDateSpecified when date is not declared", false, wrapper.RecapitulationDateSpecified);
			});
		}

		public void TestSecurityFlag()
		{
			AssertEquals("Expected filled SecurityFlag", "2", wrapper.SecurityFlag);
		}

		public void TestSpecificCircumstance()
		{
			CombineAssertions(() =>
			{
				declaration.ZG_SpecificCircumstanceIndicator = "AAA";
				AssertEquals("Expected empty SpecificCircumstance when it is not A20", ZString.Empty, wrapper.SpecificCircumstance);

				declaration.ZG_SpecificCircumstanceIndicator = "A20";
				AssertEquals("Expected filled SpecificCircumstance when it is A20", "A20", wrapper.SpecificCircumstance);
			});
		}

		public void TestTotalAmount()
		{
			CombineAssertions(() =>
			{
				invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";

				invoiceLine.JI_LinePrice = 1.12m;
				AssertEquals("Expected filled TotalAmount with 1 invoice line", 1.12m, wrapper.TotalAmount);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 2.32m;

				var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge", true, mergeResult);
				entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals("Expected filled TotalAmount with 2 invoice lines", 3.44m, wrapper.TotalAmount);

				invoiceHeader.JZ_RX_NKInvoice_Currency = "000";
				AssertEquals("Expected 0 TotalAmount when currency is 000", ZDecimal.Zero, wrapper.TotalAmount);
			});
		}

		public void TestCurrency()
		{
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			AssertEquals("Expected filled Currency", "EUR", wrapper.Currency);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "11";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "ES00001";

			wrapper = GetWrapper(entryHeader, "2", DeclarationMessageTypeList.Codes.ExportUcc6);
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		DeclarationAESExportOperationWrapper wrapper;

		DeclarationAESExportOperationWrapper GetWrapper(CusEntryHeader entryHeader, ZString securityCode, ZString messageType, bool isComplementaryCWithMRN = false) => new DeclarationAESExportOperationWrapper(entryHeader, securityCode, messageType, isComplementaryCWithMRN);

		protected override DeclarationAESExportOperationWrapper GetProvider() => wrapper;
	}
}
