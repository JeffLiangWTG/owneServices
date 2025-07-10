using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
	sealed class IncotermFieldsBox20EvaluatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("entry header required", () => new IncotermFieldsBox20Evaluator(null));
			AssertNoExceptionThrown("Valid entry header", () => new IncotermFieldsBox20Evaluator(Factory.New<CusEntryHeader>()));
		}

		public void TestEvaluate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var evaluator = new IncotermFieldsBox20Evaluator(entryHeader);
			var incotermFields = evaluator.Evaluate();

			CombineAssertions("Invoice with no IncoTerm fields", () =>
			{
				AssertEquals("ShipmentIncoTerm", "", incotermFields.ShipmentIncoTerm);
				AssertEquals("AgreedPlace", "", incotermFields.IncoTermPlace);
				AssertEquals("AgreedPlaceCode", "", incotermFields.AgreedPlaceCode);
				AssertEquals("AgreedPlaceCode2 last box", ZString.Empty, incotermFields.AgreedPlaceCode2);
			});

			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_IncoTermPlace = "TARANTO";
			invoiceHeader.ZG_AgreedPlaceCode = "1";

			incotermFields = evaluator.Evaluate();

			CombineAssertions("Invoice header set up", () =>
			{
				AssertEquals("ShipmentIncoTerm", "FOB", incotermFields.ShipmentIncoTerm);
				AssertEquals("AgreedPlace", "TARANTO", incotermFields.IncoTermPlace);
				AssertEquals("AgreedPlaceCode", ZString.Empty, incotermFields.AgreedPlaceCode);
				AssertEquals("AgreedPlaceCode2 last box", "1", incotermFields.AgreedPlaceCode2);
			});
		}

		public void TestEvaluateWithNoInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var evaluator = new IncotermFieldsBox20Evaluator(entryHeader);
			var incotermFields = evaluator.Evaluate();

			CombineAssertions("No invoices in Entry header", () =>
			{
				AssertEquals("ShipmentIncoTerm", "", incotermFields.ShipmentIncoTerm);
				AssertEquals("AgreedPlace", "", incotermFields.IncoTermPlace);
				AssertEquals("AgreedPlaceCode", "", incotermFields.AgreedPlaceCode);
				AssertEquals("AgreedPlaceCode2 last box", ZString.Empty, incotermFields.AgreedPlaceCode2);
			});
		}
	}
}
