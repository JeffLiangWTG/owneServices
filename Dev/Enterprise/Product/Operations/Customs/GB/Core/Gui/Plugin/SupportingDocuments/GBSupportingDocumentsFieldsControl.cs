using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Plugin
{
	public partial class GBSupportingDocumentsFieldsControl : BaseCustomsEntryUserControl
	{
		public GBSupportingDocumentsFieldsControl()
		{
			InitializeComponent();

			CSI_ReferenceNumberCodeFindBox.AllowOverlap(CSI_ReferenceNumberTextBox);
			CSI_UnitOfQuantity2TextBox.AllowOutsideOfParent();
			CSI_RX_NKCurrencyCodeFindBox.AllowOutsideOfParent();
			CSI_DateOfExpiryDateEdit.AllowOutsideOfParent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			AddVisibleBindings();
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			WireHandlersFromBiz((JobDeclaration)JobDeclaration);
		}

		void AddVisibleBindings()
		{
			if (DataSource != null)
			{
				var bindTo = ((GBSupportingDocumentsUserControl)Parent.Parent).SupportingDocumentsGrid.BindTo;
				CSI_ReferenceNumberCodeFindBox.DataBindings.Add(new KBinding(nameof(IsVisibleForBinding), DataSource, bindTo + "." + nameof(SupportingDocument.ShowCodeFindBoxForReferenceNumber)));
				CSI_ReferenceNumberTextBox.DataBindings.Add(new KBinding(nameof(IsVisibleForBinding), DataSource, bindTo + "." + nameof(SupportingDocument.ShowTextBoxForReferenceNumber)));
			}
		}

		void Dec_OnAppCodeChanged(object sender, EventArgs e)
		{
			SupportingDocumentsGroupBox.Text = ((JobDeclaration)sender)?.SupportingDocumentsCaption ?? string.Empty;
		}

		void WireHandlersFromBiz(JobDeclaration header)
		{
			if (header != null)
			{
				header.OnApplicationCodeChanged -= Dec_OnAppCodeChanged;
				header.OnApplicationCodeChanged += Dec_OnAppCodeChanged;
				Dec_OnAppCodeChanged(header, null);
			}
		}
	}
}
