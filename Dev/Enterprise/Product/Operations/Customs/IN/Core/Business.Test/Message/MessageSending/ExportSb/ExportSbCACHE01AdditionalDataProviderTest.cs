using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01AdditionalDataProvider))]
sealed class ExportSbCACHE01AdditionalDataProviderTest : TestCaseWithFactory
{
	public void TestGetIgstPaymentStatus()
	{
		var invoiceLine = InvoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CL = EntryLine.PK;
		AssertIgstPaymentStatus(invoiceLine, ZString.Empty, true, IGSTPaymentStatusCodeList.Codes.NotApplicable);
		AssertIgstPaymentStatus(invoiceLine, ZString.Empty, false, ZString.Empty);
		AssertIgstPaymentStatus(invoiceLine, IGSTPaymentStatusCodeList.Codes.NotApplicable, true, IGSTPaymentStatusCodeList.Codes.NotApplicable);
		AssertIgstPaymentStatus(invoiceLine, IGSTPaymentStatusCodeList.Codes.NotApplicable, false, IGSTPaymentStatusCodeList.Codes.NotApplicable);
		AssertIgstPaymentStatus(invoiceLine, IGSTPaymentStatusCodeList.Codes.ExportAgainstPayment, true, IGSTPaymentStatusCodeList.Codes.NotApplicable);
		AssertIgstPaymentStatus(invoiceLine, IGSTPaymentStatusCodeList.Codes.ExportAgainstPayment, false, IGSTPaymentStatusCodeList.Codes.ExportAgainstPayment);
		AssertIgstPaymentStatus(invoiceLine, IGSTPaymentStatusCodeList.Codes.ExportUnderBondNotPaid, true, IGSTPaymentStatusCodeList.Codes.NotApplicable);
		AssertIgstPaymentStatus(invoiceLine, IGSTPaymentStatusCodeList.Codes.ExportUnderBondNotPaid, false, IGSTPaymentStatusCodeList.Codes.ExportUnderBondNotPaid);
	}

	void AssertIgstPaymentStatus(JobComInvoiceLine invoiceLine, ZString gstPaymentStatus, bool gstPayNotApplicable, ZString expectResult)
	{
		invoiceHeader.JZ_GSTPaymentStatus = gstPaymentStatus;
		invoiceLine.JI_GSTPayNotApplicable = gstPayNotApplicable;
		var additionalDataProvider = CreateAdditionalDataProvider("A");
		AssertEquals("IGST Payment Status", expectResult, additionalDataProvider.GetIgstPaymentStatus(EntryLine));
	}

	public void TestGetPortOfFinalDestinationAndGetPortOfDischarge()
	{
		var refUNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
		refUNLOCO.RL_Code = "CNHXU";
		refUNLOCO.RL_IATA = "HXU";
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		Declaration.JE_RL_NKFinalDestination = refUNLOCO.RL_Code;
		Declaration.JE_RL_NKPortOfArrival = refUNLOCO.RL_Code;
		var additionalDataProvider = CreateAdditionalDataProvider("A");
		AssertEquals("SEA PortOfFinalDestination", refUNLOCO.RL_Code, additionalDataProvider.GetPortOfFinalDestination(Header));
		AssertEquals("SEA PortOfDischarge", refUNLOCO.RL_Code, additionalDataProvider.GetPortOfDischarge(Header));
		Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
		AssertEquals("AIR PortOfFinalDestination", refUNLOCO.RL_IATA, additionalDataProvider.GetPortOfFinalDestination(Header));
		AssertEquals("AIR PortOfDischarge", refUNLOCO.RL_IATA, additionalDataProvider.GetPortOfDischarge(Header));
	}

