using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI
{
	public class CusCAeMHConsolPlugIn : CAConsolPlugIn
	{
		public CusCAeMHConsolPlugIn(ForwardingConsol consol)
			: base(consol)
		{
		}

		internal CusCAeMHMaster MasterBill
		{
			get { return (CusCAeMHMaster)base.bizObj; }
		}

		ACIHouseBillMultiMessageManager Manager
		{
			get { return fManager ?? (fManager = new ACIHouseBillMultiMessageManager(() => MasterBill)); }
		}
		ACIHouseBillMultiMessageManager fManager;

		protected override MenuItem GetNewTopLevelMenu()
		{
			var result = new CusCAeMHMessageMenu(Manager);
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

		protected internal override void InitialiseBusinessObject()
		{
			base.InitialiseBusinessObject();
			if (MasterBill != null)
			{
				MasterBill.EnableAndSynchronise();
			}
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			return new CusCAeMHMasterUserControl();
		}

		protected override BusinessObject FindBusinessObject()
		{
			var query = new ZQuery(CusCAeMHMasterSchema.BP_ParentID, consol.PK);
			query.FetchOnlyFromLocalCache = !consol.IsInDatabase;
			query.ReLoadExistingRows = true;
			return Factory.LoadTop1<CusCAeMHMaster>(query);
		}

		protected override BusinessObject CreateBusinessObject()
		{
			var result = Factory.New<CusCAeMHMaster>();
			result.BP_ParentID = consol.PK;
			result.BP_ParentTableCode = consol.TablePrefix;
			return result;
		}

		public override string Name
		{
			get { return "eManifest Fwdr"; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		ZGlobalMutex mutex;
		public override ZGlobalMutex Mutex
		{
			get { return mutex ?? (mutex = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, consol.PK + ConsolMutexString)); }
		}

		const string ConsolMutexString = "_Only1eManifestPerCAConsol";

		public override void OnGUIShown()
		{
			CreateHouseBillsIfRequired();
			base.OnGUIShown();
		}

		void CreateHouseBillsIfRequired()
		{
			InitialiseBusinessObject();
			if (MasterBill != null && !MasterBill.IsDeleted)
			{
				var synchronizeASMwithLeadShipment = CACustomsDataRegistry.Instance.SynchronizeAssemblyMasterwithLeadShipment.Value;
				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					if ((!shipment.IsBlindCoLoadMaster || CACustomsDataRegistry.Instance.SynchronizeBlindColoadMasterWithConsol.Value) && (synchronizeASMwithLeadShipment ? !shipment.HasMaster(Core.Constants.ShipmentTypes.AssemblyMaster) : !shipment.IsAssemblyMaster))
					{
						if (shipment.IsHighVolumeLowValue)
						{
							HouseBillFromHVLVShipmentPopulator.Populate(shipment);
						}
						else
						{
							var query = new ZQuery(CusCAeMHHouseSchema.BW_ParentID, shipment.PK);
							query.FetchOnlyFromLocalCache = !shipment.IsInDatabase;
							query.ReLoadExistingRows = true;
							var houseBill = Factory.LoadTop1<CusCAeMHHouse>(query);
							if (houseBill == null)
							{
								houseBill = MasterBill.HouseBills.AddNew();
								houseBill.BW_ParentID = shipment.PK;
								houseBill.BW_ParentTableCode = shipment.TablePrefix;

								var countryCodeOfLastLeg = shipment.ArrivalConsol?.Transports.ArrivalTransport?.JW_RL_NKDiscPort.SubstringSafe(0, 2) ?? ZString.Empty;
								if (countryCodeOfLastLeg != Core.Constants.CountryCodes.Canada)
								{
									houseBill.BW_MovementType = (ZString)eMHMovementTypeList.Codes.InTransit;
								}
								else
								{
									houseBill.BW_MovementType = eMHMovementTypeList.Codes.Import;
								}
							}
							houseBill.HookShipmentSynchronisingEvent();
							houseBill.EnableAndSynchronise();
						}
					}
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (MasterBill != null && !MasterBill.IsDeleted)
				{
					foreach (var houseBill in MasterBill.HouseBills)
					{
						houseBill.UnHookShipmentSynchronisingEvent();
					}
				}
			}
			base.Dispose(disposing);
		}

		#region HVLV

		ICAeManifestHouseBillFromHVLVShipmentPopulator HouseBillFromHVLVShipmentPopulator => houseBillFromHVLVShipmentPopulator ?? (houseBillFromHVLVShipmentPopulator = ObjectFactory.Get<ICAeManifestHouseBillFromHVLVShipmentPopulator>("ICAeManifestHouseBillFromHVLVShipmentPopulator", MasterBill));
		ICAeManifestHouseBillFromHVLVShipmentPopulator houseBillFromHVLVShipmentPopulator;

		#endregion
	}
}
