using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class NBStandaloneMessageWrapperTest : NBMessageWrapperBaseTest
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NBStandaloneMessageWrapper(null, indexOfMessageInDeclaration: 0));
		AssertNoExceptionThrown(() => new NBStandaloneMessageWrapper(dynamicWrappableObject.Object, 0));
	}

	public override void TestAnnualProgressiveNumber()
	{
		var nbMessageWrapper = new NBStandaloneMessageWrapper(dynamicWrappableObject.Object, indexOfMessageInDeclaration: 0);
		AssertEquals("NB AnnualProgressiveNumber", "<<MSGNO PLACEHOLDER NB 0>>", nbMessageWrapper.AnnualProgressiveNumber);

		nbMessageWrapper = new NBStandaloneMessageWrapper(dynamicWrappableObject.Object, indexOfMessageInDeclaration: 1);
		AssertEquals("NB AnnualProgressiveNumber", "<<MSGNO PLACEHOLDER NB 1>>", nbMessageWrapper.AnnualProgressiveNumber);
	}

	public override void TestHeader()
	{
		var entryNumber = Factory.New<CusEntryNumber>();

		entryNumber.CE_EntryType = "REG";
		entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		entryNumber.CE_EntryNum = "4 T-61689G";
		entryNumber.CE_IssueDate = ZDateTime.Today;

		dynamicWrappableObject.Setup(m => m.EntryNumberWrapper).Returns(RegCusEntryNumberWrapper.Load(entryNumber));

		var nbHeaderWrapper = wrapper.Header;
		AssertNotNull(nbHeaderWrapper);
		AssertType<NBStandaloneHeaderWrapper>(nbHeaderWrapper);

		dynamicWrappableObject.Setup(m => m.LineNumber).Returns(1);
		CombineAssertions("Test NBStandaloneHeaderWrapper properties", () =>
		{
			AssertEquals("MessageCodeEntry", "4 T", nbHeaderWrapper.MessageCodeEntry);
			AssertEquals("ReferenceNumber", "61689", nbHeaderWrapper.ReferenceNumber);
			AssertEquals("DeclarationCIN", "G", nbHeaderWrapper.DeclarationCIN);
			AssertEquals("DeclarationDate", ZDate.Today, nbHeaderWrapper.DeclarationDate);
			AssertEquals("ItemNumber", 1, nbHeaderWrapper.ItemNumber);
		});
	}

	protected override NBMessageWrapperBase GetNBMessageSendingObject(INBWrappableBusinessObject nbObject) => new NBStandaloneMessageWrapper(dynamicWrappableObject.Object, indexOfMessageInDeclaration: 0);
}
