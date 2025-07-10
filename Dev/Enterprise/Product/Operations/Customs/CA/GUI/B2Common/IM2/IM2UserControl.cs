using CargoWise.Types;

namespace Enterprise.Customs.CA.GUI
{
	public partial class IM2UserControl : BaseB2UserControl
	{
		public IM2UserControl() : base()
		{
			InitializeComponent();
			SetControlsReadOnly();
		}

		void SetControlsReadOnly()
		{
			B2TypeDropEdit.ReadOnly = true;
			OriginalTransactionNumberCodeFindBox.ReadOnly = true;
			VersionNumberTextBox.ReadOnly = true;
			OriginalJobNumberTextBox.ReadOnly = true;
			SubmittedZDateEdit.ReadOnly = false;
		}

		protected override ZBool SupportTransactionNumberControl() => JobDeclaration?.IsIM2 ?? ZBool.False;
	}
}
