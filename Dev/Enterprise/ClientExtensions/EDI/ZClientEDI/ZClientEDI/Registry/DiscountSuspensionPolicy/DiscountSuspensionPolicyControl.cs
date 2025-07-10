using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class DiscountSuspensionPolicyControl : RegistryZUserControl
	{
		public DiscountSuspensionPolicyControl()
		{
			InitializeComponent();
		}

		public ZGrid Grid
		{
			get { return grid; }
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			Grid.ReadOnly = readOnly;
		}
	}
}
