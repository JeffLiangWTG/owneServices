using System.Collections.Generic;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class RelatedDocumentsUserControlWithGrid : AdditionalInfosUserControlWithGrid, IAdditionalTabPage
	{
		public RelatedDocumentsUserControlWithGrid(ResourceStringData resString, int tabPageSequence)
		{
			AdditionalTabPageCaption = resString;
			TabPageSequence = tabPageSequence;
			new ControlRebinder().Rebind(this, "FilteredInvoiceLines.AdditionalInfos",
				resString.Caption.Replace(" ", ""));
			if (Controls.Find("AdditionalInfosGroupBox", searchAllChildren: true)?[0] is ZGroupBox additionalInfosGroupBox)
			{
				additionalInfosGroupBox.CaptionResourceString = resString;
			}
		}

		protected override void AdditionalInfosGridColumnsVisible()
		{
			using (Grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				if (Grid.GetColumnStyle(DocumentDescription) == null)
				{
					Grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DocumentDescription, 300));
				}

				Grid.SetAvailability(available: false, [AutoCusSupportingInfo.Schema.CSI_ReferenceNumber2,
					AutoCusSupportingInfo.Schema.CSI_RX_NKCurrency, AutoCusSupportingInfo.Schema.CSI_Value,
					AutoCusSupportingInfo.Schema.CSI_SubType, AutoCusSupportingInfo.Schema.CSI_Description]);
				Grid.ReOrderColumns(OrderedColumns);
			}
		}

		protected override void SetAdditionalInfosLayout()
		{
			DetailsLayoutControl.SetLayout(new RelatedDocumentsDetailsLayout());
			if (Controls.Find("DescriptionTextBox", searchAllChildren: true)?[0] is ZTextBox foundDescriptionTextBox)
			{
				foundDescriptionTextBox.SetBindingMember(DocumentDescription);
			}
		}

		#region IAdditionalTabPage Implementation

		public ZUserControl AdditionalTabPageUserControl => this;

		public int TabPageSequence { get; }

		public ResourceStringData AdditionalTabPageCaption { get; }

		public AdditionalTabPageVisibility AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		#endregion

		protected virtual IReadOnlyList<string> OrderedColumns => new[]
		{
			AutoCusSupportingInfo.Schema.CSI_Code,
			DocumentDescription,
			AutoCusSupportingInfo.Schema.CSI_ReferenceNumber,
		};

		protected const string DocumentDescription = "DocumentDescription";

		public static ResourceStringData ResStringSupportingDocuments => Res.GetData("fb36643b-9c6f-410e-bfe4-f22096b5d544", "Supporting Documents");

		public static ResourceStringData ResStringPreviousDocuments => Res.GetData("e247e243-ca9b-4984-a502-6966d68948a9", "Previous Documents");

		public const int SupportingDocumentsTabSequence = 40;
		public const int PreviousDocumentsTabSequence = 50;
	}
}
