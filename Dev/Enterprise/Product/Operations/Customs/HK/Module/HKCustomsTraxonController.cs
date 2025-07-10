using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.HK.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.HK.Module
{
	public class HKCustomsTraxonController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.HK.Traxon; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ForwardingConsol); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not implemented yet");
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.HK.Module.Res.GetData("PlugInTabPage|HKCustomsISAC", "ISAC"); } }

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new TraxonConsolPlugIn(businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CustomsDeclarationEnquiryDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CustomsDeclarationEnquiryEdit; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CustomsDeclarationEnquiry; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CustomsDeclarationEnquiryNew; }
		}
	}
}
