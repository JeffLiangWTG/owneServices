using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using IUserNotification = Enterprise.Customs.Business.MessageManagers.IUserNotification;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public class ShipmentCargoReportPlugIn : ZMutexedPlugIn
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ShipmentCargoReportPlugIn(ForwardingShipment shipment)
			: base(shipment)
		{
			CargoWise.Common.Argument.NotNull(shipment, "Shipment");
			this.shipment = shipment;
			((IManifestProvider)this.shipment).CustomsManifestVisibilityChanged += OnChangeTheVisibilityRequired;
			ChangeTheVisibility();
			SetCoveringLabelText();
		}

		public ShipmentCargoReportPlugIn(ForwardingShipment shipment, bool isCalledFromConsolPlugin)
			: this(shipment)
		{
			this.isCalledFromConsolPlugin = isCalledFromConsolPlugin;
		}

		#region Overrides

		protected override void Dispose(bool disposing)
		{
			if (disposing && consolACIPluginCreated != null)
			{
				consolACIPluginCreated.Dispose();
			}
			base.Dispose(disposing);
			if (disposing)
			{
				((IManifestProvider)shipment).CustomsManifestVisibilityChanged -= OnChangeTheVisibilityRequired;
				if (cusSCAHouse != null)
				{
					cusSCAHouse.OnApplicationTypeChanged -= CusSCAHouse_OnApplicationTypeChanged;
				}
			}
		}

		protected sealed override ZBool HasUserControl
		{
			get { return ZBool.True; }
		}

		protected override Control GetNewUserControl()
		{
			return new ShipmentCargoReportUserControl();
		}

		protected sealed override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return cusSCAHouse;
		}

		public override ZGlobalMutex Mutex
		{
			get { return mutex ?? (mutex = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, shipment.PK + HouseMutexString)); }
		}

		ZGlobalMutex mutex;

		public override ZString PlugInNotDisplayedMessage
		{
			get { return CoveringLabelText; }
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return cusSCAHouse != null;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			InitialiseCusSCAHouse();
			return base.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
		}

		public override string Name
		{
			get { return "ACI"; }
		}

		protected sealed override MenuItem GetNewTopLevelMenu()
		{
			var cargoReportMenu = new ZMenuItem("ACI");
			cargoReportMenu.MenuItems.Add(ResString.GetMultilingualString("aaacb3cd-e095-42b4-b13a-51e0fa47776d", "Send ACI Supplementary (House) Cargo Message"), SendCargoReport_Click);
			cargoReportMenu.MenuItems.Add(ResString.GetMultilingualString("8b78fc59-fc46-4f16-936f-ecc8ad41187b", "Cancel ACI Supplementary (House) Cargo Message"), CancelCargoReport_Click);
			var forcedMessagesMenu = new ZMenuItem(ResString.GetMultilingualString("3191c0ac-1f77-40ba-ad4a-a1937f8608f2", "Forced Messages"));
			forcedMessagesMenu.MenuItems.Add(ResString.GetMultilingualString("127826ec-9d18-44e9-8cc8-7255dc2f32b9", "Send ACI Supplementary (House) Original Cargo Message"), SendOriginalCargoReport_Click);
			forcedMessagesMenu.MenuItems.Add(ResString.GetMultilingualString("a6865978-ea10-4df3-b7e5-d44b515cdce1", "Send ACI Supplementary (House) Amendment Cargo Message"), SendAmendCargoReport_Click);
			cargoReportMenu.MenuItems.Add(forcedMessagesMenu);
			cargoReportMenu.MenuItems.Add(ResString.GetMultilingualString("38da37b8-821c-4169-86b5-359fe8235f63", "Refresh Shipment ACI Data"), RefreshData_Click);
			cargoReportMenu.MenuItems.Add(ResString.GetMultilingualString("606a3e3e-2bd8-4664-a888-ff366849c1c2", "Reset To Original"), ResetToOriginal_Click);
			return cargoReportMenu;
		}

		#endregion

		#region Implementation

		#region On Click

		#region SendCargoReport_Click

		void SendCargoReport_Click(object sender, EventArgs eventArgs)
		{
			DoMessageManagerAction(() => messageManager.SendMessage(MessageSubTypes.Undefined));
		}

		#endregion

		#region CancelCargoReport_Click

		void CancelCargoReport_Click(object sender, EventArgs eventArgs)
		{
			DoMessageManagerAction(() => messageManager.SendMessage(MessageSubTypes.Withdraw));
		}

		#endregion

		#region SendOriginalCargoReport_Click

		void SendOriginalCargoReport_Click(object sender, EventArgs eventArgs)
		{
			DoMessageManagerAction(() => messageManager.SendMessage(MessageSubTypes.Create));
		}

		#endregion

		#region SendAmendCargoReport_Click

		void SendAmendCargoReport_Click(object sender, EventArgs eventArgs)
		{
			DoMessageManagerAction(() => messageManager.SendMessage(MessageSubTypes.Change));
		}

		#endregion

		#region ResetToOriginal_Click

		void ResetToOriginal_Click(object sender, EventArgs eventArgs)
		{
			DoMessageManagerAction(() => messageManager.ResetToOriginal(Notification));
		}

		#endregion

		#region RefreshData_Click

		void RefreshData_Click(object sender, EventArgs eventArgs)
		{
			if (Notification.ShowConfirmation(RefreshDataComments, Res.GetString("132442ae-c9ca-4711-8f93-afed3887b9a0", "Warning - Refresh data?"), Res.GetString("0cc8375d-d3d3-469a-b5cc-62c742dbcb68", "If you are sure you want to refresh ACI data, please type:") + " ", Res.GetString("337f65f8-b2e7-499e-bb0b-b0a489586e2e", "REFRESH DATA")))
			{
				RefreshDataCusSCAHouse();
			}
		}

		static string RefreshDataComments
		{
			get
			{
				return Res.GetString("e4f70cff-0090-476a-bf64-1809f5598760", @"Warning - You are about to refresh ACI data!
Data entered on the ACI tab of this shipment will be refreshed from the shipment data. Any data that has been overridden on the ACI tab may be replaced.  If a Supplementary Cargo Report has already been submitted to Customs then you should send another message, after the refresh, to ensure that correct data has been reported.");
			}
		}

		public void RefreshDataCusSCAHouse()
		{
			InitialiseCusSCAHouse();
			if (cusSCAHouse != null)
			{
				SyncroniseWithConsol(true);
				cusSCAHouse.EnableAndSynchronise(true);
			}
		}

		#endregion

		SupplementaryCargoReportMessageManager messageManager;

		void DoMessageManagerAction(Action messageManagerAction)
		{
			if (messageManager != null)
			{
				messageManagerAction.Invoke();
			}
			else
			{
				InitialiseCusSCAHouse();
				if (cusSCAHouse != null)
				{
					messageManager = new SupplementaryCargoReportMessageManager(cusSCAHouse, Notification);
					messageManagerAction.Invoke();
				}
			}
		}

		#endregion

		#region On Change Visibility

		void OnChangeTheVisibilityRequired(object sender, EventArgs e)
		{
			ChangeTheVisibility();
		}

		internal void ChangeTheVisibility()
		{
			consolForCanada = null;
			Enabled = !Factory.CanadianCarrierCode().IsEmpty && (shipment.IsAir || shipment.IsSea) && IsConsolForCanadaAirOrSea;
		}

		bool IsConsolForCanadaAirOrSea
		{
			get
			{
				var consol = ConsolForCanada;
				return consol != null && (consol.IsAir || consol.IsSea);
			}
		}

		#endregion

		#region InitialiseCusSCAHouse

#if DEBUG
		internal ZInt DelayPluginForTestingMutex { get; set; }
#endif

		internal void InitialiseCusSCAHouse()
		{
			if (cusSCAHouse == null)
			{
				cusSCAHouse = GetHouseBillForShipment(reloadQuery: false);
				if (cusSCAHouse == null)
				{
					var result = DialogResult.Yes;
					if (ConsolForCanada == null)
					{
						result = Globals.Message.Show(Res.GetString("B5EF45F1-E69E-44D2-A952-6475B734A85E", "This shipment is not attached to a Consol discharging in Canada."),
									"",
									MessageBoxButtons.OK,
									MessageBoxIcon.Warning);
						result = DialogResult.No;
					}

					if (result == DialogResult.Yes && !isCalledFromConsolPlugin)
					{
						result = Globals.Message.Show(Res.GetString("4E5FACEB-7CB3-44B4-ACE2-5ED25D57CB1B", "Do you want to create an ACI Job at this time?"),
									"",
									MessageBoxButtons.YesNo,
									MessageBoxIcon.Information);
					}

#if DEBUG
					if (DelayPluginForTestingMutex > 0)
					{
						System.Threading.Thread.Sleep(DelayPluginForTestingMutex);
					}
#endif

					if (result == DialogResult.Yes)
					{
						if (Mutex.Lock())
						{
							cusSCAHouse = GetHouseBillForShipment(reloadQuery: true);
							if (cusSCAHouse == null)
							{
								var oceanBillLoader = new BaseCusSCAOceanBill.Loader(Factory);
								var master = (CusSCAOceanBill)oceanBillLoader.LoadFromConsolAndApplicationCode(ConsolForCanada, CusSCAOceanBill.ApplicationCodes);

								if (master == null)
								{
									consolACIPluginCreated = new CAConsolACIPlugIn(ConsolForCanada, true);
									if (!consolACIPluginCreated.Mutex.IsLocked)
									{
										consolACIPluginCreated.InitialiseBusinessObject();
										master = (CusSCAOceanBill)oceanBillLoader.LoadFromConsolAndApplicationCode(ConsolForCanada, CusSCAOceanBill.ApplicationCodes);
									}
								}
								if (master != null)
								{
									CreateNewCusSCAHouse(master);
								}
							}
						}
					}
					else
					{
						CoveringLabelText = Res.GetString("E2855C3E-D294-4D67-8BEB-4BB746385877", "You have chosen not to create an ACI Job now.\r\nPlease change to another tab, then click back to this tab to create an ACI Job for this Shipment.");
					}
				}
				if (cusSCAHouse != null)
				{
					cusSCAHouse.isCalledFromConsolPlugin = isCalledFromConsolPlugin;
				}
				if (shipment != null && cusSCAHouse != null)
				{
					shipment.RegisterEditableChildObject(cusSCAHouse);
					if (!isCalledFromConsolPlugin)
					{
						SyncroniseWithConsol();
					}

					cusSCAHouse.EnableAndSynchronise();
				}
				if (cusSCAHouse != null)
				{
					cusSCAHouse.OnApplicationTypeChanged += CusSCAHouse_OnApplicationTypeChanged;
					Enabled = true;
				}
			}
		}
		CAConsolACIPlugIn consolACIPluginCreated;

		BaseCusSCAHouse.Loader HouseBillLoader
		{
			get { return houseBillLoader ?? (houseBillLoader = new BaseCusSCAHouse.Loader(Factory)); }
		}
		BaseCusSCAHouse.Loader houseBillLoader;

		CusSCAHouse GetHouseBillForShipment(bool reloadQuery)
		{
			return (CusSCAHouse)HouseBillLoader.LoadFromShipmentAndApplicationCode(shipment.PK, CusSCAOceanBill.ApplicationCodes, reloadQuery);
		}

		static void CusSCAHouse_OnApplicationTypeChanged(object sender, ValueChangedEventArgs e)
		{
			var house = ((CusSCAHouse)sender);
			var oceanBill = house.OceanBill;
			if (oceanBill != null && oceanBill.HouseBills.Count > 1 && ShowChangeApplicationTypeConfirmation(e))
			{
				foreach (var houseBill in oceanBill.HouseBills.Where(h => h != house))
				{
					houseBill.CA_FROBTransitImportCode = (ZString)e.NewValue;
				}
			}
		}

		internal static bool ShowChangeApplicationTypeConfirmation(ValueChangedEventArgs e)
		{
			var caption = Res.GetString("d16ec235-8923-4c57-8171-15816032999e", "ACI Application Type Changed");
			var message = Res.GetString("67f6c443-22a6-4727-89b8-c5d10af4bb4b", "ACI Application Type has been changed to {0}.\r\nWould you like the application type on all shipments,\r\nof the current console, changed to this new value?", e.NewValue);
			return Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}

		internal ZString CoveringLabelText { get; set; }

		protected void SetCoveringLabelText()
		{
			CoveringLabelText = Res.GetString("59a7dcae-3aaf-4a2e-981e-9fd70c9cd5f4", "Someone else is already in the process of creating an ACI job for this shipment.");
			CoveringLabelText += "\r\n" + Res.GetString("280fe5f2-16a9-436a-a761-77a6a94416eb", "You cannot process this ACI job until they save their data. Please try later.");
		}

		void CreateNewCusSCAHouse(CusSCAOceanBill master)
		{
			cusSCAHouse = master.HouseBills.AddNew();
			cusSCAHouse.CA_JS = shipment.PK;
			cusSCAHouse.isCalledFromConsolPlugin = isCalledFromConsolPlugin;
		}

		void SyncroniseWithConsol(bool forceSynch = false)
		{
			var oceanBill = cusSCAHouse.OceanBill;
			if (oceanBill != null)
			{
				oceanBill.EnableAndSynchronise(forceSynch);
			}
		}

		internal ForwardingConsol ConsolForCanada
		{
			get
			{
				if (consolForCanada == null && shipment != null)
				{
					consolForCanada = shipment.ConsolForCountry(Constants.CountryCodes.Canada) ?? shipment.Consols.Cast<ForwardingConsol>().FirstOrDefault(x => x.IsGoingViaIgnoringDomesticRoute(Constants.CountryCodes.Canada));
				}

				return consolForCanada;
			}
		}
		ForwardingConsol consolForCanada;

		#endregion

		readonly ForwardingShipment shipment;
		readonly bool isCalledFromConsolPlugin;
		CusSCAHouse cusSCAHouse;

		IUserNotification Notification
		{
			get { return notification ?? (notification = new MessageInstructionUserNotification()); }
		}

		IUserNotification notification;

		internal const string ConsolMutexString = "_Only1ACIperCAConsol";
		internal const string HouseMutexString = "_Only1ACIperCAShipment";

		#endregion

#if DEBUG
		internal void SetNewNotification(IUserNotification newNotifiction)
		{
			notification = newNotifiction;
		}
#endif
	}
}
