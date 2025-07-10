using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class CustomsAndExciseReportsForm : ZTemplateForm
	{
		public CustomsAndExciseReportsForm() : base()
		{
			if (!DesignMode)
			{
				throw new InvalidOperationException("This constructor is only for the designer. Please use the one that takes a business object.");
			}
		}

		public CustomsAndExciseReportsForm(CustomsAndExciseReportOutboundMessage reportMessage) : base(reportMessage)
		{
			InitializeComponent();
			InitializeMessageControls();
		}

		void InitializeMessageControls()
		{
			MessagesUserControl.UserControlType = GetMessagesTabUserControlType();
			BindingSource.SetBindingMember(MessagesUserControl, "Messages");
		}

		protected virtual Type GetMessagesTabUserControlType() => typeof(EU.GUI.MessagesTabUserControl);

		public CustomsAndExciseReportOutboundMessage ReportMessage => (CustomsAndExciseReportOutboundMessage)CurrentDataItem;

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				Globals.Message.ShowInformation(Res.GetString("A59CCE50-9628-43D6-865A-1F89BBB93E51", "Message queued for sending."));
			}
			return result;
		}
	}
}
