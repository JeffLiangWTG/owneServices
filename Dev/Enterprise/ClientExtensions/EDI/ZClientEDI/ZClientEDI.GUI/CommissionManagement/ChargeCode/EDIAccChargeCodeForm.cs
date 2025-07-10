using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.EDI.CommissionManagement.GUI
{
	public partial class EDIAccChargeCodeForm : AccChargeCodeForm
	{
		public EDIAccChargeCodeForm()
		{
		}

		public EDIAccChargeCodeForm(AccChargeCode chargeCode)
			: base(chargeCode)
		{
			PlugIns.Add(ClientControllerRegistration.ChargeCodeCommissionConfiguration);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