	[TestDate(2025, 04, 22)]
	public void TestGetAmendmentInfo()
	{
		var additionalDataProvider = CreateAdditionalDataProvider(DeclarationMessageTypeList.Codes.Fresh);
		AssertEquals("AmendmentType", ZString.Empty, additionalDataProvider.GetAmendmentType(Header));
		AssertEquals("AmendmentNo", ZString.Empty, additionalDataProvider.GetAmendmentNo(Header));
		AssertEquals("AmendmentDate", null, additionalDataProvider.GetAmendmentDate(Header));
		additionalDataProvider = CreateAdditionalDataProvider(DeclarationMessageTypeList.Codes.Supplementary);
		AssertEquals("AmendmentType", DeclarationMessageTypeList.Codes.Supplementary, additionalDataProvider.GetAmendmentType(Header));
		AssertEquals("AmendmentNo", Constants.Messaging.INMessageNumPlaceHolder, additionalDataProvider.GetAmendmentNo(Header));
		AssertEquals("AmendmentDate", new ZDateTime(2025, 04, 22), additionalDataProvider.GetAmendmentDate(Header));
	}

	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ExportSbCACHE01AdditionalDataProvider(null));
			AssertNoExceptionThrown(() => new ExportSbCACHE01AdditionalDataProvider(MessageSendingObject));
		});
	}

	public void TestGetNatureOfCargo()
	{
		CombineAssertions(() =>
		{
			var additionalDataProvider = CreateAdditionalDataProvider("A");
			AssertEquals(string.Empty, additionalDataProvider.GetNatureOfCargo(null));

			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			AssertEquals(string.Empty, additionalDataProvider.GetNatureOfCargo(Header));

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(string.Empty, additionalDataProvider.GetNatureOfCargo(Header));

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("C", additionalDataProvider.GetNatureOfCargo(Header));

			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.BreakBulk;
			AssertEquals("P", additionalDataProvider.GetNatureOfCargo(Header));

			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Bulk;
			AssertEquals("DB", additionalDataProvider.GetNatureOfCargo(Header));

			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Liquid;
			AssertEquals("LB", additionalDataProvider.GetNatureOfCargo(Header));

			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.ContainerisedAndPackaged;
			AssertEquals("CP", additionalDataProvider.GetNatureOfCargo(Header));
		});
	}

	public void TestGetNatureOfContract()
	{
		CombineAssertions(() =>
		{
			var additionalDataProvider = CreateAdditionalDataProvider("A");
			AssertEquals(string.Empty, additionalDataProvider.GetNatureOfContract(null));

			InvoiceHeader.JZ_IncoTerm = INIncoTermList.Codes.FreeOnBoard;
			AssertEquals("FOB", additionalDataProvider.GetNatureOfContract(InvoiceHeader));

			InvoiceHeader.JZ_IncoTerm = INIncoTermList.Codes.CostInsuranceAndFreight;
			AssertEquals("CIF", additionalDataProvider.GetNatureOfContract(InvoiceHeader));

			InvoiceHeader.JZ_IncoTerm = INIncoTermList.Codes.CostAndFreight;
			AssertEquals("CF", additionalDataProvider.GetNatureOfContract(InvoiceHeader));

			InvoiceHeader.JZ_IncoTerm = INIncoTermList.Codes.CostAndInsurance;
			AssertEquals("CI", additionalDataProvider.GetNatureOfContract(InvoiceHeader));
		});
	}

	public void TestGetFactoryStuffed()
	{
		CombineAssertions(() =>
		{
			var additionalDataProvider = CreateAdditionalDataProvider("A");
			AssertEquals(string.Empty, additionalDataProvider.GetFactoryStuffed(null));

			Declaration.JE_StuffingAt = StuffingAtList.Codes.FAC;
			AssertEquals(string.Empty, additionalDataProvider.GetFactoryStuffed(Header));

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(string.Empty, additionalDataProvider.GetFactoryStuffed(Header));

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(string.Empty, additionalDataProvider.GetFactoryStuffed(Header));

			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Liquid;
			AssertEquals(string.Empty, additionalDataProvider.GetFactoryStuffed(Header));

			Declaration.JE_ContainerMode = Common.IN.INContainerModeList.Codes.Containerised;
			AssertEquals("Y", additionalDataProvider.GetFactoryStuffed(Header));

			Declaration.JE_StuffingAt = StuffingAtList.Codes.CFS;
			AssertEquals("N", additionalDataProvider.GetFactoryStuffed(Header));
		});
	}

	public void TestGetMarksNumbers()
	{
		CombineAssertions(() =>
		{
			var invoiceHeader1 = Declaration.Invoices.AddNew();
			var invoiceHeader2 = Declaration.Invoices.AddNew();
			var invoiceHeader3 = Declaration.Invoices.AddNew();
			var invoiceHeader4 = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();
			var invoiceLine4 = invoiceHeader4.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = Header.MergedLines.AddNew().PK;
			invoiceLine2.JI_CL = Header.MergedLines.AddNew().PK;
			invoiceLine3.JI_CL = Header.MergedLines.AddNew().PK;

			var additionalDataProvider = CreateAdditionalDataProvider("A");
			AssertNullOrEmpty(additionalDataProvider.GetMarksNumbers(null));
			AssertNullOrEmpty(additionalDataProvider.GetMarksNumbers(Header));

			invoiceHeader1.JZ_MarksAndNumbers = "1234567";
			invoiceHeader2.JZ_MarksAndNumbers = "9876543";

			AssertEquals("1234567 9876543", additionalDataProvider.GetMarksNumbers(Header));

			var longString = new string('A', 300);
			invoiceHeader1.JZ_MarksAndNumbers = longString;
			invoiceHeader2.JZ_MarksAndNumbers = longString;

			var result = additionalDataProvider.GetMarksNumbers(Header);
			AssertEquals(300, result.Length);
			AssertEquals(longString, result);
		});
	}

	public void TestGetJobNumber()
	{
		CombineAssertions(() =>
		{
			var additionalDataProvider = CreateAdditionalDataProvider("A");
			AssertNullOrEmpty(additionalDataProvider.GetJobNumber(InvoiceHeader));
			Header.CH_BGMReference = "123";
			AssertEquals("123", additionalDataProvider.GetJobNumber(InvoiceHeader));
		});
	}

	public void TestGetJobDate()
	{
		CombineAssertions(() =>
		{
			var additionalDataProvider = CreateAdditionalDataProvider("A");
			AssertNull(additionalDataProvider.GetJobDate(InvoiceHeader));
			Header.CH_SystemCreateTimeUtc = new ZDateTime(2021, 1, 1);
			AssertEquals(new ZDateTime(2021, 1, 1).ToLocalBranchTime(), additionalDataProvider.GetJobDate(InvoiceHeader));
		});
	}

	public void TestGetSbNo()
	{
		CombineAssertions(() =>
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			Header.CH_CEI_Instruction = entryInstruction.PK;
			var additionalDataProvider = CreateAdditionalDataProvider("A");
			AssertNullOrEmpty(additionalDataProvider.GetSbNo(InvoiceHeader));
			Header.EntryInstruction.ShippingBillNumber = "123";
			AssertEquals("123", additionalDataProvider.GetSbNo(InvoiceHeader));
		});
	}

	public void TestGetSbDate()
	{
		CombineAssertions(() =>
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			Header.CH_CEI_Instruction = entryInstruction.PK;
			var additionalDataProvider = CreateAdditionalDataProvider("A");
			AssertNull(additionalDataProvider.GetJobDate(InvoiceHeader));
			Header.EntryInstruction.ShippingBillDate = new ZDateTime(2021, 1, 1);
			AssertEquals(new ZDateTime(2021, 1, 1), additionalDataProvider.GetSbDate(InvoiceHeader));
		});
	}

	public void TestGetMessageType()
	{
		CombineAssertions(() =>
		{
			var additionalDataProvider = CreateAdditionalDataProvider("F");
			var cusContainerOnEntryInstruction = new CusContainerOnEntryInstruction(Instruction);
			AssertEquals("F", additionalDataProvider.GetMessageType(InvoiceHeader));
			AssertEquals("F", additionalDataProvider.GetMessageType(Header));
			AssertEquals("F", additionalDataProvider.GetMessageType(EntryLine));
			AssertEquals("F", additionalDataProvider.GetMessageType(JobWork));
			AssertEquals("F", additionalDataProvider.GetMessageType(cusContainerOnEntryInstruction));
		});
	}

	public void TestGetStateOfOriginExporter()
	{
		const string vaildOriginState = "CA";
		const string inVaildOriginState = "MM";
		const string customCodeIN = "CP";
		var today = ZDateTime.Today;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusMapType(RefCusMapTypeList.Codes.STATE, "OUT", "State Mapping", true);
		helper.CreateCusMap(RefCusMapTypeList.Codes.STATE, vaildOriginState, customCodeIN, today.AddDays(-1), today.AddDays(+1), Core.Constants.CountryCodes.India);
		Factory.Save();
		var declaration = Declaration;

		CombineAssertions(() =>
		{
			var additionalDataProvider = CreateAdditionalDataProvider("A");
			AssertNullOrEmpty("Bizo is null", additionalDataProvider.GetStateOfOriginExporter(null));
			AssertNullOrEmpty("OriginState not assigned", additionalDataProvider.GetStateOfOriginExporter(Header));

			declaration.JE_RW_NKOriginState = inVaildOriginState;
			AssertNullOrEmpty("Invalid OriginState", additionalDataProvider.GetStateOfOriginExporter(Header));
			declaration.JE_RW_NKOriginState = vaildOriginState;
			AssertEquals("Valid OriginState", customCodeIN, additionalDataProvider.GetStateOfOriginExporter(Header));
		});
	}

	public void TestGetSourceState()
	{
		const string vaildOriginState = "RJ";
		const string inVaildOriginState = "MM";
		const string customCodeIN = "CP";
		var today = ZDateTime.Today;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusMapType(RefCusMapTypeList.Codes.STATE, "OUT", "State Mapping", true);
		helper.CreateCusMap(RefCusMapTypeList.Codes.STATE, vaildOriginState, customCodeIN, today.AddDays(-1), today.AddDays(+1), Core.Constants.CountryCodes.India);
		Factory.Save();
		var invoiceLine = EntryLine.InvoiceLines.AddNew();
		CombineAssertions(() =>
		{
			var additionalDataProvider = CreateAdditionalDataProvider("A");
			AssertNullOrEmpty("Bizo is null", additionalDataProvider.GetSourceState(null));
			AssertNullOrEmpty("OriginState not assigned", additionalDataProvider.GetSourceState(EntryLine));

			invoiceLine.JI_StateOrRegionOfOrigin = inVaildOriginState;
			AssertNullOrEmpty("Invalid OriginState", additionalDataProvider.GetSourceState(EntryLine));
			invoiceLine.JI_StateOrRegionOfOrigin = vaildOriginState;
			AssertEquals("Valid OriginState", customCodeIN, additionalDataProvider.GetSourceState(EntryLine));
		});
	}

	IExportSbCACHE01AdditionalDataProvider CreateAdditionalDataProvider(string messageType)
	{
		MessageSendingObject.MessageType = messageType;
		return new ExportSbCACHE01AdditionalDataProvider(MessageSendingObject);
	}

	DeclarationMessageSendingObject MessageSendingObject => messageSendingObject ??= new DeclarationMessageSendingObject(Header);
	DeclarationMessageSendingObject messageSendingObject;

	CusEntryHeader Header => header ??= Factory.New<CusEntryHeader>();
	CusEntryHeader header;

	CusEntryLine EntryLine => entryLine ??= Header.MergedLines.AddNew();
	CusEntryLine entryLine;

	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;

	JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Declaration.Invoices.AddNew();
	JobComInvoiceHeader invoiceHeader;

	JobComInvoiceLine InvoiceLine => invoiceLine ??= InvoiceHeader.InvoiceLines.AddNew();
	JobComInvoiceLine invoiceLine;

	JobWork JobWork => jobWork ??= InvoiceLine.JobWorks.AddNew();
	JobWork jobWork;

	CusEntryInstruction Instruction => instruction ??= Declaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction instruction;

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		Header.CH_JE = jobDeclaration.PK;
		return jobDeclaration;
	}
}
