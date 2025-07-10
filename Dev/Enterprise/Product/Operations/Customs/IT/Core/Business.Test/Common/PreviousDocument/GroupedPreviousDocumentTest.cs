using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(GroupedPreviousDocument))]
sealed class GroupedPreviousDocumentTest : NonPersistentBusinessObjectTestCase
{
	public void TestPropertiesWhenIsBuiltBySummaryDeclarationDocument()
	{
		var mergedPreviousSummaryDeclarationDocument = PreviousDocumentTestHelper.CreateMergedPreviousDocument(csiCode: ""
			, dateOfIssue: new ZDate(2020, 02, 06)
			, referenceNumber: "123456"
			, referenceNumberCin: "A"
			, subType: ""
			, status: "B1"
			, customsOffice: "OFFICE1"
			, lineNo: 1
			, procedure: "A1"
			, referenceNumber2: "MRN1"
			, tariff: "TARIFF1"
			, netMass: 150
			, supplementaryQuantity: 30
			, grossMass: 170
			, packageQuantity: 10);

		var mergedPreviousDocumentProvider = GetNewMergedPreviousDocumentsProvider();

		var groupedPreviousDocument = new GroupedPreviousDocument(mergedPreviousDocumentProvider, mergedPreviousSummaryDeclarationDocument, null, Factory);
		CombineAssertions("GroupedPreviousDocument Summary Declaration Mode", () =>
		{
			AssertEquals("SummaryDeclarationDocumentRegister", "A1", groupedPreviousDocument.SummaryDeclarationDocumentRegister);
			AssertEquals("SummaryDeclarationDocumentReferenceNumber", "123456", groupedPreviousDocument.SummaryDeclarationDocumentReferenceNumber);
			AssertEquals("SummaryDeclarationDocumentReferenceCIN", "A", groupedPreviousDocument.SummaryDeclarationDocumentReferenceCIN);
			AssertEquals("SummaryDeclarationDocumentDate", new ZDateTime(2020, 02, 06), groupedPreviousDocument.SummaryDeclarationDocumentDate);
			AssertEquals("SummaryDeclarationDocumentSeries", "B1", groupedPreviousDocument.SummaryDeclarationDocumentSeries);
			AssertEquals("SummaryDeclarationDocumentCustomsOffice", "OFFICE1", groupedPreviousDocument.SummaryDeclarationDocumentCustomsOffice);
			AssertEquals("SummaryDeclarationDocumentItemNumber", 1, groupedPreviousDocument.SummaryDeclarationDocumentItemNumber);
			AssertEquals("SummaryDeclarationDocumentMRN", "MRN1", groupedPreviousDocument.SummaryDeclarationDocumentMRN);
			AssertEquals("PreviousProcedureDocumentRegister", ZString.Empty, groupedPreviousDocument.PreviousProcedureDocumentRegister);
			AssertEquals("PreviousProcedureDocumentReferenceNumber", ZString.Empty, groupedPreviousDocument.PreviousProcedureDocumentReferenceNumber);
			AssertEquals("PreviousProcedureDocumentReferenceCIN", ZString.Empty, groupedPreviousDocument.PreviousProcedureDocumentReferenceCIN);
			AssertEquals("PreviousProcedureDocumentDate", ZDateTime.Empty, groupedPreviousDocument.PreviousProcedureDocumentDate);
			AssertEquals("PreviousProcedureDocumentSeries", ZString.Empty, groupedPreviousDocument.PreviousProcedureDocumentSeries);
			AssertEquals("PreviousProcedureDocumentCustomsOffice", ZString.Empty, groupedPreviousDocument.PreviousProcedureDocumentCustomsOffice);
			AssertEquals("PreviousProcedureDocumentItemNumber", ZInt.Zero, groupedPreviousDocument.PreviousProcedureDocumentItemNumber);
			AssertEquals("PackageQuantity", 10, groupedPreviousDocument.PackageQuantity);
			AssertEquals("GrossMass", 170m, groupedPreviousDocument.GrossMass);
			AssertEquals("NetMass", 150m, groupedPreviousDocument.NetMass);
			AssertEquals("SupplementaryQuantity", 30m, groupedPreviousDocument.SupplementaryQuantity);
			AssertEquals("Tariff", "TARIFF1", groupedPreviousDocument.Tariff);
		});
	}

