using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module
{
	public class OpportunityClientOrgLicenceController : ClientOrgController
	{
		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new ClientOrgLicencePlugIn((EDIOrgOpportunity)businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption { get { return ZClientEDI.Res.GetData("PlugInTabPage|OpportunityClientOrgLicence", "License"); } }

		public override ControllerID ID
		{
			get { return ClientControllerRegistration.OpportunityClientOrgLicence; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(EDIOrgOpportunity); }
		}
	}
}
