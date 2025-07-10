using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EComplaintMessageSendingObjectLineValidationTest : TestCaseWithFactory
{
	public void TestCheckLocation()
	{
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.LocationInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(sendingObject.LocationInfo, "X", EComplaintLocationList.Codes.Header);
		});
	}

	public void TestEntryLinePK()
	{
		var entryLine = entryHeader.AllEntryLines.AddNew();

		CombineAssertions(() =>
		{
			sendingObject.Location = EComplaintLocationList.Codes.Header;
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(sendingObject.EntryLinePKInfo);
			sendingObject.Location = EComplaintLocationList.Codes.Line;
			ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.EntryLinePKInfo);
			ValidationTestHelper.AssertErrorIfInvalidPK(sendingObject.EntryLinePKInfo, ZGuid.NewZGuid(), entryLine.PK);
		});
	}

	public void TestFieldName()
	{
		RefCusCodeTestHelper.CreateEComplaintFieldNames(Factory);

		CombineAssertions(() =>
		{
			sendingObject.Location = EComplaintLocationList.Codes.Header;
			ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.FieldNameInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(sendingObject.FieldNameInfo, RefCusCodeTestHelper.ValidEComplaintLineField, RefCusCodeTestHelper.ValidEComplaintHeaderField);
			sendingObject.Location = EComplaintLocationList.Codes.Line;
			ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.FieldNameInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(sendingObject.FieldNameInfo, RefCusCodeTestHelper.ValidEComplaintHeaderField, RefCusCodeTestHelper.ValidEComplaintLineField);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		var sendingObjectHeader = new EComplaintMessageSendingObjectLine(entryHeader);
		sendingObject = new EComplaintMessageSendingObjectLine(entryHeader);
	}
	CusEntryHeader entryHeader;
	EComplaintMessageSendingObjectLine sendingObject;
}
