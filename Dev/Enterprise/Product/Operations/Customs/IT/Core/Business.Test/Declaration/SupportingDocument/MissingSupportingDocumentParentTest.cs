using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;
using CoreCountry = Enterprise.Core.Constants.CountryCodes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(MissingSupportingDocumentParent))]
public sealed class MissingSupportingDocumentParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestLoadNew()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when invoiceLine.Declaration is null", () => MissingSupportingDocumentParent.LoadNew(null));
		AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when invoiceLine.Declaration is null", () => MissingSupportingDocumentParent.LoadNew(Factory.New<JobComInvoiceLine>()));
		AssertNoExceptionThrown("No exception expected", () => MissingSupportingDocumentParent.LoadNew(Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew()));
	}

	public void TestLoadMissingSupportingDocumentsWhenRelatedInvoiceLineIsEmptyOrInvalid()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		invoiceLine.JI_Tariff = ZString.Empty;
		var missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine);

		AssertEquals("When invoiceLine.JI_Tariff is Empty, MissedSupportingDocumentGrouped count", 0, missingSupportingDocumentParent.MissingSupportingDocumentGrouped.Count());

		invoiceLine.JI_Tariff = "XXXX";
		missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine);

		AssertEquals("When invoiceLine.JI_Tariff is an invalid code, MissedSupportingDocumentGrouped count", 0, missingSupportingDocumentParent.MissingSupportingDocumentGrouped.Count());

		invoiceLine.JI_Tariff = "800900";
		missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine);

		AssertEquals("When declaration type is not set, MissedSupportingDocumentGrouped count", 0, missingSupportingDocumentParent.MissingSupportingDocumentGrouped.Count());
	}

	public void TestLoadMissingSupportingDocumentsWithEXPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		invoiceLine.JI_Tariff = "01012100";

		AssertLoadMissingSupportDocumentGrouped(invoiceLine);
	}

	void AssertLoadMissingSupportDocumentGrouped(JobComInvoiceLine invoiceLine)
	{
		var missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine);

		AssertEquals("MissedSupportingDocumentGrouped count", 2, missingSupportingDocumentParent.MissingSupportingDocumentGrouped.Count());

		var missingSupportingDocumentGrouped = missingSupportingDocumentParent.MissingSupportingDocumentGrouped;
		AssertCollectionContains("MissedSupportingDocumentGrouped", "Export control CITES", missingSupportingDocumentGrouped.Select(x => x.ConditionTypeComment).ToArray());
		AssertCollectionContains("MissedSupportingDocumentGrouped", "Control on cat and dog fur", missingSupportingDocumentGrouped.Select(x => x.ConditionTypeComment).ToArray());
	}

	public void TestLoadMissingSupportingDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		invoiceLine.JI_Tariff = "800900";
		var missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine);

		AssertEquals("MissedSupportingDocumentGrouped count", 2, missingSupportingDocumentParent.MissingSupportingDocumentGrouped.Count());

		var missingSupportingDocumentGrouped = missingSupportingDocumentParent.MissingSupportingDocumentGrouped;
		CombineAssertions("Check Missing Supporting Document Groups", () =>
		{
			AssertCollectionContains("MissedSupportingDocumentGrouped", "Import control CITES", missingSupportingDocumentGrouped.Select(x => x.ConditionTypeComment).ToArray());
			AssertCollectionContains("MissedSupportingDocumentGrouped", "Control on cat and dog fur", missingSupportingDocumentGrouped.Select(x => x.ConditionTypeComment).ToArray());
		});

		AssertArrayEqualsByElements("Check missing supporting document group order", new ZString[] { "Control on cat and dog fur", "Import control CITES" }, missingSupportingDocumentGrouped.Select(x => x.ConditionTypeComment).ToArray());

		var importControlCitiesGroupedMissingSupportingDocument = missingSupportingDocumentGrouped.SingleOrDefault(x => x.ConditionTypeComment == "Import control CITES");

		CombineAssertions("Assert Import Control Cities Grouped Missing Supporting Document properties", () =>
		{
			AssertEquals("Missing Supporting Documents count", 5, importControlCitiesGroupedMissingSupportingDocument.MissingSupportingDocuments.Count);
			AssertEquals("ConditionTypeDescription", "Test Ctrl Condition Type 2", importControlCitiesGroupedMissingSupportingDocument.ConditionTypeDescription);
			AssertEquals("ConditionTypeComment", "Import control CITES", importControlCitiesGroupedMissingSupportingDocument.ConditionTypeComment);
			AssertArrayEqualsByElements("Check Missing Supporting Documents Order", new ZString[] { "XA", "XB", "XC", "XD", "XE" }, importControlCitiesGroupedMissingSupportingDocument.MissingSupportingDocuments.Select(x => x.Code).ToArray());
		});

		AssertMissingSupportingDocumentProperties(importControlCitiesGroupedMissingSupportingDocument, "XA", "XA Description");
		AssertMissingSupportingDocumentProperties(importControlCitiesGroupedMissingSupportingDocument, "XB", "XB Description");
		AssertMissingSupportingDocumentProperties(importControlCitiesGroupedMissingSupportingDocument, "XD", "");

		var importControlOnCatAndDogFurGroupedMissingSupportingDocument = missingSupportingDocumentGrouped.SingleOrDefault(x => x.ConditionTypeComment == "Control on cat and dog fur");
		AssertEquals("Control on cat and dog fur Missing Supporting Documents count", 1, importControlOnCatAndDogFurGroupedMissingSupportingDocument.MissingSupportingDocuments.Count);

		CombineAssertions("Assert Control on cat and dog fur Grouped Missing Supporting Document properties", () =>
		{
			AssertEquals("Missing Supporting Documents count", 1, importControlOnCatAndDogFurGroupedMissingSupportingDocument.MissingSupportingDocuments.Count);
			AssertEquals("ConditionTypeDescription", "Test Ctrl Condition Type 1", importControlOnCatAndDogFurGroupedMissingSupportingDocument.ConditionTypeDescription);
			AssertEquals("ConditionTypeComment", "Control on cat and dog fur", importControlOnCatAndDogFurGroupedMissingSupportingDocument.ConditionTypeComment);
		});

		AssertMissingSupportingDocumentProperties(importControlOnCatAndDogFurGroupedMissingSupportingDocument, "YA", "YA Description");
	}

	public void TestLoadMissingSupportingDocumentsWhenInvoiceLineHasAlreadyDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		invoiceLine.JI_Tariff = "800900";
		var yaSupportingDocument = invoiceLine.SupportingDocuments.AddNew();
		yaSupportingDocument.CSI_Code = "XA";

		var missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine);

		AssertEquals("MissedSupportingDocumentGrouped count", 1, missingSupportingDocumentParent.MissingSupportingDocumentGrouped.Count());

		var missingSupportingDocumentGrouped = missingSupportingDocumentParent.MissingSupportingDocumentGrouped.Select(x => x.ConditionTypeComment).ToArray();
		CombineAssertions("Check Missing Supporting Document Groups", () =>
		{
			AssertCollectionNotContains("MissedSupportingDocumentGrouped", "Import control CITES", missingSupportingDocumentGrouped);
			AssertCollectionContains("MissedSupportingDocumentGrouped", "Control on cat and dog fur", missingSupportingDocumentGrouped);
		});
	}

	public void TestLoadMissingSupportingDocumentsWhenInvoiceLineInheritsSupportingDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		invoiceLine.JI_Tariff = "800900";
		var xaSupportingDocument = invoice.SupportingDocuments.AddNew();
		xaSupportingDocument.CSI_Code = "XA";

		var missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine);

		AssertEquals("MissedSupportingDocumentGrouped count", 1, missingSupportingDocumentParent.MissingSupportingDocumentGrouped.Count());

		var missingSupportingDocumentGrouped = missingSupportingDocumentParent.MissingSupportingDocumentGrouped.Select(x => x.ConditionTypeComment).ToArray();
		CombineAssertions("Check Missing Supporting Document Groups", () =>
		{
			AssertCollectionNotContains("MissedSupportingDocumentGrouped", "Import control CITES", missingSupportingDocumentGrouped);
			AssertCollectionContains("MissedSupportingDocumentGrouped", "Control on cat and dog fur", missingSupportingDocumentGrouped);
		});

		var yaSupportingDocument = declaration.SupportingDocuments.AddNew();
		yaSupportingDocument.CSI_Code = "YA";

		missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine);
		AssertEquals("MissedSupportingDocumentGrouped count", 0, missingSupportingDocumentParent.MissingSupportingDocumentGrouped.Count());
	}

	public void TestGetMissingSupportingDocumentsToImport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		invoiceLine.JI_Tariff = "800900";
		var missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine);

		var allMissingSupportingDocuments = missingSupportingDocumentParent.MissingSupportingDocumentGrouped.SelectMany(x => x.MissingSupportingDocuments);
		allMissingSupportingDocuments.SingleOrDefault(x => x.Code == "XA").ShouldImport = true;
		allMissingSupportingDocuments.SingleOrDefault(x => x.Code == "YA").ShouldImport = true;

		var missingSupportingDocumentsToImport = missingSupportingDocumentParent.GetMissingSupportingDocumentsToImport().Select(x => x.Code).ToArray();
		AssertEquals("Missing Supporting Documents To Import count", 2, missingSupportingDocumentsToImport.Length);

		CombineAssertions("Missing Supporting Documents To Import", () =>
		{
			AssertCollectionContains("MissedSupportingDocumentGrouped", "XA", missingSupportingDocumentsToImport);
			AssertCollectionContains("MissedSupportingDocumentGrouped", "YA", missingSupportingDocumentsToImport);
		});
	}

	public void TestTariffCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		invoiceLine.JI_Tariff = "800900";
		var missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine);

		AssertEquals("TariffCode", "800900", missingSupportingDocumentParent.TariffCode);

		invoiceLine.JI_Tariff = ZString.Empty;
		missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine);

		AssertEquals("TariffCode", "", missingSupportingDocumentParent.TariffCode);
	}

	protected override void SetUp()
	{
		base.SetUp();

		SetupRefCusConditionValueForMissingSupportingDocumentImportTest(Factory);
		SetupRefCusConditionValueForMissingSupportingDocumentExportTest(Factory);
		Factory.Save();
	}

	public static void SetupRefCusConditionValueForMissingSupportingDocumentImportTest(BusinessObjectFactory factory, bool insertDuplicateConditionValue = false)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Document Type (EU Box 44 Exports)");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "XC", "XC Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "XA", "XA Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "XB", "XB Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "YA", "YA Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		var tariffType = helper.CreateNewOrGetExistingTariffType(CoreCountry.Italy, "IMP");
		var certificateSupType = helper.CreateOrGetExistingRefCusConditionValueType(CoreCountry.Italy, "SUP");
		var certificateSnrType = helper.CreateOrGetExistingRefCusConditionValueType(CoreCountry.Italy, "SNR");
		var formulaType = helper.CreateOrGetExistingRefCusConditionValueType(CoreCountry.Italy, "FRM", afterCreate: x => x.ZX4_IsFormula = true);
		formulaType.ZX4_IsFormula = ZBool.True;
		var conditionType1 = helper.CreateOrGetExistingRefCusConditionType(CoreCountry.Italy, RefCusConditionTypes.ConditionClass.Control, "TSTC1", "Test Ctrl Condition Type 2");
		var conditionType2 = helper.CreateOrGetExistingRefCusConditionType(CoreCountry.Italy, RefCusConditionTypes.ConditionClass.Control, "TSTC2", "Test Ctrl Condition Type 1");
		factory.Save();

		var tariff = helper.CreateTariff(CoreCountry.Italy, tariffType.PK, "800900", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		var condition1 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.Italy, conditionType1.PK, tariff.PK, "Import control CITES", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_Source = "www.google.com");
		helper.CreateOrGetExistingRefCusConditionValue(certificateSupType.PK, condition1.PK, "XD", v => v.ZX3_LogicalORWithinGroup = 0);
		helper.CreateOrGetExistingRefCusConditionValue(certificateSupType.PK, condition1.PK, "XC", v => v.ZX3_LogicalORWithinGroup = 0);
		helper.CreateOrGetExistingRefCusConditionValue(certificateSupType.PK, condition1.PK, "XB", v => v.ZX3_LogicalORWithinGroup = 1);
		helper.CreateOrGetExistingRefCusConditionValue(certificateSnrType.PK, condition1.PK, "XA", v => v.ZX3_LogicalORWithinGroup = 2);
		helper.CreateOrGetExistingRefCusConditionValue(certificateSnrType.PK, condition1.PK, "XE", v => v.ZX3_LogicalORWithinGroup = 2);
		helper.CreateOrGetExistingRefCusConditionValue(formulaType.PK, condition1.PK, "VFD * 1000", v => v.ZX3_LogicalORWithinGroup = 3);

		var condition2 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.Italy, conditionType2.PK, tariff.PK, "Control on cat and dog fur", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateOrGetExistingRefCusConditionValue(certificateSupType.PK, condition2.PK, "YA");
		if (insertDuplicateConditionValue)
		{
			helper.CreateOrGetExistingRefCusConditionValue(certificateSupType.PK, condition2.PK, "YA");
			helper.CreateOrGetExistingRefCusConditionValue(certificateSupType.PK, condition2.PK, "XA");
		}

		factory.Save();
	}

	public static void SetupRefCusConditionValueForMissingSupportingDocumentExportTest(BusinessObjectFactory factory, bool insertDuplicateConditionValue = false)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Document Type (EU Box 44 Exports)");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "ZC", "ZC Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "ZA", "ZA Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "ZB", "ZB Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "WA", "WA Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		var tariffType = helper.CreateNewOrGetExistingTariffType(CoreCountry.Italy, "EXP");
		var certificateSupType = helper.CreateOrGetExistingRefCusConditionValueType(CoreCountry.Italy, "SUP");
		var certificateSnrType = helper.CreateOrGetExistingRefCusConditionValueType(CoreCountry.Italy, "SNR");
		var formulaType = helper.CreateOrGetExistingRefCusConditionValueType(CoreCountry.Italy, "FRM", afterCreate: x => x.ZX4_IsFormula = true);
		formulaType.ZX4_IsFormula = ZBool.True;
		var conditionType1 = helper.CreateOrGetExistingRefCusConditionType(CoreCountry.Italy, RefCusConditionTypes.ConditionClass.Control, "TSTC3", "Test Ctrl Condition Type 2");
		var conditionType2 = helper.CreateOrGetExistingRefCusConditionType(CoreCountry.Italy, RefCusConditionTypes.ConditionClass.Control, "TSTC4", "Test Ctrl Condition Type 1");
		factory.Save();

		var tariff = helper.CreateTariff(CoreCountry.Italy, tariffType.PK, "01012100", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		var condition1 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.Italy, conditionType1.PK, tariff.PK, "Export control CITES", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_Source = "www.google.com");
		helper.CreateOrGetExistingRefCusConditionValue(certificateSupType.PK, condition1.PK, "ZD", v => v.ZX3_LogicalORWithinGroup = 0);
		helper.CreateOrGetExistingRefCusConditionValue(certificateSupType.PK, condition1.PK, "ZC", v => v.ZX3_LogicalORWithinGroup = 0);
		helper.CreateOrGetExistingRefCusConditionValue(certificateSupType.PK, condition1.PK, "ZB", v => v.ZX3_LogicalORWithinGroup = 1);
		helper.CreateOrGetExistingRefCusConditionValue(certificateSnrType.PK, condition1.PK, "ZA", v => v.ZX3_LogicalORWithinGroup = 2);
		helper.CreateOrGetExistingRefCusConditionValue(certificateSnrType.PK, condition1.PK, "ZE", v => v.ZX3_LogicalORWithinGroup = 2);
		helper.CreateOrGetExistingRefCusConditionValue(formulaType.PK, condition1.PK, "VFD * 1000", v => v.ZX3_LogicalORWithinGroup = 3);

		var condition2 = helper.CreateOrGetExistingRefCusCondition(CoreCountry.Italy, conditionType2.PK, tariff.PK, "Control on cat and dog fur", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateOrGetExistingRefCusConditionValue(certificateSupType.PK, condition2.PK, "WA");
		if (insertDuplicateConditionValue)
		{
			helper.CreateOrGetExistingRefCusConditionValue(certificateSupType.PK, condition2.PK, "WA");
			helper.CreateOrGetExistingRefCusConditionValue(certificateSupType.PK, condition2.PK, "ZA");
		}

		factory.Save();
	}

	void AssertMissingSupportingDocumentProperties(GroupedMissingSupportingDocument groupedMissingSupportingDocument, ZString expectedCode, ZString expectedDescription)
	{
		var missingSupportingDocument = groupedMissingSupportingDocument.MissingSupportingDocuments.SingleOrDefault(x => x.Code == expectedCode);
		AssertNotNull($"{expectedCode} Missing Supporting Document", missingSupportingDocument);

		CombineAssertions("Assert Missing Supporting Document Properties", () =>
		{
			AssertEquals("Code", expectedCode, missingSupportingDocument.Code);
			AssertEquals("Description", expectedDescription, missingSupportingDocument.Description);

			var expectedCodeAndDescription = !expectedDescription.IsEmpty ? $" - {expectedDescription}" : "";
			AssertEquals("CodeAndDescription", $"{expectedCode}{expectedCodeAndDescription}", missingSupportingDocument.CodeAndDescription);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "800900";
		var missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine);
		return missingSupportingDocumentParent;
	}
}
