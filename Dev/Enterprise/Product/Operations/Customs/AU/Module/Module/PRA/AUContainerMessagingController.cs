using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.PRA.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.PRA.Module
{
	public class AUContainerMessagingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AUContainerMessagingController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("This function is not supported.");
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AUContainerMessaging; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CommonContainer); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return Factory.New<CommonContainer>();
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ForwardingContainerPRAMessagingAU; }
		}
		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ForwardingContainerPRAMessagingAU; }
		}
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ForwardingContainerPRAMessagingAU; }
		}
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ForwardingContainerPRAMessagingAU; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.AU.Module.Res.GetData("PlugInTabPage|AUContainerMessaging", "PRA Messaging", "The PRA Messaging tab."); } }

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new AUContainerMessagingPlugIn(businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
