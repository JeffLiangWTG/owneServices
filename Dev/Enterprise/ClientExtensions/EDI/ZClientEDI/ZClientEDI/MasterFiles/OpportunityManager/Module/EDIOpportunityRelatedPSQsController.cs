using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIOpportunityRelatedPSQsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("");
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new EDIOpportunityRelatedPSQsPlugIn((EDIOrgOpportunity)businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption { get { return ZClientEDI.Res.GetData("PlugInTabPage|EDIOpportunityRelatedPSQs", "Related PSQs", "Related Professional Services Quotes"); } }

		public override ControllerID ID
		{
			get { return ClientControllerRegistration.EDIOpportunityRelatedPSQs; }
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
