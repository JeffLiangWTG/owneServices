using System;
using System.Windows.Forms;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class ImportAdditionalInfosUserControlWithGrid : EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid
	{
		public ImportAdditionalInfosUserControlWithGrid()
		{
		}

		protected override void AdjustControlProperties()
		{
			base.AdjustControlProperties();

			if (Grid.ListManager is CurrencyManager listManager)
			{
				listManager.CurrentChanged -= ListManager_CurrentChanged;
				listManager.CurrentChanged += ListManager_CurrentChanged;
				ListManager_CurrentChanged(listManager, EventArgs.Empty);
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			if (Grid?.ListManager?.GetCurrent() is AdditionalInfo additionalInfo
				&& additionalInfo.Declaration is JobDeclaration declaration
				&& declaration.IsUCC5AndIsImport
				&& additionalInfo.CSI_SubType == EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation)
			{
				AdditionalInfosGroupBox.CaptionResourceString = Res.GetData("D9C04929-A9E9-4397-80E5-44366AD3F20D", "[2/2] Additional Information");
			}
			else
			{
				AdditionalInfosGroupBox.CaptionResourceString = Res.GetData("443CF40D-CF25-4C97-8BB4-312F6988F84E", "Additional Documents");
			}
			AdditionalInfosGroupBox.UpdateCaption();
		}

		protected override void AdditionalInfosGridColumnsVisible()
		{
			using (Grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				Grid.SetAvailability(false, [AdditionalInfo.Schema.CSI_ReferenceNumber2, AdditionalInfo.Schema.CSI_RX_NKCurrency, AdditionalInfo.Schema.CSI_Value]);
				Grid.ReOrderColumns(OrderedColumns);
				Grid.SetColumnMandatory(AdditionalInfo.Schema.CSI_Description, false);
			}
		}

		protected override IPanelLayoutProvider CreateNewInvoiceHeaderAdditionalInformationDetailsLayout()
		{
			return new ImportAdditionalInformationDetailsLayout();
		}

		protected override IPanelLayoutProvider CreateNewInvoiceLineAdditionalInformationDetailsLayout()
		{
			return new ImportAdditionalInformationDetailsLayout();
		}

		protected override IPanelLayoutProvider CreateNewExitSummaryAdditionalInformationDetailsLayout()
		{
			return new ImportAdditionalInformationDetailsLayout();
		}

		protected override IPanelLayoutProvider CreateNewClassPartPivotAdditionalInformationDetailsLayout()
		{
			return new ImportAdditionalInformationDetailsLayout();
		}

		string[] OrderedColumns => new[]
		{
			AdditionalInfo.Schema.CSI_SubType,
			AdditionalInfo.Schema.CSI_Code,
			AdditionalInfo.Schema.CSI_ReferenceNumber,
			AdditionalInfo.Schema.CSI_Description
		};
	}
}
