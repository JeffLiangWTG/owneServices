using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.Testing;

public class SupportingDocumentHelperTest : TestCaseWithFactory
{
	public void TestMatchesAnyPreviouslySentDocument()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var supDoc = AddDocumentToDeclaration(declaration, "N380", "Document1");
		var supDoc2 = AddDocumentToDeclaration(declaration, "N325", "Document2");
		var supDoc3 = AddDocumentToDeclaration(declaration, "N740", "Document3");
		var supDoc4 = AddDocumentToDeclaration(declaration, "N705", "Document4");
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.AddEntryLineDocument<SupportingDocument>("N325", "Document2");
		entryLine.AddEntryLineDocument<SupportingDocument>("N740", "Document3");
		var supDocsInEntryLine = entryLine.GetPreviouslySentSupportingDocuments();

		CombineAssertions(() =>
		{
			AssertEquals("Supporting Document N380 is not in entry line", false, supDoc.MatchesAnyPreviouslySentDocument(supDocsInEntryLine));
			AssertEquals("Supporting Document N325 is in entry line", true, supDoc2.MatchesAnyPreviouslySentDocument(supDocsInEntryLine));
			AssertEquals("Supporting Document N740 is in entry line", true, supDoc3.MatchesAnyPreviouslySentDocument(supDocsInEntryLine));
			AssertEquals("Supporting Document N705 is not in entry line", false, supDoc4.MatchesAnyPreviouslySentDocument(supDocsInEntryLine));
		});
	}

	public void TestProcessImportAcceptedMessageCreateNewEntryLineSupportingDocsCusEntryLineAndCusEntryHeader_SubStyleNotT2LT2C_StyleNotH2_UCC()
	{
		var (entryLine, declaration) = GetTestEntryLine();
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions(() =>
			{
				AssertEquals("There are no EntryLine SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryLine(entryLine).Length);
				AssertEquals("There are no EntryHeader SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header).Length);

				entryLine.ProcessImportEntryLineSupportingDocuments();
				var entryHeaderSupDocs = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
				AssertEquals("There are 2 EntryHeader SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments", 2, entryHeaderSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 2 EntryHeader SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001", "X002" }, entryHeaderSupDocs.Select(x => x.CSI_Code).ToArray());
				AssertEquals("The 2 EntryHeader SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Status", false, entryHeaderSupDocs.Any(x => x.CSI_Status != "ACC"));

				var entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
				AssertEquals("There are 2 EntryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments", 2, entryLineSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 2 EntryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X003", "N380" }, entryLineSupDocs.Select(x => x.CSI_Code).ToArray());
				AssertEquals("The 2 EntryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Status", false, entryLineSupDocs.Any(x => x.CSI_Status != "ACC"));
			});
		}
	}

	public void TestProcessImportAcceptedMessageCreateNewEntryLineSupportingDocsCusEntryLine_SubStyleNotT2LT2C_StyleNotH2_PreUCC()
	{
		var (entryLine, declaration) = GetTestEntryLine();
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			CombineAssertions(() =>
			{
				AssertEquals("There are no EntryLine SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryLine(entryLine).Length);

				entryLine.ProcessImportEntryLineSupportingDocuments();
				var entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
				AssertEquals("There are 4 EntryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments", 4, entryLineSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 4 EntryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001", "X002", "X003", "N380" }, entryLineSupDocs.Select(x => x.CSI_Code).ToArray());
				AssertEquals("The 4 EntryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Status", false, entryLineSupDocs.Any(x => x.CSI_Status != "ACC"));
			});
		}
	}

	public void TestProcessImportAcceptedMessageCreateNewEntryLineSupportingDocsCusEntryLineAndCusEntryHeader_SubStyleNotT2LT2C_StyleH2()
	{
		var (entryLine, _) = GetTestEntryLine(instructionStyle: IMPDeclarationTypeList.Codes.H2);
		CombineAssertions(() =>
		{
			AssertEquals("There are no EntryLine SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryLine(entryLine).Length);
			AssertEquals("There are no EntryHeader SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header).Length);

			entryLine.ProcessImportEntryLineSupportingDocuments();
			var entryHeaderSupDocs = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
			AssertEquals("There are 2 EntryHeader SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments", 2, entryHeaderSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 2 EntryHeader SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001", "X002" }, entryHeaderSupDocs.Select(x => x.CSI_Code).ToArray());
			AssertEquals("The 2 EntryHeader SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Status", false, entryHeaderSupDocs.Any(x => x.CSI_Status != "ACC"));

			var entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 2 EntryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments", 2, entryLineSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 2 EntryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X003", "N380" }, entryLineSupDocs.Select(x => x.CSI_Code).ToArray());
			AssertEquals("The 2 EntryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Status", false, entryLineSupDocs.Any(x => x.CSI_Status != "ACC"));
		});
	}

	public void TestProcessImportAcceptedMessageCreateNewEntryLineSupportingDocsCusEntryLineAndCusEntryHeader_SubStyleT2LT2C_StyleEmpty()
	{
		var (entryLine, _) = GetTestEntryLine(instructionSubStyle: EntrySubStyleList.Codes.T2L);
		CombineAssertions(() =>
		{
			AssertEquals("There are no EntryLine SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryLine(entryLine).Length);
			AssertEquals("There are no EntryHeader SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header).Length);

			entryLine.ProcessImportEntryLineSupportingDocuments();
			var entryHeaderSupDocs = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
			AssertEquals("There are 2 EntryHeader SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments", 2, entryHeaderSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 2 EntryHeader SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001", "X002" }, entryHeaderSupDocs.Select(x => x.CSI_Code).ToArray());
			AssertEquals("The 2 EntryHeader SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Status", false, entryHeaderSupDocs.Any(x => x.CSI_Status != "ACC"));

			var entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 2 EntryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments", 2, entryLineSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 2 EntryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X003", "N380" }, entryLineSupDocs.Select(x => x.CSI_Code).ToArray());
			AssertEquals("The 2 EntryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Status", false, entryLineSupDocs.Any(x => x.CSI_Status != "ACC"));
		});
	}

	public void TestProcessExportAcceptedMessageCreateNewEntryLineSupportingDocsCusEntryLineAndCusEntryHeader_SubStyleT2LT2C_StyleEmpty()
	{
		var (entryLine, _) = GetTestEntryLine(messageType: Enterprise.Customs.Business.JobMessageTypeList.Codes.Export, instructionSubStyle: EntrySubStyleList.Codes.T2L);
		CombineAssertions(() =>
		{
			AssertEquals("There are no EntryLine SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryLine(entryLine).Length);
			AssertEquals("There are no EntryHeader SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header).Length);

			entryLine.ProcessExportEntryLineSupportingDocuments();
			var entryHeaderSupDocs = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
			AssertEquals("There are 2 EntryHeader SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments", 2, entryHeaderSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 2 EntryHeader SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001", "X002" }, entryHeaderSupDocs.Select(x => x.CSI_Code).ToArray());
			AssertEquals("The 2 EntryHeader SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Status", false, entryHeaderSupDocs.Any(x => x.CSI_Status != "ACC"));

			var entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 2 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments", 2, entryLineSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 2 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X003", "N380" }, entryLineSupDocs.Select(x => x.CSI_Code).ToArray());
			AssertEquals("The 2 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Status", false, entryLineSupDocs.Any(x => x.CSI_Status != "ACC"));
		});
	}

	public void TestProcessExportAcceptedMessageCreateNewEntryLineSupportingDocsCusEntryLine_SubStyleNotEXST2LT2C_StyleEmpty_InTransitPeriod()
	{
		var (entryLine, declaration) = GetTestEntryLine(messageType: Enterprise.Customs.Business.JobMessageTypeList.Codes.Export);
		using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, true))
		{
				CombineAssertions(() =>
			{
				AssertEquals("There are no EntryLine SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryLine(entryLine).Length);

				entryLine.ProcessExportEntryLineSupportingDocuments();
				var entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
				AssertEquals("There are 4 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments", 4, entryLineSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 4 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001", "X002", "X003", "N380" }, entryLineSupDocs.Select(x => x.CSI_Code).ToArray());
				AssertEquals("The 4 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Status", false, entryLineSupDocs.Any(x => x.CSI_Status != "ACC"));
			});
		}
	}

	public void TestProcessExportAcceptedMessageCreateNewEntryLineSupportingDocsCusEntryLineAndCusEntryHeader_SubStyleNotEXST2LT2C_StyleEmpty_InFinalPeriod()
	{
		var (entryLine, declaration) = GetTestEntryLine(messageType: Enterprise.Customs.Business.JobMessageTypeList.Codes.Export);
		using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, false))
		{
			CombineAssertions(() =>
			{
				AssertEquals("There are no EntryLine SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryLine(entryLine).Length);
				AssertEquals("There are no EntryHeader SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header).Length);

				entryLine.ProcessExportEntryLineSupportingDocuments();
				var entryHeaderSupDocs = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
				AssertEquals("There are 2 EntryHeader SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments", 2, entryHeaderSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 2 EntryHeader SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001", "X002" }, entryHeaderSupDocs.Select(x => x.CSI_Code).ToArray());
				AssertEquals("The 2 EntryHeader SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Status", false, entryHeaderSupDocs.Any(x => x.CSI_Status != "ACC"));

				var entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
				AssertEquals("There are 2 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments", 2, entryLineSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 2 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X003", "N380" }, entryLineSupDocs.Select(x => x.CSI_Code).ToArray());
				AssertEquals("The 2 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Status", false, entryLineSupDocs.Any(x => x.CSI_Status != "ACC"));
			});
		}
	}

	public void TestProcessExportAcceptedMessageCreateNewEntryLineSupportingDocsCusEntryLine_SubStyleEXS_StyleEmpty()
	{
		var (entryLine, _) = GetTestEntryLine(messageType: Enterprise.Customs.Business.JobMessageTypeList.Codes.Export, instructionSubStyle: ExsEntrySubStyleList.Codes.EXS);
		CombineAssertions(() =>
		{
			AssertEquals("There are no EntryLine SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryLine(entryLine).Length);

			entryLine.ProcessExportEntryLineSupportingDocuments();
			var entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 4 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments", 4, entryLineSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 3 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001", "X002", "X003", "N380" }, entryLineSupDocs.Select(x => x.CSI_Code).ToArray());
			AssertEquals("The 4 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Status", false, entryLineSupDocs.Any(x => x.CSI_Status != "ACC"));
		});
	}

	public void TestProcessImportAcceptedMessageDeleteAndCreateNewEntryLineSupportingDocs()
	{
		var (entryLine, declaration) = GetTestEntryLine();
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions(() =>
			{
				entryLine.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
				entryLine.AddEntryLineDocument<SupportingDocument>("X005", "ES3600000005", subType: "LIQ");
				entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("X001", "ES3600000001");
				entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("X006", "ES3600000006", subType: "LIQ");

				var entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
				AssertEquals("There are 2 EntryLine SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 2, entryLineSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 2 EntryLine SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X004", "X005" }, entryLineSupDocs.Select(x => x.CSI_Code).ToArray());

				var entryHeaderSupDocs = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
				AssertEquals("There are 2 EntryHeader SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 2, entryHeaderSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 2 EntryHeader SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001", "X006" }, entryHeaderSupDocs.Select(x => x.CSI_Code).ToArray());

				entryLine.ProcessImportEntryLineSupportingDocuments();
				entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
				AssertEquals("There are 3 EntryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments", 3, entryLineSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 3 EntryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X003", "N380", "X005" }, entryLineSupDocs.Select(x => x.CSI_Code).ToArray());

				entryHeaderSupDocs = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
				AssertEquals("There are only 3 EntryHeader SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments", 3, entryHeaderSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 3 EntryHeader SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001", "X002", "X006" }, entryHeaderSupDocs.Select(x => x.CSI_Code).ToArray());
			});
		}
	}

	public void TestProcessImportAcceptedMessageRemoveAndUpdate_LIQ_EntryLineSupportingDocs()
	{
		var (entryLine, _) = GetTestEntryLine();
		CombineAssertions(() =>
		{
			entryLine.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
			entryLine.AddEntryLineDocument<SupportingDocument>("AAA", "10,00", subType: "LIQ", status: "ACC");
			entryLine.AddEntryLineDocument<SupportingDocument>("BBB", "10,00", subType: "LIQ", status: ZString.Empty);
			entryLine.AddEntryLineDocument<SupportingDocument>("CCC", "10,00", subType: "LIQ", status: "REJ");
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("X001", "ES3600000001", subType: "LIQ");
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("X006", "ES3600000006", subType: "LIQ", status: "ACC");

			var clSupDocsEntryLine = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 4 entryLine SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 4, clSupDocsEntryLine.Length);
			AssertContainsExactElementsInAnyOrder("The 4 entryLine SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X004", "AAA", "BBB", "CCC" }, clSupDocsEntryLine.Select(x => x.CSI_Code).ToArray());

			var clSupDocsEntryHeader = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
			AssertEquals("There are 2 entryHeader SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 2, clSupDocsEntryHeader.Length);
			AssertContainsExactElementsInAnyOrder("The 4 entryHeader SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001", "X006" }, clSupDocsEntryHeader.Select(x => x.CSI_Code).ToArray());

			entryLine.ProcessImportEntryLineSupportingDocuments();
			clSupDocsEntryLine = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 5 entryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments (only the non LIQ or LIQ ACC have been deleted)", 5, clSupDocsEntryLine.Length);
			AssertContainsExactElementsInAnyOrder("The 5 entryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X002", "X003", "N380", "BBB", "CCC" }, clSupDocsEntryLine.Select(x => x.CSI_Code).ToArray());
			AssertEquals("The 5 entryLine SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Statuc (ACC)", true, clSupDocsEntryLine.All(x => x.CSI_Status == "ACC"));

			clSupDocsEntryHeader = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
			AssertEquals("There are 1 entryHeader SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments", 1, clSupDocsEntryHeader.Length);
			AssertContainsExactElementsInAnyOrder("The 1 entryHeader SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001" }, clSupDocsEntryHeader.Select(x => x.CSI_Code).ToArray());
		});
	}

	public void TestProcessDVD5018EntryLineSupportingDocuments()
	{
		var (entryLine, _) = GetTestEntryLine();
		CombineAssertions(() =>
		{
			entryLine.AddEntryLineDocument<SupportingDocument>("5018", "ES3600000001", subType: "LIQ", status: "ACC");
			entryLine.AddEntryLineDocument<SupportingDocument>("5018", "ES3600000002", subType: "LIQ", status: ZString.Empty);
			entryLine.AddEntryLineDocument<SupportingDocument>("5018", "ES3600000003", subType: "LIQ", status: "REJ");
			entryLine.AddEntryLineDocument<SupportingDocument>("5018", "ES3600000004", subType: ZString.Empty, status: ZString.Empty);
			entryLine.AddEntryLineDocument<SupportingDocument>("AAAA", "ES3600000005", subType: "LIQ", status: "ACC");

			var clSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 5 entryLine SupportingDocuments before calling ProcessDVD5018EntryLineSupportingDocuments", 5, clSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 5 entryLine SupportingDocuments before calling ProcessDVD5018EntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003", "ES3600000004", "ES3600000005" }, clSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());

			entryLine.ProcessDVD5018EntryLineSupportingDocuments();
			clSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 4 entryLine SupportingDocuments after calling ProcessDVD5018EntryLineSupportingDocuments (only the 5018 LIQ ACC have been deleted)", 4, clSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 4 entryLine SupportingDocuments after calling ProcessDVD5018EntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000002", "ES3600000003", "ES3600000004", "ES3600000005" }, clSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000002"), "5018", "ES3600000002", "LIQ", "ACC");
			AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000003"), "5018", "ES3600000003", "LIQ", "ACC");
			AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000004"), "5018", "ES3600000004", ZString.Empty, ZString.Empty);
			AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000005"), "AAAA", "ES3600000005", "LIQ", "ACC");
		});
	}

	public void TestProcessExportAcceptedMessageDeleteAndCreateNewEntryLineSupportingDocs()
	{
		var (entryLine, _) = GetTestEntryLine(messageType: Enterprise.Customs.Business.JobMessageTypeList.Codes.Export);
		CombineAssertions(() =>
		{
			entryLine.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
			entryLine.AddEntryLineDocument<SupportingDocument>("X005", "ES3600000005", subType: "LIQ");
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("X001", "ES3600000001");
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("X00A", "ES360000000A", subType: "LIQ");

			var entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 2 EntryLine SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments", 2, entryLineSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 2 EntryLine SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X004", "X005" }, entryLineSupDocs.Select(x => x.CSI_Code).ToArray());

			var entryHeaderSupDocs = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
			AssertEquals("There are 2 EntryHeader SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments", 2, entryHeaderSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 2 EntryHeader SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001", "X00A" }, entryHeaderSupDocs.Select(x => x.CSI_Code).ToArray());

			entryLine.ProcessExportEntryLineSupportingDocuments();
			entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are only 5 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments (previous 1 have been deleted)", 5, entryLineSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 5 EntryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001", "X002", "X003", "N380", "X005" }, entryLineSupDocs.Select(x => x.CSI_Code).ToArray());

			entryHeaderSupDocs = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
			AssertEquals("There are only 1 entryHeader SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments (previous 1 have been deleted)", 1, entryHeaderSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 1 entryHeader SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X00A" }, entryHeaderSupDocs.Select(x => x.CSI_Code).ToArray());
		});
	}

	public void TestProcessExportAcceptedMessageUpdate_LIQ_EntryLineSupportingDocs()
	{
		var (entryLine, declaration) = GetTestEntryLine(messageType: Enterprise.Customs.Business.JobMessageTypeList.Codes.Export);
		CombineAssertions(() =>
		{
			entryLine.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
			entryLine.AddEntryLineDocument<SupportingDocument>("AAA", "10,00", subType: "LIQ", status: "ACC");
			entryLine.AddEntryLineDocument<SupportingDocument>("BBB", "10,00", subType: "LIQ", status: ZString.Empty);
			entryLine.AddEntryLineDocument<SupportingDocument>("CCC", "10,00", subType: "LIQ", status: "REJ");
			entryLine.AddEntryLineDocument<SupportingDocument>("DDD", "10,00", subType: "T2L", status: "ACC");
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("X001", "ES3600000001");
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("X00A", "ES360000000A", subType: "LIQ", status: "ACC");

			var clSupDocsEntryLine = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 5 entryLine SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments", 5, clSupDocsEntryLine.Length);
			AssertContainsExactElementsInAnyOrder("The 5 entryLine SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X004", "AAA", "BBB", "CCC", "DDD" }, clSupDocsEntryLine.Select(x => x.CSI_Code).ToArray());

			var clSupDocsEntryHeader = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
			AssertEquals("There are 2 entryHeader SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments", 2, clSupDocsEntryHeader.Length);
			AssertContainsExactElementsInAnyOrder("The 2 entryHeader SupportingDocuments before calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001", "X00A" }, clSupDocsEntryHeader.Select(x => x.CSI_Code).ToArray());

			entryLine.ProcessExportEntryLineSupportingDocuments();
			clSupDocsEntryLine = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 7 entryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments (only the non LIQ have been deleted)", 7, clSupDocsEntryLine.Length);
			AssertContainsExactElementsInAnyOrder("The 7 entryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X001", "X002", "X003", "N380", "AAA", "BBB", "CCC" }, clSupDocsEntryLine.Select(x => x.CSI_Code).ToArray());
			AssertEquals("The 7 entryLine SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Statuc (ACC)", true, clSupDocsEntryLine.All(x => x.CSI_Status == "ACC"));

			clSupDocsEntryHeader = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
			AssertEquals("There are 1 entryHeader SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments (only the non LIQ have been deleted)", 1, clSupDocsEntryHeader.Length);
			AssertContainsExactElementsInAnyOrder("The 1 entryHeader SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X00A" }, clSupDocsEntryHeader.Select(x => x.CSI_Code).ToArray());
			AssertEquals("The 1 entryHeader SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Statuc (ACC)", true, clSupDocsEntryHeader.All(x => x.CSI_Status == "ACC"));
		});
	}

	public void TestProcessComplXExportAcceptedMessageCreateNewEntryLineSupportingDocsCusEntryLineAndCusEntryHeader_SubStyleT2LT2C_StyleEmpty()
	{
		var (entryLine, _) = GetComplXExportTestEntryLine(instructionSubStyle: EntrySubStyleList.Codes.T2L);
		CombineAssertions(() =>
		{
			AssertEquals("There are no EntryLine SupportingDocuments before calling ProcessComplXExportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryLine(entryLine).Length);
			AssertEquals("There are no EntryHeader SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header).Length);

			entryLine.ProcessComplXExportEntryLineSupportingDocuments();
			var entryHeaderSupDocs = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
			AssertEquals("There are 2 EntryHeader SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments", 2, entryHeaderSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 2 EntryHeader SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002" }, entryHeaderSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertEquals("The 2 EntryHeader SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_Status", false, entryHeaderSupDocs.Any(x => x.CSI_Status != "ACC"));

			var entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 1 EntryLine SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments", 1, entryLineSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 1 EntryLine SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000003" }, entryLineSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertEquals("The 1 EntryLine SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_Status", false, entryLineSupDocs.Any(x => x.CSI_Status != "ACC"));
		});
	}

	public void TestProcessComplXExportAcceptedMessageCreateNewEntryLineSupportingDocsCusEntryLine_SubStyleNotEXST2LT2C_StyleEmpty_InTransitPeriod()
	{
		var (entryLine, declaration) = GetComplXExportTestEntryLine();
		using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, true))
		{
			CombineAssertions(() =>
			{
				AssertEquals("There are no EntryLine SupportingDocuments before calling ProcessComplXExportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryLine(entryLine).Length);

				entryLine.ProcessComplXExportEntryLineSupportingDocuments();
				var entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
				AssertEquals("There are 3 EntryLine SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments", 3, entryLineSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 3 EntryLine SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003" }, entryLineSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 3 EntryLine SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_Status", false, entryLineSupDocs.Any(x => x.CSI_Status != "ACC"));
			});
		}
	}

	public void TestProcessComplXExportAcceptedMessageCreateNewEntryLineSupportingDocsCusEntryLineAndCusEntryHeader_SubStyleNotEXST2LT2C_StyleEmpty_InFinalPeriod()
	{
		var (entryLine, declaration) = GetComplXExportTestEntryLine();
		using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, false))
		{
			CombineAssertions(() =>
			{
				AssertEquals("There are no EntryLine SupportingDocuments before calling ProcessComplXExportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryLine(entryLine).Length);
				AssertEquals("There are no EntryHeader SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header).Length);

				entryLine.ProcessComplXExportEntryLineSupportingDocuments();
				var entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
				AssertEquals("There are 1 EntryLine SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments", 1, entryLineSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 1 EntryLine SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000003" }, entryLineSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 1 EntryLine SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_Status", false, entryLineSupDocs.Any(x => x.CSI_Status != "ACC"));

				var entryHeaderSupDocs = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
				AssertEquals("There are 2 EntryHeader SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments", 2, entryHeaderSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 2 EntryHeader SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002" }, entryHeaderSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertEquals("The 2 EntryHeader SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_Status", false, entryHeaderSupDocs.Any(x => x.CSI_Status != "ACC"));
			});
		}
	}

	public void TestProcessComplXExportAcceptedMessageCreateNewEntryLineSupportingDocsCusEntryLine_SubStyleEXS_StyleEmpty()
	{
		var (entryLine, _) = GetComplXExportTestEntryLine(instructionSubStyle: ExsEntrySubStyleList.Codes.EXS);
		CombineAssertions(() =>
		{
			AssertEquals("There are no EntryLine SupportingDocuments before calling ProcessComplXExportEntryLineSupportingDocuments", 0, GetEntryLineSupportingDocumentsCusEntryLine(entryLine).Length);

			entryLine.ProcessComplXExportEntryLineSupportingDocuments();
			var entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 3 EntryLine SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments", 3, entryLineSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 3 EntryLine SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003" }, entryLineSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			AssertEquals("The 3 EntryLine SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_Status", false, entryLineSupDocs.Any(x => x.CSI_Status != "ACC"));
		});
	}

	public void TestProcessComplXExportExportAcceptedMessageMergeEntryLineSupportingDocs()
	{
		var (entryLine, _) = GetComplXExportTestEntryLine();
		CombineAssertions(() =>
		{
			entryLine.AddEntryLineDocument<SupportingDocument>("N380", "ES3600000003");
			entryLine.AddEntryLineDocument<SupportingDocument>("N380", "ES3600000004");
			entryLine.AddEntryLineDocument<SupportingDocument>("N380", "ES3600000005");
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("N380", "ES3600000001");
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("N380", "ES3600000007");
			Factory.Save();

			var entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 3 EntryLine SupportingDocuments before calling ProcessComplXExportEntryLineSupportingDocuments", 3, entryLineSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 3 EntryLine SupportingDocuments before calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000003", "ES3600000004", "ES3600000005" }, entryLineSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());

			var entryHeaderSupDocs = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
			AssertEquals("There are 2 entryHeader SupportingDocuments before calling ProcessComplXExportEntryLineSupportingDocuments", 2, entryHeaderSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 2 entryHeader SupportingDocuments before calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000007" }, entryHeaderSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());

			entryLine.ProcessComplXExportEntryLineSupportingDocuments();
			entryLineSupDocs = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 6 EntryLine SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments.", 6, entryLineSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 6 EntryLine SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003", "ES3600000004", "ES3600000005", "ES3600000007" }, entryLineSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());

			entryHeaderSupDocs = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
			AssertEquals("There are 2 entryHeader SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments.", 2, entryHeaderSupDocs.Length);
			AssertContainsExactElementsInAnyOrder("The 2 entryHeader SupportingDocuments after calling ProcessComplXExportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000007" }, entryHeaderSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
		});
	}

	public void TestProcessBox44ImportEntryLineSupportingDocuments()
	{
		var (entryLine, declaration) = GetTestEntryLine();
		CombineAssertions(() =>
		{
			entryLine.AddEntryLineDocument<SupportingDocument>("X004", "ES3600000004");
			entryLine.AddEntryLineDocument<SupportingDocument>("X005", "ES3600000005");
			entryLine.AddEntryLineDocument<SupportingDocument>("N380", "ES36000N3801");
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("X001", "ES3600000001");
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("X00A", "ES360000000A", subType: "LIQ", status: "ACC");
			Factory.Save();
			var clSupDocsEntryLine = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are 3 EntryLine SupportingDocuments before calling ProcessBox44ImportEntryLineSupportingDocuments", 3, clSupDocsEntryLine.Length);
			AssertContainsExactElementsInAnyOrder("The 3 EntryLine SupportingDocuments before calling ProcessBox44ImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X004", "X005", "N380" }, clSupDocsEntryLine.Select(x => x.CSI_Code).ToArray());

			var clSupDocsEntryHeader = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
			AssertEquals("There are 2 EntryHeader SupportingDocuments before calling ProcessBox44ImportEntryLineSupportingDocuments", 2, clSupDocsEntryHeader.Length);
			AssertContainsExactElementsInAnyOrder("The 2 EntryHeader SupportingDocuments before calling ProcessBox44ImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X00A", "X001" }, clSupDocsEntryHeader.Select(x => x.CSI_Code).ToArray());

			entryLine.ProcessBox44ImportEntryLineSupportingDocuments();
			clSupDocsEntryLine = GetEntryLineSupportingDocumentsCusEntryLine(entryLine);
			AssertEquals("There are only 7 EntryLine SupportingDocuments after calling ProcessBox44ImportEntryLineSupportingDocuments (previous 4 have been presisted)", 7, clSupDocsEntryLine.Length);
			AssertContainsExactElementsInAnyOrder("The 7 EntryLine SupportingDocuments after calling ProcessBox44ImportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X00A", "X001", "X002", "X003", "X004", "X005", "N380" }, clSupDocsEntryLine.Select(x => x.CSI_Code).ToArray());

			clSupDocsEntryHeader = GetEntryLineSupportingDocumentsCusEntryHeader(entryLine.Header);
			AssertEquals("There are 2 entryHeader SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments", 2, clSupDocsEntryHeader.Length);
			AssertContainsExactElementsInAnyOrder("The 2 entryHeader SupportingDocuments after calling ProcessExportEntryLineSupportingDocuments have the correct CSI_Codes", new ZString[] { "X00A", "X001" }, clSupDocsEntryHeader.Select(x => x.CSI_Code).ToArray());
		});
	}

	public void TestGetSupportingDocumentByCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		var supportingDocument = SupportingDocumentHelper.GetSupportingDocumentByCode(declaration.SupportingDocuments, "1003");
		CombineAssertions(() =>
		{
			AssertEquals("List empty return nothing", null, supportingDocument);

			var expectedDocument = AddDocumentToDeclaration(declaration, "N380", "Invoice Document");
			supportingDocument = SupportingDocumentHelper.GetSupportingDocumentByCode(declaration.SupportingDocuments, "1003");
			AssertEquals("No document match in the list return nothing", null, supportingDocument);

			supportingDocument = SupportingDocumentHelper.GetSupportingDocumentByCode(declaration.SupportingDocuments, "N380");
			AssertEquals("Document found in the list return the document", expectedDocument, supportingDocument);
		});
	}

	public void TestIsDocumentInComplXExportAcceptedDocumentList()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
		var supDoc = AddDocumentToDeclaration(declaration, "DOC", "Invoice Document");
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();
		var readOnlySupDoc = entryLine.ReadOnlySupportingDocuments.Cast<ReadOnlySupportingDocument>().FirstOrDefault();

		CombineAssertions(() =>
		{
			AssertEquals("Supporting Document is not on the list", false, supDoc.IsDocumentInComplXExportAcceptedDocumentList());
			AssertEquals("Read Only Supporting Document is not on the list", false, readOnlySupDoc.IsDocumentInComplXExportAcceptedDocumentList());
			supDoc.CSI_Code = "N380";
			readOnlySupDoc = entryLine.ReadOnlySupportingDocuments.Cast<ReadOnlySupportingDocument>().FirstOrDefault();
			AssertEquals("Supporting Document is on the list", true, supDoc.IsDocumentInComplXExportAcceptedDocumentList());
			AssertEquals("Read Only Supporting Document is on the list", true, readOnlySupDoc.IsDocumentInComplXExportAcceptedDocumentList());
		});
	}

	public void TestDoesNotMatchAnyPreviouslySentComplXExportDocument()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var supDoc = AddDocumentToDeclaration(declaration, "N380", "Document1");
		var supDoc2 = AddDocumentToDeclaration(declaration, "N325", "Document2");
		var supDoc3 = AddDocumentToDeclaration(declaration, "N740", "Document3");
		var supDoc4 = AddDocumentToDeclaration(declaration, "N705", "Document4");
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.AddEntryLineDocument<SupportingDocument>("N325", "Document2");
		entryLine.AddEntryLineDocument<SupportingDocument>("N740", "Document3");
		var supDocsInEntryLine = entryLine.GetPreviouslySentSupportingDocuments();

		CombineAssertions(() =>
		{
			AssertEquals("Supporting Document is a Invoice Document and is not in EntryLine", true, supDoc.DoesNotMatchAnyPreviouslySentComplXExportDocument(supDocsInEntryLine));
			AssertEquals("Supporting Document is a Invoice Document and is in EntryLine", false, supDoc2.DoesNotMatchAnyPreviouslySentComplXExportDocument(supDocsInEntryLine));
			AssertEquals("Supporting Document is not a Invoice Document and is in EntryLine", false, supDoc3.DoesNotMatchAnyPreviouslySentComplXExportDocument(supDocsInEntryLine));
			AssertEquals("Supporting Document is not a Invoice Document and is not in EntryLine", false, supDoc4.DoesNotMatchAnyPreviouslySentComplXExportDocument(supDocsInEntryLine));
		});
	}

	public void TestGetPreviouslySentComplXExportAcceptedDocuments()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.AddEntryLineDocument<SupportingDocument>("N705", "Document1");
		entryLine.AddEntryLineDocument<SupportingDocument>("N325", "Document2");
		entryLine.AddEntryLineDocument<SupportingDocument>("N740", "Document3");
		entryLine.AddEntryLineDocument<SupportingDocument>("N380", "Document4");
		var supDocInEntryLine = SupportingDocumentHelper.GetPreviouslySentComplXExportAcceptedDocumentsEntryLine(entryLine);

		AssertEquals("Total Compl X Export Accepted Documents in EntryLine", 2, supDocInEntryLine.Count());
	}

	public void TestGetReadOnlyComplXExportAcceptedDocuments()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		AddDocumentToDeclaration(declaration, "N705", "Document1");
		AddDocumentToDeclaration(declaration, "N325", "Document2");
		AddDocumentToDeclaration(declaration, "N740", "Document3");
		AddDocumentToDeclaration(declaration, "N380", "Document4");
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();
		var supDocInEntryLine = SupportingDocumentHelper.GetReadOnlyComplXExportAcceptedDocuments(entryLine);

		AssertEquals("Total Compl X Export Accepted Documents in ReadOnly", 2, supDocInEntryLine.Count());
	}

	public void TestHasComplXExportDocuments()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var supDoc = AddDocumentToDeclaration(declaration, "N380", "Document1");
		var supDoc2 = AddDocumentToDeclaration(declaration, "N325", "Document2");
		var supDoc3 = AddDocumentToDeclaration(declaration, "N740", "Document3");
		var supDoc4 = AddDocumentToDeclaration(declaration, "N705", "Document4");

		var supDocsList = new List<SupportingDocument>();

		CombineAssertions(() =>
		{
			AssertEquals("Given list is empty so method returns false", false, SupportingDocumentHelper.HasComplXExportDocuments(supDocsList.ToArray()));

			supDocsList.Add(supDoc);
			AssertEquals("Given list has one document and it is ComplX Export Accepted Document so method returns true", true, SupportingDocumentHelper.HasComplXExportDocuments(supDocsList.ToArray()));

			supDocsList.Remove(supDoc);
			supDocsList.Add(supDoc3);
			supDocsList.Add(supDoc4);
			AssertEquals("Given list has two documents but none are ComplX Export Accepted Documents so method returns false", false, SupportingDocumentHelper.HasComplXExportDocuments(supDocsList.ToArray()));

			supDocsList.Add(supDoc2);
			supDocsList.Add(supDoc);
			AssertEquals("Given list has four documents and at least one is ComplX Export Accepted Document so method returns true", true, SupportingDocumentHelper.HasComplXExportDocuments(supDocsList.ToArray()));
		});
	}

	public void TestGetFirstComplXExportDocument()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var supDoc = AddDocumentToDeclaration(declaration, "N380", "Document1");
		var supDoc2 = AddDocumentToDeclaration(declaration, "N325", "Document2");
		var supDoc3 = AddDocumentToDeclaration(declaration, "N740", "Document3");
		var supDoc4 = AddDocumentToDeclaration(declaration, "N705", "Document4");

		var supDocsList = new List<SupportingDocument>();

		CombineAssertions(() =>
		{
			AssertNull("Given list is empty so method returns null", SupportingDocumentHelper.GetFirstComplXExportDocument(supDocsList.ToArray()));

			supDocsList.Add(supDoc);
			AssertEquals("Given list has one document and it is ComplX Export Accepted Document so method returns that document", supDoc, SupportingDocumentHelper.GetFirstComplXExportDocument(supDocsList.ToArray()));

			supDocsList.Remove(supDoc);
			supDocsList.Add(supDoc3);
			supDocsList.Add(supDoc4);
			AssertNull("Given list has two documents but none are ComplX Export Accepted Documents so method returns null", SupportingDocumentHelper.GetFirstComplXExportDocument(supDocsList.ToArray()));

			supDocsList.Add(supDoc2);
			supDocsList.Add(supDoc);
			AssertEquals("Given list has four documents and at least one is ComplX Export Accepted Document so method returns the first one in the list", supDoc2, SupportingDocumentHelper.GetFirstComplXExportDocument(supDocsList.ToArray()));
		});
	}

	void AssertDocument(SupportingDocument doc, ZString code, ZString reference, ZString subtype, ZString status)
	{
		AssertEquals("Doc with reference " + reference + " has correct CSI_Code", code, doc.CSI_Code);
		AssertEquals("Doc with reference " + reference + " has correct CSI_SubType", subtype, doc.CSI_SubType);
		AssertEquals("Doc with reference " + reference + " has correct CSCSI_StatusI_Code", status, doc.CSI_Status);
	}

	(CusEntryLine, JobDeclaration) GetTestEntryLine(string messageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import, string instructionSubStyle = null, string instructionStyle = null)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		declaration.JE_DataModel = Core.Constants.CountryCodes.Spain;
		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		if (instructionSubStyle != null)
		{
			entryInstruction.CEI_SubStyle = instructionSubStyle;
		}
		if (instructionStyle != null)
		{
			entryInstruction.CEI_Style = instructionStyle;
		}
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var suppDoc1 = declaration.SupportingDocuments.AddNew();
		suppDoc1.CSI_Code = "X001";
		suppDoc1.CSI_ReferenceNumber = "ES3600000001";
		suppDoc1.CSI_Status = ZString.Empty;

		var suppDoc11 = declaration.SupportingDocuments.AddNew();
		suppDoc11.CSI_Code = "X001";
		suppDoc11.CSI_ReferenceNumber = "es3600000001";
		suppDoc11.CSI_Status = ZString.Empty;

		var suppDoc2 = entryInstruction.SupportingDocuments.AddNew();
		suppDoc2.CSI_Code = "X002";
		suppDoc2.CSI_ReferenceNumber = "ES3600000002";
		suppDoc2.CSI_Status = ZString.Empty;

		var suppDoc3 = invoice.SupportingDocuments.AddNew();
		suppDoc3.CSI_Code = "X003";
		suppDoc3.CSI_ReferenceNumber = "ES3600000003";
		suppDoc3.CSI_Status = ZString.Empty;

		var suppDoc4 = invoiceLine.SupportingDocuments.AddNew();
		suppDoc4.CSI_Code = "N380";
		suppDoc4.CSI_ReferenceNumber = "ES36000N3801";
		suppDoc4.CSI_Status = ZString.Empty;

		var suppDoc41 = invoiceLine.SupportingDocuments.AddNew();
		suppDoc41.CSI_Code = "N380";
		suppDoc41.CSI_ReferenceNumber = "es36000N3801";
		suppDoc41.CSI_Status = ZString.Empty;

		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "EntryNum";
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entryHeader.AllEntryLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		Factory.Save();
		return (entryLine, declaration);
	}

	(CusEntryLine, JobDeclaration) GetComplXExportTestEntryLine(string instructionSubStyle = null, string instructionStyle = null)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		if (instructionSubStyle != null)
		{
			entryInstruction.CEI_SubStyle = instructionSubStyle;
		}
		if (instructionStyle != null)
		{
			entryInstruction.CEI_Style = instructionStyle;
		}
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var suppDoc1 = declaration.SupportingDocuments.AddNew();
		suppDoc1.CSI_Code = "N380";
		suppDoc1.CSI_ReferenceNumber = "ES3600000001";
		suppDoc1.CSI_Status = ZString.Empty;

		var suppDoc2 = entryInstruction.SupportingDocuments.AddNew();
		suppDoc2.CSI_Code = "N380";
		suppDoc2.CSI_ReferenceNumber = "ES3600000002";
		suppDoc2.CSI_Status = ZString.Empty;

		var suppDoc3 = invoice.SupportingDocuments.AddNew();
		suppDoc3.CSI_Code = "N380";
		suppDoc3.CSI_ReferenceNumber = "ES3600000003";
		suppDoc3.CSI_Status = ZString.Empty;

		var suppDoc4 = invoiceLine.SupportingDocuments.AddNew();
		suppDoc4.CSI_Code = "X001";
		suppDoc4.CSI_ReferenceNumber = "ES36000X0011";
		suppDoc4.CSI_Status = ZString.Empty;

		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "EntryNum";
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entryHeader.AllEntryLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		Factory.Save();
		return (entryLine, declaration);
	}

	SupportingDocument[] GetEntryLineSupportingDocumentsCusEntryLine(CusEntryLine entryLine)
	{
		var query = new ZQuery(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument);
		query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, entryLine.PK);
		query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, entryLine.TablePrefix);
		return Factory.Load<SupportingDocument>(query);
	}

	SupportingDocument[] GetEntryLineSupportingDocumentsCusEntryHeader(CusEntryHeader header)
	{
		var query = new ZQuery(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument);
		query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, header.PK);
		query.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, header.TablePrefix);
		return Factory.Load<SupportingDocument>(query);
	}

	SupportingDocument AddDocumentToDeclaration(JobDeclaration declaration, ZString code, ZString reference)
	{
		var supDoc = Factory.CreateSupportingDocument(code, reference);
		declaration.SupportingDocuments.Add(supDoc);

		return supDoc;
	}

	protected IDisposable TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(JobDeclaration declaration, bool configurationValue)
	{
		ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);
		return ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(declaration.Factory, "IsTransitionPeriodAES30Core", configurationValue, declaration.GetDefaultDataGroupingCode());
	}
}
