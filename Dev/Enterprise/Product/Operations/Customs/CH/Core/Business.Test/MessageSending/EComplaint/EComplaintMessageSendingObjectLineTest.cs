using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EComplaintMessageSendingObjectLine))]
class EComplaintMessageSendingObjectLineTest : NonPersistentBusinessObjectTestCase
{
	public void TestGetNewValidation()
	{
		AssertType<EComplaintMessageSendingObjectLineValidation>(sendingObject.Validation);
	}

	public void TestGetNewLookup()
	{
		AssertType<EComplaintMessageSendingObjectLineLookups>(sendingObject.Lookups);
	}

	public void TestLocation()
	{
		AssertEquals("Caption", "Location", sendingObject.LocationInfo.Description);
	}

	public void TestFieldName()
	{
		AssertEquals("Caption", "Field Name", sendingObject.FieldNameInfo.Description);
	}

	public void TestEntryLinePK()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Entry Line #", sendingObject.EntryLinePKInfo.Description);

			AssertEquals("Location=empty", true, sendingObject.EntryLinePKInfo.ReadOnly);
			sendingObject.Location = EComplaintLocationList.Codes.Line;
			AssertEquals("Location=Line", false, sendingObject.EntryLinePKInfo.ReadOnly);
			sendingObject.EntryLinePK = ZGuid.NewZGuid();
			sendingObject.Location = EComplaintLocationList.Codes.Header;
			AssertEquals("Location=Header", true, sendingObject.EntryLinePKInfo.ReadOnly);
			AssertEquals("Clear EntryLinePK on set Location=Header", ZGuid.Empty, sendingObject.EntryLinePK);
		});
	}

	public void TestRemark()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Remark", sendingObject.RemarkInfo.Description);
			AssertEquals("MaxLength", 4000, sendingObject.RemarkInfo.MaxLength);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var sendingObject = new EComplaintMessageSendingObjectLine(entryHeader);
		return sendingObject;
	}

	protected override void SetUp()
	{
		base.SetUp();
		var entryHeader = Factory.New<CusEntryHeader>();
		sendingObject = new EComplaintMessageSendingObjectLine(entryHeader);
	}
	EComplaintMessageSendingObjectLine sendingObject;
}
