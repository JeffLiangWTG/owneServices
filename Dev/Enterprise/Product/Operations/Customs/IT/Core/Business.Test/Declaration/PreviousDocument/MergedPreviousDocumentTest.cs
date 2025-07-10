using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class MergedPreviousDocumentTest : TestCaseWithFactory
{
	public void TestFromPreviousDocumentButQuantities()
	{
		AssertExceptionThrown<ArgumentNullException>(() => { MergedPreviousDocument.FromPreviousDocumentButQuantities(null); });
		AssertNoExceptionThrown(() => { MergedPreviousDocument.FromPreviousDocumentButQuantities(Factory.New<PreviousDocument>()); });

		var previousDocument = Factory.New<PreviousDocument>();
		previousDocument.CSI_Code = "ZZZ";
		previousDocument.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
		previousDocument.CSI_ReferenceNumber = "1A";
		previousDocument.CSI_SubType = "Z";
		previousDocument.CSI_Status = "S";
		previousDocument.CSI_CustomsOffice = "IT137100";
		previousDocument.CSI_LineNo = 0;
		previousDocument.CSI_Procedure = "A3";
		previousDocument.CSI_ReferenceNumber2 = "A";
		previousDocument.CSI_Tariff = "870202";
		previousDocument.CSI_UnitOfQuantity2 = "KG";
		previousDocument.CSI_Quantity = 1m;
		previousDocument.CSI_Quantity2 = 2m;
		previousDocument.CSI_Quantity3 = 3m;
		previousDocument.CSI_PackQty = 4;

		var mergedDocument = MergedPreviousDocument.FromPreviousDocumentButQuantities(previousDocument);
		AssertEquals("CSI_Code should be", "ZZZ", mergedDocument.CSI_Code);
		AssertEquals("CSI_DateOfIssue should be", new ZDateTime(2020, 01, 01), mergedDocument.CSI_DateOfIssue);
		AssertEquals("CSI_ReferenceNumber should be", "1A", mergedDocument.CSI_ReferenceNumber);
		AssertEquals("ReferenceNumberWithoutCin should be", "1", mergedDocument.ReferenceNumberWithoutCin);
		AssertEquals("ReferenceNumberCin should be", "A", mergedDocument.ReferenceNumberCin);
		AssertEquals("CSI_SubType should be", "Z", mergedDocument.CSI_SubType);
		AssertEquals("CSI_Status should be", "S", mergedDocument.CSI_Status);
		AssertEquals("CSI_CustomsOffice should be", "IT137100", mergedDocument.CSI_CustomsOffice);
		AssertEquals("CSI_LineNo should be", (ZShort)0, mergedDocument.CSI_LineNo);
		AssertEquals("CSI_Procedure should be", "A3", mergedDocument.CSI_Procedure);
		AssertEquals("CSI_ReferenceNumber2 should be", "A", mergedDocument.CSI_ReferenceNumber2);
		AssertEquals("CSI_Tariff should be", "870202", mergedDocument.CSI_Tariff);
		AssertEquals("CSI_UnitOfQuantity2 should be", "KG", mergedDocument.CSI_UnitOfQuantity2);
		AssertEquals("NetMass should be", 0m, mergedDocument.NetMass);
		AssertEquals("SupplementaryQuantity should be", 0m, mergedDocument.SupplementaryQuantity);
		AssertEquals("GrossMass should be", 0m, mergedDocument.GrossMass);
		AssertEquals("PackageQuantity should be", 0, mergedDocument.PackageQuantity);
		AssertEquals("Key", "ZZZ202001011AZSIT1371000A3A870202KG", mergedDocument.Key);
	}

	public void TestFromPreviousDocument()
	{
		AssertExceptionThrown<ArgumentNullException>(() => { MergedPreviousDocument.FromPreviousDocument(null); });
		AssertNoExceptionThrown(() => { MergedPreviousDocument.FromPreviousDocument(Factory.New<PreviousDocument>()); });

		var previousDocument = Factory.New<PreviousDocument>();
		previousDocument.CSI_Code = "ZZZ";
		previousDocument.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
		previousDocument.CSI_ReferenceNumber = "1A";
		previousDocument.CSI_SubType = "Z";
		previousDocument.CSI_Status = "S";
		previousDocument.CSI_CustomsOffice = "IT137100";
		previousDocument.CSI_LineNo = 0;
		previousDocument.CSI_Procedure = "A3";
		previousDocument.CSI_ReferenceNumber2 = "A";
		previousDocument.CSI_Tariff = "870202";
		previousDocument.CSI_UnitOfQuantity2 = "KG";
		previousDocument.CSI_Quantity = 1m;
		previousDocument.CSI_UnitOfQuantity = "G";
		previousDocument.CSI_Quantity2 = 2m;
		previousDocument.CSI_Quantity3 = 3m;
		previousDocument.CSI_UnitOfQuantity3 = "T";
		previousDocument.CSI_PackQty = 4;

		var mergedDocument = MergedPreviousDocument.FromPreviousDocument(previousDocument);
		AssertEquals("CSI_Code should be", "ZZZ", mergedDocument.CSI_Code);
		AssertEquals("CSI_DateOfIssue should be", new ZDateTime(2020, 01, 01), mergedDocument.CSI_DateOfIssue);
		AssertEquals("CSI_ReferenceNumber should be", "1A", mergedDocument.CSI_ReferenceNumber);
		AssertEquals("ReferenceNumberWithoutCin should be", "1", mergedDocument.ReferenceNumberWithoutCin);
		AssertEquals("ReferenceNumberCin should be", "A", mergedDocument.ReferenceNumberCin);
		AssertEquals("CSI_SubType should be", "Z", mergedDocument.CSI_SubType);
		AssertEquals("CSI_Status should be", "S", mergedDocument.CSI_Status);
		AssertEquals("CSI_CustomsOffice should be", "IT137100", mergedDocument.CSI_CustomsOffice);
		AssertEquals("CSI_LineNo should be", (ZShort)0, mergedDocument.CSI_LineNo);
		AssertEquals("CSI_Procedure should be", "A3", mergedDocument.CSI_Procedure);
		AssertEquals("CSI_ReferenceNumber2 should be", "A", mergedDocument.CSI_ReferenceNumber2);
		AssertEquals("CSI_Tariff should be", "870202", mergedDocument.CSI_Tariff);
		AssertEquals("CSI_UnitOfQuantity2 should be", "KG", mergedDocument.CSI_UnitOfQuantity2);
		AssertEquals("NetMass should be", (ZDecimal)0.001m, mergedDocument.NetMass);
		AssertEquals("SupplementaryQuantity should be", (ZDecimal)2m, mergedDocument.SupplementaryQuantity);
		AssertEquals("GrossMass should be", (ZDecimal)3000m, mergedDocument.GrossMass);
		AssertEquals("PackageQuantity should be", (ZInt)4, mergedDocument.PackageQuantity);
		AssertEquals("Key", "ZZZ202001011AZSIT1371000A3A870202KG", mergedDocument.Key);
	}

	public void TestIsSummaryDeclarationDocument()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		var mergedPreviousDocument = MergedPreviousDocument.FromPreviousDocument(previousDocument);
		mergedPreviousDocument.CSI_Procedure = "";
		AssertEquals("Empty is not a summary declaration document", false, mergedPreviousDocument.IsSummaryDeclarationDocument);
		mergedPreviousDocument.CSI_Procedure = "2";
		AssertEquals("2 is not a summary declaration document", false, mergedPreviousDocument.IsSummaryDeclarationDocument);
		mergedPreviousDocument.CSI_Procedure = "A3";
		AssertEquals("A3 is a summary declaration document", true, mergedPreviousDocument.IsSummaryDeclarationDocument);
		mergedPreviousDocument.CSI_Procedure = "PF";
		AssertEquals("PF is a summary declaration document", true, mergedPreviousDocument.IsSummaryDeclarationDocument);
		mergedPreviousDocument.CSI_Procedure = "MRN";
		AssertEquals("MRN is a summary declaration document", true, mergedPreviousDocument.IsSummaryDeclarationDocument);
		mergedPreviousDocument.CSI_Procedure = "NN";
		AssertEquals("NN is a summary declaration document", true, mergedPreviousDocument.IsSummaryDeclarationDocument);
		mergedPreviousDocument.CSI_Procedure = "CIM";
		AssertEquals("CIM is a summary declaration document", true, mergedPreviousDocument.IsSummaryDeclarationDocument);
		mergedPreviousDocument.CSI_Procedure = "A44";
		AssertEquals("A44 is a summary declaration document", true, mergedPreviousDocument.IsSummaryDeclarationDocument);
	}

	public void TestIsPreviousProcedureDocument()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		var mergedPreviousDocument = MergedPreviousDocument.FromPreviousDocument(previousDocument);
		mergedPreviousDocument.CSI_Procedure = "";
		AssertEquals("Empty is not a previous procedure document", false, mergedPreviousDocument.IsPreviousProcedureDocument);
		mergedPreviousDocument.CSI_Procedure = "A3";
		AssertEquals("A3 is not a previous procedure document", false, mergedPreviousDocument.IsPreviousProcedureDocument);
		mergedPreviousDocument.CSI_Procedure = "2";
		AssertEquals("2 is a previous procedure document", true, mergedPreviousDocument.IsPreviousProcedureDocument);
		mergedPreviousDocument.CSI_Procedure = "2S";
		AssertEquals("2S is a previous procedure document", true, mergedPreviousDocument.IsPreviousProcedureDocument);
		mergedPreviousDocument.CSI_Procedure = "2T";
		AssertEquals("2T is a previous procedure document", true, mergedPreviousDocument.IsPreviousProcedureDocument);
		mergedPreviousDocument.CSI_Procedure = "5";
		AssertEquals("5 is a previous procedure document", true, mergedPreviousDocument.IsPreviousProcedureDocument);
		mergedPreviousDocument.CSI_Procedure = "5S";
		AssertEquals("5S is a previous procedure document", true, mergedPreviousDocument.IsPreviousProcedureDocument);
		mergedPreviousDocument.CSI_Procedure = "5T";
		AssertEquals("5T is a previous procedure document", true, mergedPreviousDocument.IsPreviousProcedureDocument);
		mergedPreviousDocument.CSI_Procedure = "7";
		AssertEquals("7 is a previous procedure document", true, mergedPreviousDocument.IsPreviousProcedureDocument);
		mergedPreviousDocument.CSI_Procedure = "7S";
		AssertEquals("7S is a previous procedure document", true, mergedPreviousDocument.IsPreviousProcedureDocument);
		mergedPreviousDocument.CSI_Procedure = "7T";
		AssertEquals("7T is a previous procedure document", true, mergedPreviousDocument.IsPreviousProcedureDocument);
	}

	public void TestIMergedPreviousDocumentMembers()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		previousDocument.CSI_Procedure = "A3";
		previousDocument.CSI_SubType = "Z";
		previousDocument.CSI_Code = "ZZZ";
		previousDocument.CSI_ReferenceNumber2 = "A";
		previousDocument.CSI_ReferenceNumber = "1A";
		previousDocument.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
		previousDocument.CSI_Status = "S";
		previousDocument.CSI_CustomsOffice = "IT137100";
		previousDocument.CSI_LineNo = 0;
		previousDocument.CSI_PackQty = 10;
		previousDocument.CSI_Quantity3 = 120.82m;
		previousDocument.CSI_UnitOfQuantity3 = "KG";
		previousDocument.CSI_Quantity = 100.20m;
		previousDocument.CSI_UnitOfQuantity = "KG";
		previousDocument.CSI_Quantity2 = 58.43m;
		previousDocument.CSI_Tariff = "800321";

		var mergedDocument = (IMergedPreviousDocument)MergedPreviousDocument.FromPreviousDocument(previousDocument);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(mergedDocument.Type), "Z", mergedDocument.Type);
			AssertEquals(nameof(mergedDocument.Category), "ZZZ", mergedDocument.Category);
			AssertEquals(nameof(mergedDocument.Mrn), "A", mergedDocument.Mrn);
			AssertEquals(nameof(mergedDocument.Register), "A3", mergedDocument.Register);
			AssertEquals(nameof(mergedDocument.ReferenceNumber), "1", mergedDocument.ReferenceNumber);
			AssertEquals(nameof(mergedDocument.ReferenceNumberCin), "A", mergedDocument.ReferenceNumberCin);
			AssertEquals(nameof(mergedDocument.Date), new ZDate(2020, 01, 01), mergedDocument.Date);
			AssertEquals(nameof(mergedDocument.Series), "S", mergedDocument.Series);
			AssertEquals(nameof(mergedDocument.CustomsOffice), "IT137100", mergedDocument.CustomsOffice);
			AssertEquals(nameof(mergedDocument.ItemNumber), (ZShort)0, mergedDocument.ItemNumber);
			AssertEquals(nameof(mergedDocument.IsSummaryDeclarationDocument), true, mergedDocument.IsSummaryDeclarationDocument);
			AssertEquals(nameof(mergedDocument.IsPreviousProcedureDocument), false, mergedDocument.IsPreviousProcedureDocument);
			AssertEquals(nameof(mergedDocument.PackageQuantity), 10, mergedDocument.PackageQuantity);
			AssertEquals(nameof(mergedDocument.GrossMass), 120.82m, mergedDocument.GrossMass);
			AssertEquals(nameof(mergedDocument.NetMass), 100.20m, mergedDocument.NetMass);
			AssertEquals(nameof(mergedDocument.SupplementaryQuantity), 58.43m, mergedDocument.SupplementaryQuantity);
			AssertEquals(nameof(mergedDocument.Tariff), "800321", mergedDocument.Tariff);
		});
	}
}
