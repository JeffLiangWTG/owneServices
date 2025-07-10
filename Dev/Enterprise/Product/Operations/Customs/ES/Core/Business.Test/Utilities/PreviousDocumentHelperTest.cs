using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.Business.Testing
{
	sealed class PreviousDocumentHelperTest : TestCaseWithFactory
	{
		public void TestMatchesAnyPreviouslySentPreviousDocument()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var preDoc = AddPreviousDocument(1, "N380", "Document1");
			var preDoc2 = AddPreviousDocument(0, "N325", "Document2");
			var preDoc3 = AddPreviousDocument(0, "N740", "Document3");
			var preDoc4 = AddPreviousDocument(1, "N705", "Document4");
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.AddEntryLineDocument<PreviousDocument>("N325", "Document2");
			entryLine.AddEntryLineDocument<PreviousDocument>("N740", "Document3");
			var preDocsInEntryLine = entryLine.GetPreviouslySentPreviousDocuments();

			CombineAssertions(() =>
			{
				AssertEquals("Previous Document N380 is not in entry line", false, preDoc.MatchesAnyPreviouslySentPreviousDocument(preDocsInEntryLine));
				AssertEquals("Previous Document N325 is in entry line", true, preDoc2.MatchesAnyPreviouslySentPreviousDocument(preDocsInEntryLine));
				AssertEquals("Previous Document N705 is not in entry line", false, preDoc4.MatchesAnyPreviouslySentPreviousDocument(preDocsInEntryLine));
				AssertEquals("Previous Document N740 is in entry line", true, preDoc3.MatchesAnyPreviouslySentPreviousDocument(preDocsInEntryLine));
			});
		}

		public void TestProcessImportAcceptedMessageCreateNewEntryLinePreviousDocs()
		{
			var entryLine = GetImportTestEntryLine();
			CombineAssertions(() =>
			{
				AssertEquals("There are no EntryLine PreviousDocuments before calling ProcessImportEntryLinePreviousDocuments", 0, GetEntryLinePreviousDocuments(entryLine).Length);

				entryLine.ProcessImportEntryLinePreviousDocuments();
				var entryLinePrevDocs = GetEntryLinePreviousDocuments(entryLine);
				AssertEquals("There are 1 EntryLine PreviousDocuments after calling ProcessImportEntryLinePreviousDocuments", 1, entryLinePrevDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 1 EntryLine PreviousDocuments after calling ProcessImportEntryLinePreviousDocuments have the correct CSI_Codes", new ZString[] { "X003" }, entryLinePrevDocs.Select(x => x.CSI_Code).ToArray());
				AssertEquals("The 1 EntryLine PreviousDocuments after calling ProcessImportEntryLinePreviousDocuments have the correct CSI_Status", false, entryLinePrevDocs.Any(x => x.CSI_Status != "ACC"));
			});
		}

		public void TestProcessImportAcceptedMessageDeleteAndCreateNewEntryLinePreviousDocs()
		{
			var entryLine = GetImportTestEntryLine();
			CombineAssertions(() =>
			{
				entryLine.AddEntryLineDocument<PreviousDocument>("X005", "ES3600000005");
				entryLine.AddEntryLineDocument<PreviousDocument>("X006", "ES3600000006");
				var entryLinePrevDocs = GetEntryLinePreviousDocuments(entryLine);
				AssertEquals("There are 2 EntryLine PreviousDocuments before calling ProcessImportEntryLinePreviousDocuments", 2, entryLinePrevDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 2 EntryLine SupportingDocuments before calling ProcessImportEntryLinePreviousDocuments have the correct CSI_Codes", new ZString[] { "X005", "X006" }, entryLinePrevDocs.Select(x => x.CSI_Code).ToArray());

				entryLine.ProcessImportEntryLinePreviousDocuments();
				entryLinePrevDocs = GetEntryLinePreviousDocuments(entryLine);
				AssertEquals("There are only 1 EntryLine PreviousDocuments after calling ProcessImportEntryLinePreviousDocuments (previous 2 have been deleted)", 1, entryLinePrevDocs.Length);
				AssertEquals("The 1 EntryLine PreviousDocuments after calling ProcessImportEntryLinePreviousDocuments have the correct CSI_Codes", true, new ZString[] { "X003", "X004" }.Contains(entryLinePrevDocs[0].CSI_Code));
			});
		}

		public void TestGetSUMReferenceNumberToSend()
		{
			CombineAssertions(() =>
			{
				var doc = SetDocument("X", "SUM", "99997000002", 1);
				AssertEquals("For SUM previous documents reference number must have line number attached when not 0", "9999700000200001", PreviousDocumentHelper.GetSUMReferenceNumberToSend(doc));

				doc = SetDocument("X", "SUM", "99997000123", 0);
				AssertEquals("For SUM previous documents reference number must not have line number attached when 0", "99997000123", PreviousDocumentHelper.GetSUMReferenceNumberToSend(doc));

				doc = SetDocument("X", "IRR", "99997000456", 5);
				AssertEquals("For non SUM previous documents reference number must not have line number attached", "99997000456", PreviousDocumentHelper.GetSUMReferenceNumberToSend(doc));
			});
		}

		public void TestProcessExportAcceptedMessageCreateNewEntryLinePreviousDocs()
		{
			var entryLine = GetExportTestEntryLineWithPrevDocs();
			CombineAssertions(() =>
			{
				AssertEquals("There are no EntryLine PreviousDocuments before calling ProcessExportEntryLinePreviousDocuments", 0, GetEntryLinePreviousDocuments(entryLine).Length);

				entryLine.ProcessExportEntryLinePreviousDocuments();
				var entryLinePrevDocs = GetEntryLinePreviousDocuments(entryLine);
				AssertEquals("There are 4 EntryLine PreviousDocuments after calling ProcessExportEntryLinePreviousDocuments", 4, entryLinePrevDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 4 EntryLine PreviousDocuments after calling ProcessExportEntryLinePreviousDocuments have the correct CSI_Codes", new ZString[] { "X001", "X002", "X003", "N380" }, entryLinePrevDocs.Select(x => x.CSI_Code).ToArray());
				AssertEquals("The 4 EntryLine PreviousDocuments after calling ProcessExportEntryLinePreviousDocuments have the correct CSI_Status", false, entryLinePrevDocs.Any(x => x.CSI_Status != "ACC"));
			});
		}

		public void TestProcessExportAcceptedMessageDeleteAndCreateNewEntryLinePreviousDocs()
		{
			var entryLine = GetExportTestEntryLineWithPrevDocs();
			CombineAssertions(() =>
			{
				entryLine.AddEntryLineDocument<PreviousDocument>("X004", "ES3600000004");
				entryLine.AddEntryLineDocument<PreviousDocument>("X005", "ES3600000005");
				var entryLinePrevDocs = GetEntryLinePreviousDocuments(entryLine);
				AssertEquals("There are 2 EntryLine PreviousDocuments before calling ProcessExportEntryLinePreviousDocuments", 2, entryLinePrevDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 2 EntryLine PreviousDocuments before calling ProcessExportEntryLinePreviousDocuments have the correct CSI_Codes", new ZString[] { "X004", "X005" }, entryLinePrevDocs.Select(x => x.CSI_Code).ToArray());

				entryLine.ProcessExportEntryLinePreviousDocuments();
				entryLinePrevDocs = GetEntryLinePreviousDocuments(entryLine);
				AssertEquals("There are only 4 EntryLine PreviousDocuments after calling ProcessExportEntryLinePreviousDocuments (previous 2 have been deleted)", 4, entryLinePrevDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 4 EntryLine PreviousDocuments after calling ProcessExportEntryLinePreviousDocuments have the correct CSI_Codes", new ZString[] { "X001", "X002", "X003", "N380" }, entryLinePrevDocs.Select(x => x.CSI_Code).ToArray());
			});
		}

		public void TestProcessExportAcceptedMessageCreateNewEntryLinePreviousDocsC651()
		{
			var entryLine = GetExportTestEntryLineWithPrevDocs(withDocsC651: true);

			CombineAssertions(() =>
			{
				entryLine.AddEntryLineDocument<PreviousDocument>("X004", "ES3600000004", status: "BBB");
				entryLine.AddEntryLineDocument<PreviousDocument>("C651", "ESC651000000", status: "BBB");
				var entryLinePrevDocs = GetEntryLinePreviousDocuments(entryLine);
				AssertEquals("There are 2 EntryLine PreviousDocuments before calling ProcessC651EntryLinePreviousDocuments", 2, entryLinePrevDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 2 EntryLine PreviousDocuments before calling ProcessC651EntryLinePreviousDocuments have the correct CSI_Codes", new ZString[] { "X004", "C651" }, entryLinePrevDocs.Select(x => x.CSI_Code).ToArray());

				entryLine.ProcessC651EntryLinePreviousDocuments();
				entryLinePrevDocs = GetEntryLinePreviousDocuments(entryLine);
				AssertEquals("There are only 4 EntryLine PreviousDocuments after calling ProcessC651EntryLinePreviousDocuments", 4, entryLinePrevDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 4 EntryLine PreviousDocuments after calling ProcessC651EntryLinePreviousDocuments have the correct CSI_Codes", new ZString[] { "X004", "C651", "C651", "C651" }, entryLinePrevDocs.Select(x => x.CSI_Code).ToArray());
				AssertContainsExactElementsInAnyOrder("The 4 EntryLine PreviousDocuments after calling ProcessC651EntryLinePreviousDocuments have the correct CSI_Codes", new ZString[] { "ES3600000004", "ESC651000000", "ESC651000001", "ESC651000002" }, entryLinePrevDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 3 EntryLine PreviousDocuments after calling ProcessC651EntryLinePreviousDocuments have the correct number with CSI_Status = ACC", 2, entryLinePrevDocs.Count(x => x.CSI_Status == "ACC"));
				AssertEquals("The 1 EntryLine PreviousDocuments after calling ProcessC651EntryLinePreviousDocuments have the correct number with CSI_Status = BBB", 2, entryLinePrevDocs.Count(x => x.CSI_Status == "BBB"));
			});
		}

		public void TestGetUOMAndQuantityForPreviousDocument()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			var vehicle3 = invoiceLine3.Vehicles.AddNew();
			invoiceLine3.JI_CL = entryLine2.PK;
			var invoiceLine4 = invoice.InvoiceLines.AddNew();
			var vehicle4 = invoiceLine4.Vehicles.AddNew();
			invoiceLine4.JI_CL = entryLine2.PK;
			var invoiceLine5 = invoice.InvoiceLines.AddNew();
			var vehicle5 = invoiceLine5.Vehicles.AddNew();
			invoiceLine5.JI_CL = entryLine2.PK;

			var entryLine3 = entryHeader.AllEntryLines.AddNew();
			var invoiceLine6 = invoice.InvoiceLines.AddNew();
			invoiceLine6.JI_CL = entryLine3.PK;

			var prevDoc1 = AddPreviousDocument(0,"X001", "REF1", CustomsUq.Number.NumberOfItems, 2m);
			invoiceLine.PreviousDocuments.Add(prevDoc1);
			invoiceLine2.PreviousDocuments.Add(AddPreviousDocument(0,"X001", "REF1", CustomsUq.Number.NumberOfItems, 1m));

			var prevDoc2 = AddPreviousDocument(0, "X002", "REF2");
			invoiceLine3.PreviousDocuments.Add(prevDoc2);
			vehicle3.CVH_VehicleIdentificationNumber = "VIN1";
			invoiceLine4.PreviousDocuments.Add(AddPreviousDocument(0, "X002", "REF2"));
			vehicle4.CVH_VehicleIdentificationNumber = "VIN2";
			invoiceLine5.PreviousDocuments.Add(AddPreviousDocument(0, "X002", "REF2"));
			vehicle5.CVH_VehicleIdentificationNumber = "VIN1";

			var prevDoc3 = AddPreviousDocument(0, "X003", "REF3", quantity: 5m);
			invoiceLine6.PreviousDocuments.Add(prevDoc3);

			CombineAssertions(() =>
			{
				PreviousDocumentHelper.GetUOMAndQuantityForPreviousDocument(prevDoc1, entryLine.InvoiceLines, entryLine.InvoiceLinesWithVehicles.Count(), out var expectedUOM, out var expectedQuantity);
				AssertEquals("Calc UOM should be empty if CSI_UnitOfQuantity is not empty and there are no vehicles", ZString.Empty, expectedUOM);
				AssertEquals("Calc Quantity should be the sum of same previous doc in Invoice lines of the entryLine if CSI_UnitOfQuantity is not empty", 3m, expectedQuantity);

				PreviousDocumentHelper.GetUOMAndQuantityForPreviousDocument(prevDoc2, entryLine2.InvoiceLines, entryLine2.InvoiceLinesWithVehicles.Count(), out expectedUOM, out expectedQuantity);
				AssertEquals("Calc UOM should be NAR if CSI_UnitOfQuantity is empty and there are vehicles", CustomsUq.Number.NumberOfItems, expectedUOM);
				AssertEquals("Calc Quantity should be the sum of vehicles if CSI_UnitOfQuantity is empty and there are vehicles", 3m, expectedQuantity);

				PreviousDocumentHelper.GetUOMAndQuantityForPreviousDocument(prevDoc3, entryLine3.InvoiceLines, entryLine3.InvoiceLinesWithVehicles.Count(), out expectedUOM, out expectedQuantity);
				AssertEquals("Calc UOM should be empty if CSI_UnitOfQuantity is empty", ZString.Empty, expectedUOM);
				AssertEquals("Calc Quantity should be 0 if CSI_UnitOfQuantity is empty and there are no vehicles", 0m, expectedQuantity);
			});
		}

		public void TestAddPreviousDocumentWithLengthForGoodsItemNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var prevDoc1 = AddPreviousDocument(1, "NMRN", "REF1", CustomsUq.Number.NumberOfPairs, 2m);
			invoiceLine.PreviousDocuments.Add(prevDoc1);

			var list = new List<AESCommonDocumentWrapper>();

			CombineAssertions(() =>
			{
				PreviousDocumentHelper.AddPreviousDocumentWithLengthForGoodsItemNumber(entryLine, list, prevDoc1, 1);
				AssertEquals("Expected length of 5 with Type NMRN", "00001", list.First().LineNumber);

				list.RemoveAt(0);
				prevDoc1.CSI_Code = "AH3";
				PreviousDocumentHelper.AddPreviousDocumentWithLengthForGoodsItemNumber(entryLine, list, prevDoc1, 1);
				AssertEquals("Expected length of 3 with Type different than NMRN", "001", list.First().LineNumber);
			});
		}

		PreviousDocument SetDocument(ZString subType, ZString code, ZString reference, ZShort lineNo)
		{
			var doc = Factory.New<PreviousDocument>();
			doc.CSI_SubType = subType;
			doc.CSI_Code = code;
			doc.CSI_ReferenceNumber = reference;
			doc.CSI_LineNo = lineNo;
			return doc;
		}

		CusEntryLine GetImportTestEntryLine()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "BLT";
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var prevDoc1 = AddPreviousDocument(1, "X001", "ES3600000001");
			declaration.PreviousDocuments.Add(prevDoc1);

			var prevDoc2 = AddPreviousDocument(2, "X002", "ES3600000002");
			invoice.PreviousDocuments.Add(prevDoc2);

			var prevDoc3 = AddPreviousDocument(3, "X003", "ES3600000003");
			invoiceLine.PreviousDocuments.Add(prevDoc3);

			var prevDoc4 = AddPreviousDocument(4, "X004", "ES3600000004");
			invoiceLine.PreviousDocuments.Add(prevDoc4);

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "EntryNum";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			Factory.Save();

			return entryLine;
		}

		CusEntryLine GetExportTestEntryLineWithPrevDocs(bool withDocsC651 = false)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_DataModel = Core.Constants.CountryCodes.Spain;

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var prevDoc1 = AddPreviousDocument(0, "X001", "ES3600000001");
			invoiceLine.PreviousDocuments.Add(prevDoc1);

			var prevDoc2 = AddPreviousDocument(0, "X002", "ES3600000002");
			invoiceLine.PreviousDocuments.Add(prevDoc2);

			var prevDoc3 = AddPreviousDocument(0, "X003", "ES3600000003");
			invoiceLine.PreviousDocuments.Add(prevDoc3);

			var prevDoc4 = AddPreviousDocument(0, "N380", "ES36000N3801");
			invoiceLine.PreviousDocuments.Add(prevDoc4);

			if (withDocsC651)
			{
				var prevDoc5 = AddPreviousDocument(0, "C651", "ESC651000001");
				invoiceLine.PreviousDocuments.Add(prevDoc5);

				var prevDoc6 = AddPreviousDocument(0, "C651", "ESC651000002");
				invoiceLine.PreviousDocuments.Add(prevDoc6);

				var prevDoc7 = AddPreviousDocument(0, "C651", "ESC651000001");
				invoiceLine.PreviousDocuments.Add(prevDoc7);

				var prevDoc8 = AddPreviousDocument(0, "C651", "ESC651000000");
				invoiceLine.PreviousDocuments.Add(prevDoc8);
			}
			Factory.Save();
			return entryLine;
		}

		PreviousDocument[] GetEntryLinePreviousDocuments(CusEntryLine entryLine)
		{
			var query = new ZQuery(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument);
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, entryLine.PK);
			return Factory.Load<PreviousDocument>(query);
		}

		PreviousDocument AddPreviousDocument(int i, ZString code, ZString reference, string uom = "", decimal quantity = 0)
		{
			var preDoc = Factory.New<PreviousDocument>();
			preDoc.SuspendValidation();

			preDoc.CSI_Code = code;
			preDoc.CSI_ReferenceNumber = reference;
			preDoc.CSI_Status = ZString.Empty;
			preDoc.CSI_UnitOfQuantity = uom;
			preDoc.CSI_Quantity = quantity;
			preDoc.CSI_LineNo = i;

			return preDoc;
		}
	}
}

