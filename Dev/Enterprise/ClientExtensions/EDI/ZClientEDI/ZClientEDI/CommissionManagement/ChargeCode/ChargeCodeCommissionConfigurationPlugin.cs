using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.EDI.CommissionManagement.GUI
{
	public class ChargeCodeCommissionConfigurationPlugin : ZPlugIn
	{
		public ChargeCodeCommissionConfigurationPlugin(AccChargeCode hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		public override string Name
		{
			get { return "Commission Configuration"; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			return new ChargeCodeCommissionDetailsControl();
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return null; }
		}
	}
}
