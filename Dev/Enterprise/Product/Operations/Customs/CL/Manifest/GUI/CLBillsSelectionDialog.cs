using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CL.Manifest.GUI
{
	public partial class CLBillsSelectionDialog : ASYCUDA.GUI.AsycudaItemSelectionDialog
	{
		[Obsolete("This constructor is just for the designer")]
		public CLBillsSelectionDialog()
		{
			InitializeComponent();
		}

		public CLBillsSelectionDialog(CLMessageChooser messageChooser, string itemsType)
			: base(messageChooser, itemsType)
		{
			chooser = Argument.NotNull(messageChooser, nameof(messageChooser));

			InitializeComponent();
			ItemsGroupBox.Text = ResString.GetMultilingualString("F4C4BAEC-F472-4BF2-8149-5B7602EA5C0F", "Manifest - {0}", BusinessEntity.Header.AMA_JobReference);
			Name = FormattableString.Invariant($"MessageTo{messageChooser.ActionType}Dialog");
			Text = ResString.GetMultilingualString("CF86BF11-4132-4C74-892E-0DC4093E188F", "Message to {0}", messageChooser.ActionType);
			ItemsGrid.SetColumnCaption("Description", Text);
			ReasonTextBox.Visible = messageChooser.IsCancellation || messageChooser.IsAmendment;
			AmendReasonComboBox.Visible = messageChooser.IsAmendment;
			AmendTypeComboBox.Visible = messageChooser.IsAmendment;
			ReasonTextBox.CaptionResourceString = messageChooser.IsCancellation ?
				Res.GetData("F287E72C-666E-4AE3-8CA9-10F0D7EF7497", "Cancel Reason") : Res.GetData("C3A0F7D2-C930-4714-BFC6-5257ED9D72DB", "Observation");
		}
		readonly CLMessageChooser chooser;

		public new CLMessageChooser BusinessEntity => (CLMessageChooser)base.BusinessEntity;

		protected override bool IsValidToSend()
		{
			var result = base.IsValidToSend();
			if (result && (chooser.IsCancellation || chooser.IsAmendment))
			{
				chooser.Validation.ValidateAll();
				var messages = chooser.Notifications.GetMessageErrors().Select(c => c.Message).Distinct();

				if (messages.Any())
				{
					Globals.Message.Show(Res.GetString(
						"DE850302-1F07-46D6-85B0-BFE494C59DFE",
						"It is likely that your message(s) will be rejected by Customs, please check these below message errors and fix them before sending.{0}{1}",
						System.Environment.NewLine,
						string.Join(System.Environment.NewLine, messages)));

					result = false;
				}
			}
			return result;
		}
	}
}
