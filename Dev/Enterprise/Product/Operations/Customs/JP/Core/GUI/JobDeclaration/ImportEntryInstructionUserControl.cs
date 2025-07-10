using System;
using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;

namespace Enterprise.Customs.JP.GUI
{
	public partial class ImportEntryInstructionUserControl : BaseCustomsEntryUserControl
	{
		public ImportEntryInstructionUserControl()
		{
			InitializeComponent();
			NotesForCustomsTextBox.ReadOnlyChanged += (_, __) => NotesForCustomsOverrideCheckBox.Visible = NotesForCustomsTextBox.ReadOnly || NotesForCustomsOverrideCheckBox.Checked;
			ResetColumnsInEntryInstructionsGrid();
			GuaranteesGrid.MaximumRows = CusGuaranteeReferenceCollection.MaxRowCount;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			DetailsLayoutPanel.UpdateLayout(new ImportEntryInstructionLayouts());
			BindingSource.SetBindingMember(DetailsLayoutPanel, "CustomsEntryInstructions");

			CertificateLayoutPanel.UpdateLayout(new CertificateLayouts());
			BindingSource.SetBindingMember(CertificateLayoutPanel, "CustomsEntryInstructions");
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			MarksAndNumbersTextBox.Visible = !JobDeclaration.IsAir;
		}

		void ResetColumnsInEntryInstructionsGrid()
		{
			var ceiDescriptionColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ceiDescriptionColumnStyleInfo.ColumnName = "CEI_Description";
			ceiDescriptionColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			ceiDescriptionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			EntryInstructionsGrid.ColumnStyles.Insert(1, ceiDescriptionColumnStyleInfo);
		}
	}
}
