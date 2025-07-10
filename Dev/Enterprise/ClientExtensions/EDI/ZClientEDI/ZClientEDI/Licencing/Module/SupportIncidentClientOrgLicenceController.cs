using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module
{
	public class SupportIncidentClientOrgLicenceController : ClientOrgController
	{
		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new ClientOrgLicencePlugIn((SupportIncident)businessEntity, true);
		}

		public override ResourceStringData PluginTabPageCaption { get { return ZClientEDI.Res.GetData("PlugInTabPage|SupportIncidentClientOrgLicence", "Licenses"); } }

		public override ControllerID ID
		{
			get { return ClientControllerRegistration.SupportIncidentClientOrgLicence; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(SupportIncident); }
		}
	}
}
