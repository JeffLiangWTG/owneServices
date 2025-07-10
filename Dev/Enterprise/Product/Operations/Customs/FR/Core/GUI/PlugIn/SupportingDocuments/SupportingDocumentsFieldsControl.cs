using CargoWise.Windows.UI;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.FR.GUI.PlugIn
{
	public partial class SupportingDocumentsFieldsControl : BaseCustomsEntryUserControl
	{
		public SupportingDocumentsFieldsControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var isVisibleForBinding = nameof(IsVisibleForBinding);
			CSI_ReferenceNumberCodeFindBox.DataBindings.RemoveBinding(isVisibleForBinding);
			CSI_ReferenceNumberTextBox.DataBindings.RemoveBinding(isVisibleForBinding);
			CSI_UnitOfQuantityDropEdit.DataBindings.RemoveBinding(isVisibleForBinding);
			CSI_UnitOfQuantityTextBox.DataBindings.RemoveBinding(isVisibleForBinding);
			CSI_ItemNumberCalcEdit.DataBindings.RemoveBinding(isVisibleForBinding);
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				var bindTo = ((SupportingDocumentsUserControl)Parent.Parent).SupportingDocumentsGrid.BindTo;
				CSI_ReferenceNumberCodeFindBox.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, bindTo + "." + nameof(SupportingDocument.ShowCodeFindBoxForReferenceNumber)));
				CSI_ReferenceNumberTextBox.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, bindTo + "." + nameof(SupportingDocument.ShowTextBoxForReferenceNumber)));
				CSI_UnitOfQuantityDropEdit.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, bindTo + "." + nameof(SupportingDocument.ShowDropEditForUnitOfQuantity)));
				CSI_UnitOfQuantityTextBox.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, bindTo + "." + nameof(SupportingDocument.ShowTextBoxForUnitOfQuantiy)));
				CSI_ItemNumberCalcEdit.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, bindTo + "." + nameof(SupportingDocument.ShowCalcEditItemNumber)));
			}
		}
	}
}
