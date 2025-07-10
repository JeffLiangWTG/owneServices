using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsAmendmentHandler))]
sealed class NctsAmendmentHandlerTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor_NctsHeader()
	{
		var handler = new NctsAmendmentHandler(Factory);
		AssertSame(Factory, handler.Factory);
	}

	public void TestMovementReferenceNumber_Caption()
	{
		var handler = (NctsAmendmentHandler)GetNewBusinessObject();
		AssertEquals("MRN Number to amend", DataBoundResourceStrings.GetDataForProperty(handler.MovementReferenceNumberInfo).Caption);
	}

	public void TestMovementReferenceNumber_MaxLength()
	{
		var handler = (NctsAmendmentHandler)GetNewBusinessObject();
		AssertEquals(18, handler.MovementReferenceNumberInfo.MaxLength);
	}

	public void TestHumanReadableName()
	{
		var handler = (NctsAmendmentHandler)GetNewBusinessObject();
		AssertEquals("NCTS Amendment", handler.HumanReadableName);
	}

	public void TestShowTotalEntryLines()
	{
		var handler = (NctsAmendmentHandler)GetNewBusinessObject();
		Assert("ShowTotalEntryLines should be false", !handler.ShowTotalEntryLines);
	}

	protected override BusinessObject GetNewBusinessObject() => new NctsAmendmentHandler(Factory);
}
