using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.SeaCargo.GUI;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module.SeaCargo
{
	/// <summary>
	/// Module Controller for AuCustomsSeaCargo.
	/// </summary>
	public class AUCustomsSCDController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AUCustomsSCDController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.SeaCargoDepot; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Not implemented yet"); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not implemented yet");
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.AU.Module.Res.GetData("PlugInTabPage|AUCustomsSeaCargoDepot", "Sea Cargo"); } }

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			if (businessEntity is CFSLoadListConsol)
			{
				return new SCDLoadListPlugIn((CFSLoadListConsol)businessEntity);
			}
			else if (businessEntity is GatePassShipment)
			{
				return new SCDGatePassPlugIn((GatePassShipment)businessEntity);
			}
			else if (businessEntity is TallyContainer)
			{
				return new SCDTallyPlugIn((TallyContainer)businessEntity);
			}
			else if (businessEntity is CFSContainer)
			{
				return new SCDContainerPlugIn((CFSContainer)businessEntity);
			}
			else if (businessEntity is CFSShipment)
			{
				return new SCDShipmentPlugIn((CFSShipment)businessEntity);
			}
			else if (businessEntity is ForwardingShipment)
			{
				return new SCDForwardingShipmentPlugIn(businessEntity);
			}
			else if (businessEntity is ForwardingConsol)
			{
				return new SCDForwardingConsolPlugIn(businessEntity);
			}
			return null;
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AUCustomsSCA; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AUCustomsSCAImportModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AUCustomsSCAImportModify; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AUCustomsSCAImportModify; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
