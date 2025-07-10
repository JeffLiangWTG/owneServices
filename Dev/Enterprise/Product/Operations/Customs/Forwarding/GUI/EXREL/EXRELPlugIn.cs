using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.Forwarding.GUI
{
	public class EXRELPlugIn : ZPlugIn
	{
		public EXRELPlugIn(ForwardingShipment hostEntity)
			: base(hostEntity)
		{
			this.shipment = hostEntity;
			if (this.shipment != null)
			{
				Enabled = true;
			}
		}
		readonly ForwardingShipment shipment;

		#region IZPlugIn Members

		public override string Name
		{
			get { return (NoResString)"Export Release"; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Forwarder; }
		}

		public override bool CanDelete
		{
			get { return true; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			return new EXRELControl();
		}

		#endregion

	}
}