	public void TestPropertiesWhenIsBuildByPreviousProcedure()
	{
		var mergedPreviousSummaryDeclarationDocument = PreviousDocumentTestHelper.CreateMergedPreviousDocument(csiCode: ""
			, dateOfIssue: new ZDate(2020, 03, 06)
			, referenceNumber: "654321"
			, referenceNumberCin: "Z"
			, subType: ""
			, status: "B2"
			, customsOffice: "OFFICE2"
			, lineNo: 2
			, procedure: "A2"
			, referenceNumber2: "MRN1"
			, tariff: "TARIFF2"
			, netMass: 200
			, supplementaryQuantity: 50
			, grossMass: 240
			, packageQuantity: 5);

		var mergedPreviousDocumentProvider = GetNewMergedPreviousDocumentsProvider();

		var groupedPreviousDocument = new GroupedPreviousDocument(mergedPreviousDocumentProvider, null, mergedPreviousSummaryDeclarationDocument, Factory);
		CombineAssertions("GroupedPreviousDocument Previous Procedure Document Mode", () =>
		{
			AssertEquals("SummaryDeclarationDocumentRegister", ZString.Empty, groupedPreviousDocument.SummaryDeclarationDocumentRegister);
			AssertEquals("SummaryDeclarationDocumentReferenceNumber", ZString.Empty, groupedPreviousDocument.SummaryDeclarationDocumentReferenceNumber);
			AssertEquals("SummaryDeclarationDocumentReferenceCIN", ZString.Empty, groupedPreviousDocument.SummaryDeclarationDocumentReferenceCIN);
			AssertEquals("SummaryDeclarationDocumentDate", ZDateTime.Empty, groupedPreviousDocument.SummaryDeclarationDocumentDate);
			AssertEquals("SummaryDeclarationDocumentSeries", ZString.Empty, groupedPreviousDocument.SummaryDeclarationDocumentSeries);
			AssertEquals("SummaryDeclarationDocumentCustomsOffice", ZString.Empty, groupedPreviousDocument.SummaryDeclarationDocumentCustomsOffice);
			AssertEquals("SummaryDeclarationDocumentItemNumber", ZInt.Zero, groupedPreviousDocument.SummaryDeclarationDocumentItemNumber);
			AssertEquals("SummaryDeclarationDocumentMRN", ZString.Empty, groupedPreviousDocument.SummaryDeclarationDocumentMRN);
			AssertEquals("PreviousProcedureDocumentRegister", "A2", groupedPreviousDocument.PreviousProcedureDocumentRegister);
			AssertEquals("PreviousProcedureDocumentReferenceNumber", "654321", groupedPreviousDocument.PreviousProcedureDocumentReferenceNumber);
			AssertEquals("PreviousProcedureDocumentReferenceCIN", "Z", groupedPreviousDocument.PreviousProcedureDocumentReferenceCIN);
			AssertEquals("PreviousProcedureDocumentDate", new ZDateTime(2020, 03, 06), groupedPreviousDocument.PreviousProcedureDocumentDate);
			AssertEquals("PreviousProcedureDocumentSeries", "B2", groupedPreviousDocument.PreviousProcedureDocumentSeries);
			AssertEquals("PreviousProcedureDocumentCustomsOffice", "OFFICE2", groupedPreviousDocument.PreviousProcedureDocumentCustomsOffice);
			AssertEquals("PreviousProcedureDocumentItemNumber", 2, groupedPreviousDocument.PreviousProcedureDocumentItemNumber);
			AssertEquals("PackageQuantity", 5, groupedPreviousDocument.PackageQuantity);
			AssertEquals("GrossMass", 240m, groupedPreviousDocument.GrossMass);
			AssertEquals("NetMass", 200m, groupedPreviousDocument.NetMass);
			AssertEquals("SupplementaryQuantity", 50m, groupedPreviousDocument.SupplementaryQuantity);
			AssertEquals("Tariff", "TARIFF2", groupedPreviousDocument.Tariff);
		});
	}

