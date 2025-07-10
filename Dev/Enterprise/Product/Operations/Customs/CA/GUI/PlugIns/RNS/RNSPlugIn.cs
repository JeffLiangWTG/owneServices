using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Licensing;

namespace Enterprise.Customs.CA.GUI.PlugIns
{
	public class RNSPlugIn : CustomsPlugIn
	{
		public RNSPlugIn(IRNSPlugInSupport plugInSupport)
			: base(plugInSupport.Master)
		{
			Argument.NotNull(plugInSupport, "plugInSupport");
			this.plugInSupport = plugInSupport;
			rnsMessaging = new RNSMessagingBO(plugInSupport);
			plugInSupport.PlugInVisibilityDataChanged += ChangeTheVisibility;
			ChangeTheVisibility();
		}

		void ChangeTheVisibility(object sender, System.EventArgs e)
		{
			ChangeTheVisibility();
		}

		#region Overrides of ZPlugIn

		public override string Name
		{
			get { return "RNS"; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.ReleaseNotificationSystem; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				plugInSupport.PlugInVisibilityDataChanged -= ChangeTheVisibility;
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Overrides of CustomsPlugIn

		protected override Control GetNewUserControl()
		{
			return new RNSUserControl(rnsMessaging);
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			if (this.rnsMessaging.PlugInSupport.Master is GatePassShipment)
			{
				return null;
			}
			else
			{
				return new RNSMenu(rnsMessaging);
			}
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return plugInSupport.Master;
		}

		protected override sealed void ChangeTheVisibilityCore()
		{
			Enabled = CACustomsDataRegistry.Instance.RNSActive.Value && plugInSupport.PlugInVisible;
		}

		protected override string TextOverride
		{
			get { return "RNS/MF"; } //Consol's plugin provider takes Tab name from this property
		}

		#endregion

		protected readonly IRNSPlugInSupport plugInSupport;
		readonly RNSMessagingBO rnsMessaging;
	}
}
