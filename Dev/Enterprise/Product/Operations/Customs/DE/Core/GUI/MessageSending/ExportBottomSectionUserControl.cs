using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ExportBottomSectionUserControl : ZUserControl
	{
		public ExportBottomSectionUserControl()
		{
			InitializeComponent();
			AfterFirstBinding += ExportBottomSectionUserControl_AfterFirstBinding;
		}

		void ExportBottomSectionUserControl_AfterFirstBinding(object sender, System.EventArgs e)
		{
			dataSource = (ExportDeclarationMessageSendingActionParent)DataSource;
			foreach (ExportEntryMessageSendingAction action in dataSource.SendingObjectsCollection)
			{
				action.EntryTypeInfo.ValueChanged += EntryTypeInfo_ValueChanged;
			}
			SplitContainer.Panel2Collapsed = true;
		}

		ExportDeclarationMessageSendingActionParent dataSource;

		public void UpdateAnnotationGroupBoxCaption(ExportEntryMessageSendingAction action)
		{
			AnnotationGroupBox.CaptionResourceString = action.AnnotationCaption;
			CaptionRenderingSupport.UpdateCaption(AnnotationGroupBox);
			AnnotationTextBox.CaptionResourceString = action.AnnotationCaption;
		}

		public void SetControlsVisibility(ExportEntryMessageSendingAction action)
		{
			var isSupplementaryExportDeclaration = action.EntryType == ExportEntryTypeList.Codes.SupplementaryExportDeclaration;
			NotSubmitConsigneeCheckBox.Visible = isSupplementaryExportDeclaration;
			MovementReferenceNumberTextBox.Visible = isSupplementaryExportDeclaration;
			SplitContainer.Panel2Collapsed = !ExportEntryTypeList.IsExportAmendmentOrSupplementaryExportDeclaration(action.EntryType);
		}

		void EntryTypeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			var action = (ExportEntryMessageSendingAction)sender;
			EntryTypeChanged(action);
		}

		public void EntryTypeChanged(ExportEntryMessageSendingAction action)
		{
			UpdateAnnotationGroupBoxCaption(action);
			SetControlsVisibility(action);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();

				if (dataSource != null)
				{
					foreach (ExportEntryMessageSendingAction action in dataSource.SendingObjectsCollection)
					{
						action.EntryTypeInfo.ValueChanged -= EntryTypeInfo_ValueChanged;
					}
				}
			}
			base.Dispose(disposing);
		}
	}
}
