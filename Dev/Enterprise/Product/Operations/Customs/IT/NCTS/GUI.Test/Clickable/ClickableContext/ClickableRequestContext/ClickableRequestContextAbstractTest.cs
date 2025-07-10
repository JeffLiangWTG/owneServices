using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using GlbStaffWrapper = Enterprise.Customs.IT.Business.GlbStaffWrapper;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

abstract class ClickableRequestContextAbstractTest : ClickableContextAbstractTest
{
	protected void SetupCryptokiCertificate()
	{
		var currentStaff = GlbStaff.CurrentUser;
		var staffWrapper = GlbStaffWrapper.Get(currentStaff);
		var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
		cryptokiCertificate.GP_Name = ChipsetList.Codes.Bit4id;
		cryptokiCertificate.GP_CertificateSerialNumber = "0123456789";
	}

	protected void AssertRequestCreation(NctsHeader header, string requestType, string expectedMessage)
	{
		AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

		var messages = header?.MovementHeader?.Messages;
		AssertNotNull(messages);

		messages.Reload(reLoadExistingRows: false);
		var lastMessage = messages.GetLastMessageByType(requestType);
		AssertNotNull($"Message '{requestType}' should have been created", lastMessage);
	}

	protected IClickableItem GetClickableItem(IClickableContext clickableContext = null) => new ClickableMenuItemComponent(clickableContext ?? GetClickableContext());
}
