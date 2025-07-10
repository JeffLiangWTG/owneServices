using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public class AirCargoShipmentPlugIn : AirCargoPlugIn
	{
		public AirCargoShipmentPlugIn(CommonShipment shipment)
			: base(shipment)
		{
		}

		protected ForwardingShipment Shipment
		{
			get { return (ForwardingShipment)ManifestProvider; }
		}

		#region Manager

		CusHAWBMessageManager fManager;

		protected internal virtual CusHAWBMessageManager Manager
		{
			get
			{
				if (fManager == null)
				{
					fManager = new CusHAWBMessageManager(() => HouseBill);
				}
				return fManager;
			}
		}

		#endregion

		#region IZPlugIn Members

		protected override MenuItem GetNewTopLevelMenu()
		{
			if (fMainMenuItem == null)
			{
				fMainMenuItem = AirCargoShipmentMenu.New(Manager);
				fMainMenuItem.Enabled = IsFormEditable;
			}
			return fMainMenuItem;
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return HouseBill;
		}

		public override bool CanDelete
		{
			get { return Shipment == null || HouseBill == null || (HouseBill != null && HouseBill.CanDelete); }
		}

		public override string CannotDeleteMessage
		{
			get { return "You cannot delete this record as there are air cargo messages sent to the Customs."; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return MasterBill != null && HouseBill != null;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			CreateAirCargoJobIfRequired();
			return MasterBill != null && HouseBill != null;
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get { return plugInNotDisplayedMessage; }
		}
		string plugInNotDisplayedMessage;

		public void CreateAirCargoJobIfRequired()
		{
			if (Shipment != null && Consol != null)
			{
				plugInNotDisplayedMessage = string.Empty;

				if (MasterBill == null && CheckForInvalidAirCargo() && MutexForConsol != null)
				{
					if (QueryUserToCreateAirCargoJob())
					{
						if (FormPreSaved(Shipment, Form))
						{
							if (MutexForConsol.Lock())
							{
								var newFactory = new BusinessObjectFactory();
								var consolInNewFactory = newFactory.Load<ForwardingConsol>(Consol.PK);
								var mawbInNewFactory = CusMAWB.Load(consolInNewFactory);
								if (mawbInNewFactory == null)
								{
									mawbInNewFactory = CusMAWB.CreateNew(consolInNewFactory);
								}
								else
								{
									mawbInNewFactory.SynchroniseData();
								}

								try
								{
									newFactory.Save();
								}
								catch (Exception e) when (!e.IsCriticalException())
								{
									if (!mawbInNewFactory.IsInDatabase)
									{
										mawbInNewFactory.Delete();
									}

									ZExceptionReporting.HandleSaveException(e);
								}
								finally
								{
									MutexForConsol.Unlock();
								}
							}
							else
							{
								plugInNotDisplayedMessage = SomeoneIsEditingTheShipment;
							}
						}
						else
						{
							plugInNotDisplayedMessage = YouHaveToSaveBeforeCreateAnAirCargo;
						}
					}
					else
					{
						plugInNotDisplayedMessage = YouHaveChosenNotToCreateAnAirCargo;
					}
				}

				if (string.IsNullOrEmpty(plugInNotDisplayedMessage) && !CreateAirCargoCusHawbIfRequired(MasterBill))
				{
					plugInNotDisplayedMessage = SomeoneIsEditingTheShipment;
				}
			}
		}

		bool CheckForInvalidAirCargo()
		{
			var filter = ForwardingConsol.GetMAWBsFilter(Consol.PK, reloadExistingRows: true, new ZString[] { string.Empty, Core.Constants.Customs.CusSCAOceanBillApplicationCodes.AustraliaLegacy });
			var legacyMAWB = Consol.Factory.LoadTop1<Business.CusMAWB>(filter);
			if (legacyMAWB != null)
			{
				plugInNotDisplayedMessage = Res.GetString("DE536FC9-E4DC-4168-88C6-FFC720183A2C", "Air Cargo Data is Invalid.");
				return false;
			}

			return true;
		}

		bool QueryUserToCreateAirCargoJob()
		{
			return Globals.Message.Show(DoYouWantToCreateAnAirCargo, string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
		}

		public const string SomeoneIsEditingTheShipment = "Someone else is already in the process of creating an air cargo master bill for this shipment.\r\nYou cannot process this job until they save their data. Please try later.";
		public const string YouHaveChosenNotToCreateAnAirCargo = "You have chosen not to create an air cargo job now.\r\nPlease change to another tab. Click back to this tab later if you want to create an air cargo job for this shipment.";
		public const string DoYouWantToCreateAnAirCargo = "Do you want to create an air cargo job at this time?";
		public const string YouHaveToSaveBeforeCreateAnAirCargo = "You have to save the data first before creating an air cargo job.\r\nPlease change to another tab and save the job, then click back to this tab to create an air cargo job for this shipment.";

		internal bool CreateAirCargoCusHawbIfRequired(CusMAWB mawb)
		{
			var result = false;
			if (fHouseBill == null)
			{
				HouseBill = Loader.Load(mawb, reloadExistingRows: true);
			}

			if (HouseBill == null && mawb != null)
			{
				HouseBill = Loader.LoadOrCreate(mawb);

				if (HouseBill != null)
				{
					result = true;
					if (!mawb.ChildBills.Contains(HouseBill))
					{
						mawb.ChildBills.Add(HouseBill);
					}

					try
					{
						HouseBill.Factory.Save();
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						var houseBill = HouseBill;

						if (houseBill != null && !houseBill.IsInDatabase)
						{
							houseBill.Delete();
							fHouseBill = null;
						}

						ZExceptionReporting.HandleSaveException(e);
					}
				}
			}

			return result;
		}

		public override void OnGUIShown()
		{
			base.OnGUIShown();
			CreateAirCargoJobIfRequired();
			if (MasterBill != null && HouseBill != null)
			{
				((AirCargoDeclarationUserControl)UserControl).SetupPlugins();
			}
		}

		protected override Control GetNewUserControl()
		{
			_ = HouseBill; // initialise fHouseBill
			return new AirCargoDeclarationUserControl();
		}

		public override string Name
		{
			get { return "Air Cargo"; }
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			ContinueWithSave result = base.ShowPreSaveDialogsCore();

			if (result == ContinueWithSave.Yes && HouseBillFromLocalCache != null && HouseBillFromLocalCache.HasChanges)
			{
				result = new SendsMessagesToCustomsGUI().DetermineRequiredMessagesAndSendThem(Manager);
			}

			return result;
		}

		protected override void OnIsFormEditableChanged()
		{
			base.OnIsFormEditableChanged();
			TopLevelMenu.Enabled = IsFormEditable;
		}

		#endregion

		#region Implementation

		protected const string MasterAndHouseBillChangedCaption = "MasterBill and HouseBill Changed";
		protected const string MasterAndHouseBillChangedMessage = "There are changes made on master and this house bills. The system cannot save the changes made on this housebill \nwhile sending zero-landing messages to the Customs. Please make necessary changes on a Master first and wait for the response from Customs.";
		protected const string AmendmentCaption = "Sending Amendment Messages";
		protected const string NormalAmendmentMessage = "You have messages sent for this housebill. The changes you made will result in the system sending an amendment message to the Customs.\nAre you sure you wish to continue?";
		protected const string ZeroLandingHousebillCaption = "House bill number changed";
		protected const string ZeroLandingThisHouseBillMessage = "You changed the house bill number. System is about to send zero-landing message and\r\nprealert with a new house bill number you just entered. Are you sure you wish to continue?";
		protected const string BulkZeroLandingMessage = "You changed the master bill number. System is about to zero-land all pre-alerted house bills under this master\r\nand send new original messages. Are you sure you wish to continue?";
		protected const string BulkZeroLandingCaption = "Zero-Landing";
		protected const string ChangeMasterColoadMessage = "The changes you just made result in the system sending a Change Master/Co-Load amendment message.\nAre you sure you wish to continue?";

		protected override void ChangeTheVisibilityCore()
		{
			Enabled = Shipment != null && Shipment.IsAir && Shipment.HasConsolsDischargingInCurrentCountry;
		}

		protected override ForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = GetConsolFromShipment();
				}
				return fConsol;
			}
		}

		protected ForwardingConsol GetConsolFromShipment()
		{
			foreach (ForwardingConsol consol in Shipment.Consols)
			{
				if (consol.JK_RL_NKDischargePort.StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
				{
					return consol;
				}
			}
			return null;
		}

		protected internal CusMAWB MasterBill
		{
			get
			{
				if (fMasterBill == null && Consol != null)
				{
					fMasterBill = CusMAWB.Load(Consol);
					if (fMasterBill != null)
					{
						Shipment.RegisterEditableChildObject(fMasterBill);
					}
				}
				return fMasterBill;
			}
		}
		CusMAWB fMasterBill;

		protected internal CusHAWB HouseBill
		{
			get
			{
				var result = fHouseBill;
				if (fHouseBill == null || MasterBill != null && fHouseBill.CS_CM.IsEmpty)
				{
					result = (HouseBill = Loader.Load(MasterBill));
				}
				return result != null && result.IsDeleted ? null : result;
			}
			set
			{
				fHouseBill = value;
				if (fHouseBill != null)
				{
					var mawb = fHouseBill.MAWB;
					if (mawb != null)
					{
						mawb.CurrentHouseBill = value;
						mawb.UnregisterHouseBillsAsEditableChildren();
					}
				}
			}
		}
		CusHAWB fHouseBill;

		CusHAWB HouseBillFromLocalCache
		{
			get { return CusHAWB.Load(Factory, new ZQuery(CusHAWBSchema.CS_JS, Shipment.PK) { FetchOnlyFromLocalCache = true }).FirstOrDefault(); }
		}

		protected ForwardingConsol fConsol;
		AirCargoShipmentMenu fMainMenuItem;

		protected bool CheckMessageErrors()
		{
			bool hasMessageErrors = HouseBill.HasMessageErrors || HouseBill.MAWB.MessageErrorsString.Length > 0;
			if (hasMessageErrors)
			{
				Globals.Message.Show("Please clear the message errors first.", "Message Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			return hasMessageErrors;
		}

		protected void InformUsersOnMessageSent()
		{
			Globals.Message.Show("Message sent", "Air Cargo Automation", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		public override void OnSaveCompletedOrAborted(bool saved)
		{
			base.OnSaveCompletedOrAborted(saved);
			if (!saved && HouseBill != null)
			{
				HouseBill.DeleteAnyNewMessages();
			}
		}

		#endregion

		#region Mutex

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnlockMutexIfNecessary();
			}

			base.Dispose(disposing);
		}

		CusHAWBLoaderWithMutexManagement Loader
		{
			get { return loader ?? (loader = new CusHAWBLoaderWithMutexManagement(Shipment)); }
		}
		CusHAWBLoaderWithMutexManagement loader;

		protected void UnlockMutexIfNecessary()
		{
			if (MutexForConsol != null && MutexForConsol.HasLock)
			{
				MutexForConsol.Unlock();
			}

			Loader.UnlockMutexIfNecessary();
		}

		#endregion

	}
}
