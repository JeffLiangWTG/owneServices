using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Forwarding.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.Forwarding.Module
{
	class EXRELController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Res.GetData("PlugInTabPage|EXREL", "Export Release"); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new EXRELPlugIn((ForwardingShipment)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ExportConsignmentReleaseAdvice; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Non persistent top level business object (EXREL)"); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not implemented yet");
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ExportConsignmentReleaseAdvice; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ExportConsignmentReleaseAdvice; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.MaintainShipmentNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.MaintainShipmentDelete; }
		}
	}
}
