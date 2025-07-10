using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class MessageSendingForm<ActionParentType> : MessageSendingFormWithValidationDetails
		where ActionParentType : BaseMessageSendingObjectParent
	{
		[Obsolete("Do not call. Only for designer use.")]
		public MessageSendingForm()
		{
		}

		public MessageSendingForm(ActionParentType parent, string messageType) : base(parent)
		{
			this.messageType = messageType;
		}

		public MessageSendingForm(ActionParentType parent, string messageType, string groupBoxText) : base(parent)
		{
			this.messageType = messageType;
			messageSendingObjectsGroupBox.Text = Res.GetString("841A3B58-1810-4480-B7AC-535C61012A60", "{0}", groupBoxText);
		}

		readonly string messageType;

		public override string FormHeading => Res.GetString("C7D2A104-917A-4F6F-B00B-B894802BF986", "Send {0}", messageType);

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override bool SendWithAdditionalWarningCheckBoxVisible => false;
	}
}
