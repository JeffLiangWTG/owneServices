using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.SeaCargo.GUI;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module.SeaCargo
{
	/// <summary>
	/// Module Controller for AuCustomsSeaCargo.
	/// </summary>
	public class CusSCADepotHouseController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public CusSCADepotHouseController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.CusSCADepotHouse; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusSCADepotHouse); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not implemented yet");
		}

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
