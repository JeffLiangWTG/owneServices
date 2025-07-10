
using System;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Messaging
{
	[SuppressBindingMemberBashingTest]
	public partial class EHubMessageDecodeForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public EHubMessageDecodeForm()
		{
			InitializeComponent();
		}

		public EHubMessageDecodeForm(EDIMessage message)
			: base(message)
		{
			InitializeComponent();
			encodedMessageBody = message.EM_MessageText;
			customerServiceNumberLabel = "Message number: ";
			customerServiceNumber = message.EM_MessageNum;
		}

		public EHubMessageDecodeForm(EDIInterchange interchange)
			: base(interchange)
		{
			InitializeComponent();
			encodedMessageBody = interchange.EI_BodyText;
			customerServiceNumberLabel = "Interchange number: ";
			customerServiceNumber = interchange.EI_InterchangeNum;
		}

		readonly ZString encodedMessageBody;
		readonly ZString customerServiceNumberLabel;
		readonly ZString customerServiceNumber;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			OutputTextBox.Text = EHubMessageDecoder.UnpackMessage(encodedMessageBody);
			CustomerServiceNumberLabel.Text = customerServiceNumberLabel;
			CustomerServiceNumberTextBox.Text = customerServiceNumber;
			OriginalContentsTextBox.Text = encodedMessageBody;
		}

		#region Event Handlers

		void CopyButton_Click(object sender, EventArgs e)
		{
			SafeClipboard.SetText(OutputTextBox.Text);
		}

		#endregion

	}
}
