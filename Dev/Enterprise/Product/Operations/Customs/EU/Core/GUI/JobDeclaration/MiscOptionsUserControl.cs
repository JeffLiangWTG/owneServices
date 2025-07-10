using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class MiscOptionsUserControl : BaseMiscOptionsUserControl
	{
		public MiscOptionsUserControl()
		{
			InitializeComponent();

			InitializeRelatedDeclarationUserControl();

			InitAdditionalInfosUserControl();
			AdditionalInfoTabPage.RunWhenBindingOrFirstShown((s, args) => InitAdditionalInfosUserControl());

			InitSupportingDocumentsUserControl();
			SupportingDocumentTabPage.RunWhenBindingOrFirstShown((s, args) => InitSupportingDocumentsUserControl());

			InitPreviousDocumentsUserControl();
			PreviousDocumentTabPage.RunWhenBindingOrFirstShown((s, args) => InitPreviousDocumentsUserControl());

			InitializeGuaranteesControls();
		}
		protected const string SupportingInfoColumnLayoutContext = "DEC";

		void InitializeRelatedDeclarationUserControl()
		{
			this.RelatedDeclarationsUserControl = GetRelatedDeclarationsUserControl();
			this.RelatedDeclarationsUserControl.SuspendLayout();
			//
			// RelatedDeclarationsUserControl
			//
			this.RelatedDeclarationsUserControl.AllowDrop = true;
			this.RelatedDeclarationsUserControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RelatedDeclarationsUserControl, ".");
			this.RelatedDeclarationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 260, true);
			this.RelatedDeclarationsUserControl.Name = "RelatedDeclarationsUserControl";
			this.RelatedDeclarationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 267, true);
			this.RelatedDeclarationsUserControl.TabIndex = 3;
			this.Controls.Add(this.RelatedDeclarationsUserControl);
			this.Controls.SetChildIndex(this.RelatedDeclarationsUserControl, 0);
			this.RelatedDeclarationsUserControl.ResumeLayout(true);
			this.RelatedDeclarationsUserControl.PerformLayout();
		}

		protected virtual RelatedDeclarationsUserControl GetRelatedDeclarationsUserControl()
		{
			return new RelatedDeclarationsUserControl();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			InitTabsVisibility();
		}

		void InitTabsVisibility()
		{
			var currentDataItem = CurrentDataItem;
			var configuration = currentDataItem?.Configuration;
			if (configuration != null)
			{
				SupportingDocumentTabPage.TabVisible = configuration.MiscSupportingDocumentsSupport(currentDataItem);
				AdditionalInfoTabPage.TabVisible = configuration.MiscAdditionalInfosSupport(currentDataItem);
				PreviousDocumentTabPage.TabVisible = configuration.MiscPreviousDocumentsSupport(currentDataItem);
				GuaranteesTabPage.TabVisible = configuration.MiscGuaranteesSupport(currentDataItem);

				SupportingInformationTabControl.Visible = SupportingInformationTabControl.TabPages.Cast<ZTabPage>().Any(x => x.TabVisible);
			}
		}

		public new JobDeclaration CurrentDataItem => (JobDeclaration)base.CurrentDataItem;

		void InitAdditionalInfosUserControl()
		{
			additionalInfosUserControl.UserControlType = GetAdditionalInfosUserControlType();
			additionalInfosUserControl.HostedControlCreated += (sender, args) =>
			{
				var hostedControl = additionalInfosUserControl.HostedControl;
				if (hostedControl is ISupportingInfoUserControls)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(hostedControl, "", SupportingInfoColumnLayoutContext, GetAdditionalInfosColumnWidths());
				}
			};
		}

		protected virtual ColumnWidth[] GetAdditionalInfosColumnWidths()
		{
			return new[]
			{
				new ColumnWidth(AdditionalInfo.Schema.CSI_Code, 47),
				new ColumnWidth(AdditionalInfo.Schema.CSI_Description, 77),
				new ColumnWidth(AdditionalInfo.Schema.CSI_RN_NKCountryCode, 142),
				new ColumnWidth(AdditionalInfo.Schema.CSI_NctsExportFromEC, 89)
			};
		}

		protected virtual Type GetAdditionalInfosUserControlType() => typeof(AdditionalInfosUserControl);

		protected void InitPreviousDocumentsUserControl()
		{
			PreviousDocumentsUserControl.UserControlType = GetPreviousDocumentsUserControlType();
			PreviousDocumentsUserControl.HostedControlCreated += (sender, args) =>
			{
				if (PreviousDocumentsUserControl.HostedControl is ISupportingInfoUserControls)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(PreviousDocumentsUserControl.HostedControl, "", SupportingInfoColumnLayoutContext, GetPreviousDocumentsColumnWidths());
				}
			};
		}

		protected virtual ColumnWidth[] GetPreviousDocumentsColumnWidths()
		{
			return new[]
			{
				new ColumnWidth(PreviousDocument.Schema.CSI_Code, 50),
				new ColumnWidth(PreviousDocument.Schema.CSI_SubType, 50),
				new ColumnWidth(PreviousDocument.Schema.CSI_ReferenceNumber, 120),
				new ColumnWidth(PreviousDocument.Schema.CSI_DateOfIssue, 120)
			};
		}

		protected virtual Type GetPreviousDocumentsUserControlType() => typeof(PreviousDocumentsUserControl);

		void InitSupportingDocumentsUserControl()
		{
			SupportingDocumentsUserControl.UserControlType = GetSupportingDocumentsUserControlType();
			SupportingDocumentsUserControl.HostedControlCreated += (sender, args) =>
			{
				if (SupportingDocumentsUserControl.HostedControl is ISupportingDocumentsUserControl supportingDocumentsUserControl && supportingDocumentsUserControl is Control control)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(control, "", SupportingInfoColumnLayoutContext, GetSupportingDocumentColumnWidths());
					InitSupportingDocumentsUserControlDetails(supportingDocumentsUserControl);
				}
			};
		}

		protected virtual void InitSupportingDocumentsUserControlDetails(ISupportingDocumentsUserControl control)
		{
			if (control.SupportingDocumentsFieldsControl is ISupportingDocumentsFieldsControl fieldsControl && fieldsControl.CSI_RX_NKCurrencyCodeFindBox is ZCodeFindBox csi_RX_NKCurrencyCodeFindBox)
			{
				csi_RX_NKCurrencyCodeFindBox.ShowDescriptionBox = false;
			}
		}
		protected virtual ColumnWidth[] GetSupportingDocumentColumnWidths()
		{
			return new[]
			{
				new ColumnWidth(SupportingDocument.Schema.CSI_ReferenceNumber, 67),
				new ColumnWidth(SupportingDocument.Schema.CSI_Code, 47),
				new ColumnWidth(SupportingDocument.Schema.CSI_Status, 47),
				new ColumnWidth(SupportingDocument.Schema.CSI_Quantity, 54)
			};
		}

		protected virtual Type GetSupportingDocumentsUserControlType() => typeof(SupportingDocumentsUserControl);

		void InitializeGuaranteesControls()
		{
			GuaranteesUserControl.UserControlType = GetGuaranteesUserControlType();
		}

		protected virtual Type GetGuaranteesUserControlType() => typeof(GuaranteesUserControl);
	}
}
