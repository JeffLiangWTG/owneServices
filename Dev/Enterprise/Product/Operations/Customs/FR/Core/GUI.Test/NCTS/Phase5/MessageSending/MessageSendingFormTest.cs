using System.Windows.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.NCTS.Messaging;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	sealed class MessageSendingFormTest : Customs.GUI.Testing.MessageSendingFormWithValidationDetailsAbstractTest
	{
		protected override Form GetFormToBashCore() => new MessageSendingForm(new TP5MessageSendingObjectParent(nctsHeader));

		public void TestTypeOfBottomSection()
		{
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			form = new MessageSendingFormForTest(messageSendingObjectParent);
			var bottomSection = form.GetBottomSectionUserControlExposed();
			AssertType<MessageSendingFormBottomSectionUserControl>(bottomSection);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		protected override void SetUp()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			messageSendingObjectParent = new TP5MessageSendingObjectParent(nctsHeader);
		}

		NctsHeader nctsHeader;
		TP5MessageSendingObjectParent messageSendingObjectParent;
		MessageSendingFormForTest form;

		sealed class MessageSendingFormForTest : MessageSendingForm
		{
			public MessageSendingFormForTest(TP5MessageSendingObjectParent sendingObjectParent) : base(sendingObjectParent)
			{
			}

			public ZUserControl GetBottomSectionUserControlExposed() => GetBottomSectionUserControl();
		}
	}
}
