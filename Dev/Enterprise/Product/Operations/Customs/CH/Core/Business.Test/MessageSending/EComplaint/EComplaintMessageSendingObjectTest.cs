using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EComplaintMessageSendingObject))]
class EComplaintMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestCanSend_DeclarantConfigured()
	{
		const string messageText = "Declarant Number is not configured for the current user. Please contact your system administrator.";

		var staffWrapper = CHGlbStaffWrapper.Get(GlbStaff.CurrentUser);

		CombineAssertions(() =>
		{
			staffWrapper.CHDPassword.GP_UserID = ZString.Empty;
			AssertEquals("Declarant number not configured", messageText, SendingObject.CanSendMessage());

			staffWrapper.CHDPassword.GP_UserID = "123456";
			AssertEquals("Declarant number configured", ZString.Empty, SendingObject.CanSendMessage());
		});
	}

	public void TestCorrectionReason()
	{
		AssertEquals("Caption", "Correction Reason", SendingObject.CorrectionReasonInfo.Description);
	}

	public void TestIsAttachedDeclaration()
	{
		AssertEquals("Caption", "Attached Declaration", SendingObject.IsAttachedDeclarationInfo.Description);
	}

	public void TestHumanReadableName()
	{
		AssertEquals("eCom Message Sending", SendingObject.HumanReadableName);
	}

	protected override BusinessObject GetNewBusinessObject() => new EComplaintMessageSendingObject(EntryHeader);

	JobDeclaration Declaration => declaration ?? (declaration = Factory.NewWithValidTestData<JobDeclaration>());
	JobDeclaration declaration;

	CusEntryHeader EntryHeader => entryHeader ?? (entryHeader = Declaration.CustomsEntryHeaders.AddNew());
	CusEntryHeader entryHeader;

	EComplaintMessageSendingObject SendingObject => sendingObject ?? (sendingObject = new EComplaintMessageSendingObject(EntryHeader));
	EComplaintMessageSendingObject sendingObject;
}
