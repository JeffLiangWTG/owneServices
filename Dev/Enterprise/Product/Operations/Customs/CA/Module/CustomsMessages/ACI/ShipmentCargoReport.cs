using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class CAShipmentCargoReportController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("This controller does not have GUI.");
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.CAShipmentCargoReport; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusSCAHouse); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		#region Security
		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ACIReport; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ACIReportDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ACIReportEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ACIReportNew; }
		}
		#endregion

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.CA.Module.Res.GetData("PlugInTabPage|CAShipmentCargoReport", "ACI"); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new ShipmentCargoReportPlugIn((ForwardingShipment)businessEntity);
		}
	}
}
