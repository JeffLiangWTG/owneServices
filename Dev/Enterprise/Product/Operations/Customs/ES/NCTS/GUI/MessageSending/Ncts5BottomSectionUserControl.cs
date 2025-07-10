using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class Ncts5BottomSectionUserControl : ZUserControl
	{
		public Ncts5BottomSectionUserControl()
		{
			InitializeComponent();
			AfterFirstBinding += ExportBottomSectionUserControl_AfterFirstBinding;
		}

		NctsHeaderMessageSendingObjectParent nctsHeaderMessageSendingObjectParent => DataSource as NctsHeaderMessageSendingObjectParent;

		void ExportBottomSectionUserControl_AfterFirstBinding(object sender, System.EventArgs e)
		{
			foreach (NctsHeaderMessageSendingObject action in nctsHeaderMessageSendingObjectParent.SendingObjectsCollection)
			{
				action.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
			}
		}

		void MessageTypeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			var action = (NctsHeaderMessageSendingObject)sender;
			MessageTypeChanged(action);
		}

		public void MessageTypeChanged(NctsHeaderMessageSendingObject action)
		{
			SetControlsVisibility(action);
		}

		public void SetControlsVisibility(NctsHeaderMessageSendingObject action)
		{
			CancellationGroupBox.Visible = ShouldCancellationGroupBoxBeVisible(action);
			RequestDispatchDropEdit.Visible = ShouldRequestDispatchDropEditBeVisible(action);
		}

		bool ShouldCancellationGroupBoxBeVisible(NctsHeaderMessageSendingObject action) => action.MessageType.Equals(DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation);

		bool ShouldRequestDispatchDropEditBeVisible(NctsHeaderMessageSendingObject action) => action.MessageType.Equals(DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes);

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();

				if (nctsHeaderMessageSendingObjectParent != null)
				{
					foreach (NctsHeaderMessageSendingObject action in nctsHeaderMessageSendingObjectParent.SendingObjectsCollection)
					{
						action.MessageTypeInfo.ValueChanged -= MessageTypeInfo_ValueChanged;
					}
				}

				base.Dispose(disposing);
			}
		}
	}
}
