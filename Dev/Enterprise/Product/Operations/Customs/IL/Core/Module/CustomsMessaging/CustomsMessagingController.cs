using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IL.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.IL.Module
{
	public class CustomsMessagingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ResourceStringData PluginTabPageCaption { get { return Res.GetData("PlugInTabPage|CustomsMessages", "Customs Messages");  } }
		
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.IL.CustomsMessaging; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("No form"); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
			=> businessEntity switch
			{
				ForwardingShipment shipment => new ShipmentCustomsMessagingPlugIn(shipment),
				ForwardingConsol consol => new ConsolCustomsMessagingPlugIn(consol),
				_ => throw new ModuleGuiNotSupportedException("Not supported"),
			};

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("No form");
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}
	}
}
