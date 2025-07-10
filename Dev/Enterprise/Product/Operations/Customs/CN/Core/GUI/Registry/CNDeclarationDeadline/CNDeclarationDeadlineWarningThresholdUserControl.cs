using Enterprise.Registry.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class CNDeclarationDeadlineWarningThresholdUserControl : RegistryZUserControl
	{
		public CNDeclarationDeadlineWarningThresholdUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			MainGrid.ReadOnly = readOnly;
		}
	}
}
