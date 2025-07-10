using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class NBHeaderWrapperWithinOriginalDeclarationTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NBHeaderWrapperWithinOriginalDeclaration(null));
		AssertNoExceptionThrown(() => new NBHeaderWrapperWithinOriginalDeclaration(dynamicWrappableObjectIM.Object));
	}

	public void TestNBHeaderWrapper()
	{
		var wrapperIM = new NBHeaderWrapperWithinOriginalDeclaration(dynamicWrappableObjectIM.Object);

		CombineAssertions("NBHeaderWrapperWithinOriginalDeclaration", () =>
		 {
			 AssertEquals("MessageCodeEntry for IMP", "IM", wrapperIM.MessageCodeEntry);
			 AssertEquals("ReferenceNumber", "<<MSGNO PLACEHOLDER>>", wrapperIM.ReferenceNumber);
			 AssertEquals("DeclarationCIN is empty", "", wrapperIM.DeclarationCIN);
			 AssertEquals("DeclarationDate is empty", ZDate.Empty, wrapperIM.DeclarationDate);
			 AssertEquals("ItemNumber", 100, wrapperIM.ItemNumber);
		 });

		var wrapperET = new NBHeaderWrapperWithinOriginalDeclaration(dynamicWrappableObjectET.Object);
		AssertEquals("MessageCodeEntry for EXP", "ET", wrapperET.MessageCodeEntry);
	}

	protected override void SetUp()
	{
		base.SetUp();

		dynamicWrappableObjectIM = new Mock<INBWrappableBusinessObject>() { CallBase = true };
		dynamicWrappableObjectIM.Setup(m => m.IsExport).Returns(false);
		dynamicWrappableObjectIM.Setup(m => m.IsImport).Returns(true);
		dynamicWrappableObjectIM.Setup(m => m.LineNumber).Returns(100);
		dynamicWrappableObjectIM.Setup(m => m.NBGroupedPreviousDocuments).Returns(Array.Empty<GroupedPreviousDocument>());

		dynamicWrappableObjectET = new Mock<INBWrappableBusinessObject>() { CallBase = true };
		dynamicWrappableObjectET.Setup(m => m.IsExport).Returns(true);
		dynamicWrappableObjectET.Setup(m => m.IsImport).Returns(false);
		dynamicWrappableObjectET.Setup(m => m.LineNumber).Returns(100);
		dynamicWrappableObjectET.Setup(m => m.NBGroupedPreviousDocuments).Returns(Array.Empty<GroupedPreviousDocument>());
	}

	Mock<INBWrappableBusinessObject> dynamicWrappableObjectIM;
	Mock<INBWrappableBusinessObject> dynamicWrappableObjectET;
}
