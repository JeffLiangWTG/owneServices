using Enterprise.Registry.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class FamilyRelationUserControl : RegistryZUserControl
	{
		public FamilyRelationUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			FamilyRelationGrid.ReadOnly = readOnly;
		}
	}
}
