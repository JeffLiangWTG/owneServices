using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.H7.Business.Testing;

sealed class MessageSenderTest : TestCaseWithFactory
{
	public void TestSendH7D_MessageCreated()
	{
		SetupTestData();

		var manifestHeader = Factory.New<AsycudaManifestHeader>();
		manifestHeader.AMA_GB = branch.PK;
		manifestHeader.AMA_OA_Declarant = declarant.PK;
		var bill = manifestHeader.Bills.AddNew();
		AssertEquals("Pre-condition : Bill doesn't has EDIMessage on it", 0, bill.Messages.Count);

		var messageSendingObject = new MessageSendingObject(bill);
		messageSendingObject.Action = ITH7MessageTypes.Codes.H7D;
		var sender = new MessageSender(messageSendingObject);
		var message = sender.Send();

		AssertCollectionContains("A new EDIMessage created for bill", message, bill.Messages);
		CombineAssertions($"message details of H7D", () =>
		{
			AssertEquals("EM_GB", branch.PK, message.EM_GB);
			AssertEquals("Application Code", "ITH", message.EM_ApplicationCode);
			AssertEquals("Receive Transmit", "TRX", message.EM_ReceiveTransmit);
			AssertEquals("Message Type", "H7D", message.EM_MessageType);
			AssertEquals("MessageSubType", "H7", message.EM_MessageSubType);
			AssertEquals("Application Reference", "IMP", message.EM_ApplicationReference);
			AssertEquals("Status", "QUE", message.EM_Status);
			AssertNotNullOrEmpty("Text", message.EM_MessageText);
		});
	}

	public void TestSendH7D_ExceptionWhenGeneratingXMLMessage_NoLRNShouldBeCreated()
	{
		SetupTestData();

		var manifestHeader = Factory.New<AsycudaManifestHeader>();
		manifestHeader.AMA_GB = branch.PK;
		var bill = manifestHeader.Bills.AddNew();

		var messageSendingObject = new MessageSendingObject(bill);
		messageSendingObject.Action = ITH7MessageTypes.Codes.H7D;
		var sender = new MessageSender(messageSendingObject);

		AssertExceptionThrown<InvalidOperationException>("Pre-condition: Exception should be thrown when AMA_OA_Declarant is missing", () => sender.Send());

		var messageInFactory = Factory.LoadTop1<EDIMessage>(new ZQuery());
		AssertNull("Message should be deleted on exception happens", messageInFactory);
		AssertEquals("LRN is not generated", string.Empty, bill.LocalReferenceNumber);
	}

	void SetupTestData()
	{
		company = Factory.New<GlbCompany>();
		company.GC_Code = "C";
		company.GC_Name = "TEST COMP";
		company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		company.GC_RN_NKCountryCode = CountryCodes.Italy;

		branch = company.Branches.AddNew();
		branch.GB_Code = "B";
		branch.GB_BranchName = "TEST BRANCH";
		branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
		branch.GB_RN_NKCountryCode = CountryCodes.Italy;

		var org = Factory.New<OrgHeader>();
		declarant = org.Addresses.AddNew();

		var contact = org.Contacts.AddNew();
		var contactAllocation = contact.Allocations.AddNew();
		contactAllocation.PC_Type = OrgConstants.ContactAllocationType.CUS;
	}

	GlbCompany company;
	GlbBranch branch;
	OrgAddress declarant;
}
