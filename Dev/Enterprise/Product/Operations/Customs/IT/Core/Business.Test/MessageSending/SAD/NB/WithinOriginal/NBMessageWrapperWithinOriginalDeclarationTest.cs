using System;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class NBMessageWrapperWithinOriginalDeclarationTest : NBMessageWrapperBaseTest
{
	public override void TestAnnualProgressiveNumber()
	{
		AssertEquals("<<MSGNO PLACEHOLDER>>", wrapper.AnnualProgressiveNumber);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NBMessageWrapperWithinOriginalDeclaration(null));
		AssertNoExceptionThrown(() => new NBMessageWrapperWithinOriginalDeclaration(dynamicWrappableObject.Object));
	}

	public override void TestHeader()
	{
		var nbHeaderWrapper = wrapper.Header;
		AssertNotNull(nbHeaderWrapper);
		AssertType<NBHeaderWrapperWithinOriginalDeclaration>(nbHeaderWrapper);

		dynamicWrappableObject.Setup(m => m.LineNumber).Returns(100);
		CombineAssertions("NBHeaderWrapperWithinOriginalDeclaration", () =>
		{
			AssertEquals("MessageCodeEntry", "IM", nbHeaderWrapper.MessageCodeEntry);
			AssertEquals("ReferenceNumber", "<<MSGNO PLACEHOLDER>>", nbHeaderWrapper.ReferenceNumber);
			AssertEquals("DeclarationCIN", "", nbHeaderWrapper.DeclarationCIN);
			AssertEquals("DeclarationDate", ZDate.Empty, nbHeaderWrapper.DeclarationDate);
			AssertEquals("ItemNumber", 100, nbHeaderWrapper.ItemNumber);
		});
	}

	protected override NBMessageWrapperBase GetNBMessageSendingObject(INBWrappableBusinessObject nbObject) => new NBMessageWrapperWithinOriginalDeclaration(dynamicWrappableObject.Object);
}
