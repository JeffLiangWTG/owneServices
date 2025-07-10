using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocumentWrappers.DocForwardingConsol))]
sealed class DocForwardingConsolTest : DocumentWrapperTestCase
{
	public override DocumentWrapper[] GetDocumentWrappers()
	{
		var consol = Factory.New<ForwardingConsol>();
		return new [] { DocForwardingConsol.New(Factory, consol.PK) };
	}

	public void TestDeclarations()
	{
		var auCompany = Factory.NewWithValidTestData<GlbCompany>();
		auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
		var auBranch = Factory.NewWithValidTestData<GlbBranch>();
		auBranch.GB_GC = auCompany.PK;
		Factory.Save();

		AssertEquals(0, DocConsol.Declarations.Count());

		var shipment1 = Consol.Shipments.AddNew();
		var shipment2 = Consol.Shipments.AddNew();

		JobDeclaration declaration1, declaration2;

		using (DisposableEnvironment.ForBranch(auBranch.PK.ToGuid()))
		{
			Factory.New<Customs.Business.BaseJobDeclaration>().JE_JS = shipment1.PK;
			Factory.New<Customs.Business.BaseJobDeclaration>().JE_JS = shipment2.PK;
		}
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
		{
			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_JS = shipment1.PK;
			declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_JS = shipment2.PK;
		}

		AssertEquals("Only CH declarations", 2, DocConsol.Declarations.Count());
		AssertContainsExactElementsInAnyOrder(new[] { declaration1, declaration2 }, DocConsol.Declarations);
	}

	public void TestCustomsOffice()
	{
		const string customsOfficeCode = "CH001251";
		const string customsOfficeName = "Allschwil 1";

		RefCusCodeTestHelper.CreateCustomsOfficesList(Factory);

		AssertNull("Customs Office", DocConsol.CustomsOffice);

		var shipment = Consol.Shipments.AddNew();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_JS = shipment.PK;
		declaration.JE_CustomsOffice = customsOfficeCode;

		CombineAssertions(() =>
		{
			AssertEquals("Customs Office Code", customsOfficeCode, DocConsol.CustomsOffice.Code);
			AssertEquals("Customs Office Name", customsOfficeName, DocConsol.CustomsOffice.Description);
		});
	}

	public void TestTransportRegNumber()
	{
		AssertEquals("Transport Reg. Number", "", DocConsol.TransportRegNumber);

		const string transportRegNumber = "GOX 2478";
		var transport = Consol.Transports[0];
		transport.JW_VoyageFlight = transportRegNumber;
		AssertEquals("Transport Reg. Number", transportRegNumber, DocConsol.TransportRegNumber);
	}

