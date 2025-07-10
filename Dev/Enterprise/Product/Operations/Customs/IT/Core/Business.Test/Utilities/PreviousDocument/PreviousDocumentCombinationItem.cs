using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class PreviousDocumentCombinationItemTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("It should thrown an exception if factory is null", () => new PreviousDocumentCombinationItem(null, "PROC", "CODE", "SUB", PreviousDocumentCombinationTemplate._1));
		AssertExceptionThrown<ArgumentNullException>("It should thrown an exception if procedure is null", () => new PreviousDocumentCombinationItem(Factory, null, "CODE", "SUB", PreviousDocumentCombinationTemplate._1));
		AssertExceptionThrown<ArgumentNullException>("It should thrown an exception if code is null", () => new PreviousDocumentCombinationItem(Factory, "PROC", null, "SUB", PreviousDocumentCombinationTemplate._1));
		AssertExceptionThrown<ArgumentNullException>("It should thrown an exception if sub type is null", () => new PreviousDocumentCombinationItem(Factory, "PROC", "CODE", null, PreviousDocumentCombinationTemplate._1));
		AssertNoExceptionThrown("Should not be exception", () => new PreviousDocumentCombinationItem(Factory, "PROC", "CODE", "SUB", PreviousDocumentCombinationTemplate._1));

		var previousDocumentCombinationItem = new PreviousDocumentCombinationItem(Factory, "LC", "720", "X", PreviousDocumentCombinationTemplate._1);

		CombineAssertions("Checking PreviousDocumentCombinationItem created with LC, 270, X", () =>
		{
			AssertEquals("Procedure", "LC", previousDocumentCombinationItem.Procedure);
			AssertNotNull("Code", previousDocumentCombinationItem.Code);
			AssertEquals("Code.Code", "720", previousDocumentCombinationItem.Code?.Code);
			AssertNotNull("SubType", previousDocumentCombinationItem.SubType);
			AssertEquals("SubType.Code", "X", previousDocumentCombinationItem.SubType?.Code);
		});

		var previousDocumentCombinationItemNotValid = new PreviousDocumentCombinationItem(Factory, "PROC", "CODE", "SUB", PreviousDocumentCombinationTemplate._1);
		CombineAssertions("Checking PreviousDocumentCombinationItem created with PROC, CODE, SUB", () =>
		{
			AssertEquals("Procedure", "PROC", previousDocumentCombinationItemNotValid.Procedure);
			AssertNull("Code", previousDocumentCombinationItemNotValid.Code);
			AssertNull("SubType", previousDocumentCombinationItemNotValid.SubType);
		});
	}
}
