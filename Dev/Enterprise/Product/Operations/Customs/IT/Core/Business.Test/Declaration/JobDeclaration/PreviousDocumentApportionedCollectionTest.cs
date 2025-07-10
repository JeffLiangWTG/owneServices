using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class PreviousDocumentApportionedCollectionTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => { PreviousDocumentApportionedCollection.LoadNew(null); });
		AssertNoExceptionThrown(() => { PreviousDocumentApportionedCollection.LoadNew(Factory.New<JobDeclaration>()); });
	}

	public void TestIndexer()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var apportionedDocuments = PreviousDocumentApportionedCollection.LoadNew(declaration);
		AssertExceptionThrown<ArgumentException>("Invalid ZGuid", () => { _ = apportionedDocuments[""]; });
		AssertExceptionThrown<ArgumentException>("Invalid ZGuid", () => { _ = apportionedDocuments["11"]; });
		AssertExceptionThrown<ArgumentException>("Invalid ZGuid", () => { _ = apportionedDocuments["0000ffff-ffff-ffff-ffff-ffffffffffff"]; });
		AssertExceptionThrown<ArgumentException>("Valid ZGuid, but not registered to an entry line", () => { _ = apportionedDocuments[ZGuid.NewZGuid().ToString()]; });
		AssertNoExceptionThrown("Valid ZGuid and registered to an entry line", () => { _ = apportionedDocuments[entryLine.PK.ToString()]; });
	}

	public void TestApportionmentOfJobLevelDocumentsToEntryLines()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();
		var invoiceLine1 = declaration.InvoiceLines.AddNew();
		invoiceLine1.JI_Weight = 100m;
		invoiceLine1.JI_NetWeight = 200m;
		invoiceLine1.JI_CL = entryLine1.PK;
		var invoiceLine2 = declaration.InvoiceLines.AddNew();
		invoiceLine2.JI_Weight = 200m;
		invoiceLine2.JI_NetWeight = 400m;
		invoiceLine2.JI_CL = entryLine2.PK;

		var declarationPreviousDocumentProcedure2 = PreviousDocumentTestHelper.CreateDocument(Factory, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 100m, 90m, 100m, 100);
		declaration.PreviousDocuments.Add(declarationPreviousDocumentProcedure2);
		var anotherDeclarationPreviousDocumentProcedure2 = PreviousDocumentTestHelper.CreateDocument(Factory, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 75m, 70m, 75m, 75);
		declaration.PreviousDocuments.Add(anotherDeclarationPreviousDocumentProcedure2);
		var declarationPreviousDocumentProcedureA3 = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 100m, 100m, 100m, 100);
		declaration.PreviousDocuments.Add(declarationPreviousDocumentProcedureA3);

		var apportionedDocuments = PreviousDocumentApportionedCollection.LoadNew(declaration);

		CombineAssertions("Previous Documents for entryLine1", () =>
		{
			var mergedPreviousDocuments = apportionedDocuments[entryLine1.PK.ToString()];
			AssertEquals("Previous Documents count", 2, mergedPreviousDocuments.Count());
			var previousDocumentWithProcedure2 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "2");
			CheckDocument(previousDocumentWithProcedure2, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 58.33333m, 53.33333m, 58.33333m, 58);
			var previousDocumentWithProcedureA3 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3");
			CheckDocument(previousDocumentWithProcedureA3, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 33.33333m, 33.33333m, 33.33333m, 33);
		});

		CombineAssertions("Previous Documents for entryLine2", () =>
		{
			var mergedPreviousDocuments = apportionedDocuments[entryLine2.PK.ToString()];
			AssertEquals("Previous Documents count", 2, mergedPreviousDocuments.Count());
			var previousDocumentWithProcedure2 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "2");
			CheckDocument(previousDocumentWithProcedure2, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 116.66667m, 106.66667m, 116.66667m, 117);
			var previousDocumentWithProcedureA3 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3");
			CheckDocument(previousDocumentWithProcedureA3, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 66.66667m, 66.66667m, 66.66667m, 67);
		});
	}

	public void TestApportionmentOfInvoiceLevelDocumentsToEntryLines()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();
		var entryLine3 = entryHeader.MergedLines.AddNew();

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_Weight = 100m;
		invoiceLine1.JI_NetWeight = 200m;
		invoiceLine1.JI_CL = entryLine1.PK;

		var invoiceLine2 = invoice1.InvoiceLines.AddNew();
		invoiceLine2.JI_Weight = 200m;
		invoiceLine2.JI_NetWeight = 400m;
		invoiceLine2.JI_CL = entryLine2.PK;

		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine3 = invoice2.InvoiceLines.AddNew();
		invoiceLine3.JI_Weight = 500m;
		invoiceLine3.JI_NetWeight = 550m;
		invoiceLine3.JI_CL = entryLine3.PK;

		var invoicePreviousDocumentProcedure2 = PreviousDocumentTestHelper.CreateDocument(Factory, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 100m, 100m, 100m, 100);
		invoice1.PreviousDocuments.Add(invoicePreviousDocumentProcedure2);
		var anotherInvoicePreviousDocumentProcedure2 = PreviousDocumentTestHelper.CreateDocument(Factory, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 75m, 75m, 75m, 75);
		invoice1.PreviousDocuments.Add(anotherInvoicePreviousDocumentProcedure2);
		var invoicePreviousDocumentProcedureA3 = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 100m, 100m, 100m, 100);
		invoice1.PreviousDocuments.Add(invoicePreviousDocumentProcedureA3);
		var anotherInvoicePreviousDocumentProcedureA3 = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 200m, 200m, 200m, 200);
		invoice2.PreviousDocuments.Add(anotherInvoicePreviousDocumentProcedureA3);

		var apportionedDocuments = PreviousDocumentApportionedCollection.LoadNew(declaration);

		CombineAssertions("Previous Documents for entryLine1", () =>
		{
			var mergedPreviousDocuments = apportionedDocuments[entryLine1.PK.ToString()];
			AssertEquals("Previous Documents count", 2, mergedPreviousDocuments.Count());
			var previousDocumentWithProcedure2 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "2");
			CheckDocument(previousDocumentWithProcedure2, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 58.33333m, 58.33333m, 58.33333m, 58);
			var previousDocumentWithProcedureA3 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3");
			CheckDocument(previousDocumentWithProcedureA3, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 33.33333m, 33.33333m, 33.33333m, 33);
		});

		CombineAssertions("Previous Documents for entryLine2", () =>
		{
			var mergedPreviousDocuments = apportionedDocuments[entryLine2.PK.ToString()];
			AssertEquals("Previous Documents count", 2, mergedPreviousDocuments.Count());
			var previousDocumentWithProcedure2 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "2");
			CheckDocument(previousDocumentWithProcedure2, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 116.66667m, 116.66667m, 116.66667m, 117);
			var previousDocumentWithProcedureA3 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3");
			CheckDocument(previousDocumentWithProcedureA3, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 66.66667m, 66.66667m, 66.66667m, 67);
		});

		CombineAssertions("Previous Documents for entryLine3", () =>
		{
			var mergedPreviousDocuments = apportionedDocuments[entryLine3.PK.ToString()];
			AssertEquals("Previous Documents count", 1, mergedPreviousDocuments.Count());
			var previousDocumentWithProcedureA3 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3");
			CheckDocument(previousDocumentWithProcedureA3, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 200m, 200m, 200m, 200);
		});
	}

	public void TestApportionmentOfInvoiceLineLevelDocumentsToEntryLines()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_Weight = 100m;
		invoiceLine1.JI_NetWeight = 200m;
		invoiceLine1.JI_CL = entryLine1.PK;

		var invoiceLine2 = invoice1.InvoiceLines.AddNew();
		invoiceLine2.JI_Weight = 200m;
		invoiceLine2.JI_NetWeight = 400m;
		invoiceLine2.JI_CL = entryLine1.PK;

		var invoiceLinePreviousDocumentProcedure2 = PreviousDocumentTestHelper.CreateDocument(Factory, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 100m, 100m, 100m, 100);
		invoiceLine1.PreviousDocuments.Add(invoiceLinePreviousDocumentProcedure2);
		var anotherInvoiceLinePreviousDocumentProcedure2 = PreviousDocumentTestHelper.CreateDocument(Factory, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 75m, 75m, 75m, 75);
		invoiceLine1.PreviousDocuments.Add(anotherInvoiceLinePreviousDocumentProcedure2);
		var invoiceLinePreviousDocumentProcedureA3 = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 100m, 100m, 100m, 100);
		invoiceLine2.PreviousDocuments.Add(invoiceLinePreviousDocumentProcedureA3);

		var apportionedDocuments = PreviousDocumentApportionedCollection.LoadNew(declaration);

		CombineAssertions("Previous Documents for entryLine1", () =>
		{
			var mergedPreviousDocuments = apportionedDocuments[entryLine1.PK.ToString()];
			AssertEquals("Previous Documents count", 2, mergedPreviousDocuments.Count());
			var previousDocumentWithProcedure2 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "2");
			CheckDocument(previousDocumentWithProcedure2, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 175m, 175m, 175m, 175);
			var previousDocumentWithProcedureA3 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3");
			CheckDocument(previousDocumentWithProcedureA3, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 100m, 100m, 100m, 100);
		});

		CombineAssertions("Previous Documents for entryLine2", () =>
		{
			var mergedPreviousDocuments = apportionedDocuments[entryLine2.PK.ToString()];
			AssertEquals("Previous Documents count", 0, mergedPreviousDocuments.Count());
		});
	}

	public void TestApportionmentOfDocumentsFromAllLevels()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_Weight = 100m;
		invoiceLine1.JI_NetWeight = 200m;
		invoiceLine1.JI_CL = entryLine1.PK;

		var invoiceLine2 = invoice1.InvoiceLines.AddNew();
		invoiceLine2.JI_Weight = 200m;
		invoiceLine2.JI_NetWeight = 400m;
		invoiceLine2.JI_CL = entryLine2.PK;

		var declarationPreviousDocumentProcedure2 = PreviousDocumentTestHelper.CreateDocument(Factory, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 100m, 100m, 100m, 100);
		declaration.PreviousDocuments.Add(declarationPreviousDocumentProcedure2);
		var declarationPreviousDocumentProcedureA3 = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 100m, 100m, 100m, 100);
		declaration.PreviousDocuments.Add(declarationPreviousDocumentProcedureA3);

		var invoicePreviousDocumentProcedure2 = PreviousDocumentTestHelper.CreateDocument(Factory, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 100m, 100m, 100m, 100);
		invoice1.PreviousDocuments.Add(invoicePreviousDocumentProcedure2);
		var invoicePreviousDocumentProcedureA3 = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "2", "Z", "", "IT137100", 0, "A3", "", "", "", 100m, 100m, 100m, 100);
		invoice1.PreviousDocuments.Add(invoicePreviousDocumentProcedureA3);

		var invoiceLinePreviousDocumentProcedure2 = PreviousDocumentTestHelper.CreateDocument(Factory, "CO", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 1, "2", "", "", "", 100m, 100m, 100m, 100);
		invoiceLine1.PreviousDocuments.Add(invoiceLinePreviousDocumentProcedure2);
		var invoiceLinePreviousDocumentProcedureA3 = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 0, "A3", "", "", "", 100m, 100m, 100m, 100);
		invoiceLine2.PreviousDocuments.Add(invoiceLinePreviousDocumentProcedureA3);

		var apportionedDocuments = PreviousDocumentApportionedCollection.LoadNew(declaration);

		CombineAssertions("Previous Documents for entryLine1", () =>
		{
			var mergedPreviousDocuments = apportionedDocuments[entryLine1.PK.ToString()];
			AssertEquals("Previous Documents count", 4, mergedPreviousDocuments.Count());
			var previousDocumentWithProcedure2AndReferenceNumber1 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "2" && x.CSI_ReferenceNumber == "1");
			CheckDocument(previousDocumentWithProcedure2AndReferenceNumber1, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 66.66666m, 66.66666m, 66.66666m, 66);
			var previousDocumentWithProcedureA3AndReferenceNumber1 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3" && x.CSI_ReferenceNumber == "1");
			CheckDocument(previousDocumentWithProcedureA3AndReferenceNumber1, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 33.33333m, 33.33333m, 33.33333m, 33);
			var previousDocumentWithProcedureA3AndReferenceNumber2 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3" && x.CSI_ReferenceNumber == "2");
			CheckDocument(previousDocumentWithProcedureA3AndReferenceNumber2, "ZZZ", new ZDate(2020, 01, 01), "2", "Z", "", "IT137100", 0, "A3", "", "", "", 33.33333m, 33.33333m, 33.33333m, 33);
			var previousDocumentWithProcedure2AndReferenceNumber999 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "2" && x.CSI_ReferenceNumber == "999");
			CheckDocument(previousDocumentWithProcedure2AndReferenceNumber999, "CO", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 1, "2", "", "", "", 100m, 100m, 100m, 100);
		});

		CombineAssertions("Previous Documents for entryLine2", () =>
		{
			var mergedPreviousDocuments = apportionedDocuments[entryLine2.PK.ToString()];
			AssertEquals("Previous Documents count", 4, mergedPreviousDocuments.Count());
			var previousDocumentWithProcedure2AndReferenceNumber1 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "2" && x.CSI_ReferenceNumber == "1");
			CheckDocument(previousDocumentWithProcedure2AndReferenceNumber1, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 133.33334m, 133.33334m, 133.33334m, 134);
			var previousDocumentWithProcedureA3AndReferenceNumber1 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3" && x.CSI_ReferenceNumber == "1");
			CheckDocument(previousDocumentWithProcedureA3AndReferenceNumber1, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 66.66667m, 66.66667m, 66.66667m, 67);
			var previousDocumentWithProcedureA3AndReferenceNumber2 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3" && x.CSI_ReferenceNumber == "2");
			CheckDocument(previousDocumentWithProcedureA3AndReferenceNumber2, "ZZZ", new ZDate(2020, 01, 01), "2", "Z", "", "IT137100", 0, "A3", "", "", "", 66.66667m, 66.66667m, 66.66667m, 67);
			var previousDocumentWithProcedureA3AndReferenceNumber999 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3" && x.CSI_ReferenceNumber == "999");
			CheckDocument(previousDocumentWithProcedureA3AndReferenceNumber999, "ZZZ", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 0, "A3", "", "", "", 100m, 100m, 100m, 100);
		});
	}

	public void TestEdgeCases()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();
		var invoiceLine1 = declaration.InvoiceLines.AddNew();
		invoiceLine1.JI_Weight = 100m;
		invoiceLine1.JI_NetWeight = 200m;
		invoiceLine1.JI_CL = entryLine1.PK;

		var declarationPreviousDocumentProcedure2 = PreviousDocumentTestHelper.CreateDocument(Factory, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 0m, 0m, 0m, 0);
		declaration.PreviousDocuments.Add(declarationPreviousDocumentProcedure2);

		var apportionedDocuments = PreviousDocumentApportionedCollection.LoadNew(declaration);

		CombineAssertions("Previous Documents for entryLine1", () =>
		{
			var mergedPreviousDocuments = apportionedDocuments[entryLine2.PK.ToString()];
			AssertEquals("Previous Documents count", 1, mergedPreviousDocuments.Count());
			var previousDocumentWithProcedure2 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "2");
			CheckDocument(previousDocumentWithProcedure2, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 0m, 0m, 0m, 0);
		});
	}

	public void TestMassQuantitiesWithDifferentUnit()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_Weight = 100m;
		invoiceLine1.JI_NetWeight = 200m;
		invoiceLine1.JI_CL = entryLine1.PK;

		var invoiceLine2 = invoice1.InvoiceLines.AddNew();
		invoiceLine2.JI_Weight = 200m;
		invoiceLine2.JI_NetWeight = 400m;
		invoiceLine2.JI_CL = entryLine2.PK;

		var declarationPreviousDocumentProcedure2 = PreviousDocumentTestHelper.CreateDocument(Factory, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 100m, 100m, 100m, 100, unitOfNetMass: "G", unitOfGrossMass: "G");
		declaration.PreviousDocuments.Add(declarationPreviousDocumentProcedure2);
		var declarationPreviousDocumentProcedureA3 = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 100m, 100m, 100m, 100);
		declaration.PreviousDocuments.Add(declarationPreviousDocumentProcedureA3);

		var invoicePreviousDocumentProcedure2 = PreviousDocumentTestHelper.CreateDocument(Factory, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 100m, 100m, 100m, 100);
		invoice1.PreviousDocuments.Add(invoicePreviousDocumentProcedure2);
		var invoicePreviousDocumentProcedureA3 = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "2", "Z", "", "IT137100", 0, "A3", "", "", "", 100m, 100m, 100m, 100, unitOfNetMass: "T", unitOfGrossMass: "T");
		invoice1.PreviousDocuments.Add(invoicePreviousDocumentProcedureA3);

		var invoiceLinePreviousDocumentProcedure2 = PreviousDocumentTestHelper.CreateDocument(Factory, "CO", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 1, "2", "", "", "", 100m, 100m, 100m, 100);
		invoiceLine1.PreviousDocuments.Add(invoiceLinePreviousDocumentProcedure2);
		var invoiceLinePreviousDocumentProcedureA3 = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 0, "A3", "", "", "", 100m, 100m, 100m, 100);
		invoiceLine2.PreviousDocuments.Add(invoiceLinePreviousDocumentProcedureA3);

		var apportionedDocuments = PreviousDocumentApportionedCollection.LoadNew(declaration);

		CombineAssertions("Previous Documents for entryLine1", () =>
		{
			var mergedPreviousDocuments = apportionedDocuments[entryLine1.PK.ToString()];
			AssertEquals("Previous Documents count", 4, mergedPreviousDocuments.Count());
			var previousDocumentWithProcedure2AndReferenceNumber1 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "2" && x.CSI_ReferenceNumber == "1");
			CheckDocument(previousDocumentWithProcedure2AndReferenceNumber1, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 33.36666m, 66.66666m, 33.36666m, 66);
			var previousDocumentWithProcedureA3AndReferenceNumber1 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3" && x.CSI_ReferenceNumber == "1");
			CheckDocument(previousDocumentWithProcedureA3AndReferenceNumber1, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 33.33333m, 33.33333m, 33.33333m, 33);
			var previousDocumentWithProcedureA3AndReferenceNumber2 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3" && x.CSI_ReferenceNumber == "2");
			CheckDocument(previousDocumentWithProcedureA3AndReferenceNumber2, "ZZZ", new ZDate(2020, 01, 01), "2", "Z", "", "IT137100", 0, "A3", "", "", "", 33333.33333m, 33.33333m, 33333.33333m, 33);
			var previousDocumentWithProcedure2AndReferenceNumber999 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "2" && x.CSI_ReferenceNumber == "999");
			CheckDocument(previousDocumentWithProcedure2AndReferenceNumber999, "CO", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 1, "2", "", "", "", 100m, 100m, 100m, 100);
		});

		CombineAssertions("Previous Documents for entryLine2", () =>
		{
			var mergedPreviousDocuments = apportionedDocuments[entryLine2.PK.ToString()];
			AssertEquals("Previous Documents count", 4, mergedPreviousDocuments.Count());
			var previousDocumentWithProcedure2AndReferenceNumber1 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "2" && x.CSI_ReferenceNumber == "1");
			CheckDocument(previousDocumentWithProcedure2AndReferenceNumber1, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 66.73334m, 133.33334m, 66.73334m, 134);
			var previousDocumentWithProcedureA3AndReferenceNumber1 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3" && x.CSI_ReferenceNumber == "1");
			CheckDocument(previousDocumentWithProcedureA3AndReferenceNumber1, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 66.66667m, 66.66667m, 66.66667m, 67);
			var previousDocumentWithProcedureA3AndReferenceNumber2 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3" && x.CSI_ReferenceNumber == "2");
			CheckDocument(previousDocumentWithProcedureA3AndReferenceNumber2, "ZZZ", new ZDate(2020, 01, 01), "2", "Z", "", "IT137100", 0, "A3", "", "", "", 66666.66667m, 66.66667m, 66666.66667m, 67);
			var previousDocumentWithProcedureA3AndReferenceNumber999 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3" && x.CSI_ReferenceNumber == "999");
			CheckDocument(previousDocumentWithProcedureA3AndReferenceNumber999, "ZZZ", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 0, "A3", "", "", "", 100m, 100m, 100m, 100);
		});
	}

	void CheckDocument(
		MergedPreviousDocument mergedPreviousDocument
		, ZString csiCode
		, ZDate dateOfIssue
		, ZString referenceNumber
		, ZString subType
		, ZString status
		, ZString customsOffice
		, ZShort lineNo
		, ZString procedure
		, ZString referenceNumber2
		, ZString tariff
		, ZString unitOfQuantity2
		, ZDecimal quantity
		, ZDecimal quantity2
		, ZDecimal grossMass
		, ZInt packageQuantity)
	{
		AssertEquals("CSI_Code should be", csiCode, mergedPreviousDocument.CSI_Code);
		AssertEquals("CSI_DateOfIssue should be", dateOfIssue, mergedPreviousDocument.CSI_DateOfIssue);
		AssertEquals("CSI_ReferenceNumber should be", referenceNumber, mergedPreviousDocument.CSI_ReferenceNumber);
		AssertEquals("CSI_SubType should be", subType, mergedPreviousDocument.CSI_SubType);
		AssertEquals("CSI_Status should be", status, mergedPreviousDocument.CSI_Status);
		AssertEquals("CSI_CustomsOffice should be", customsOffice, mergedPreviousDocument.CSI_CustomsOffice);
		AssertEquals("CSI_LineNo should be", lineNo, mergedPreviousDocument.CSI_LineNo);
		AssertEquals("CSI_Procedure should be", procedure, mergedPreviousDocument.CSI_Procedure);
		AssertEquals("CSI_ReferenceNumber2 should be", referenceNumber2, mergedPreviousDocument.CSI_ReferenceNumber2);
		AssertEquals("CSI_Tariff should be", tariff, mergedPreviousDocument.CSI_Tariff);
		AssertEquals("CSI_UnitOfQuantity2 should be", unitOfQuantity2, mergedPreviousDocument.CSI_UnitOfQuantity2);
		AssertEquals("NetMass should be", quantity, mergedPreviousDocument.NetMass);
		AssertEquals("SupplementaryQuantity should be", quantity2, mergedPreviousDocument.SupplementaryQuantity);
		AssertEquals("GrossMass should be", grossMass, mergedPreviousDocument.GrossMass);
		AssertEquals("PackageQuantity should be", packageQuantity, mergedPreviousDocument.PackageQuantity);
	}
}
