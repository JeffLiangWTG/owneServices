using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.GUI.PlugIns.RNS;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.CA.Module
{
	public class RNSMFLoadListPlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("This controller does not have GUI.");
		}

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.RNSMFLoadListPlugIn; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CFSLoadListConsol); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new RNSMFPlugIn(new RNSPlugInSupportLoadListWrapper((CFSLoadListConsol)businessEntity));
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
