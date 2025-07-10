using CargoWise.Windows.UI;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.GUI.PlugIn
{
	public partial class PreviousDocumentsUserControl : EU.GUI.PlugIn.PreviousDocumentsUserControl
	{
		public PreviousDocumentsUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var bindTo = PreviousDocumentsGrid.BindTo;
			var isVisibleForBinding = nameof(IsVisibleForBinding);

			PrevDocsReferenceTextBox.DataBindings.RemoveBinding(isVisibleForBinding);
			PrevDocsReferenceCodeFindBox.DataBindings.RemoveBinding(isVisibleForBinding);
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				PrevDocsReferenceTextBox.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, bindTo + "." + nameof(PreviousDocument.ShowTextBoxForReferenceNumber)));
				PrevDocsReferenceCodeFindBox.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, bindTo + "." + nameof(PreviousDocument.ShowCodeFindBoxForReferenceNumber)));
			}
		}
	}
}
