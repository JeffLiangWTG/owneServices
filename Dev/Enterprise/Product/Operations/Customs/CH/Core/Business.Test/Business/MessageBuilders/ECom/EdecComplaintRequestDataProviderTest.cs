using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EdecComplaintRequestDataProvider))]
sealed class EdecComplaintRequestDataProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertNull("Null", EdecComplaintRequestDataProvider.New(null));
			AssertNotNull("Not Null", EdecComplaintRequestDataProvider.New(new EComplaintMessageSendingObject(entryHeader)));
		});
	}

	public void TestProvider()
	{
		var chUser = "CHUSER";
		var credential = CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();
		credential.Company.GC_CustomsRegistrationNo = "72";
		CHGlbStaffWrapper.Get(GlbStaff.CurrentUser).CHDPassword.GP_UserID = chUser;
		var messageSendingObject = new EComplaintMessageSendingObject(entryHeader);
		messageSendingObject.CorrectionReason = "1";

		CombineAssertions(() =>
		{
			var dataProvider = EdecComplaintRequestDataProvider.New(messageSendingObject);

			AssertEquals(nameof(dataProvider.RequestorTraderIdentificationNumber), "72", dataProvider.RequestorTraderIdentificationNumber);
			AssertEquals(nameof(dataProvider.RequestorCorrelationID), EDIMessage.MessageNumberPlaceHolder, dataProvider.RequestorCorrelationID);
			AssertEquals(nameof(dataProvider.Item), chUser, dataProvider.Item);
			AssertEquals(nameof(dataProvider.ItemElementName), "declarantNumber", dataProvider.ItemElementName);
			AssertEquals(nameof(dataProvider.CustomsDeclarationNumber), entryHeader.EntryNumber, dataProvider.CustomsDeclarationNumber);
			AssertEquals(nameof(dataProvider.CorrectionReason), messageSendingObject.CorrectionReason, dataProvider.CorrectionReason);
			AssertEquals(nameof(dataProvider.AttachedDeclaration), false, dataProvider.AttachedDeclaration);
			AssertNull(nameof(dataProvider.AppealText), dataProvider.AppealText);
			AssertNull(nameof(dataProvider.PaperCorrespondence), dataProvider.PaperCorrespondence);

			AssertEquals("no complaint lines", 0, dataProvider.Complaints.Count());

			messageSendingObject.SendingObjectLines.AddNew();
			messageSendingObject.SendingObjectLines.AddNew();
			dataProvider = EdecComplaintRequestDataProvider.New(messageSendingObject);

			AssertEquals("has complaint lines", 2, dataProvider.Complaints.Count());
		});
	}

	public void TestCustomsDeclarationNumber()
	{
		var messageSendingObject = new EComplaintMessageSendingObject(entryHeader);
		var dataProvider = EdecComplaintRequestDataProvider.New(messageSendingObject);
		CombineAssertions(() =>
		{
			entryHeader.EntryNumber = "22CHEI000043145845";
			AssertEquals($"Without version: {entryHeader.EntryNumber}", "22CHEI000043145845", dataProvider.CustomsDeclarationNumber);
			entryHeader.EntryNumber = "22CHEI000043145846.1";
			AssertEquals($"With version: {entryHeader.EntryNumber}", "22CHEI000043145846", dataProvider.CustomsDeclarationNumber);
		});
	}

	[TestDate(2022, 11, 1, 12, 00, 00)]
	public void TestRequestDateTime()
	{
		var dateNow = ZDateTime.Now.ToDateTime();
		var dataProvicer = EdecComplaintRequestDataProvider.New(new EComplaintMessageSendingObject(entryHeader));
		AssertEquals("now", dateNow, dataProvicer.RequestDateTime);
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
