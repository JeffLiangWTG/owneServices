using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class OverrideUserControl : ZUserControl
	{
		public OverrideUserControl()
		{
			InitializeComponent();
		}

		public string BindToOverride
		{
			get => overrideCheckBox.BindTo;
			set => overrideCheckBox.BindTo = value;
		}

		public string BindToCode
		{
			get => codeDropEdit.BindTo;
			set => codeDropEdit.BindTo = value;
		}
	}
}
