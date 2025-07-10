using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public sealed partial class OrganisationalUnitPickerForm : ZChildForm
	{
		public OrganisationalUnitPickerForm()
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb => string.Empty;
	}
}
