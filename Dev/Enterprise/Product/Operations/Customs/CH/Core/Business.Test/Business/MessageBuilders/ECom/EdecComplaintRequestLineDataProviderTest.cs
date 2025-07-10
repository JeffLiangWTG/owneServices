using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public class EdecComplaintRequestLineDataProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var messageSendingObject = new EComplaintMessageSendingObject(entryHeader);
		var complaintRequestLine = messageSendingObject.SendingObjectLines.AddNew();

		CombineAssertions(() =>
		{
			AssertNull("Null", EdecComplaintRequestLineDataProvider.New(null));
			AssertNotNull("Not Null", EdecComplaintRequestLineDataProvider.New(complaintRequestLine));
		});
	}

	public void TestNewCollection()
	{
		var messageSendingObject = new EComplaintMessageSendingObject(entryHeader);

		AssertEquals("empty collection", 0, EdecComplaintRequestLineDataProvider.NewCollection(messageSendingObject).Count());

		messageSendingObject.SendingObjectLines.AddNew();
		messageSendingObject.SendingObjectLines.AddNew();

		AssertEquals("collection with 2 lines", 2, EdecComplaintRequestLineDataProvider.NewCollection(messageSendingObject).Count());
	}

	public void TestProvider()
	{
		var entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.CL_LineNumber = 1;

		var messageSendingObject = new EComplaintMessageSendingObject(entryHeader);
		var complaintRequestLine = messageSendingObject.SendingObjectLines.AddNew();
		complaintRequestLine.Location = EComplaintLocationList.Codes.Line;
		complaintRequestLine.EntryLinePK = entryLine.PK;
		complaintRequestLine.FieldName = "FEE";
		complaintRequestLine.Remark = "Remark";

		var dataProvider = EdecComplaintRequestLineDataProvider.New(complaintRequestLine);

		CombineAssertions(() =>
		{
			AssertEquals(nameof(dataProvider.Location), "Position", dataProvider.Location);
			AssertEquals(nameof(dataProvider.TraderItemID), entryLine.CL_LineNumber.ToString(), dataProvider.TraderItemID);
			AssertEquals(nameof(dataProvider.ElementName), complaintRequestLine.FieldName, dataProvider.ElementName);
			AssertEquals(nameof(dataProvider.Remark), complaintRequestLine.Remark, dataProvider.Remark);
		});
	}

	public void TestTraderItemID()
	{
		var entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.CL_LineNumber = 1;

		var messageSendingObject = new EComplaintMessageSendingObject(entryHeader);
		var complaintRequestLine = messageSendingObject.SendingObjectLines.AddNew();
		complaintRequestLine.Location = EComplaintLocationList.Codes.Header;
		complaintRequestLine.FieldName = "REASON";
		complaintRequestLine.Remark = "Remark";

		var dataProvider = EdecComplaintRequestLineDataProvider.New(complaintRequestLine);

		AssertNull("no trader item id", dataProvider.TraderItemID);

		complaintRequestLine.Location = EComplaintLocationList.Codes.Header;
		complaintRequestLine.EntryLinePK = entryLine.PK;
		complaintRequestLine.FieldName = "REASON";
		complaintRequestLine.Remark = "Remark";

		AssertNull("no trader item id", dataProvider.TraderItemID);

		complaintRequestLine.Location = EComplaintLocationList.Codes.Line;
		complaintRequestLine.EntryLinePK = entryLine.PK;
		complaintRequestLine.FieldName = "FEE";
		complaintRequestLine.Remark = "Remark";

		AssertEquals(nameof(dataProvider.TraderItemID), entryLine.CL_LineNumber.ToString(), dataProvider.TraderItemID);
	}

	protected override void SetUp()
	{
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Description = "DESCRIPTION";
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.EntryNumber = "NO1";
	}
	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	CusEntryInstruction entryInstruction;
}
