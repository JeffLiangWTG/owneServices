using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	public partial class ExportSupplierHeaderSupportingDocumentsFieldsControl : BaseCustomsEntryUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public ExportSupplierHeaderSupportingDocumentsFieldsControl()
		{
			InitializeComponent();
			SetFieldsMaxLength();
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
				var bindTo = ((ExportSupplierHeaderSupportingDocumentsUserControl)Parent.Parent).SupportingDocumentsGrid.BindTo;
				CSI_ReferenceNumberCodeFindBox.DataBindings.Add(new KBinding(nameof(IsVisibleForBinding), DataSource, bindTo + "." + nameof(SupportingDocument.ShowCodeFindBoxForReferenceNumber)));
				CSI_ReferenceNumberTextBox.DataBindings.Add(new KBinding(nameof(IsVisibleForBinding), DataSource, bindTo + "." + nameof(SupportingDocument.ShowTextBoxForReferenceNumber)));
			}
		}

		void SetFieldsMaxLength()
		{
			CSI_ReferenceNumberTextBox.MaxLength = 35;
			CSI_ReferenceNumberCodeFindBox.CodeBox.MaxLength = 35;
			CSI_AdditionalDescriptionTextBox.MaxLength = 70;
			CSI_ItemNumberCalcEdit.MaxLength = 5;
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return (control is ZTextBox && previousControl is ZCodeFindBox);
		}
	}
}
