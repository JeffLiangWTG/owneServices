using System;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.GUI.Plugin;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class MiscOptionsUserControl : EU.GUI.MiscOptionsUserControl
	{
		public MiscOptionsUserControl()
		{
			InitializeComponent();
			NCHRouteLabel.AllowOutsideOfParent();
			GBRelatedDeclarationsGroupBox.AllowOverlap(SupportingInformationTabControl);
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			WireHandlersFromBiz((JobDeclaration)JobDeclaration);
		}

		protected override RelatedDeclarationsUserControl GetRelatedDeclarationsUserControl()
		{
			return new GBRelatedDeclarationsUserControl();
		}

		void Dec_OnAppCodeChanged(object sender, EventArgs e)
		{
			var jobDeclaration = sender as JobDeclaration;

			if (jobDeclaration != null)
			{
				SupportingDocumentTabPage.GetExtension<ILabelCaptionRenderer>().Caption = jobDeclaration.SupportingDocumentsCaption;
				PreviousDocumentTabPage.GetExtension<ILabelCaptionRenderer>().Caption = jobDeclaration.PreviousDocumentsCaption;
				AdditionalInfoTabPage.GetExtension<ILabelCaptionRenderer>().Caption = jobDeclaration.AdditionalInfoCaption;
				GuaranteesTabPage.GetExtension<ILabelCaptionRenderer>().Caption = jobDeclaration.GuaranteesCaption;

				var guaranteesTab = GuaranteesTabPage.Controls.Find("GuaranteesUserControl", true).FirstOrDefault() as ZDynamicControlCreationUserControl;
				if (guaranteesTab?.HostedControl is GBGuaranteesUserControl guaranteesControl)
				{
					guaranteesControl.SetCaptions();
				}

				valueBuildUpUserControl.Visible = false;
				var width = Width - MiscOptionsGroupBox.Width;
				GBRelatedDeclarationsGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(402, 8);
				GBRelatedDeclarationsGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(width - ControlDpiScalingHelper.ScaleToCurrentDpiX(15), valueBuildUpUserControl.Height, isInStandardDpi: false);
				GBRelatedDeclarationsGroupBox.Refresh();
			}
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

		protected override Type GetAdditionalInfosUserControlType()
		{
			return typeof(GBAdditionalInfosUserControl);
		}

		protected override Type GetPreviousDocumentsUserControlType()
		{
			return typeof(GBPreviousDocumentsUserControl);
		}

		protected override Type GetSupportingDocumentsUserControlType()
		{
			return typeof(GBSupportingDocumentsUserControl);
		}

		protected override Type GetGuaranteesUserControlType()
		{
			return typeof(GBGuaranteesUserControl);
		}

		protected override ColumnWidth[] GetSupportingDocumentColumnWidths()
		{
			return base.GetSupportingDocumentColumnWidths().Concat(new[] {
					new ColumnWidth(SupportingDocument.Schema.CSI_SubType, 43),
					new ColumnWidth(SupportingDocument.Schema.CSI_Description, 71),
					new ColumnWidth(SupportingDocument.Schema.CSI_Availability, 38),
					new ColumnWidth(SupportingDocument.Schema.CSI_Actions, 45)
				}).ToArray();
		}
	}
}
