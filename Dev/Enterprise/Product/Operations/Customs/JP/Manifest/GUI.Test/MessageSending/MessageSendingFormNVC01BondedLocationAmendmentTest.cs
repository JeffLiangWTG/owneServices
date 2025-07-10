using System.Windows.Forms;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing;

[TestedType(typeof(MessageSendingFormNVC01BondedLocationAmendment))]
sealed class MessageSendingFormNVC01BondedLocationAmendmentTest : MessageSendingFormTest
{
	public void TestControlVisible()
	{
		using (var form = GetFormToBash())
		{
			form.Show();
			var messageSendingObjectsGroupBox = form.FindSingle<ZGroupBox>("messageSendingObjectsGroupBox");
			Assert(!messageSendingObjectsGroupBox.Visible);
		}
	}

	public override void TestHDF01_RemovePreviousSortAndSortByBillStatus()
	{
		Assert("This test is not applicable for NVC01 Bonded Location Amendment.", true);
	}

	public override void TestSortByBillStatus_NV01()
	{
		Assert("This test is not applicable for NVC01 Bonded Location Amendment.", true);
	}

	protected override Form GetFormToBashCore()
	{
		var sendingObjectParent = new ManifestMessageSendingObjectParent(Header);
		return new MessageSendingFormNVC01BondedLocationAmendment(sendingObjectParent);
	}

	protected override MessageSendingForm GetMessageSendingForm(ManifestMessageSendingObjectParent sendingObjectParent)
	{
		return new MessageSendingFormNVC01BondedLocationAmendment(sendingObjectParent);
	}

	protected override MessageSendingForm GetMessageSendingFormForTest(ManifestMessageSendingObjectParent sendingObjectParent)
	{
		return new MessageSendingFormNVC01BondedLocationAmendmentForTest(sendingObjectParent);
	}

	class MessageSendingFormNVC01BondedLocationAmendmentForTest : MessageSendingFormNVC01BondedLocationAmendment
	{
		public MessageSendingFormNVC01BondedLocationAmendmentForTest(ManifestMessageSendingObjectParent sendingObjectParent)
			: base(sendingObjectParent)
		{
		}

		protected override bool CheckIsOKToSend() => true;
	}
}