	public void TestPropertiesWhenIsBuiltByBothPreviousDocument()
	{
		var mergedPreviousSummaryDeclarationDocument = PreviousDocumentTestHelper.CreateMergedPreviousDocument(csiCode: ""
			, dateOfIssue: new ZDate(2020, 02, 06)
			, referenceNumber: "123456"
			, referenceNumberCin: "A"
			, subType: ""
			, status: "B1"
			, customsOffice: "OFFICE1"
			, lineNo: 1
			, procedure: "A1"
			, referenceNumber2: "MRN1"
			, tariff: "TARIFF1"
			, netMass: 150
			, supplementaryQuantity: 30
			, grossMass: 170
			, packageQuantity: 10);

		var mergedPreviousProcedureDocument = PreviousDocumentTestHelper.CreateMergedPreviousDocument(csiCode: ""
			, dateOfIssue: new ZDate(2020, 03, 06)
			, referenceNumber: "654321"
			, referenceNumberCin: "Z"
			, subType: ""
			, status: "B2"
			, customsOffice: "OFFICE2"
			, lineNo: 2
			, procedure: "A2"
			, referenceNumber2: "MRN1"
			, tariff: "TARIFF2"
			, netMass: 200
			, supplementaryQuantity: 50
			, grossMass: 240
			, packageQuantity: 5);

		var mergedPreviousDocumentProvider = GetNewMergedPreviousDocumentsProvider();

		var groupedPreviousDocument = new GroupedPreviousDocument(mergedPreviousDocumentProvider, mergedPreviousSummaryDeclarationDocument, mergedPreviousProcedureDocument, Factory);
		CombineAssertions("GroupedPreviousDocument", () =>
		{
			AssertEquals("SummaryDeclarationDocumentRegister", "A1", groupedPreviousDocument.SummaryDeclarationDocumentRegister);
			AssertEquals("SummaryDeclarationDocumentReferenceNumber", "123456", groupedPreviousDocument.SummaryDeclarationDocumentReferenceNumber);
			AssertEquals("SummaryDeclarationDocumentReferenceCIN", "A", groupedPreviousDocument.SummaryDeclarationDocumentReferenceCIN);
			AssertEquals("SummaryDeclarationDocumentDate", new ZDateTime(2020, 02, 06), groupedPreviousDocument.SummaryDeclarationDocumentDate);
			AssertEquals("SummaryDeclarationDocumentSeries", "B1", groupedPreviousDocument.SummaryDeclarationDocumentSeries);
			AssertEquals("SummaryDeclarationDocumentCustomsOffice", "OFFICE1", groupedPreviousDocument.SummaryDeclarationDocumentCustomsOffice);
			AssertEquals("SummaryDeclarationDocumentItemNumber", 1, groupedPreviousDocument.SummaryDeclarationDocumentItemNumber);
			AssertEquals("SummaryDeclarationDocumentMRN", "MRN1", groupedPreviousDocument.SummaryDeclarationDocumentMRN);
			AssertEquals("PreviousProcedureDocumentRegister", "A2", groupedPreviousDocument.PreviousProcedureDocumentRegister);
			AssertEquals("PreviousProcedureDocumentReferenceNumber", "654321", groupedPreviousDocument.PreviousProcedureDocumentReferenceNumber);
			AssertEquals("PreviousProcedureDocumentReferenceCIN", "Z", groupedPreviousDocument.PreviousProcedureDocumentReferenceCIN);
			AssertEquals("PreviousProcedureDocumentDate", new ZDateTime(2020, 03, 06), groupedPreviousDocument.PreviousProcedureDocumentDate);
			AssertEquals("PreviousProcedureDocumentSeries", "B2", groupedPreviousDocument.PreviousProcedureDocumentSeries);
			AssertEquals("PreviousProcedureDocumentCustomsOffice", "OFFICE2", groupedPreviousDocument.PreviousProcedureDocumentCustomsOffice);
			AssertEquals("PreviousProcedureDocumentItemNumber", 2, groupedPreviousDocument.PreviousProcedureDocumentItemNumber);
			AssertEquals("PackageQuantity", 10, groupedPreviousDocument.PackageQuantity);
			AssertEquals("GrossMass", 170m, groupedPreviousDocument.GrossMass);
			AssertEquals("NetMass", 200m, groupedPreviousDocument.NetMass);
			AssertEquals("SupplementaryQuantity", 50m, groupedPreviousDocument.SupplementaryQuantity);
			AssertEquals("Tariff", "TARIFF2", groupedPreviousDocument.Tariff);
		});
	}

	public void TestConstructor()
	{
		var mergedPreviousDocument = new Mock<IMergedPreviousDocument>();
		AssertExceptionThrown<ArgumentNullException>(() => new GroupedPreviousDocument(null, null, mergedPreviousDocument.Object, null));
		AssertExceptionThrown<ArgumentNullException>(() => new GroupedPreviousDocument(null, null, null, Factory));
		AssertNoExceptionThrown(() => new GroupedPreviousDocument(GetNewMergedPreviousDocumentsProvider(), mergedPreviousDocument.Object, null, Factory));
	}

	public void TestEntryLineNumber()
	{
		var mergedPreviousDocument = new Mock<IMergedPreviousDocument>();
		var mergedPreviousDocumentProviderMock = new Mock<IMergedPreviousDocumentsProvider>();
		mergedPreviousDocumentProviderMock.Setup(m => m.LineNumber).Returns(100);

		var groupedPreviousDocument = new GroupedPreviousDocument(mergedPreviousDocumentProviderMock.Object, mergedPreviousDocument.Object, null, Factory);
		AssertEquals(100, groupedPreviousDocument.EntryLineNumber);
	}

	public void TestEntryLineCustomsStatusDescription()
	{
		var mergedPreviousDocument = new Mock<IMergedPreviousDocument>();

		var mergedPreviousDocumentProviderMock = new Mock<IMergedPreviousDocumentsProvider>();
		mergedPreviousDocumentProviderMock.Setup(m => m.NBStatus).Returns(EntryLineCustomsStatusList.Codes.Approved);

		var groupedPreviousDocument = new GroupedPreviousDocument(mergedPreviousDocumentProviderMock.Object, mergedPreviousDocument.Object, null, Factory);
		AssertEquals(EntryLineCustomsStatusList.Descriptions.Approved, groupedPreviousDocument.EntryLineCustomsStatusDescription);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var mergedPreviousDocumentProvider = new Mock<IMergedPreviousDocumentsProvider>();
		var mergedPreviousDocumentMock = new Mock<IMergedPreviousDocument>();
		mergedPreviousDocumentMock.Setup(m => m.Register).Returns("MRN");

		return new GroupedPreviousDocument(mergedPreviousDocumentProvider.Object, mergedPreviousDocumentMock.Object, null, Factory);
	}

	IMergedPreviousDocumentsProvider GetNewMergedPreviousDocumentsProvider() => new Mock<IMergedPreviousDocumentsProvider>().Object;
}
