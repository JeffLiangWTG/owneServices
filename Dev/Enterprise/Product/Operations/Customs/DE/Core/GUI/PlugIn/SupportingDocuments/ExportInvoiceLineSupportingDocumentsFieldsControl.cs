using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	public partial class ExportInvoiceLineSupportingDocumentsFieldsControl : BaseCustomsEntryUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public ExportInvoiceLineSupportingDocumentsFieldsControl()
		{
			InitializeComponent();
			CSI_FullTypeCodeFindBox.CodeBox.CharacterCasing = CharacterCasing.Normal;
			CSI_ReferenceNumberCodeFindBox.AllowOverlap(CSI_ReferenceNumberTextBox);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			AddVisibleBindings();
		}

		void AddVisibleBindings()
		{
			if (DataSource != null)
			{
				var bindTo = ((ExportInvoiceLineSupportingDocumentsUserControl)Parent.Parent).SupportingDocumentsGrid.BindTo;
				CSI_ReferenceNumberCodeFindBox.DataBindings.Add(new KBinding(nameof(IsVisibleForBinding), DataSource, bindTo + "." + nameof(SupportingDocument.ShowCodeFindBoxForReferenceNumber)));
				CSI_ReferenceNumberTextBox.DataBindings.Add(new KBinding(nameof(IsVisibleForBinding), DataSource, bindTo + "." + nameof(SupportingDocument.ShowTextBoxForReferenceNumber)));
			}
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return (control is ZTextBox && previousControl is ZCodeFindBox);
		}
	}
}
