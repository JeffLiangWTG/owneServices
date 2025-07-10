using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.SDF
{
	/// <summary>
	/// Summary description for DocumentSDFControl.
	/// </summary>
	public partial class DocumentSDFControl : ZUserControl
	{
		public DocumentSDFControl()
		{
			InitializeComponent();

			CommonFieldsCheckBox.AllowOutsideOfParent();
		}

		public void SetHinLabelVisibility(bool shouldBeReadOnly) => HintLabel.Visible = shouldBeReadOnly;
	}
}
