using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using Enterprise.MasterFiles.Business;
using ITCusEntryHeader = Enterprise.Customs.IT.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.IT.Business.Testing;

[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Italy)]
sealed class ITDocSADHTest : DocSADHTest
{
	public void TestShowEpuEnoDoeLabelsText()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals("Print EPU, ENO, DOE Labels", false, wrapper.ShowEpuEnoDoeLabelsText);
	}

	public void TestShowEpuEnoDoeLabelsTextOnBIS()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals("On BIS pages, EPU, ENO, DOE row never visible in IT", false, wrapper.ShowEpuEnoDoeLabelsTextOnBIS);
	}

	public void TestBox54Details_Box54Date()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals("When Instruction is null, empty string is expected", ZString.Empty, wrapper.Box54Date);

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals("When Instruction is available, but no CEI_DateForDuty has been provided, empty string is expected", ZString.Empty, wrapper.Box54Date);

		var yesterday = ZDate.Today.AddDays(-1);
		entryInstruction.CEI_DateForDuty = yesterday;
		wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals("Box 54 date must be equal to CEI_DateForDuty", yesterday.ToString(), wrapper.Box54Date);
	}

	public void TestLines()
	{
		var wrapper = GetNewDocumentWrapper() as ITDocSADH;
		AssertEquals(typeof(ITDocSADHLineCollection), wrapper.Lines.GetType());
	}

	public void TestPages()
	{
		var wrapper = GetNewDocumentWrapper() as ITDocSADH;
		AssertEquals(typeof(ITDocSADHPageCollection), wrapper.Pages.GetType());
	}

	public void TestBoxA()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunZZZPK = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZPK);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE503199", "VERDEN", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE503099", "ZENTRALEORTVONDEUTSCHLANDUNDOESTERREICH", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

		Factory.Save();

		var today = ZDate.Today;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_DateForDuty = today;
		entryInstruction1.CEI_SubStyle = "B";

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;

			var officeCode = Factory.New<EuOfficeCode>();
			declaration.CustomsOffices.Add(officeCode);
			officeCode.CY_Code = "ENT";
			officeCode.CY_Data = "DE503199";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var wrapper = ITDocSADH.New(entryHeader, Factory);

		CombineAssertions("TestBoy A", () =>
		{
			declaration.JE_CustomsOffice = "DE503199";
			AssertEquals("Box A", "DE503199 - VERDEN", wrapper.BoxA);

			declaration.JE_CustomsOffice = "";
			AssertEquals("Box A", ZString.Empty, wrapper.BoxA);

			declaration.JE_CustomsOffice = "DE503099";
			AssertEquals("Box A", "DE503099 - ZENTRALEORTVONDEUTSCHLANDUNDOE", wrapper.BoxA);
		});
	}

	public override void TestBox12ValueDetails()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = ITDocSADH.New(entryHeader, Factory);
		entryHeader.CH_FreightAdjustment = -2.12m;

		AssertEquals("CH_FreightAdjustment", "-2.12", wrapper.Box12ValueDetails);
	}

	public void TestGetBox16CountryOfOriginEvaluatorBasedOnMessageType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var sadhWrapperForTest = new ITDocSADHForTest(entryHeader, Factory);

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = "IMP";
			AssertType<SingleOrEmptyIfMultipleBox16CountryOfOriginEvaluator>("Box16CountryOfOriginEvaluator for IMP", sadhWrapperForTest.GetBox16CountryOfOriginEvaluatorExposed());

			declaration.JE_MessageType = "EXP";
			AssertType<EmptyBox16CountryOfOriginEvaluator>("Box16CountryOfOriginEvaluator for EXP", sadhWrapperForTest.GetBox16CountryOfOriginEvaluatorExposed());
		});
	}

	public void TestBox16CountryOfOriginForImportDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoice = declaration.Invoices.AddNew();

		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();

		var entryLine = entryHeader.MergedLines.AddNew();

		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine2.JI_CL = entryLine.PK;

		invoiceLine1.JI_CountryOfOrigin = "ES";
		invoiceLine2.JI_CountryOfOrigin = "ES";

		var wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals("Same Country of origin for entries", "Spain", wrapper.Box16CountryOfOrigin);

		invoiceLine2.JI_CountryOfOrigin = "IT";
		AssertEquals("Different Country of origin for entries", ZString.Empty, wrapper.Box16CountryOfOrigin);
	}

	public void TestBox16CountryOfOriginForImportDeclarationWithEmptyOrInvalidCodes()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoice = declaration.Invoices.AddNew();

		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();

		var entryLine = entryHeader.MergedLines.AddNew();

		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine2.JI_CL = entryLine.PK;

		invoiceLine1.JI_CountryOfOrigin = "ES";
		invoiceLine2.JI_CountryOfOrigin = ZString.Empty;

		var wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals("One Country empty", ZString.Empty, wrapper.Box16CountryOfOrigin);

		invoiceLine1.JI_CountryOfOrigin = ZString.Empty;
		invoiceLine2.JI_CountryOfOrigin = ZString.Empty;
		AssertEquals("Two Countries empty", ZString.Empty, wrapper.Box16CountryOfOrigin);

		invoiceLine1.JI_CountryOfOrigin = "ES";
		invoiceLine2.JI_CountryOfOrigin = "XX";
		AssertEquals("One Country invalid", ZString.Empty, wrapper.Box16CountryOfOrigin);

		invoiceLine1.JI_CountryOfOrigin = "XX";
		invoiceLine2.JI_CountryOfOrigin = "XX";
		AssertEquals("Two Countries invalid", ZString.Empty, wrapper.Box16CountryOfOrigin);
	}

	public void TestBox16CountryOfOriginForExportDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals("Box16CountryOfOrigin", ZString.Empty, wrapper.Box16CountryOfOrigin);
	}

	public void TestBox17bImporterState()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_RL_NKFinalDestination = "ITBRI";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		declaration.JE_MessageType = "IMP";
		var wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals("Province of Destination", "BA", wrapper.Box17ImporterState);

		declaration.JE_MessageType = "EXP";
		AssertEquals("Province of Destination", "", wrapper.Box17ImporterState);
	}

	public void TestBox28FinancialAndBankingData()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		var invoiceHeader1 = declaration.Invoices.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.FinancialAndBankingDataLine1 = "line1";
		entryInstruction.FinancialAndBankingDataLine2 = "line2";
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_CL = entryLine1.PK;

		var wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals("Financial and Banking data line 1", "line1", wrapper.Box28FinancialAndBankingDataLine1);
		AssertEquals("Financial and Banking data line 2", "line2", wrapper.Box28FinancialAndBankingDataLine2);
	}

	public void TestBox29OfficeOfExit()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunZZZPK = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZPK);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT018100", "Bari", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		declaration.CustomsOffices.RemoveAndDeleteAll();
		declaration.JE_CustomsOffice = "IT017000";
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IT018100");

		declaration.JE_MessageType = "EXP";
		var wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals("Box29ExitOffice Export case", "IT018100 Bari", wrapper.Box29ExitOffice);
	}

	public void TestBox35GrossWeightInKG()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_Weight = 555.000m;
		invoiceHeader.JZ_WeightUQ = Core.Constants.Weight.Kilograms;

		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_Weight = 100123.456m;
		invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Grams;

		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_Weight = 200.00m;
		invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;

		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine2.JI_CL = entryLine.PK;

		CombineAssertions("When declaration is EXP", () =>
		{
			invoiceLine1.JI_Weight = 100.345m;
			invoiceLine2.JI_Weight = 200.00m;

			declaration.JE_MessageType = "EXP";
			var wrapper = ITDocSADH.New(entryHeader, Factory);
			AssertEquals("For EXP declaration, invoice header weight is taken", "200.10035", wrapper.Box35GrossWeightInKG);

			invoiceLine1.JI_Weight = 0.00m;
			invoiceLine2.JI_Weight = 0.00m;
			AssertEquals("For EXP declaration, if sum of weights is 0 then print empty string", ZString.Empty, wrapper.Box35GrossWeightInKG);
		});

		CombineAssertions("When declaration is IMP", () =>
		{
			declaration.JE_MessageType = "IMP";
			var wrapper = ITDocSADH.New(entryHeader, Factory);
			AssertEquals("For IMP declaration, invoice header weight is taken", "555", wrapper.Box35GrossWeightInKG);
		});
	}

	public void TestBox35GrossWeightInKGWithMultipleEntryLinesInEXPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_Weight = 10.000m;
		invoiceHeader.JZ_WeightUQ = Core.Constants.Weight.Kilograms;

		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_Weight = 0.030m;
		invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;

		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_Weight = 0.020m;
		invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;

		var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine3.JI_Weight = 3.000m;
		invoiceLine3.JI_WeightUQ = Core.Constants.Weight.Kilograms;

		var entryLine1 = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine2.JI_CL = entryLine1.PK;

		var entryLine2 = entryHeader.MergedLines.AddNew();
		invoiceLine3.JI_CL = entryLine2.PK;

		CombineAssertions("When declaration is EXP", () =>
		{
			declaration.JE_MessageType = "EXP";
			var wrapper = ITDocSADH.New(entryHeader, Factory);
			AssertEquals("For EXP declaration, sum of entrylines weight is taken", "3.05", wrapper.Box35GrossWeightInKG);
		});
	}

	public void TestBox35GrossWeightInKGWithNoEntryLinesInEXPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		CombineAssertions("When declaration is EXP", () =>
		{
			declaration.JE_MessageType = "EXP";
			var wrapper = ITDocSADH.New(entryHeader, Factory);
			AssertNoExceptionThrown("EntryHeader with no lines must not throw exceptions", () => { var value = wrapper.Box35GrossWeightInKG; });

			AssertEquals("For EXP declaration with empty Entry Header, empty string is printed", ZString.Empty, wrapper.Box35GrossWeightInKG);
		});
	}

	public void TestRegistrationInfo()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = "BLT";
		declaration.JE_MessageType = "IMP";
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = ITDocSADH.New(entryHeader1, Factory);
		AssertEquals(ZString.Empty, wrapper.RegistrationInfo);

		var cusEntryNumber = Factory.New<CusEntryNumber>();
		cusEntryNumber.CE_EntryType = "REG";
		cusEntryNumber.CE_ParentID = entryHeader1.PK;
		cusEntryNumber.CE_ParentTable = entryHeader1.TableName;
		cusEntryNumber.CE_Category = "CUS";
		cusEntryNumber.CE_EntryNum = "4 T-2343G";
		cusEntryNumber.CE_IssueDate = new ZDateTime(2020, 01, 01);
		wrapper = ITDocSADH.New(entryHeader1, Factory);
		AssertEquals("4 T-2343G del 01/01/2020", wrapper.RegistrationInfo);
	}

	public void TestLinesWithAttachment()
	{
		var declaration = Factory.New<JobDeclaration>();
		var container1 = declaration.CusContainers.AddNew();
		container1.CO_ContainerNumber = "CNT1";
		var container2 = declaration.CusContainers.AddNew();
		container2.CO_ContainerNumber = "CNT2";
		var container3 = declaration.CusContainers.AddNew();
		container3.CO_ContainerNumber = "CNT3";
		var container4 = declaration.CusContainers.AddNew();
		container4.CO_ContainerNumber = "CNT4";
		var container5 = declaration.CusContainers.AddNew();
		container5.CO_ContainerNumber = "CNT5";
		var container6 = declaration.CusContainers.AddNew();
		container6.CO_ContainerNumber = "CNT6";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		var wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals(0, wrapper.LinesWithAttachment.Count);

		invoiceLine.ContainersPivot.AddNew().C2_CO = container1.PK;
		invoiceLine.ContainersPivot.AddNew().C2_CO = container2.PK;
		invoiceLine.ContainersPivot.AddNew().C2_CO = container3.PK;
		invoiceLine.ContainersPivot.AddNew().C2_CO = container4.PK;
		invoiceLine.ContainersPivot.AddNew().C2_CO = container5.PK;
		wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals(0, wrapper.LinesWithAttachment.Count);

		invoiceLine.ContainersPivot.AddNew().C2_CO = container6.PK;
		wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals(1, wrapper.LinesWithAttachment.Count);
	}

	public void TestAgentsReference()
	{
		var documentWrapper = (ITDocSADH)GetNewDocumentWrapper();
		var declaration = documentWrapper.Declaration;
		AssertEquals(ZString.Empty, documentWrapper.AgentsReference);

		declaration.JE_AgentsReference = "Agents Ref";
		AssertEquals("Agents Ref", documentWrapper.AgentsReference);
	}

	public void TestBox30LocationOfGoods_WhenImport()
	{
		var documentWrapper = (ITDocSADH)GetNewDocumentWrapper();
		var declaration = documentWrapper.Declaration;
		declaration.JE_MessageType = "IMP";
		var entryHeader = documentWrapper.EntryHeader;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		declaration.JE_LocationOfGoods = "30LOC";
		entryInstruction.ElectronicDocuments = ZBool.True;
		AssertEquals("30LOC-FE", documentWrapper.Box30LocationOfGoods);
		entryInstruction.ElectronicDocuments = ZBool.False;
		AssertEquals("30LOC", documentWrapper.Box30LocationOfGoods);
		declaration.JE_LocationOfGoods = ZString.Empty;
		entryInstruction.ElectronicDocuments = ZBool.True;
		AssertEquals("FE", documentWrapper.Box30LocationOfGoods);
		declaration.JE_LocationOfGoods = " ";
		entryInstruction.ElectronicDocuments = ZBool.True;
		AssertEquals("FE", documentWrapper.Box30LocationOfGoods);
	}

	public void TestBox30LocationOfGoods_WhenExport()
	{
		var documentWrapper = (ITDocSADH)GetNewDocumentWrapper();
		var declaration = documentWrapper.Declaration;
		declaration.JE_MessageType = "EXP";
		var entryHeader = documentWrapper.EntryHeader;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			CombineAssertions("When Export And UCC6", () =>
			{
				declaration.GoodsLocation.CGL_Qualifier = "V";
				declaration.GoodsLocation.CGL_Type = "B";
				declaration.GoodsLocation.CGL_CustomsOffice = "AAAAAA";
				AssertEquals("When Qualifier is 'V'.", "V-B-AAAAAA", documentWrapper.Box30LocationOfGoods);

				declaration.GoodsLocation.CGL_Qualifier = "Y";
				declaration.GoodsLocation.CGL_Type = "C";
				declaration.GoodsLocation.CGL_AdditionalIdentifier = "BBBBBB";
				AssertEquals("When Qualifier is 'Y'.", "Y-C-BBBBBB", documentWrapper.Box30LocationOfGoods);

				declaration.GoodsLocation.CGL_Qualifier = "Z";
				declaration.GoodsLocation.CGL_Type = "D";
				declaration.GoodsLocation.Address.E2_City = "CCCCCC";
				AssertEquals("When Qualifier is 'Z'.", "Z-D-CCCCCC", documentWrapper.Box30LocationOfGoods);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
		{
			CombineAssertions("When Export Non UCC6", () =>
			{
				declaration.JE_LocationOfGoods = "30LOC";
				entryInstruction.ElectronicDocuments = ZBool.True;
				AssertEquals("30LOC-FE", documentWrapper.Box30LocationOfGoods);
				entryInstruction.ElectronicDocuments = ZBool.False;
				AssertEquals("30LOC", documentWrapper.Box30LocationOfGoods);
				declaration.JE_LocationOfGoods = ZString.Empty;
				entryInstruction.ElectronicDocuments = ZBool.True;
				AssertEquals("FE", documentWrapper.Box30LocationOfGoods);
				declaration.JE_LocationOfGoods = " ";
				entryInstruction.ElectronicDocuments = ZBool.True;
				AssertEquals("FE", documentWrapper.Box30LocationOfGoods);
			});
		}
	}

	public void TestBoxDControlResult()
	{
		var documentWrapper = (ITDocSADH)GetNewDocumentWrapper();
		AssertEquals(ZString.Empty, documentWrapper.BoxDControlResult);

		documentWrapper = (ITDocSADH)GetNewDocumentWrapper();
		var entryHeader = documentWrapper.EntryHeader;
		var cusEntryNumber = Factory.New<CusEntryNumber>();
		cusEntryNumber.CE_EntryType = "CLR";
		cusEntryNumber.CE_ParentID = entryHeader.PK;
		cusEntryNumber.CE_ParentTable = entryHeader.TableName;
		cusEntryNumber.CE_Category = "CUS";
		cusEntryNumber.CE_EntryNum = "123456X";
		AssertEquals("Dichiarazione considerata conforme - Codice di svincolo: 123456X", documentWrapper.BoxDControlResult);
	}

	public void TestBoxDSignature()
	{
		var documentWrapper = (ITDocSADH)GetNewDocumentWrapper();
		AssertEquals("Trasmissione telematica ai sensi dell'art.6 p.1 del Reg.UE 952/13", documentWrapper.BoxDSignature);
	}

	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		return ITDocSADH.New(entryHeader, Factory);
	}

	public void TestLayoutStyle2()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var documentWrapper = ITDocSADH.New(entryHeader, Factory);

		declaration.DocumentSupporter.SadDocumentSupporter.LayoutStyle = "";
		CombineAssertions("When LayoutStyle is empty, default EU Values", () =>
		{
			AssertEquals("LayoutStyle2", "6", documentWrapper.LayoutStyle2);
			AssertEquals("LayoutStyl21Description", "Copy for the country of destination", documentWrapper.LayoutStyle2Description);
		});

		declaration.DocumentSupporter.SadDocumentSupporter.LayoutStyle = "8R";
		CombineAssertions("When LayoutStyle is 8R and Declaration is Import", () =>
		{
			AssertEquals("LayoutStyle2", "8R", documentWrapper.LayoutStyle2);
			AssertEquals("LayoutStyle2Description", "Esemplare per il riscontro", documentWrapper.LayoutStyle2Description);
		});

		declaration.JE_MessageType = "EXP";
		declaration.DocumentSupporter.SadDocumentSupporter.LayoutStyle = "8R";
		CombineAssertions("When LayoutStyle is 8R and Declaration is Export", () =>
		{
			AssertEquals("LayoutStyle2", "8R", documentWrapper.LayoutStyle2);
			AssertEquals("LayoutStyle2Description", "", documentWrapper.LayoutStyle2Description);
		});
	}

	public void TestBoxABarcode()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var documentWrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals($"When entryHeader has no entry numbers, {nameof(ITDocSADH.BoxABarcode)}", "", documentWrapper.BoxABarcode);

		entryHeader.MovementReferenceNumberSetter("20ITQ3J08000387XXX");
		documentWrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals(nameof(ITDocSADH.BoxABarcode), "20ITQ3J08000387XXX", documentWrapper.BoxABarcode);
	}

	public void TestBoxBAccountingDetails()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		CombineAssertions("Assert when message type is EXP/COM/IMP", () =>
		{
			var documentWrapper = ITDocSADH.New(entryHeader, Factory);
			var expectedEmptyAccountingDetails = @"

EUR 0.00
EUR 0.00";
			AssertEquals(nameof(ITDocSADH.BoxBAccountingDetails), expectedEmptyAccountingDetails, documentWrapper.BoxBAccountingDetails);

			Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-95870G", issueDate: new ZDateTime(2021, 06, 01));
			documentWrapper = ITDocSADH.New(entryHeader, Factory);
			var expectedBoxBAccountingDetails = @"REG. 4 T-95870G 01/06/2021

EUR 0.00
EUR 0.00";
			AssertEquals(nameof(ITDocSADH.BoxBAccountingDetails), expectedBoxBAccountingDetails, documentWrapper.BoxBAccountingDetails);

			var entryPayInfoCollection = entryHeader.EntryPayInfos;

			declaration.JE_DefermentAccountNumber = "123456A";
			entryPayInfoCollection.InsertOrUpdateEntryPayInfo(predicate: x => x.MethodOfPayment == "T", totalAmount: 1m, transactionType: "4 T", expirationDate: new ZDateTime(2021, 06, 30), "45678", "T");
			documentWrapper = ITDocSADH.New(entryHeader, Factory);
			expectedBoxBAccountingDetails = @"REG. 4 T-95870G 01/06/2021
ACCOUNT N. 123456A - A93 N. 45678
EUR 0.00
EUR 0.00";
			AssertEquals(nameof(ITDocSADH.BoxBAccountingDetails), expectedBoxBAccountingDetails, documentWrapper.BoxBAccountingDetails);

			entryPayInfoCollection.InsertOrUpdateEntryPayInfo(predicate: x => x.MethodOfPayment == "G", totalAmount: 55555.55m, transactionType: "4 T", expirationDate: new ZDateTime(2021, 06, 30), "45678", "G");
			documentWrapper = ITDocSADH.New(entryHeader, Factory);
			expectedBoxBAccountingDetails = @"REG. 4 T-95870G 01/06/2021
ACCOUNT N. 123456A - A93 N. 45678
EUR 55555.55 EXP. 30/06/2021
EUR 0.00";
			AssertEquals(nameof(ITDocSADH.BoxBAccountingDetails), expectedBoxBAccountingDetails, documentWrapper.BoxBAccountingDetails);

			entryPayInfoCollection.InsertOrUpdateEntryPayInfo(predicate: x => x.MethodOfPayment == "F", totalAmount: 222.22m, transactionType: "4 T", expirationDate: new ZDateTime(2021, 06, 30), "45678", "F");
			documentWrapper = ITDocSADH.New(entryHeader, Factory);
			expectedBoxBAccountingDetails = @"REG. 4 T-95870G 01/06/2021
ACCOUNT N. 123456A - A93 N. 45678
EUR 55555.55 EXP. 30/06/2021
EUR 222.22 EXP. 30/06/2021";
			AssertEquals(nameof(ITDocSADH.BoxBAccountingDetails), expectedBoxBAccountingDetails, documentWrapper.BoxBAccountingDetails);

			declaration.JE_MessageType = "COM";
			documentWrapper = ITDocSADH.New(entryHeader, Factory);
			AssertEquals(nameof(ITDocSADH.BoxBAccountingDetails), expectedBoxBAccountingDetails, documentWrapper.BoxBAccountingDetails);

			declaration.JE_MessageType = "IMP";
			entryPayInfoCollection.InsertOrUpdateEntryPayInfo(predicate: x => x.MethodOfPayment == "E", totalAmount: 333.33m, transactionType: "4 T", expirationDate: new ZDateTime(2021, 06, 30), "45678", "E");
			var impDocumentWrapper = ITDocSADH.New(entryHeader, Factory);
			expectedBoxBAccountingDetails = @"REG. 4 T-95870G 01/06/2021
ACCOUNT N. 123456A - A93 N. 45678
EUR 55555.55 EXP. 30/06/2021
EUR 333.33 EXP. 30/06/2021";
			AssertEquals($"When Message Type is IMP, {nameof(ITDocSADH.BoxBAccountingDetails)}", expectedBoxBAccountingDetails, impDocumentWrapper.BoxBAccountingDetails);
		});
	}

	public void TestBoxCOfficeOfDeparture()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunZZZPK = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZPK);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT304100", "PESCARA", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		declaration.CustomsOffices.RemoveAndDeleteAll();
		CombineAssertions("Assert when message type is EXP", () =>
		{
			var documentWrapper = ITDocSADH.New(entryHeader, Factory);
			AssertEquals(nameof(ITDocSADH.BoxCOfficeOfDeparture), "", documentWrapper.BoxCOfficeOfDeparture);

			Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-2343G", issueDate: new ZDateTime(2021, 06, 01), "304100");
			entryHeader.MovementReferenceNumberSetter("20ITQ3J08000387XXX");
			documentWrapper = ITDocSADH.New(entryHeader, Factory);
			var expectedBoxCOfficeOfDeparture = @"304100 - PESCARA

REG: 4 T-2343G
DEL 01/06/2021";
			AssertEquals(nameof(ITDocSADH.BoxCOfficeOfDeparture), expectedBoxCOfficeOfDeparture, documentWrapper.BoxCOfficeOfDeparture);
		});

		declaration.JE_MessageType = "IMP";
		var impDocumentWrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals($"When Message Type is IMP, {nameof(ITDocSADH.BoxCOfficeOfDeparture)}", "", impDocumentWrapper.BoxCOfficeOfDeparture);
	}

	public void TestBox18IdentityOfTransportAtDepartureLineForIMPUCC6_COM_EXPNonUCC6Declarations()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var documentWrapper = ITDocSADH.New(entryHeader, Factory);

		CombineAssertions("Box18 value", () =>
		{
			AssertBox18IdentityOfTransportAtDepartureForIMPUCC6_EXPNonUCC6Declarations(
				declaration,
				documentWrapper,
				"IMP",
				"AB 123CD",
				ZString.Empty);

			AssertBox18IdentityOfTransportAtDepartureForIMPUCC6_EXPNonUCC6Declarations(
				declaration,
				documentWrapper,
				"EXP",
				"AB 123CD",
				ZString.Empty);
		});
	}

	public void TestBox18IdentityOfTransportAtDepartureLineFormattedForEXPUCC6Declarations()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var documentWrapper = ITDocSADH.New(entryHeader, Factory);

		CombineAssertions("Box18 value for EXP", () =>
		{
			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				documentWrapper,
				"AIR",
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty);

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				documentWrapper,
				"AIR",
				"XYZ-10001000101",
				ZString.Empty,
				ZString.Empty,
				"XYZ-10001000101");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				documentWrapper,
				"RAI",
				"RAI-10001000101",
				ZString.Empty,
				ZString.Empty,
				"RAI-10001000101");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				documentWrapper,
				"FIX",
				"XYZ-10001000101",
				ZString.Empty,
				ZString.Empty,
				"XYZ-10001000101");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				documentWrapper,
				"AIR",
				"XYZ-10001000101",
				"KA29",
				ZString.Empty,
				"KA29");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				documentWrapper,
				"AIR",
				ZString.Empty,
				"KA29",
				ZString.Empty,
				"KA29");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				documentWrapper,
				"RAI",
				"RAI-10001000101",
				"KA29",
				"RAI29",
				"RAI-10001000101");

			AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
				declaration,
				documentWrapper,
				"RAI",
				ZString.Empty,
				"KA29",
				"RAI29",
				"RAI29");
		});
	}

	void AssertBox18IdentityOfTransportAtDepartureForIMPUCC6_EXPNonUCC6Declarations(
		JobDeclaration declaration,
		ITDocSADH wrapper,
		string messageType,
		string box18TransportID,
		ZString expectedValueWhenNoID)
	{
		declaration.JE_MessageType = messageType;
		declaration.ZG_Box18TransportID = box18TransportID;

		AssertEquals(
			$"For {declaration.JE_MessageType} when Box18TransportID available",
			box18TransportID,
			wrapper.Box18IdentityOfTransportAtDeparture);

		declaration.ZG_Box18TransportID = "";
		AssertEquals(
			$"For {declaration.JE_MessageType} when Box18TransportID not available",
			expectedValueWhenNoID,
			wrapper.Box18IdentityOfTransportAtDeparture);
	}

	void AssertBox18IdentityOfTransportAtDepartureForEXPUCC6Declarations(
		JobDeclaration declaration,
		ITDocSADH wrapper,
		ZString transportModeInland,
		ZString transportIDInland,
		ZString aircraftRegistrationInland,
		ZString trailer1RegNo,
		ZString expectedValue)
	{
		declaration.JE_MessageType = "EXP";
		declaration.MessageVersion = "XML";
		declaration.ZG_Box18TransportID = "AB 123CD";
		declaration.JE_TransportModeInland = transportModeInland;
		declaration.JE_TransportIDInland = transportIDInland;
		declaration.JE_AircraftRegistrationInland = aircraftRegistrationInland;
		declaration.JE_Trailer1RegNo = trailer1RegNo;

		AssertEquals(
			$@"When JE_TransportModeInland is {transportModeInland},JE_TransportIDInland is {transportIDInland},
				JE_AircraftRegistrationInland is {aircraftRegistrationInland} and JE_Trailer1RegNo is {trailer1RegNo}",
			expectedValue, wrapper.Box18IdentityOfTransportAtDeparture);
	}

	public void TestBox18NationalityOfTransportAtDeparture()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = ITDocSADH.New(entryHeader, Factory);

		declaration.ZG_Box18TransportNationality = "CN";

		CombineAssertions("Box18TransportNationalityAtDeparture value", () =>
		{
			AssertEquals("When MessageType is EXP", "CN", wrapper.Box18TransportNationalityAtDeparture);

			declaration.JE_MessageType = "IMP";

			AssertEquals("When MessageType is IMP", "CN", wrapper.Box18TransportNationalityAtDeparture);
		});
	}

	public void TestIssuingDate()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var number = Factory.New<CusEntryNumber>();
		number.CE_IssueDate = new ZDateTime(2021, 06, 28);
		number.CE_ParentID = entryHeader.PK;
		number.CE_ParentTable = ITCusEntryHeader.Schema.TableName;
		number.CE_EntryType = "MRN";
		number.CE_Category = "CUS";

		var wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals("28/06/2021", wrapper.IssuingDate);
	}

	public void TestShouldShowNotInEcsCaptionIfNeeded()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapper = ITDocSADH.New(entryHeader, Factory);

		AssertEquals(nameof(ITDocSADH.ShouldShowNotInEcsCaptionIfNeeded), ZBool.False, wrapper.ShouldShowNotInEcsCaptionIfNeeded);
	}

	public void TestBisPageTraderBox()
	{
		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = ITDocSADH.New(entryHeader, Factory);

			declaration.JE_MessageType = "EXP";
			AssertType<SupplierBisPageTraderBox>("When MessageType is EXP, BisPageTraderBox Type", wrapper.BisPageTraderBox);

			declaration.JE_MessageType = "XYZ";
			AssertType<ImporterBisPageTraderBox>("When MessageType is not EXP, BisPageTraderBox Type", wrapper.BisPageTraderBox);
		});
	}

	public void TestBox54SignatoryNameAndPosition()
	{
		CreateBroker("ABC");
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_GS_NKCusAgent = "ABC";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals(nameof(ITDocSADH.Box54SignatoryNameAndPosition), "", wrapper.Box54SignatoryNameAndPosition);
	}

	public void TestBox54SignatoryContactDetails()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.Branch.GB_Phone = "0420 019 999";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals(nameof(ITDocSADH.Box54SignatoryContactDetails), "", wrapper.Box54SignatoryContactDetails);
	}

	public void TestBoxC()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_CustomsOffice = "IT134000";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		declaration.JE_MessageType = "IMP";
		var docSadH = ITDocSADH.New(entryHeader, Factory);
		AssertEquals(nameof(docSadH.BoxC), "", docSadH.BoxC);

		declaration.JE_MessageType = "EXP";
		docSadH = ITDocSADH.New(entryHeader, Factory);
		AssertEquals(nameof(docSadH.BoxC), "IT134000", docSadH.BoxC);
	}

	public override void TestBox17CountryOfDestination()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: grouping);

		helper.CreateNewOrGetExistingCusCodeType(Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "Origin country/territory for entry style CO");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy
			, Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17
			, "XK"
			, "Kosovo"
			, ZDateTime.BrettsBirthday
			, ZDateTime.Now.AddMonths(2)
			, RefCusCodeListAttributeTypes.Codes.Direction
			, Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.JE_EntryStyle = "CO";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Kosovo;
		var wrapper = ITDocSADH.New(entryHeader, Factory);
		AssertEquals("Test Box17 Country Code data", "Kosovo", wrapper.Box17CountryOfDestination);
	}

	public override void TestBox14DeclarantRepresentative()
	{
		var factory = Factory;
		var representativeOrgHeader = factory.New<OrgHeader>();
		representativeOrgHeader.OH_FullName = "Representative FullName";
		var representativeAddress = representativeOrgHeader.Addresses.AddNew();
		representativeAddress.OA_Address1 = "Representative Address Line";
		representativeOrgHeader.CustomsCodes.AddNew("EOR", "111111", "IT");

		var declarantOrgHeader = factory.New<OrgHeader>();
		declarantOrgHeader.OH_FullName = "declarant FullName";
		var declarantAddress = declarantOrgHeader.Addresses.AddNew();
		declarantAddress.OA_Address1 = "declarant Address";
		declarantOrgHeader.CustomsCodes.AddNew("EOR", "222222", "IT");

		var declaration = factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var wrapper = ITDocSADH.New(entryHeader, factory);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
		{
			AssertEquals("When not UCC6,Representative and Declarant are empty", "EDI", wrapper.Box14DeclarantRepresentative.CompanyName.Left(3));
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			declaration.JE_OA_Representative = representativeAddress.PK;
			wrapper = ITDocSADH.New(entryHeader, factory);

			CombineAssertions("When not UCC6 and not Self", () =>
			{
				AssertEquals(true, wrapper.ShowDeclarantRepresentativeAddress);
				AssertEquals(declarantOrgHeader.OH_FullName, wrapper.Box14DeclarantRepresentative.CompanyName);
				AssertEquals(declarantAddress.OA_Address1, wrapper.Box14DeclarantRepresentative.Address1);
			});

			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			wrapper = ITDocSADH.New(entryHeader, factory);
			CombineAssertions("When not UCC6 and Self", () =>
			{
				AssertEquals(false, wrapper.ShowDeclarantRepresentativeAddress);
				AssertEquals(null, wrapper.Box14DeclarantRepresentative);
				AssertEquals(ZString.Empty, wrapper.Box14AlternativeText);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			declaration.JE_OA_Representative = representativeAddress.PK;
			wrapper = ITDocSADH.New(entryHeader, factory);

			CombineAssertions("When UCC6 and both Declarant and Representative has EORI", () =>
			{
				AssertEquals(representativeOrgHeader.OH_FullName, wrapper.Box14DeclarantRepresentative.CompanyName);
				AssertEquals(representativeAddress.OA_Address1, wrapper.Box14DeclarantRepresentative.Address1);
			});

			declaration.JE_OA_Representative = ZGuid.Empty;
			wrapper = ITDocSADH.New(entryHeader, factory);

			CombineAssertions("When UCC6 and Only Declarant is valid", () =>
			{
				AssertEquals(declarantOrgHeader.OH_FullName, wrapper.Box14DeclarantRepresentative.CompanyName);
				AssertEquals(declarantAddress.OA_Address1, wrapper.Box14DeclarantRepresentative.Address1);
			});

			representativeOrgHeader.CustomsCodes.RemoveAll();
			declaration.JE_OA_Representative = representativeAddress.PK;
			wrapper = ITDocSADH.New(entryHeader, factory);

			CombineAssertions("When UCC6 and Declarant has EORI but Representative has not EORI", () =>
			{
				AssertEquals(declarantOrgHeader.OH_FullName, wrapper.Box14DeclarantRepresentative.CompanyName);
				AssertEquals(declarantAddress.OA_Address1, wrapper.Box14DeclarantRepresentative.Address1);
			});

			declarantOrgHeader.CustomsCodes.RemoveAll();
			wrapper = ITDocSADH.New(entryHeader, factory);
			AssertNull("When UCC6 and both Declarant and Representative has not EORI", wrapper.Box14DeclarantRepresentative);
		}
	}

	protected override DocSADH GetNewDocumentWrapper(EU.Business.Declaration.CusEntryHeader entryHeader) => ITDocSADH.New(entryHeader, Factory);

	protected override ZString CountrySpecificCurrency => "EUR";

	protected override ZString ExpectedEadBarCode => ZString.Empty;

	protected override ZDateTime ExpectedDOE => new ZDateTime(2008, 7, 1);
}
