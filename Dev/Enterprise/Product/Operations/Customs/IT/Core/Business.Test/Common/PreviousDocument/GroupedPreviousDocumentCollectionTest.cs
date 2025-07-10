using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class GroupedPreviousDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GroupedPreviousDocumentCollection>
{
	public void TestExceptionWhenArgumentIsNull()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when mergedPreviousDocumentsProvider parameter is null", () => GroupedPreviousDocumentCollection.LoadNew(null, Factory));
		AssertExceptionThrown<ArgumentNullException>("Exception expected when factory parameter is null", () => GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, null));
	}

	public void TestGroupedPreviousDocumentCollectionWithNoPreviousDocuments()
	{
		var groupedPreviousDocumentCollection = GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
		AssertEquals("When there are no previous document, GroupedPreviousDocumentCollection.Count", 0, groupedPreviousDocumentCollection.Count);
	}

	public void TestGroupedPreviousDocumentCollectionWithOneSummaryDeclarationAndZeroPreviousProcedures()
	{
		var summaryDeclaration1 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 01, 01), referenceNumber: "1RP", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 1, procedure: "A3", "MRN1", "Tariff1", netMass: 110m, supplementaryQuantity: 120m, grossMass: 130m, packageQuantity: 100, isSummaryDeclarationDocument: true);
		mergedPreviousDocumentCollection.Add(summaryDeclaration1);

		var groupedPreviousDocumentCollection = GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
		AssertEquals("When there is one Summary Declaration Document and zero Previous Procedures, GroupedPreviousDocumentCollection.Count", 0, groupedPreviousDocumentCollection.Count);
	}

	public void TestGroupedPreviousDocumentCollectionWithOnePreviousProcedureAndZeroSummaryDeclarations()
	{
		var previousProcedureDocument1 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 02, 01), referenceNumber: "1PA", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 4, procedure: "2", "MRNA", "Tariff1", netMass: 110m, supplementaryQuantity: 120m, grossMass: 130m, packageQuantity: 100, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument1);

		var groupedPreviousDocumentCollection = GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
		AssertEquals("When there is one Previous Procedure and zero Summary Declarations, GroupedPreviousDocumentCollection.Count", 0, groupedPreviousDocumentCollection.Count);
	}

	public void TestGroupedPreviousDocumentCollectionWithOneSummaryDeclarationAndOnePreviousProcedure()
	{
		var summaryDeclaration1 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 01, 01), referenceNumber: "1RP", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 1, procedure: "A3", "MRN1", "Tariff1", netMass: 110m, supplementaryQuantity: 120m, grossMass: 130m, packageQuantity: 100, isSummaryDeclarationDocument: true);
		mergedPreviousDocumentCollection.Add(summaryDeclaration1);

		var previousProcedureDocument1 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 02, 01), referenceNumber: "1RP", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 4, procedure: "2", "MRNA", "Tariff1", netMass: 110m, supplementaryQuantity: 120m, grossMass: 130m, packageQuantity: 100, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument1);

		if (!mergedPreviousDocumentsProviderMock.Object.IsExport)
		{
			var groupedPreviousDocumentCollection = GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
			AssertEquals("When there are one Summary Declaration and one Previous Procedure, GroupedPreviousDocumentCollection.Count", 0, groupedPreviousDocumentCollection.Count);
		}
		else
		{
			var groupedPreviousDocumentCollection = GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
			AssertEquals("When there are one Summary Declaration and one Previous Procedure, GroupedPreviousDocumentCollection.Count", 1, groupedPreviousDocumentCollection.Count);

			var groupedPreviousList = groupedPreviousDocumentCollection.Cast<GroupedPreviousDocument>();
			CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.SummaryDeclarationDocumentItemNumber == 1), summaryDeclaration1, previousProcedureDocument1, "Tariff1", 110, 120, 130, 100);
		}
	}

	public void TestGroupedPreviousDocumentCollectionWithManySummaryDeclarationsAndZeroPreviousProcedures()
	{
		var summaryDeclaration1 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 01, 01), referenceNumber: "1RP", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 1, procedure: "A3", "MRN1", "Tariff1", netMass: 110m, supplementaryQuantity: 120m, grossMass: 130m, packageQuantity: 100, isSummaryDeclarationDocument: true);
		mergedPreviousDocumentCollection.Add(summaryDeclaration1);
		var summaryDeclaration2 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 01, 02), referenceNumber: "2RP", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 2, procedure: "MRN", "MRN2", "Tariff2", netMass: 210m, supplementaryQuantity: 220m, grossMass: 230m, packageQuantity: 200, isSummaryDeclarationDocument: true);
		mergedPreviousDocumentCollection.Add(summaryDeclaration2);
		var summaryDeclaration3 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 01, 03), referenceNumber: "3RP", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 3, procedure: "PF", "MRN3", "Tariff3", netMass: 310m, supplementaryQuantity: 320m, grossMass: 330m, packageQuantity: 300, isSummaryDeclarationDocument: true);
		mergedPreviousDocumentCollection.Add(summaryDeclaration3);

		var groupedPreviousDocumentCollection = GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
		AssertEquals("When there are many Summary Declarations and zero Previous Procedures, GroupedPreviousDocumentCollection.Count", 3, groupedPreviousDocumentCollection.Count);

		var groupedPreviousList = groupedPreviousDocumentCollection.Cast<GroupedPreviousDocument>();

		CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.SummaryDeclarationDocumentItemNumber == 1), summaryDeclaration1, null, "Tariff1", 110, 120, 130, 100);
		CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.SummaryDeclarationDocumentItemNumber == 2), summaryDeclaration2, null, "Tariff2", 210, 220, 230, 200);
		CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.SummaryDeclarationDocumentItemNumber == 3), summaryDeclaration3, null, "Tariff3", 310, 320, 330, 300);
	}

	public void TestGroupedPreviousDocumentCollectionWithManyPreviousProceduresAndZeroSummaryDeclarations()
	{
		var previousProcedureDocument1 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 02, 01), referenceNumber: "1PA", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 4, procedure: "2", "MRNA", "Tariff1", netMass: 110m, supplementaryQuantity: 120m, grossMass: 130m, packageQuantity: 100, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument1);
		var previousProcedureDocument2 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 02, 02), referenceNumber: "2PA", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 5, procedure: "2S", "", "Tariff2", netMass: 210m, supplementaryQuantity: 220m, grossMass: 230m, packageQuantity: 200, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument2);
		var previousProcedureDocument3 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 02, 03), referenceNumber: "3PA", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 6, procedure: "5", "", "Tariff3", netMass: 310m, supplementaryQuantity: 320m, grossMass: 330m, packageQuantity: 300, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument3);

		var groupedPreviousDocumentCollection = GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
		AssertEquals("When there are many Previous Procedures and zero Summary Declarations, GroupedPreviousDocumentCollection.Count", 3, groupedPreviousDocumentCollection.Count);

		var groupedPreviousList = groupedPreviousDocumentCollection.Cast<GroupedPreviousDocument>();
		CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.PreviousProcedureDocumentItemNumber == 4), null, previousProcedureDocument1, "Tariff1", 110, 120, 130, 100);
		CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.PreviousProcedureDocumentItemNumber == 5), null, previousProcedureDocument2, "Tariff2", 210, 220, 230, 200);
		CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.PreviousProcedureDocumentItemNumber == 6), null, previousProcedureDocument3, "Tariff3", 310, 320, 330, 300);
	}

	public void TestGroupedPreviousDocumentCollectionWithManySummaryDeclarationAndOnePreviousProcedure()
	{
		var previousProcedureDocument1 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "1PA", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 99, procedure: "2", "", "TariffRP1", netMass: 11m, supplementaryQuantity: 12m, grossMass: 13m, packageQuantity: 10, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument1);

		var summaryDeclaration1 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 01, 01), referenceNumber: "1RP", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 1, procedure: "A3", "MRN1", "Tariff1", netMass: 110m, supplementaryQuantity: 120m, grossMass: 130m, packageQuantity: 100, isSummaryDeclarationDocument: true);
		mergedPreviousDocumentCollection.Add(summaryDeclaration1);
		var summaryDeclaration2 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 01, 02), referenceNumber: "2RP", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 2, procedure: "A3", "MRN2", "Tariff2", netMass: 210m, supplementaryQuantity: 220m, grossMass: 230m, packageQuantity: 200, isSummaryDeclarationDocument: true);
		mergedPreviousDocumentCollection.Add(summaryDeclaration2);
		var summaryDeclaration3 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 01, 03), referenceNumber: "3RP", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 3, procedure: "A3", "MRN3", "Tariff3", netMass: 310m, supplementaryQuantity: 320m, grossMass: 330m, packageQuantity: 300, isSummaryDeclarationDocument: true);
		mergedPreviousDocumentCollection.Add(summaryDeclaration3);

		var groupedPreviousDocumentCollection = GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
		AssertEquals("When there are many Summary Declarations and one Previous Procedure, GroupedPreviousDocumentCollection.Count", 3, groupedPreviousDocumentCollection.Count);

		var groupedPreviousList = groupedPreviousDocumentCollection.Cast<GroupedPreviousDocument>();
		CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.SummaryDeclarationDocumentItemNumber == 1), summaryDeclaration1, previousProcedureDocument1, "TariffRP1", 11, 12, 130, 100);
		CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.SummaryDeclarationDocumentItemNumber == 2), summaryDeclaration2, previousProcedureDocument1, "TariffRP1", 11, 12, 230, 200);
		CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.SummaryDeclarationDocumentItemNumber == 3), summaryDeclaration3, previousProcedureDocument1, "TariffRP1", 11, 12, 330, 300);
	}

	public void TestGroupedPreviousDocumentCollectionWithManyPreviousProcedureAndOneSummaryDeclaration()
	{
		var summaryDeclaration = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "1RP", "", "Z", "", "IT137100", lineNo: 99, procedure: "A3", "MRN1", "TariffPA1", netMass: 110m, supplementaryQuantity: 120m, grossMass: 130m, packageQuantity: 100, isSummaryDeclarationDocument: true);
		mergedPreviousDocumentCollection.Add(summaryDeclaration);

		var previousProcedureDocument1 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "1PA", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 1, procedure: "2", "", "TariffRP1", netMass: 11m, supplementaryQuantity: 12m, grossMass: 13m, packageQuantity: 10, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument1);
		var previousProcedureDocument2 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "2PA", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 2, procedure: "2T", "", "TariffRP2", netMass: 21m, supplementaryQuantity: 22m, grossMass: 23m, packageQuantity: 20, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument2);
		var previousProcedureDocument3 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "3PA", referenceNumberCin: "", "Z", "", "IT137100", lineNo: 3, procedure: "5", "", "TariffRP3", netMass: 31m, supplementaryQuantity: 32m, grossMass: 33m, packageQuantity: 30, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument3);

		var groupedPreviousDocumentCollection = GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
		AssertEquals("When there are many Previous Procedures and one Summary Declaration, GroupedPreviousDocumentCollection.Count", 3, groupedPreviousDocumentCollection.Count);

		var groupedPreviousList = groupedPreviousDocumentCollection.Cast<GroupedPreviousDocument>();
		CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.PreviousProcedureDocumentItemNumber == 1), summaryDeclaration, previousProcedureDocument1, "TariffRP1", 11, 12, 130, 100);
		CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.PreviousProcedureDocumentItemNumber == 2), summaryDeclaration, previousProcedureDocument2, "TariffRP2", 21, 22, 130, 100);
		CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.PreviousProcedureDocumentItemNumber == 3), summaryDeclaration, previousProcedureDocument3, "TariffRP3", 31, 32, 130, 100);
	}

	public void TestGroupedPreviousDocumentCollectionWithManySummaryDeclarationAndManyPreviousProcedure()
	{
		var summaryDeclaration1 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "1RP", "", "Z", "", "IT137100", lineNo: 99, procedure: "MRN", "MRN1", "TariffPA1", netMass: 110m, supplementaryQuantity: 120m, grossMass: 130m, packageQuantity: 100, isSummaryDeclarationDocument: true);
		mergedPreviousDocumentCollection.Add(summaryDeclaration1);

		var summaryDeclaration2 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "2RP", "", "Z", "", "IT137100", lineNo: 98, procedure: "A3", "MRN2", "TariffPA1", netMass: 210m, supplementaryQuantity: 220m, grossMass: 230m, packageQuantity: 200, isSummaryDeclarationDocument: true);
		mergedPreviousDocumentCollection.Add(summaryDeclaration2);

		var previousProcedureDocument1 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "1PA", "", "Z", "", "IT137100", lineNo: 1, procedure: "2", "", "TariffRP1", netMass: 11m, supplementaryQuantity: 12m, grossMass: 13m, packageQuantity: 10, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument1);
		var previousProcedureDocument2 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "2PA", "", "Z", "", "IT137100", lineNo: 2, procedure: "2S", "", "TariffRP2", netMass: 21m, supplementaryQuantity: 22m, grossMass: 23m, packageQuantity: 20, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument2);
		var previousProcedureDocument3 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "3PA", "", "Z", "", "IT137100", lineNo: 3, procedure: "2T", "", "TariffRP3", netMass: 31m, supplementaryQuantity: 32m, grossMass: 33m, packageQuantity: 30, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument3);

		var groupedPreviousDocumentCollection = GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
		AssertEquals("When there many Summary Declarations and many Previous Procedures, GroupedPreviousDocumentCollection.Count", 3, groupedPreviousDocumentCollection.Count);

		var groupedPreviousList = groupedPreviousDocumentCollection.Cast<GroupedPreviousDocument>();
		CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.PreviousProcedureDocumentItemNumber == 1), summaryDeclaration2, previousProcedureDocument1, "TariffRP1", 11, 12, 230, 200);
		CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.PreviousProcedureDocumentItemNumber == 2), summaryDeclaration2, previousProcedureDocument2, "TariffRP2", 21, 22, 230, 200);
		CheckGroupedDocument(groupedPreviousList.SingleOrDefault(x => x.PreviousProcedureDocumentItemNumber == 3), summaryDeclaration2, previousProcedureDocument3, "TariffRP3", 31, 32, 230, 200);
	}

	public void TestIsSendableInNbMessage()
	{
		mergedPreviousDocumentsProviderMock.Setup(m => m.NBStatus).Returns("");

		var groupedPreviousDocumentCollection = GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
		AssertEquals("[PRE-CONDITION] No previous documents, GroupedPreviousDocumentCollection.Count", 0, groupedPreviousDocumentCollection.Count);
		AssertEquals("IsSendableInNbMessage", false, groupedPreviousDocumentCollection.IsSendableInNbMessage);

		var summaryDeclaration = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "1RP", "", "Z", "", "IT137100", lineNo: 99, procedure: "A3", "MRN1", "TariffPA1", netMass: 110m, supplementaryQuantity: 120m, grossMass: 130m, packageQuantity: 100, isSummaryDeclarationDocument: true);
		mergedPreviousDocumentCollection.Add(summaryDeclaration);

		var previousProcedureDocument1 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "1PA", "", "Z", "", "IT137100", lineNo: 1, procedure: "2", "", "TariffRP1", netMass: 11m, supplementaryQuantity: 12m, grossMass: 13m, packageQuantity: 10, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument1);
		var previousProcedureDocument2 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "2PA", "", "Z", "", "IT137100", lineNo: 2, procedure: "2T", "", "TariffRP2", netMass: 21m, supplementaryQuantity: 22m, grossMass: 23m, packageQuantity: 20, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument2);

		groupedPreviousDocumentCollection = GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
		AssertEquals("[PRE-CONDITION] When there are many Summary Declarations and zero Previous Procedures, GroupedPreviousDocumentCollection.Count", 2, groupedPreviousDocumentCollection.Count);
		CombineAssertions("Testing property IsSendableInNbMessage with different CustomsStatus", () =>
		{
			mergedPreviousDocumentsProviderMock.Setup(m => m.NBStatus).Returns("NBR");
			AssertEquals($"When {nameof(IMergedPreviousDocumentsProvider.NBStatus)} = NBR, IsSendableInNbMessage", true, groupedPreviousDocumentCollection.IsSendableInNbMessage);

			mergedPreviousDocumentsProviderMock.Setup(m => m.NBStatus).Returns("NBA");
			AssertEquals($"When {nameof(IMergedPreviousDocumentsProvider.NBStatus)} = NBA, IsSendableInNbMessage", false, groupedPreviousDocumentCollection.IsSendableInNbMessage);

			mergedPreviousDocumentsProviderMock.Setup(m => m.NBStatus).Returns("NBS");
			AssertEquals($"When {nameof(IMergedPreviousDocumentsProvider.NBStatus)} = NBS, IsSendableInNbMessage", true, groupedPreviousDocumentCollection.IsSendableInNbMessage);

			mergedPreviousDocumentsProviderMock.Setup(m => m.NBStatus).Returns("");
			AssertEquals($"When {nameof(IMergedPreviousDocumentsProvider.NBStatus)} is Empty, IsSendableInNbMessage", true, groupedPreviousDocumentCollection.IsSendableInNbMessage);
		});
	}

	void CheckGroupedDocument(GroupedPreviousDocument groupedPreviousDocument, IMergedPreviousDocument summaryDeclarationDocument, IMergedPreviousDocument previousProcedureDocument, ZString tariff, ZDecimal netMass, ZDecimal supplementaryQuantity, ZDecimal grossMass, ZInt packageQuantity)
	{
		CombineAssertions("Grouped Previous Document", () =>
		 {
			 AssertEquals("SummaryDeclarationDocumentRegister", summaryDeclarationDocument?.Register ?? ZString.Empty, groupedPreviousDocument.SummaryDeclarationDocumentRegister);
			 AssertEquals("SummaryDeclarationDocumentReferenceNumber", summaryDeclarationDocument?.ReferenceNumber ?? ZString.Empty, groupedPreviousDocument.SummaryDeclarationDocumentReferenceNumber);
			 AssertEquals("SummaryDeclarationDocumentReferenceCIN", summaryDeclarationDocument?.ReferenceNumberCin ?? ZString.Empty, groupedPreviousDocument.SummaryDeclarationDocumentReferenceCIN);
			 AssertEquals("SummaryDeclarationDocumentDate", summaryDeclarationDocument?.Date ?? ZDateTime.Empty, groupedPreviousDocument.SummaryDeclarationDocumentDate);
			 AssertEquals("SummaryDeclarationDocumentSeries", summaryDeclarationDocument?.Series ?? ZString.Empty, groupedPreviousDocument.SummaryDeclarationDocumentSeries);
			 AssertEquals("SummaryDeclarationDocumentCustomsOffice", summaryDeclarationDocument?.CustomsOffice ?? ZString.Empty, groupedPreviousDocument.SummaryDeclarationDocumentCustomsOffice);
			 AssertEquals("SummaryDeclarationDocumentItemNumber", summaryDeclarationDocument?.ItemNumber ?? ZInt.Zero, groupedPreviousDocument.SummaryDeclarationDocumentItemNumber);
			 AssertEquals("SummaryDeclarationDocumentMRN", summaryDeclarationDocument?.Mrn ?? ZString.Empty, groupedPreviousDocument.SummaryDeclarationDocumentMRN);
			 AssertEquals("PreviousProcedureDocumentRegister", previousProcedureDocument?.Register ?? ZString.Empty, groupedPreviousDocument.PreviousProcedureDocumentRegister);
			 AssertEquals("PreviousProcedureDocumentReferenceNumber", previousProcedureDocument?.ReferenceNumber ?? ZString.Empty, groupedPreviousDocument.PreviousProcedureDocumentReferenceNumber);
			 AssertEquals("PreviousProcedureDocumentReferenceCIN", previousProcedureDocument?.ReferenceNumberCin ?? ZString.Empty, groupedPreviousDocument.PreviousProcedureDocumentReferenceCIN);
			 AssertEquals("PreviousProcedureDocumentDate", previousProcedureDocument?.Date ?? ZDateTime.Empty, groupedPreviousDocument.PreviousProcedureDocumentDate);
			 AssertEquals("PreviousProcedureDocumentSeries", previousProcedureDocument?.Series ?? ZString.Empty, groupedPreviousDocument.PreviousProcedureDocumentSeries);
			 AssertEquals("PreviousProcedureDocumentCustomsOffice", previousProcedureDocument?.CustomsOffice ?? ZString.Empty, groupedPreviousDocument.PreviousProcedureDocumentCustomsOffice);
			 AssertEquals("PreviousProcedureDocumentItemNumber", previousProcedureDocument?.ItemNumber ?? ZInt.Zero, groupedPreviousDocument.PreviousProcedureDocumentItemNumber);
			 AssertEquals("PackageQuantity", packageQuantity, groupedPreviousDocument.PackageQuantity);
			 AssertEquals("GrossMass", grossMass, groupedPreviousDocument.GrossMass);
			 AssertEquals("NetMass", netMass, groupedPreviousDocument.NetMass);
			 AssertEquals("SupplementaryQuantity", supplementaryQuantity, groupedPreviousDocument.SupplementaryQuantity);
			 AssertEquals("Tariff", tariff, groupedPreviousDocument.Tariff);
		 });
	}

	public void TestContainsSummaryDeclarationDocument()
	{
		var groupedPreviousDocumentCollection = GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
		Assert(!groupedPreviousDocumentCollection.ContainsSummaryDeclarationDocument);

		var previousProcedureDocument1 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 02, 01), referenceNumber: "1PA", "", "Z", "", "IT137100", lineNo: 4, procedure: "2", "MRNA", "Tariff1", netMass: 110m, supplementaryQuantity: 120m, grossMass: 130m, packageQuantity: 100, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument1);
		var previousProcedureDocument2 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 02, 02), referenceNumber: "2PA", "", "Z", "", "IT137100", lineNo: 5, procedure: "2S", "", "Tariff2", netMass: 210m, supplementaryQuantity: 220m, grossMass: 230m, packageQuantity: 200, isPreviousProcedureDocument: true);
		mergedPreviousDocumentCollection.Add(previousProcedureDocument2);
		groupedPreviousDocumentCollection = GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
		Assert(!groupedPreviousDocumentCollection.ContainsSummaryDeclarationDocument);

		var summaryDeclaration1 = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "1RP", "", "Z", "", "IT137100", lineNo: 99, procedure: "MRN", "MRN1", "TariffPA1", netMass: 110m, supplementaryQuantity: 120m, grossMass: 130m, packageQuantity: 100, isSummaryDeclarationDocument: true);
		mergedPreviousDocumentCollection.Add(summaryDeclaration1);
		groupedPreviousDocumentCollection = GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
		Assert(groupedPreviousDocumentCollection.ContainsSummaryDeclarationDocument);
	}

	protected override void SetUp()
	{
		base.SetUp();

		mergedPreviousDocumentsProviderMock = new Mock<IMergedPreviousDocumentsProvider>();
		mergedPreviousDocumentCollection = new List<IMergedPreviousDocument>();
		mergedPreviousDocumentsProviderMock.Setup(m => m.MergedPreviousDocuments).Returns(mergedPreviousDocumentCollection);
		mergedPreviousDocumentsProviderMock.Setup(m => m.IsExport).Returns(IsExport);
	}

	protected override GroupedPreviousDocumentCollection GetCollectionToTest()
	{
		mergedPreviousDocumentCollection.Add(PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "1RP", "", "Z", "", "IT137100", lineNo: 8, procedure: "A3", "", "TariffRP1", netMass: 11m, supplementaryQuantity: 12m, grossMass: 13m, packageQuantity: 10));
		mergedPreviousDocumentCollection.Add(PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "2RP", "", "Z", "", "IT137100", lineNo: 9, procedure: "PF", "", "TariffRP2", netMass: 21m, supplementaryQuantity: 22m, grossMass: 23m, packageQuantity: 20));
		mergedPreviousDocumentCollection.Add(PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "3RP", "", "Z", "", "IT137100", lineNo: 10, procedure: "MRN", "", "TariffRP3", netMass: 31m, supplementaryQuantity: 32m, grossMass: 33m, packageQuantity: 30));

		return GroupedPreviousDocumentCollection.LoadNew(mergedPreviousDocumentsProviderMock.Object, Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var mergedDocument = PreviousDocumentTestHelper.CreateMergedPreviousDocument("CO", new ZDate(2020, 1, 1), "1RP", "", "Z", "", "IT137100", lineNo: 8, procedure: "A3", "", "TariffRP1", netMass: 11m, supplementaryQuantity: 12m, grossMass: 13m, packageQuantity: 10);
		return new GroupedPreviousDocument(mergedPreviousDocumentsProviderMock.Object, mergedDocument, null, Factory);
	}

	protected abstract ZBool IsExport { get; }

	Mock<IMergedPreviousDocumentsProvider> mergedPreviousDocumentsProviderMock;
	List<IMergedPreviousDocument> mergedPreviousDocumentCollection;
}

[TestedType(typeof(GroupedPreviousDocumentCollection))]
sealed class ImportGroupedPreviousDocumentCollectionTest : GroupedPreviousDocumentCollectionTest
{
	protected override ZBool IsExport => ZBool.False;
}

[TestedType(typeof(GroupedPreviousDocumentCollection))]
sealed class ExportGroupedPreviousDocumentCollectionTest : GroupedPreviousDocumentCollectionTest
{
	protected override ZBool IsExport => ZBool.True;
}
