using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class EntryInstructionDetailBasicUserControl : ZUserControl
	{
		public EntryInstructionDetailBasicUserControl()
		{
			InitializeComponent();
		}

		public void ChangeControlsVisibility()
		{
			var jobDeclaration = CurrentDataItem as JobDeclaration;
			if (jobDeclaration != null)
			{
				BondHolderRemoverPanel.Visible = jobDeclaration.IsImport;

				ResizeOtherPartiesDetailsPanel(jobDeclaration.IsImport);
				SetEntryInstructionDetailsLayout();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			SetEntryInstructionDetailsLayout();
		}

		void ResizeOtherPartiesDetailsPanel(bool isImport)
		{
			var bottomControl = isImport ? (Control)BondHolderRemoverPanel : FromWarehouseGroupBox;
			var height = bottomControl.Bottom - OtherPartiesGroupBox.Top;
			ControlDpiScalingHelper.SetHeight(ref OtherPartiesDetailsPanel, height + ControlDpiScalingHelper.OnePixel * 2, false);
			ControlDpiScalingHelper.SetHeight(ref OtherPartiesGroupBox, height, false);
		}

		public void SetEntryInstructionDetailsLayout()
		{
			var jobDeclaration = CurrentDataItem as JobDeclaration;

			if (jobDeclaration != null && jobDeclaration.IsExport)
			{
				DetailsLayoutControl.SetLayout(new ExportEntryInstructionDetailsLayout());
			}
			else
			{
				DetailsLayoutControl.SetLayout(new ImportEntryInstructionDetailsLayout());
			}
		}

		internal static ResourceStringData ExportStyleCaption => Res.GetData("5F9A714A-CEAE-4269-A03D-86C5D667609A", "Type (Procedure)");
		internal static ResourceStringData ExportSubStyleCaption => Res.GetData("E66DFB9F-F6FD-4217-BB8D-7A12DA4FCFB1", "Type (Time)");
		internal static ResourceStringData DefaultStyleCaption => Res.GetData("22B26F2A-9A35-445D-93E0-8F85C7DEE76F", "Declaration Type");
		internal static ResourceStringData DefaultSubStyleCaption => Res.GetData("C7AA04D9-05AE-4EE7-AE47-E35B165F9A94", "Sub Style");
	}
}
