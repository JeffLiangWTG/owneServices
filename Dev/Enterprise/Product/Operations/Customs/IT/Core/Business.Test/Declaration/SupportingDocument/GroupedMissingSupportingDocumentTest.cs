using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class GroupedMissingSupportingDocumentTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Should be exception when param condition is null", () => new GroupedMissingSupportingDocument(null));
		AssertNoExceptionThrown("No exception expected", () => new GroupedMissingSupportingDocument(Factory.New<RefCusCondition>()));

		var conditionType = Factory.New<RefCusConditionType>();
		var condition = Factory.New<RefCusCondition>();
		condition.ZX1_ZX2_ConditionType = conditionType.PK;
		conditionType.ZX2_Description = "Test Description";
		condition.ZX1_Comment = "Test Comment";

		var groupedMissingSupportingDocument = new GroupedMissingSupportingDocument(condition);
		CombineAssertions("Test properties", () =>
		 {
			 AssertEquals("ConditionTypeDescription", "Test Description", groupedMissingSupportingDocument.ConditionTypeDescription);
			 AssertEquals("ConditionTypeComment", "Test Comment", groupedMissingSupportingDocument.ConditionTypeComment);
			 AssertNotNull("MissingSupportingDocuments", groupedMissingSupportingDocument.MissingSupportingDocuments);
		 });
	}

	public void TestMissingSupportingDocumentsBoolDescriptionPairList()
	{
		MissingSupportingDocumentParentTest.SetupRefCusConditionValueForMissingSupportingDocumentImportTest(Factory);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		invoiceLine.JI_Tariff = "800900";

		var missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine);
		var groupedMissingSupportingDocument = missingSupportingDocumentParent.MissingSupportingDocumentGrouped.SingleOrDefault(x => x.ConditionTypeComment == "Import control CITES");

		AssertEquals("MissingSupportingDocumentsBoolDescriptionPairList count", 5, groupedMissingSupportingDocument.MissingSupportingDocumentsBoolDescriptionPairList.Count);

		var expectedOrderedValue = new ZString[]
		{
			"XA - XA Description",
			"XB - XB Description",
			"XC - XC Description",
			"XD",
			"XE",
		};
		AssertArrayEqualsByElements("MissingSupportingDocumentsBoolDescriptionPairList should be ordered", expectedOrderedValue, groupedMissingSupportingDocument.MissingSupportingDocumentsBoolDescriptionPairList.Select(x => x.Description).ToArray());
	}
}
