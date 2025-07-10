using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.SeaCargo.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module.SeaCargo
{
	/// <summary>
	/// Module Controller for AuCustomsSeaCargo.
	/// </summary>
	public class AUCustomsSeaCargoController : JobConsolController
	{
		public AUCustomsSeaCargoController()
		{
		}

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ControllerIDs.JobConsol.ToString();
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.SeaCargo; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ForwardingConsol); }
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			if (sourceEntity is CusSCAOceanBill oceanBill && oceanBill.CB_ParentTableCode == JobConsolSchema.Constants.Prefix)
			{
				return (ForwardingConsol)Factory.Load(typeof(ForwardingConsol), (sourceEntity as CusSCAOceanBill).CB_ParentId);
			}
			else if (sourceEntity is CusSCAHouse)
			{
				var oceanbill = (sourceEntity as CusSCAHouse).OceanBill;
				if (oceanbill != null && oceanbill.CB_ParentTableCode == JobConsolSchema.Constants.Prefix)
				{
					return (ForwardingConsol)Factory.Load(typeof(ForwardingConsol), oceanbill.CB_ParentId);
				}
			}

			return base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
		}

		protected override ConsolForm GetFormCore(ForwardingConsol businessEntity)
		{
			ConsolForm result = new ConsolForm(businessEntity);
			result.PlugInIDToSelectOnLoaded = ID;
			return result;
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.AU.Module.Res.GetData("PlugInTabPage|AUCustomsSeaCargo", "Sea Cargo", "The Sea Cargo tab."); } }

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			if (businessEntity is ForwardingShipment)
			{
				return new SeaCargoShipmentPlugIn((ForwardingShipment)businessEntity);
			}
			else if (businessEntity is ForwardingConsol)
			{
				return new SeaCargoConsolWithScanPlugin((ForwardingConsol)businessEntity);
			}
			else
			{
				ErrorReporter.ReportOnce("CuckooSqueaker", "Unsupported business entity type");
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
	}
}
