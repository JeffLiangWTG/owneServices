using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class SeaCargoHouseModule : ZFilterGridModule
	{
		#region Checkpoints

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AUCustomsSCA; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.SeaCargoReport; }
		}

		#endregion

		#region Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.AU.HouseSeaCargo; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			var scaHouse = selectedBusinessObject as CusSCAHouse;
			if (scaHouse != null && scaHouse.Shipment != null)
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.AU.SeaCargoHouseShipmentController);
			}
			else if (scaHouse != null && scaHouse.Consol != null)
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.AU.SeaCargo);
			}
			else
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.AU.SeaCargoHouseController);
			}
		}

		internal ZController GetNewControllerInternal(BusinessObject selectedBusinessObject) => GetNewController(selectedBusinessObject);

		protected override IFilterControl GetNewFilterControl()
		{
			return new SeaCargoHouseFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusSCAHouseCollectionNonDependent(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new SeaCargoHouseFilterBusinessObject();
		}

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CusSCAHouseWorkflowDescriptorCode;

		public override bool AllowDelete => false;

		public override bool AllowNew => false;

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());

			if (ViewMenuItem != null)
			{
				ViewMenuItem.MenuItems.Add(Res.GetString("AUSeaCargoHouseModule.Menu.ViewHouse", "View Sea Cargo House"), HandleViewClick);
				ViewMenuItem.MenuItems.Add(Res.GetString("AUSeaCargoHouseModule.Menu.ViewReport", "View Sea Cargo Report"), HandleViewReportClick);
			}

			if (EditMenuItem != null)
			{
				EditMenuItem.MenuItems.Add(Res.GetString("AUSeaCargoHouseModule.Menu.EditHouse", "Edit Sea Cargo House"), HandleEditClick);
				EditMenuItem.MenuItems.Add(Res.GetString("AUSeaCargoHouseModule.Menu.EditReport", "Edit Sea Cargo Report"), HandleEditReportClick);
			}

			return menuItems.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewActionMenuItems());

			menuItems.Add(new ZMenuItem("-"));
			menuItems.Add(new ZMenuItem(Res.GetString("AUSeaCargoHouseModule.Menu.SendHouse", "Send Sea Cargo House Messages"), SendHouseClick));
			menuItems.Add(new ZMenuItem(Res.GetString("AUSeaCargoHouseModule.Menu.SendReport", "Send Sea Cargo Report Messages"), SendReportClick));
			menuItems.Add(new ZMenuItem(Res.GetString("AUSeaCargoHouseModule.Menu.SendUnderbondRequest", "Send Underbond Request Messages"), SendUnderbondRequestClick));

			return menuItems.ToArray();
		}

		void SendUnderbondRequestClick(object sender, EventArgs eventArgs)
		{
			var bills = GetSelectedBills();
			var bill = bills.FirstOrDefault();
			if (ContainsOnlyOneBill(bills) &&
				Globals.Message.Show(
					Res.GetString("EC46084D-A029-461C-B026-600E2C17F8BA", "This will send an Underbond Request message for every House Bill on {0} that does not have validation errors against it, do you want to proceed?", bill.CB_OceanBill),
					Res.GetString("403F3877-36FB-44BE-8C2B-F51DD437F55B", "Proceed?"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				var underbonds = new List<Declaration.Business.CusUnderbond>();
				foreach (var underbond in bill.AllUnderbonds.OfType<Declaration.Business.CusUnderbond>().Where(x => x.C4_MessageStatus == ZString.Empty || x.C4_MessageStatus == CMRBaseStatuses.Codes.NotSent))
				{
					underbond.Validation.ValidateAll();
					if (!underbond.Notifications.HasErrors() && !underbond.Notifications.HasMessageErrors())
					{
						underbonds.Add(underbond);
					}
				}

				if (underbonds.Count > 0)
				{
					SendMessagesForBill(bill, new MultiCusUnderbondMessageManager(underbonds.ToArray()));
				}
				else
				{
					ShowInformationNoMessageGenerated();
				}
			}
		}

		#region Message Sending

		void SendMessagesForBill(CusSCAOceanBill bill, Declaration.Business.MultiMessageManager manager)
		{
			var mutex = bill.Mutex;
			if (mutex != null)
			{
				try
				{
					if (mutex.Lock())
					{
						if (manager.SendOriginalMessages(new SendsMessagesToCustomsGUI()).Any())
						{
							try
							{
								Factory.Save();
							}
							catch (ZSaveException e1)
							{
								ZExceptionReporting.HandleSaveException(e1);
							}
						}
					}
					else
					{
						Globals.Message.Show(string.Format("Messages are currently being generated and sent by someone else for this master, please try again later."), "Try later", MessageBoxButtons.OK, MessageBoxIcon.Stop);
					}
				}
				finally
				{
					if (mutex.HasLock)
					{
						mutex.Unlock();
					}
				}
			}
		}

		bool ContainsOnlyOneBill(IEnumerable<CusSCAOceanBill> bills)
		{
			var result = false;

			var count = bills?.Count() ?? 0;
			if (count > 1)
			{
				Globals.Message.Show("Please select a single Sea Cargo Report (Job) and try again", "Try again", MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
			else if (count == 1)
			{
				result = true;
			}

			return result;
		}

		IEnumerable<CusSCAOceanBill> GetSelectedBills()
		{
			return SelectedBusinessObjects.OfType<CusSCAHouse>().Where(x => x.OceanBill != null).Select(x => x.OceanBill).Distinct();
		}

		class MultiCusUnderbondMessageManager : Declaration.Business.MultiMessageManager
		{
			public MultiCusUnderbondMessageManager(Declaration.Business.CusUnderbond[] underbonds)
			{
				Underbonds = underbonds;
			}

			Declaration.Business.CusUnderbond[] Underbonds { get; }

			public override IMessageManageableBizObj TopLevelBizObjToManage => null;

			protected override bool RunPreSaveValidationWhenSendingAMessage => false;

			protected override bool SendWheneverPossibleOnceMessagingActive => true;

			protected override SingleMessageManager[] GetAllMessageManagers()
			{
				var result = new ArrayList();
				if (Underbonds != null && Underbonds.Any())
				{
					foreach (var underbond in Underbonds)
					{
						result.Add(new CusUnderbondUBMREQManager(underbond, ZString.Empty));
					}
				}
				return (SingleMessageManager[])result.ToArray(typeof(SingleMessageManager));
			}
		}

		class MultiCusSCAHouseMessageManager : Declaration.Business.MultiMessageManager
		{
			public MultiCusSCAHouseMessageManager(CusSCAHouse[] houses, bool sendWheneverPossibleOnceMessagingActive)
			{
				Houses = houses;
				SendWheneverPossibleOnceMessagingActive = sendWheneverPossibleOnceMessagingActive;
			}

			CusSCAHouse[] Houses { get; }

			public override IMessageManageableBizObj TopLevelBizObjToManage => null;

			protected override bool RunPreSaveValidationWhenSendingAMessage => false;

			protected override bool SendWheneverPossibleOnceMessagingActive { get; }

			protected override SingleMessageManager[] GetAllMessageManagers()
			{
				var result = new ArrayList();
				if (Houses != null && Houses.Any())
				{
					foreach (var house in Houses)
					{
						result.Add(new CusSCAHouseSEACRManager(house, ZString.Empty));
					}
				}
				return (SingleMessageManager[])result.ToArray(typeof(SingleMessageManager));
			}
		}

		#endregion

		void SendReportClick(object sender, EventArgs eventArgs)
		{
			var bills = GetSelectedBills();
			var bill = bills.FirstOrDefault();
			if (ContainsOnlyOneBill(bills) &&
				Globals.Message.Show(
					Res.GetString("51573032-E4C3-475A-A736-A3BD3739894F", "This will send a Sea Cargo message for every House Bill on {0} that does not have validation errors against it, do you want to proceed?", bill.CB_OceanBill),
					Res.GetString("403F3877-36FB-44BE-8C2B-F51DD437F55B", "Proceed?"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				var houses = new List<CusSCAHouse>();
				foreach (var house in bill.HouseBills.OfType<CusSCAHouse>().Where(x => x.CA_MessageStatus == CMRBaseStatuses.Codes.NotSent || x.CA_MessageStatus == ZString.Empty))
				{
					using (house.ResumeValidationTemporarily())
					{
						house.Validation.ValidateAll();
					}

					if (!house.Notifications.HasErrors() && !house.Notifications.HasMessageErrors())
					{
						houses.Add(house);
					}
				}

				if (houses.Count > 0)
				{
					SendMessagesForBill(bill, new MultiCusSCAHouseMessageManager(houses.ToArray(), true));
				}
				else
				{
					ShowInformationNoMessageGenerated();
				}
			}
		}

		static void ShowInformationNoMessageGenerated()
		{
			Globals.Message.ShowInformation(
									Res.GetString("02FECDCA-4185-47BE-BD10-DB24323392CB", "No original message has been generated."),
									Res.GetString("5767E517-C07C-400E-A6D3-ADB84C7E09B2", "Message Sending Result"));
		}

		void SendHouseClick(object sender, EventArgs eventArgs)
		{
			var houses = SelectedBusinessObjects.OfType<CusSCAHouse>().Where(x => x.CA_MessageStatus == CMRBaseStatuses.Codes.NotSent || x.CA_MessageStatus == ZString.Empty);
			if (houses.Any())
			{
				var groups = houses.GroupBy(x => x.OceanBill);
				var lockedHouses = new List<CusSCAHouse>();
				try
				{
					groups.ForEach(x =>
					{
						var mutex = x.Key.Mutex;
						if (!(mutex?.Lock() ?? false))
						{
							lockedHouses.AddRange(x);
						}
					});

					if (lockedHouses.Count > 0)
					{
						Globals.Message.Show(string.Format("Messages are currently being generated and sent by someone else for this master, please try again later."), "Try later", MessageBoxButtons.OK, MessageBoxIcon.Stop);
					}
					else
					{
						var manager = new MultiCusSCAHouseMessageManager(houses.ToArray(), false);
						if (manager.SendOriginalMessages(new SendsMessagesToCustomsGUI()).Any())
						{
							try
							{
								Factory.Save();
							}
							catch (ZSaveException e1)
							{
								ZExceptionReporting.HandleSaveException(e1);
							}
						}
					}
				}
				finally
				{
					groups.ForEach(x =>
					{
						var mutex = x.Key.Mutex;
						if (mutex != null)
						{
							if (mutex.HasLock)
							{
								mutex.Unlock();
							}
						}
					});
				}
			}
		}

		void HandleViewReportClick(object sender, EventArgs eventArgs)
		{
			var house = SelectedBusinessObjects.OfType<CusSCAHouse>().FirstOrDefault();
			var bill = house?.OceanBill;
			if (bill != null)
			{
				var seaCargoController = GetControllerForSeaCargo(bill);
				seaCargoController.ShowViewForm(bill);
			}
		}

		void HandleEditReportClick(object sender, EventArgs eventArgs)
		{
			var house = SelectedBusinessObjects.OfType<CusSCAHouse>().FirstOrDefault();
			var bill = house?.OceanBill;
			if (bill != null)
			{
				var seaCargoController = GetControllerForSeaCargo(bill);
				seaCargoController.ShowEditForm(bill);
			}
		}

		ZController GetControllerForSeaCargo(CusSCAOceanBill bill)
		{
			if (bill != null && bill.Consol != null)
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.AU.SeaCargo);
			}
			else
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.AU.SeaCargoStandAloneController);
			}
		}

		protected override void HandleViewClickCore(object sender, EventArgs e)
		{
			if (ShouldCallBaseHandleMethod)
			{
				base.HandleViewClickCore(sender, e);
			}
		}

		protected override void HandleEditClickCore(object sender, EventArgs e)
		{
			if (ShouldCallBaseHandleMethod)
			{
				base.HandleEditClickCore(sender, e);
			}
		}

		const int MaxSelectionCount = 10;

		bool ShouldCallBaseHandleMethod => SelectedBusinessObjects == null
			|| SelectedBusinessObjects.Length <= MaxSelectionCount
			|| ZArchitecture.Environment.Globals.Message.Show(
				Res.GetString("606FAEDB-D307-43B6-A85B-D5B589CDF043", "You have selected a large number of records, do you want to proceed?"),
				Res.GetString("A95297A1-EA88-4E5D-8BE7-E83B563D0525", "Too many records..."),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

		#endregion
	}
}
