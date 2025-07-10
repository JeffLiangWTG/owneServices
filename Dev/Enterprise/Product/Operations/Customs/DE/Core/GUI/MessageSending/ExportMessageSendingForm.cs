using System;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ExportMessageSendingForm : MessageSendingForm<ExportDeclarationMessageSendingActionParent>
	{
		public ExportMessageSendingForm(ExportDeclarationMessageSendingActionParent parent) : base(parent, "AES")
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			MessageSendingObjectsGrid.AfterBind += MessageSendingObjectsGrid_AfterBind;
		}
		void MessageSendingObjectsGrid_AfterBind(object sender, EventArgs e)
		{
			if (MessageSendingObjectsGrid.ListManager != null)
			{
				MessageSendingObjectsGrid.ListManager.CurrentChanged += ListManager_CurrentChanged;
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			bottomSectionUserControl.EntryTypeChanged((ExportEntryMessageSendingAction)MessageSendingObjectsGrid.ListManager.GetCurrent());
		}

		protected override ZUserControl GetBottomSectionUserControl()
		{
			bottomSectionUserControl = new ExportBottomSectionUserControl();
			return bottomSectionUserControl;
		}
		ExportBottomSectionUserControl bottomSectionUserControl;
	}
}
