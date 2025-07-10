using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class NBStandaloneHeaderWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NBStandaloneHeaderWrapper(null));
		AssertNoExceptionThrown(() => new NBStandaloneHeaderWrapper(dynamicWrappableObject1.Object));
	}

	public void TestNBHeaderWrapperForImport()
	{
		var wrapper = new NBStandaloneHeaderWrapper(dynamicWrappableObject1.Object);
		AssertRegistrationInfo("NBHeaderWrapperStandaloneDeclaration (IMP) for Empty Object", wrapper, ZString.Empty, ZString.Empty, ZString.Empty, ZDate.Empty, 2);

		var entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_EntryType = "REG";
		entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		entryNumber.CE_EntryNum = "4 T-61689G";
		entryNumber.CE_IssueDate = ZDateTime.Today;

		dynamicWrappableObject1.Setup(m => m.EntryNumberWrapper).Returns(RegCusEntryNumberWrapper.Load(entryNumber));

		wrapper = new NBStandaloneHeaderWrapper(dynamicWrappableObject1.Object);
		AssertRegistrationInfo("NBHeaderWrapperStandaloneDeclaration (IMP) for EntryNum 4 T-61689G", wrapper, "4 T", "61689", "G", ZDate.Today, 2);

		entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_EntryType = "REG";
		entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		entryNumber.CE_EntryNum = "4-61689G";
		entryNumber.CE_IssueDate = ZDateTime.Today;

		dynamicWrappableObject2.Setup(m => m.EntryNumberWrapper).Returns(RegCusEntryNumberWrapper.Load(entryNumber));

		wrapper = new NBStandaloneHeaderWrapper(dynamicWrappableObject2.Object);
		AssertRegistrationInfo("NBHeaderWrapperStandaloneDeclaration (EXP) for EntryNum 4-61689G", wrapper, "4", "61689", "G", ZDate.Today, 2);
	}

	public void TestNBHeaderWrapperForExport()
	{
		var wrapper = new NBStandaloneHeaderWrapper(dynamicWrappableObject1.Object);
		AssertRegistrationInfo("NBHeaderWrapperStandaloneDeclaration (EXP) for Empty Object", wrapper, ZString.Empty, ZString.Empty, ZString.Empty, ZDate.Empty, 2);

		var entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_EntryType = "REG";
		entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		entryNumber.CE_EntryNum = "4 T-61689G";
		entryNumber.CE_IssueDate = ZDateTime.Today;

		dynamicWrappableObject1.Setup(m => m.EntryNumberWrapper).Returns(RegCusEntryNumberWrapper.Load(entryNumber));

		wrapper = new NBStandaloneHeaderWrapper(dynamicWrappableObject1.Object);
		AssertRegistrationInfo("NBHeaderWrapperStandaloneDeclaration (EXP) for EntryNum 4 T-61689G", wrapper, "4 T", "61689", "G", ZDate.Today, 2);

		entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_EntryType = "REG";
		entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		entryNumber.CE_EntryNum = "4-61689G";
		entryNumber.CE_IssueDate = ZDateTime.Today;

		dynamicWrappableObject2.Setup(m => m.EntryNumberWrapper).Returns(RegCusEntryNumberWrapper.Load(entryNumber));

		wrapper = new NBStandaloneHeaderWrapper(dynamicWrappableObject2.Object);
		AssertRegistrationInfo("NBHeaderWrapperStandaloneDeclaration (EXP) for EntryNum 4-61689G", wrapper, "4", "61689", "G", ZDate.Today, 2);
	}

	protected override void SetUp()
	{
		base.SetUp();

		dynamicWrappableObject1 = new Mock<INBWrappableBusinessObject>();
		dynamicWrappableObject1.Setup(m => m.LineNumber).Returns(2);
		dynamicWrappableObject1.Setup(m => m.IsExport).Returns(false);
		dynamicWrappableObject1.Setup(m => m.IsImport).Returns(true);
		dynamicWrappableObject1.Setup(m => m.NBGroupedPreviousDocuments).Returns(Array.Empty<GroupedPreviousDocument>());

		dynamicWrappableObject2 = new Mock<INBWrappableBusinessObject>();
		dynamicWrappableObject2.Setup(m => m.LineNumber).Returns(2);
		dynamicWrappableObject2.Setup(m => m.IsExport).Returns(false);
		dynamicWrappableObject2.Setup(m => m.IsImport).Returns(true);
		dynamicWrappableObject2.Setup(m => m.NBGroupedPreviousDocuments).Returns(Array.Empty<GroupedPreviousDocument>());
	}

	Mock<INBWrappableBusinessObject> dynamicWrappableObject1, dynamicWrappableObject2;

	void AssertRegistrationInfo(ZString assertionMessage, NBStandaloneHeaderWrapper wrapper, ZString messageCodeEntry, ZString referenceNumber, ZString declarationCIN, ZDate declarationDate, ZInt itemNumber)
	{
		CombineAssertions(assertionMessage, () =>
		{
			AssertEquals("MessageCodeEntry", messageCodeEntry, wrapper.MessageCodeEntry);
			AssertEquals("ReferenceNumber", referenceNumber, wrapper.ReferenceNumber);
			AssertEquals("DeclarationCIN", declarationCIN, wrapper.DeclarationCIN);
			AssertEquals("DeclarationDate", declarationDate, wrapper.DeclarationDate);
			AssertEquals("ItemNumber", itemNumber, wrapper.ItemNumber);
		});
	}
}
