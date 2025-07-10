using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.IE.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IE.NCTS.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	sealed class MessageSendingFormTest : Customs.GUI.Testing.MessageSendingFormWithValidationDetailsAbstractTest
	{
		protected override Form GetFormToBashCore() => new MessageSendingForm(new NctsHeaderMessageSendingObjectParent(Header));

		public void TestAvailableColumns()
		{
			using (var form = new MessageSendingForm(new Business.NctsHeaderMessageSendingObjectParent(Header)))
			{
				var messageSendingObjectsGrid = form.FindSingle<ZGrid>();
				form.Show();
				AssertSequencesEqual("Columns", new[]
				{
					EU.NCTS.Business.NctsHeaderMessageSendingObject.Schema.ShouldSend,
					AutoNctsHeaderMessageSendingObject.Schema.LRN,
					AutoNctsHeaderMessageSendingObject.Schema.MRN,
					AutoNctsHeaderMessageSendingObject.Schema.MessageType,
					AutoNctsHeaderMessageSendingObject.Schema.ReleaseRequest,
					AutoNctsHeaderMessageSendingObject.Schema.Justification,
					AutoNctsHeaderMessageSendingObject.Schema.MessageStatus,
					AutoNctsHeaderMessageSendingObject.Schema.DestinationCustomsOfficeCode,
					AutoNctsHeaderMessageSendingObject.Schema.Consignee,
					AutoNctsHeaderMessageSendingObject.Schema.TC11DeliveryDate,
					AutoNctsHeaderMessageSendingObject.Schema.EnquiryText,
				},
				messageSendingObjectsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
			}
		}

		public void TestRunPreSendValidation_FinalState()
		{
			var finalStateMessage = "The entry has reached it’s final state, please do not submit any further message.";
			Header.SetMovementType(NctsMovementType.Codes.Departure);
			var messageSendingObjectParent = new Business.NctsHeaderMessageSendingObjectParent(Header);
			using (var form = new NCTSMessageSendingFormForTest(messageSendingObjectParent))
			{
				form.Show();
				form.SendButton.Enabled = true;
				form.SendButton.PerformClick();
				AssertNullOrEmpty("BM_CustomsStatus empty, no message should be pop up.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;
			using (var form = new NCTSMessageSendingFormForTest(messageSendingObjectParent))
			{
				form.Show();
				form.SendButton.Enabled = true;
				form.SendButton.PerformClick();
				AssertEquals("BM_CustomsStatus WRO, should pop up an error dialog.", finalStateMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConfirmationFormDisplay()
		{
			Header.MovementHeader.BM_MessageStatus = "SNT";
			var messageSendingObjectParent = new Business.NctsHeaderMessageSendingObjectParent(Header);
			using (var form = new NCTSMessageSendingFormForTest(messageSendingObjectParent))
			{
				form.Show();
				form.SendButton.Enabled = true;
				form.SendButton.PerformClick();

				AssertType<ConfirmSendForm>("Last dialog form type = ConfirmSendForm", ZFormModaliser.LastFormShownDialogForTest);

				var confirmSendForm = ZFormModaliser.LastFormShownDialogForTest as ConfirmSendForm;
			}
		}

		NctsHeader Header => header ?? (header = CreateHeader());
		NctsHeader header;

		NctsHeader CreateHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			return nctsHeader;
		}

		class NCTSMessageSendingFormForTest : MessageSendingForm
		{
			public NCTSMessageSendingFormForTest(Business.NctsHeaderMessageSendingObjectParent parent) : base(parent)
			{
			}

			public new ZButton SendButton => base.SendButton;
		}
	}
}