	public void TestEntryLines() => CombineAssertions(() =>
	{
		var shipmentA = Consol.Shipments.AddNew();
		var shipmentB = Consol.Shipments.AddNew();
		var declarationA = Factory.New<JobDeclaration>();
		var declarationB = Factory.New<JobDeclaration>();
		declarationA.JE_JS = shipmentA.PK;
		declarationB.JE_JS = shipmentB.PK;
		declarationA.JE_HouseBill = "bill-a";
		declarationB.JE_HouseBill = "bill-b";
		var instructionA1 = declarationA.CustomsEntryInstructions.AddNew();
		var instructionA2 = declarationA.CustomsEntryInstructions.AddNew();
		var instructionB = declarationB.CustomsEntryInstructions.AddNew();
		var invoiceHeaderA = declarationA.Invoices.AddNew();
		var invoiceHeaderB = declarationB.Invoices.AddNew();
		var invoiceLineA1 = invoiceHeaderA.InvoiceLines.AddNew();
		var invoiceLineA2 = invoiceHeaderA.InvoiceLines.AddNew();
		var invoiceLineA3 = invoiceHeaderA.InvoiceLines.AddNew();
		var invoiceLineB1 = invoiceHeaderB.InvoiceLines.AddNew();
		invoiceLineA1.JI_Description = "inv-line-a1";
		invoiceLineA2.JI_Description = "inv-line-a2";
		invoiceLineA3.JI_Description = "inv-line-a3";
		invoiceLineB1.JI_Description = "inv-line-b1";
		invoiceLineA1.JI_Tariff = "00000001";
		invoiceLineA2.JI_Tariff = "00000002";
		invoiceLineA3.JI_Tariff = "00000003";
		invoiceLineB1.JI_Tariff = "00000004";
		invoiceLineA1.JI_CEI = instructionA1.PK;
		invoiceLineA2.JI_CEI = instructionA2.PK;
		invoiceLineA3.JI_CEI = instructionA2.PK;
		invoiceLineB1.JI_CEI = instructionB.PK;
		declarationA.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		declarationB.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		var entryHeaderA1 = instructionA1.EntryHeader;
		var entryHeaderA2 = instructionA2.EntryHeader;
		var entryHeaderB = instructionB.EntryHeader;
		entryHeaderA1.CH_BGMReference = "BGM-YA1";
		entryHeaderA2.CH_BGMReference = "BGM-XA2";
		entryHeaderB.CH_BGMReference = "BGM-ZB";

		var entryLineWrapperCollection = DocConsol.EntryLines;
		AssertEquals("count", 4, entryLineWrapperCollection.Count);
		AssertEquals("inv-line-a2", entryLineWrapperCollection[0].Description);
		AssertEquals("inv-line-a3", entryLineWrapperCollection[1].Description);
		AssertEquals("inv-line-a1", entryLineWrapperCollection[2].Description);
		AssertEquals("inv-line-b1", entryLineWrapperCollection[3].Description);
		AssertEquals("line[0] bill", "bill-a", entryLineWrapperCollection[0].Declaration.HouseBill);
		AssertEquals("line[1] bill", "bill-a", entryLineWrapperCollection[1].Declaration.HouseBill);
		AssertEquals("line[2] bill", "bill-a", entryLineWrapperCollection[2].Declaration.HouseBill);
		AssertEquals("line[3] bill", "bill-b", entryLineWrapperCollection[3].Declaration.HouseBill);

		AssertSame("cached", DocConsol.EntryLines, DocConsol.EntryLines);
	});

	public void TestHeaderSelectionResultDescription()
	{
		const string code = "456";
		const string description = "Code Description";
		RefCusCodeTestHelper.CreateSelectionResultCodeListAndFrenchLanguage(Factory, code: code, description: description);

		var shipment = Consol.Shipments.AddNew();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_JS = shipment.PK;

		var instruction1 = declaration.CustomsEntryInstructions.AddNew();
		var instruction2 = declaration.CustomsEntryInstructions.AddNew();

		var invoiceHeader = declaration.Invoices.AddNew();

		AddIvoiceLineToInstruction(instruction1, "00000001");
		AddIvoiceLineToInstruction(instruction2, "00000002");

		declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

		AddEntryNumberToEntryHeader(instruction1.EntryHeader, RefCusCodeTestHelper.EntryStatusCode);
		AddEntryNumberToEntryHeader(instruction2.EntryHeader, code);

		AssertEquals(description, DocConsol.HeaderSelectionResultDescription);

		void AddIvoiceLineToInstruction(CusEntryInstruction instruction, string tariff)
		{
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Description = $"inv line '{tariff}'";
			invoiceLine.JI_Tariff = tariff;
		}

		void AddEntryNumberToEntryHeader(Customs.Business.CusEntryHeader entryHeader, string entryStatus)
		{
			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.Parent = entryHeader;
			cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cusEntryNumber.CE_EntryStatus = entryStatus;
		}
	}

	ForwardingConsol Consol => consol ??= Factory.New<ForwardingConsol>();
	ForwardingConsol consol;

	DocForwardingConsol DocConsol => docConsol ??= DocForwardingConsol.New(Consol, Factory);
	DocForwardingConsol docConsol;
}
