using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(EntryAmendmentHandler))]
sealed class EntryAmendmentHandlerTest : NonPersistentBusinessObjectTestCase
{
	public void TestEntryHeader()
	{
		var handler = new EntryAmendmentHandler(Factory);
		AssertSame(Factory, handler.Factory);
	}

	public void TestMovementReferenceNumber_Caption()
	{
		var handler = (EntryAmendmentHandler)GetNewBusinessObject();
		AssertEquals("MRN Number to amend", DataBoundResourceStrings.GetDataForProperty(handler.MovementReferenceNumberInfo).Caption);
	}

	public void TestMovementReferenceNumber_MaxLength()
	{
		var handler = (EntryAmendmentHandler)GetNewBusinessObject();
		AssertEquals(18, handler.MovementReferenceNumberInfo.MaxLength);
	}

	public void TestTotalEntryLines()
	{
		var handler = (EntryAmendmentHandler)GetNewBusinessObject();
		AssertEquals("Total Entry Lines in Original Declaration", DataBoundResourceStrings.GetDataForProperty(handler.TotalEntryLinesInfo).Caption);
	}

	public void TestShowTotalEntryLines()
	{
		var handler = (EntryAmendmentHandler)GetNewBusinessObject();
		Assert("ShowTotalEntryLines should be true", handler.ShowTotalEntryLines);
	}

	public void TestHumanReadableName()
	{
		var handler = (EntryAmendmentHandler)GetNewBusinessObject();
		AssertEquals("Entry Amendment", handler.HumanReadableName);
	}

	protected override BusinessObject GetNewBusinessObject() => new EntryAmendmentHandler(Factory);
}
