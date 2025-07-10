using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class ComplXAESLineWrapperTest : WrapperHelperTest<ComplXAESLineWrapper>
{
	public void TestCommodity()
	{
		CombineAssertions(() =>
		{
			AssertNull("Expected empty Commodity when no additionalProcedure is declared", wrapper.Commodity);

			invoiceLine.JI_Procedure = "10499PV";
			wrapper = GetWrapper(entryLine);
			var commodity = wrapper.Commodity;
			AssertNotNull("Expected filled Commodity when additionalProcedure 9PV is declared in JI_Procedure", commodity);
			AssertSame("Cached Commodity", wrapper.Commodity, commodity);

			invoiceLine.JI_Procedure = "1049123";
			invoiceLine.AdditionalProcedureCodes.AddNew("456F89");
			invoiceLine.AdditionalProcedureCodes.AddNew("7899VA");
			wrapper = GetWrapper(entryLine);
			AssertNull("Expected empty Commodity when no additionalProcedure 9PV is declared", wrapper.Commodity);

			invoiceLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
			invoiceLine.AdditionalProcedureCodes.AddNew("7899PV");
			wrapper = GetWrapper(entryLine);
			AssertNotNull("Expected filled Commodity when additionalProcedure 9PV is declared in AdditionalProcedureCodes", wrapper.Commodity);

			invoiceLine.JI_Procedure = "1049A12";
			invoiceLine.AdditionalProcedureCodes.AddNew("789F89");
			wrapper = GetWrapper(entryLine);
			AssertNull("Expected empty Commodity when additionalProcedure 9PV is declared in AdditionalProcedureCodes but it is not in the list sent in the original declaration (first 2 additionalProcedures having ordered (desc) the list)", wrapper.Commodity);
		});
	}

	public void TestOrigin()
	{
		CombineAssertions(() =>
		{
			var origin = wrapper.Origin;
			AssertNotNull("Expected filled Origin", origin);
			AssertSame("Cached Origin", wrapper.Origin, origin);
		});
	}

	public void TestPreviousDocuments_NotC651()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty PreviousDocuments list", 0, wrapper.PreviousDocuments.Count);

			var supdoc1 = declaration.PreviousDocuments.AddNew();
			supdoc1.CSI_Code = "9001";

			var supdoc2 = invoiceHeader.PreviousDocuments.AddNew();
			supdoc2.CSI_Code = "9002";

			var supdoc3 = invoiceLine.PreviousDocuments.AddNew();
			supdoc3.CSI_Code = "9003";

			var supdoc4 = invoiceLine.PreviousDocuments.AddNew();
			supdoc4.CSI_Code = "9004";

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.PreviousDocuments;

			AssertEquals("No Expected filled PreviousDocuments (can only send C651)", 0, documents.Count);
			AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);
		});
	}

	public void TestPreviousDocuments_C651()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty PreviousDocuments list", 0, wrapper.PreviousDocuments.Count);

			var supdoc1 = declaration.PreviousDocuments.AddNew();
			supdoc1.CSI_Code = "9001";

			var supdoc2 = invoiceHeader.PreviousDocuments.AddNew();
			supdoc2.CSI_Code = "C651";

			var supdoc3 = invoiceLine.PreviousDocuments.AddNew();
			supdoc3.CSI_Code = "9002";

			var supdoc4 = invoiceLine.PreviousDocuments.AddNew();
			supdoc4.CSI_Code = "C651";

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.PreviousDocuments;

			AssertEquals("Expected filled PreviousDocuments (can only send 1 of C651)", 1, documents.Count);
			AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);
		});
	}

	public void TestPreviousDocuments_NMRN_WithoutQuantity()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty PreviousDocuments list", 0, wrapper.PreviousDocuments.Count);

			var supdoc1 = declaration.PreviousDocuments.AddNew();
			supdoc1.CSI_Code = "NMRN";

			var supdoc2 = invoiceHeader.PreviousDocuments.AddNew();
			supdoc2.CSI_Code = "9001";

			var supdoc3 = invoiceLine.PreviousDocuments.AddNew();
			supdoc3.CSI_Code = "NMRN";

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.PreviousDocuments;

			AssertEquals("No Expected filled PreviousDocuments (can only send NMRN if has Quantity)", 0, documents.Count);
			AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);
		});
	}

	public void TestPreviousDocuments_NMRN_WithQuantity()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty PreviousDocuments list", 0, wrapper.PreviousDocuments.Count);

			var supdoc1 = declaration.PreviousDocuments.AddNew();
			supdoc1.CSI_Code = "NMRN";
			supdoc1.CSI_Quantity = 1.0m;

			var supdoc2 = invoiceHeader.PreviousDocuments.AddNew();
			supdoc2.CSI_Code = "9001";
			supdoc2.CSI_Quantity = 1.0m;

			var supdoc3 = invoiceLine.PreviousDocuments.AddNew();
			supdoc3.CSI_Code = "NMRN";
			supdoc3.CSI_Quantity = 1.0m;

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.PreviousDocuments;

			AssertEquals("Expected filled PreviousDocuments (can only send 1 NMRN if has Quantity)", 1, documents.Count);
			AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);
		});
	}

	public void TestPreviousDocuments_OfficeOfPRE()
	{
		var customsOffice = declaration.CustomsOffices.AddNew();
		customsOffice.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		customsOffice.CY_Data = "FR008889";

		CombineAssertions(() =>
		{
			AssertEquals("Expected empty PreviousDocuments list", 0, wrapper.PreviousDocuments.Count);

			var supdoc1 = declaration.PreviousDocuments.AddNew();
			supdoc1.CSI_Code = "C651";

			var supdoc2 = declaration.PreviousDocuments.AddNew();
			supdoc2.CSI_Code = "NMRN";
			supdoc2.CSI_Quantity = 1.0m;

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.PreviousDocuments;

			AssertEquals("No Expected filled PreviousDocuments (since there is a PRE customs office, no documents allowed)", 0, documents.Count);
			AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);
		});
	}

	public void TestPreviousDocuments_UOMAndQuantity()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = "VIN1";
		var prevDoc1 = invoiceLine.PreviousDocuments.AddNew();
		prevDoc1.CSI_Code = "C651";
		prevDoc1.CSI_ReferenceNumber = "REF1";
		prevDoc1.CSI_UnitOfQuantity = "KGM";
		prevDoc1.CSI_Quantity = 2m;

		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle2 = invoiceLine2.Vehicles.AddNew();
		vehicle2.CVH_VehicleIdentificationNumber = "VIN2";
		var prevDoc2 = invoiceLine2.PreviousDocuments.AddNew();
		prevDoc2.CSI_Code = "C651";
		prevDoc2.CSI_ReferenceNumber = "REF1";
		prevDoc2.CSI_UnitOfQuantity = "KGM";
		prevDoc2.CSI_Quantity = 1m;

		declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = GetWrapper(entryLine);

		CombineAssertions(() =>
		{
			AssertEquals("PreviousDocuments.Count", 1, wrapper.PreviousDocuments.Count);
			var document = wrapper.PreviousDocuments.FirstOrDefault();

			AssertEquals("Expected sum of the quantities of the same previous document", 3m, document.Quantity);
			AssertEquals("Expected KGM", "KGM", document.Measurement);

			prevDoc1.CSI_UnitOfQuantity = ZString.Empty;
			prevDoc2.CSI_UnitOfQuantity = ZString.Empty;

			wrapper = GetWrapper(entryLine);
			AssertEquals("PreviousDocuments.Count", 1, wrapper.PreviousDocuments.Count);
			document = wrapper.PreviousDocuments.FirstOrDefault();
			AssertEquals("Expected sum of the number of vehicles if CSI_UnitOfQuantity is empty", 2m, document.Quantity);
			AssertEquals("Expected NAR if CSI_UnitOfQuantity is empty and there are vehicles", Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems, document.Measurement);
		});
	}

	public void TestPreviousDocuments_CL()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty PreviousDocuments list", 0, wrapper.PreviousDocuments.Count);

			BuilderHelperTest.AddEntryLineDocument<PreviousDocument>(entryLine, "9002", ZString.Empty);

			var supdoc2 = invoiceHeader.PreviousDocuments.AddNew();
			supdoc2.CSI_Code = "C651";

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.PreviousDocuments;

			AssertEquals("Expected filled PreviousDocuments (since there is CL Previous Document, no documents allowed except NMRN)", 0, documents.Count);
			AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);
		});
	}

	public void TestPreviousDocuments_NMRN_WithDocumentsInCL()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty PreviousDocuments list", 0, wrapper.PreviousDocuments.Count);

			BuilderHelperTest.AddEntryLineDocument<PreviousDocument>(entryLine, "NMRN", ZString.Empty);

			var supdoc1 = declaration.PreviousDocuments.AddNew();
			supdoc1.CSI_Code = "NMRN";
			supdoc1.CSI_Quantity = 1.0m;

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.PreviousDocuments;

			AssertEquals("Expected filled PreviousDocuments (if there is CL Previous Document, can send 1 NMRN document if has Quantity)", 1, documents.Count);
			AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);
		});
	}

	public void TestSupportingDocuments()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty SupportingDocuments list", 0, wrapper.SupportingDocuments.Count);

			var supdoc1 = declaration.SupportingDocuments.AddNew();
			supdoc1.CSI_Code = "N380";

			var supdoc2 = entryInstruction.SupportingDocuments.AddNew();
			supdoc2.CSI_Code = "D008";

			var supdoc3 = invoiceHeader.SupportingDocuments.AddNew();
			supdoc3.CSI_Code = "9002";

			var supdoc4 = invoiceLine.SupportingDocuments.AddNew();
			supdoc4.CSI_Code = "N325";

			var supdoc5 = invoiceLine.SupportingDocuments.AddNew();
			supdoc5.CSI_Code = "9004";

			var supdoc6 = invoiceLine.SupportingDocuments.AddNew();
			supdoc6.CSI_Code = "D005";

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.SupportingDocuments;
			AssertEquals("Expected filled SupportingDocuments (only included those that are Invoice Documents)", 4, documents.Count);
			AssertSame("Cached SupportingDocuments", wrapper.SupportingDocuments, documents);
		});
	}

	public void TestSupportingDocuments_NoCL()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty SupportingDocuments list", 0, wrapper.SupportingDocuments.Count);

			var supdoc1 = declaration.SupportingDocuments.AddNew();
			supdoc1.CSI_Code = "N380";

			BuilderHelperTest.AddEntryLineDocument<SupportingDocument>(entryLine, "N380", ZString.Empty);

			var supdoc3 = entryInstruction.SupportingDocuments.AddNew();
			supdoc3.CSI_Code = "D008";

			var supdoc4 = invoiceLine.SupportingDocuments.AddNew();
			supdoc4.CSI_Code = "N325";

			var supdoc5 = invoiceLine.SupportingDocuments.AddNew();
			supdoc5.CSI_Code = "N935";

			BuilderHelperTest.AddEntryLineDocument<SupportingDocument>(entryLine, "N935", ZString.Empty);

			var supdoc7 = invoiceLine.SupportingDocuments.AddNew();
			supdoc7.CSI_Code = "D005";

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.SupportingDocuments;
			AssertEquals("Expected filled SupportingDocuments (only included those that are Invoice Documents with no CL)", 3, documents.Count);
			AssertSame("Cached SupportingDocuments", wrapper.SupportingDocuments, documents);
		});
	}

	public void TestSupportingDocuments_OneCL()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty SupportingDocuments list", 0, wrapper.SupportingDocuments.Count);

			BuilderHelperTest.AddEntryLineDocument<SupportingDocument>(entryLine, "N380", ZString.Empty);

			BuilderHelperTest.AddEntryLineDocument<SupportingDocument>(entryLine, "N935", ZString.Empty);

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.SupportingDocuments;
			AssertEquals("Expected filled SupportingDocuments (only included one CL document when there are no Invoice Documents)", 1, documents.Count);
			AssertSame("Cached SupportingDocuments", wrapper.SupportingDocuments, documents);
		});
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
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;

		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = GetWrapper(entryLine);
	}
	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
	ComplXAESLineWrapper wrapper;

	ComplXAESLineWrapper GetWrapper(CusEntryLine entryLine) => new ComplXAESLineWrapper(entryLine);

	protected override ComplXAESLineWrapper GetProvider() => wrapper;
}
