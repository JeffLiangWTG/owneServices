using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class ImportSupportingDocumentReferenceNumberUserControl : ZUserControl
	{
		public ImportSupportingDocumentReferenceNumberUserControl()
		{
			InitializeComponent();
			ReferenceNumberTextBox.AllowOverlap(ReferenceNumberCodeFindBox);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			AddVisibleBindings(dataMember);
		}
		void AddVisibleBindings(string dataMember)
		{
			var isVisibleForBinding = nameof(IsVisibleForBinding);
			ReferenceNumberCodeFindBox.DataBindings.RemoveBinding(isVisibleForBinding);
			ReferenceNumberTextBox.DataBindings.RemoveBinding(isVisibleForBinding);

			if (DataSource != null)
			{
				ReferenceNumberCodeFindBox.DataBindings.Add(new KBinding(nameof(IsVisibleForBinding), DataSource, dataMember + "." + nameof(SupportingDocument.ShowCodeFindBoxForReferenceNumber)));
				ReferenceNumberTextBox.DataBindings.Add(new KBinding(nameof(IsVisibleForBinding), DataSource, dataMember + "." + nameof(SupportingDocument.ShowTextBoxForReferenceNumber)));
			}
		}
	}
}
