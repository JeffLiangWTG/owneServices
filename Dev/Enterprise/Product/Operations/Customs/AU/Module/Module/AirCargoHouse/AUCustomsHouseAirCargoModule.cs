using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module.AirCargo
{
	/// <summary>
	/// Module for AUCustomsHouseAirCargo.
	/// </summary>
	public class AUCustomsHouseAirCargoModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.AU.HouseAirCargo; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			CusHAWB hAWB = selectedBusinessObject as CusHAWB;
			if (hAWB != null && hAWB.Shipment != null)
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCargoShipmentController);
			}
			else
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.AU.HouseAirCargo);
			}
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AUCustomsHouseAirCargoFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ModuleHAWBCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AUCustomsHouseAirCargoFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AirCargoReport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ACAHouse; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.CustomsHouseAirCargoCode; }
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			var result = base.GetNewAdditionalMenuItems().ToList();
			if (Enterprise.Registry.Business.HVLVDataRegistry.HasHVLVClearance)
			{
				var releaseConsignmentsFromBondStore = new ZMenuItem("Release Cleared Consignment(s) from Bond Store");
				releaseConsignmentsFromBondStore.Click += releaseConsignmentsFromBondStore_Click;
				result.Add(releaseConsignmentsFromBondStore);
			}
			return result.ToArray();
		}

		void releaseConsignmentsFromBondStore_Click(object sender, System.EventArgs e)
		{
			if (Env.Security.ReleaseConsignmentsFromBondStore.IsAllowed)
			{
				if (SelectedBusinessObjects.Any())
				{
					var released = 0;
					var houseBills = new BusinessObjectFactory().Load<CusHAWB>(new ZQuery(CusHAWBSchema.PK, SelectedBusinessObjects.Select(x => x.PK)));
					foreach (var bill in houseBills)
					{
						released += bill.ReleaseFromBondStore() ? 1 : 0;
					}
					if (released > 0)
					{
						ZExceptionReporting.ProcessWithSaveExceptionHandling(houseBills[0].Factory.Save, null, true);
					}
					Globals.Message.ShowInformation(string.Format("Released {0} Cleared Consignment(s) from Bond Store.", released));
				}
				else
				{
					this.ShowNoSelectedMessage();
				}
			}
			else
			{
				Env.Security.ReleaseConsignmentsFromBondStore.ShowError();
			}
		}
	}
}
