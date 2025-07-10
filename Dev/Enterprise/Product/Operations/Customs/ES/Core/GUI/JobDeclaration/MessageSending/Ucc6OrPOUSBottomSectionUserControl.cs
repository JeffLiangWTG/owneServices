using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class Ucc6OrPOUSBottomSectionUserControl : ZUserControl
	{
		public Ucc6OrPOUSBottomSectionUserControl()
		{
			InitializeComponent();
			AfterFirstBinding += BottomSectionUserControl_AfterFirstBinding;
		}

		JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent => DataSource as JobDeclarationMessageSendingObjectParent;

		void BottomSectionUserControl_AfterFirstBinding(object sender, System.EventArgs e)
		{
			foreach (JobDeclarationMessageSendingObject action in jobDeclarationMessageSendingObjectParent.SendingObjectsCollection)
			{
				action.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
			}
		}

		void MessageTypeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			var action = (JobDeclarationMessageSendingObject)sender;
			MessageTypeChanged(action);
		}

		public void MessageTypeChanged(JobDeclarationMessageSendingObject action)
		{
			SetControlsVisibility(action);
		}

		public void SetControlsVisibility(JobDeclarationMessageSendingObject action)
		{
			CancellationGroupBox.Visible = ShouldCancellationGroupBoxBeVisible(action);
			ActivateByOperatorDropEdit.Visible = ShouldActivateByOperatorDropEditBeVisible(action);
			SecurityDropEdit.Visible = ShouldSecurityDropEditBeVisible(action);
			RequestDispatchDropEdit.Visible = ShouldRequestDispatchDropEditBeVisible(action);
		}

		bool ShouldCancellationGroupBoxBeVisible(JobDeclarationMessageSendingObject action) => action.MessageType.Equals(DeclarationMessageTypeList.Codes.ExportCancellation);

		bool ShouldActivateByOperatorDropEditBeVisible(JobDeclarationMessageSendingObject action) => action.ActivateByOperatorFlagVisible;

		bool ShouldSecurityDropEditBeVisible(JobDeclarationMessageSendingObject action) => action.SecurityFlagVisible;

		bool ShouldRequestDispatchDropEditBeVisible(JobDeclarationMessageSendingObject action) => action.RequestDispatchVisible;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();

				if (jobDeclarationMessageSendingObjectParent != null)
				{
					foreach (JobDeclarationMessageSendingObject action in jobDeclarationMessageSendingObjectParent.SendingObjectsCollection)
					{
						action.MessageTypeInfo.ValueChanged -= MessageTypeInfo_ValueChanged;
					}
				}

				base.Dispose(disposing);
			}
		}
	}
}
