using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class CIQProductQuantificationsUserControl : ZUserControl
	{
		public CIQProductQuantificationsUserControl()
		{
			InitializeComponent();
		}

		public void ChangeControlsVisibility(bool isImport)
		{
			VINGroupBox.Visible = isImport;
			QuantificationGroupBox.Dock = isImport ? System.Windows.Forms.DockStyle.Top : System.Windows.Forms.DockStyle.Fill;
			// Below controls caption will be lost when QuantificationGroupBox Dock changed and will fail in Export BashForm UT with caption missinf error. Us if not defining Anchor.
			// Using UpdateCaption can fix it but there are some other controls having the smame issue like CIQIngredientTextBox etc. So comment it and fix together in seperate WI
			//CSI_CodeDropEdit.UpdateCaption();
			//CSI_ReferenceNumberTextBox.UpdateCaption();
			//CSI_LineNoCalcEdit.UpdateCaption();
			//CSI_QuantityCalcDropEdit.UpdateCaption();
		}
	}
}
