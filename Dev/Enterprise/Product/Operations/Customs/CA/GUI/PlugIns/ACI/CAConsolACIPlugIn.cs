using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.GUI
{
	public class CAConsolACIPlugIn : CAConsolPlugIn
	{
		public CAConsolACIPlugIn(ForwardingConsol consol)
			: base(consol)
		{
		}

		public CAConsolACIPlugIn(ForwardingConsol consol, bool isCalledFromShipmentPlugin)
			: base(consol, isCalledFromShipmentPlugin)
		{
		}

		CusSCAOceanBill OceanBill
		{
			get { return (CusSCAOceanBill)base.bizObj; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				shipmentCargoReportPlugInsCreated?.ForEach(x => x.Dispose());
			}
			base.Dispose(disposing);
			if (disposing)
			{
				OceanBill?.HouseBills?.ForEach(x => UnHookCusSCAHouseEvents(x));
			}
		}

		protected ConsolACIMessageManager Manager
		{
			get { return manager ?? (manager = new ConsolACIMessageManager(() => OceanBill)); }
		}
		ConsolACIMessageManager manager;

		protected override MenuItem GetNewTopLevelMenu()
		{
			MenuItem result = CAConsolACIMenu.New(consol, Manager);
			result.Enabled = IsFormEditable;
			return result;
		}

		protected override void OnIsFormEditableChanged()
		{
			if (TopLevelMenu != null)
			{
				TopLevelMenu.Enabled = IsFormEditable;
			}
		}

		protected override bool ShouldEnablePlugIn
		{
			get { return !consol.IsDeleted && (consol.IsAir || consol.IsSea) && base.ShouldEnablePlugIn; }
		}

		protected sealed override ZBool HasUserControl
		{
			get { return ZBool.True; }
		}

		protected override Control GetNewUserControl()
		{
			return new ConsolACIUserControl();
		}

		protected internal override void InitialiseBusinessObject()
		{
			base.InitialiseBusinessObject();
			if (Enabled && OceanBill != null)
			{
				OceanBill.EnableAndSynchronise();
				HookCusSCAHouseEvents(OceanBill);
			}
		}

		protected override BusinessObject FindBusinessObject()
		{
			return (CusSCAOceanBill)OceanBillLoader.LoadFromConsolAndApplicationCode(consol, CusSCAOceanBill.ApplicationCodes);
		}

		protected override BusinessObject CreateBusinessObject()
		{
			return CusSCAOceanBill.GetNewOceanBill(consol);
		}

		Customs.Business.BaseCusSCAOceanBill.Loader OceanBillLoader
		{
			get { return oceanBillLoader ?? (oceanBillLoader = new Customs.Business.BaseCusSCAOceanBill.Loader(Factory)); }
		}
		Customs.Business.BaseCusSCAOceanBill.Loader oceanBillLoader;

		#region CusSCAHouse Events

		static void HookCusSCAHouseEvents(CusSCAOceanBill oceanBill)
		{
			if (oceanBill != null)
			{
				foreach (var houseBill in oceanBill.HouseBills)
				{
					UnHookCusSCAHouseEvents(houseBill);
					HookCusSCAHouseEvents(houseBill);
				}
			}
		}

		static void HookCusSCAHouseEvents(CusSCAHouse houseBill)
		{
			houseBill.OnDelete += CusSCAHouse_OnDelete;
			houseBill.OnApplicationTypeChanged += CusSCAHouse_OnApplicationTypeChanged;
		}

		static void UnHookCusSCAHouseEvents(CusSCAHouse houseBill)
		{
			houseBill.OnDelete -= CusSCAHouse_OnDelete;
			houseBill.OnApplicationTypeChanged -= CusSCAHouse_OnApplicationTypeChanged;
		}

		static void CusSCAHouse_OnDelete(object sender, EventArgs e)
		{
			UnHookCusSCAHouseEvents((CusSCAHouse)sender);
		}

		static void CusSCAHouse_OnApplicationTypeChanged(object sender, ValueChangedEventArgs e)
		{
			var house = ((CusSCAHouse)sender);
			var oceanBill = house.OceanBill;
			if (oceanBill != null && oceanBill.HouseBills.Count > 1 && ShipmentCargoReportPlugIn.ShowChangeApplicationTypeConfirmation(e))
			{
				foreach (var houseBill in oceanBill.HouseBills.Where(h => h != house))
				{
					houseBill.OnApplicationTypeChanged -= CusSCAHouse_OnApplicationTypeChanged;
					houseBill.CA_FROBTransitImportCode = (ZString)e.NewValue;
					houseBill.OnApplicationTypeChanged += CusSCAHouse_OnApplicationTypeChanged;
				}
			}
		}

		#endregion

		protected sealed override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		public override string Name
		{
			get { return "ACI"; }
		}

		protected void CreateACIJobsIfRequired()
		{
			InitialiseBusinessObject();
			if (shipmentCargoReportPlugInsCreated == null)
			{
				shipmentCargoReportPlugInsCreated = new List<ShipmentCargoReportPlugIn>();
			}
			if (OceanBill != null && !OceanBill.IsDeleted)
			{
				var synchronizeASMwithLeadShipment = CACustomsDataRegistry.Instance.SynchronizeAssemblyMasterwithLeadShipment.Value;
				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					if (!shipment.IsCoLoadMaster && !shipment.IsBlindCoLoadMaster && (synchronizeASMwithLeadShipment ? !shipment.HasMaster(Core.Constants.ShipmentTypes.AssemblyMaster) : !shipment.IsAssemblyMaster))
					{
						var shipmentPlugin = new ShipmentCargoReportPlugIn(shipment, true);
						shipmentCargoReportPlugInsCreated.Add(shipmentPlugin);
						shipmentPlugin.InitialiseCusSCAHouse();
					}
				}
				OceanBill.RegisterChildEditableForConsolUse();
			}
			HookCusSCAHouseEvents(OceanBill);
		}
		List<ShipmentCargoReportPlugIn> shipmentCargoReportPlugInsCreated;

		public override void OnGUIShown()
		{
			CreateACIJobsIfRequired();
			base.OnGUIShown();
		}

		ZGlobalMutex mutex;
		public override ZGlobalMutex Mutex
		{
			get { return mutex ?? (mutex = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, consol.PK + ShipmentCargoReportPlugIn.ConsolMutexString)); }
		}
	}
}
