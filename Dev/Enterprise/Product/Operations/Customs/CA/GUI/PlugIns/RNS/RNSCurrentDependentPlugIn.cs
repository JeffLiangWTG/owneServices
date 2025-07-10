using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Licensing;

namespace Enterprise.Customs.CA.GUI.PlugIns
{
	public class RNSCurrentDependentPlugIn : CustomsPlugIn
	{
		public RNSCurrentDependentPlugIn(IRNSPlugInSupport plugInSupport)
			: base(plugInSupport.Master)
		{
			Argument.NotNull(plugInSupport, "plugInSupport");
			this.plugInSupport = plugInSupport;
			this.plugInSupport.PlugInVisibilityDataChanged += ChangeTheVisibility;
			ChangeTheVisibility();
		}

		void ChangeTheVisibility(object sender, System.EventArgs e)
		{
			ChangeTheVisibility();
		}

		#region Overrides of ZPlugIn

		public override string Name
		{
			get { return "RNS/MF"; }
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

				if (rnsUserControl != null)
				{
					rnsUserControl.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnCurrentChanged()
		{
			rnsMessagingBO = this.Current == null ? null : getRNSMessagingBO(this.Current);

			RNSUserControl.RNSMessaging = rnsMessagingBO;
			RNSUserControl.SetDataBinding(rnsMessagingBO, "");

			if (rnsMessagingBO != null)
			{
				rnsMessagingBO.PlugInSupport.PlugInVisibilityDataChanged += ChangeTheVisibility;
			}

			ChangeTheVisibility();
		}

		#endregion

		#region Overrides of CustomsPlugIn

		protected override Control GetNewUserControl()
		{
			return RNSUserControl;
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			return null;
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return plugInSupport.Master;
		}

		protected override sealed void ChangeTheVisibilityCore()
		{
			Enabled = plugInSupport.PlugInVisible;
		}

		protected override string TextOverride
		{
			get { return "RNS/MF"; } //Consol's plugin provider takes Tab name from this property
		}

		#endregion

		#region Properties

		readonly IRNSPlugInSupport plugInSupport;
		RNSMessagingBO rnsMessagingBO;
		RNSUserControl rnsUserControl;
		Dictionary<BusinessObject, RNSMessagingBO> rnsMessagingBODictionary;

		RNSUserControl RNSUserControl
		{
			get
			{
				return rnsUserControl ?? (rnsUserControl = new RNSUserControl());
			}
		}

		RNSMessagingBO getRNSMessagingBO(BusinessObject businessObject)
		{
			if (rnsMessagingBODictionary == null)
			{
				rnsMessagingBODictionary = new Dictionary<BusinessObject, RNSMessagingBO>();
			}

			RNSMessagingBO rnsMessagingBO = null;

			if (rnsMessagingBODictionary.ContainsKey(businessObject))
			{
				rnsMessagingBO = rnsMessagingBODictionary[businessObject];
			}
			else
			{
				if (businessObject is CFSShipment shipment)
				{
					rnsMessagingBO = new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(shipment));
				}

				if (rnsMessagingBO != null)
				{
					rnsMessagingBODictionary.Add(businessObject, rnsMessagingBO);
				}
			}

			return rnsMessagingBO;
		}

		#endregion
	}
}
