using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public class CcsukShipmentMultiHawbPlugin : CcsukPlugin
	{
		public CcsukShipmentMultiHawbPlugin(CommonShipment shipment)
			: base(shipment)
		{
			if (shipment != null)
			{
				shipment.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}
		}

		protected ForwardingShipment Shipment
		{
			get { return (ForwardingShipment)ManifestProvider; }
		}

		#region IZPlugIn Members

		protected override void SetupTopLevelMenu()
		{
			CcsukConsolMultiMawbPlugin.SetUpMenu(HawbPluginHelper.Hawbs, Form, ref mainMenuItem);
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			var numberOfHawbs = HawbPluginHelper.Hawbs.Count;
			if (numberOfHawbs > 1)
			{
				return HawbPluginHelper;
			}
			else if (numberOfHawbs == 1)
			{
				return hawbPluginHelper.Hawbs[0];
			}
			else
			{
				if (UserControl != null)
				{
					DiscardCurrentUserControl();
					ShowCoveringLabel(NeedReloadText);
				}

				return null;
			}
		}

		ShipmentToManyHawbsPluginHelper hawbPluginHelper;
		public ShipmentToManyHawbsPluginHelper HawbPluginHelper
		{
			get { return hawbPluginHelper ?? (hawbPluginHelper = new ShipmentToManyHawbsPluginHelper(Shipment, Consol)); }
		}

		ConsolToManyMawbsPluginHelper mawbPluginHelper;
		public ConsolToManyMawbsPluginHelper MawbPluginHelper
		{
			get { return mawbPluginHelper ?? (mawbPluginHelper = new ConsolToManyMawbsPluginHelper(Consol)); }
		}

		public override bool CanDelete
		{
			get { return false; }
		}

		public override string CannotDeleteMessage
		{
			get { return "You cannot delete this record as there are messages sent to CCS-UK"; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return IsValidShipmentForCcsuk;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			var result = false;
			if (Shipment != null && Consol != null)
			{
				var countOfHawbs = HawbPluginHelper.ResetReloadAndCount();
				var countOfMawbs = MawbPluginHelper.ResetReloadAndCount();
				if (countOfHawbs == 0)
				{
					if (countOfMawbs == 1)
					{
						var dialogResult = DialogResult.Abort;
						var shipmentToHawbMatcherHeader = new NonPersistentShipmentToHawbMatcherHeader(Consol, MawbPluginHelper.Mawbs.OfType<CusMAWB>().ToArray(), new[] { Shipment });
						using (var form = new CcsukShipmentAndHawbLinkerForm(shipmentToHawbMatcherHeader))
						{
							dialogResult = ZFormModaliser.ShowDialogAndDispose(form);
						}

						if (dialogResult == DialogResult.None)
						{
							// old skool or unit test
							var extraWarningInfo = CompileAdditionalHelpfulWarningInformationForPopup();
							if (CcsukConsolMultiMawbPlugin.AskUserWhetherToCreateNewCcsukRecord(Shipment.JS_HouseBill, extraWarningInfo))
							{
								if (MutexForShipment != null && MutexForShipment.Lock())
								{
									MakeNewAwb(MawbPluginHelper.Mawbs[0]);
									RegisterHandlerForShedLicenceLogin();
									result = true;
								}
								else
								{
									unableToCreateMessage = MutexFailureText;
								}
							}
							else
							{
								unableToCreateMessage = CcsukConsolMultiMawbPlugin.YouHaveChosenNotToCreateARecordNowText;
							}
						}
						else if (dialogResult == DialogResult.Cancel)
						{
							unableToCreateMessage = CcsukConsolMultiMawbPlugin.YouHaveChosenNotToCreateARecordNowText;
						}
						else if (dialogResult == DialogResult.OK)
						{
							HawbPluginHelper.ResetReloadAndCount();
							EnsureCcsukMenu();
							RegisterHandlerForShedLicenceLogin();
							result = true;
						}
					}
					else if (countOfMawbs == 0)
					{
						unableToCreateMessage = ShipmentsConsolHasNoMawbText;
					}
					else
					{
						unableToCreateMessage = MultipleMawbsExistText;
					}
				}
				else
				{
					result = true;
					if ((countOfHawbs == 1 && !(UserControl is CcsukHawbControl)) ||
							(countOfHawbs > 1 && !(UserControl is CcsukAirConsignmentUserControlHawbMany)))
					{
						DiscardCurrentUserControl();
					}
					HawbPluginHelper.RegisterHawbsToShipment();  // Hawbs that we found by NK are now linked by FK for opening from the CCSUK module
					RegisterHandlerForShedLicenceLogin();
					EnsureCcsukMenu();
				}
			}
			return result;
		}

		ZString CompileAdditionalHelpfulWarningInformationForPopup()
		{
			var mawb = MawbPluginHelper.Mawbs[0];
			var extraWarningInfo = CcsukUtilities.ListHawbsOnMawb(mawb);
			if (!extraWarningInfo.IsEmpty)
			{
				extraWarningInfo = string.Format("Existing HAWB records on MAWB {0}:\r\n{1}", Consol.JK_MasterBillNum, extraWarningInfo);
			}
			else
			{
				extraWarningInfo = string.Format("No other HAWB records exist on MAWB {0}, presence {1}, status {2}; it is a basic.\r\nIf this record should remain a basic without HAWBs then answer no. If you proceed, take care that house bill number will match that of your partners.",
													Consol.JK_MasterBillNum,
													mawb.PresenceOnNetworkStatus,
													mawb.CustomsActionCode.IsEmpty ? new ZString("(none)") : mawb.CustomsActionCode);
			}
			return extraWarningInfo;
		}

		void RegisterHandlerForShedLicenceLogin()
		{
			foreach (CusHAWB hawb in HawbPluginHelper.Hawbs)
			{
				hawb.CcsukLicenceLoginHandler += new Customs.Business.LicenceLoginEventHandler(HandleShedLicenceLogin);
			}
		}

		void UnRegisterHandlerForShedLicenceLogin()
		{
			foreach (CusHAWB hawb in HawbPluginHelper.Hawbs)
			{
				hawb.CcsukLicenceLoginHandler -= new Customs.Business.LicenceLoginEventHandler(HandleShedLicenceLogin);
			}
		}

		void HandleShedLicenceLogin(object sender, Customs.Business.LicenceLoginEventArgs e)
		{
			e.LoginHasBeenAttempted = true;
			e.LicenceCheckPoint.Login(this);
		}

		void MakeNewAwb(CusMAWB onMawb)
		{
			HawbPluginHelper.MakeNewHawbAndPurgeCache(onMawb);
			HawbPluginHelper.RegisterHawbsToShipment();
			EnsureCcsukMenu();
		}

		void EnsureCcsukMenu()
		{
			if (TopLevelMenu == null)
			{
				SetupTopLevelMenu();
			}
			if (!Form.Menu.MenuItems.Contains(TopLevelMenu))
			{
				Form.Menu.MenuItems.Add(TopLevelMenu);
			}
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get { return unableToCreateMessage; }
		}

		protected string unableToCreateMessage = ShipmentIsNotConsolidatedText;
		public const string ShipmentIsNotConsolidatedText = "This shipment is not part of a consolidation. Attach it to a consolidation first before using the CCS-UK plugin.";
		public const string ShipmentsConsolHasNoMawbText = "This shipment's Consol does not have an internal link to a CCS-UK MAWB.\r\nFirst open the Consol and press its CCS-UK tab to create this link.";
		public const string MutexFailureText = "Someone else is already in the process of creating a CCS-UK job for this shipment.\r\nYou cannot process this job until they save their data. Please try later.";
		public const string MultipleMawbsExistText = "This shipment's consol has multiple MAWB records but no HAWBs yet exist. No HAWB record will be automatically created.\r\nInstead open the consol's CCSUK tab, ensure child HAWB records exist with the same HAWB number, save and re-open this tab.";
		const string NeedReloadText = "Someone else has modified the CCS-UK job for this shipment - please reload the form.";

		public void CreateHawbfRequiredWhenMakingMawb()
		{
			if (Shipment != null && Consol != null)
			{
				unableToCreateMessage = string.Empty;

				if (MawbPluginHelper.Mawbs.Count == 0)
				{
					unableToCreateMessage = ShipmentsConsolHasNoMawbText;
					return;
				}
				else if (MawbPluginHelper.Mawbs.Count == 1)
				{
					if (HawbPluginHelper.Hawbs.Count == 0)
					{
						if (MutexForShipment.Lock())
						{
							new CusHAWB.Loader(Shipment.Factory).CreateNewOnMawbLinkedToShipment(MawbPluginHelper.Mawbs[0], Shipment);
							hawbPluginHelper = null;
						}
						else
						{
							unableToCreateMessage = MutexFailureText;
						}
					}
				}
				else
				{
					unableToCreateMessage = MultipleMawbsExistText;
				}
			}
		}

		protected override Control GetNewUserControl()
		{
			var numberOfHawbs = HawbPluginHelper.Hawbs.Count;
			if (numberOfHawbs > 1)
			{
				return new CcsukAirConsignmentUserControlHawbMany();
			}
			else
			{
				return new CcsukHawbControl();
			}
		}

		protected override string NameCore
		{
			get { return "CCS-UK"; }
		}

		protected override void OnIsFormEditableChanged()
		{
			base.OnIsFormEditableChanged();
			if (TopLevelMenu != null)
			{
				TopLevelMenu.Enabled = IsFormEditable;
			}
		}

		#endregion

		#region Implementation

		protected override void ChangeTheVisibilityCore()
		{
			Enabled = IsValidShipmentForCcsuk;
		}

		protected override ForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = GetConsolFromShipment();
					if (fConsol != null && HawbPluginHelper != null)
					{
						HawbPluginHelper.SetConsol(fConsol);
					}
				}
				return fConsol;
			}
		}
		ForwardingConsol fConsol;
		internal ForwardingConsol GetConsolFromShipment()
		{
			foreach (ForwardingConsol consol in Shipment.Consols)
			{
				if (consol.JK_RL_NKDischargePort.StartsWith(Core.Constants.CountryCodes.UnitedKingdom))
				{
					return consol;
				}
			}
			return null;
		}

		#endregion

		#region Mutex

		ZGlobalMutex fMutexForShipment;
		public ZGlobalMutex MutexForShipment
		{
			get { return fMutexForShipment ?? (fMutexForShipment = CusHAWB.CreateMutexForShipment(Shipment.PK, Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode)); }
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				UnlockMutexIfNecessary();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnlockMutexIfNecessary();
				Shipment.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
				UnRegisterHandlerForShedLicenceLogin();
			}

			base.Dispose(disposing);
		}

		void UnlockMutexIfNecessary()
		{
			if (MutexForShipment.HasLock)
			{
				MutexForShipment.Unlock();
			}

			if (MutexForConsol != null && MutexForConsol.HasLock)
			{
				MutexForConsol.Unlock();
			}
		}

		#endregion

		public bool IsValidShipmentForCcsuk
		{
			get { return IsValidShipmentForCcsukCore; }
		}

		protected virtual bool IsValidShipmentForCcsukCore
		{
			get { return CcsukUtilities.IsShipmentValidForCcsuk(Shipment, Consol); }
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			var result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes)
			{
				foreach (CusHAWB hawb in HawbPluginHelper.Hawbs)
				{
					result = CcsukAirInventoryForm.SendAutoFrcIfNeeded(result, hawb);
					if (result == ContinueWithSave.No)
					{
						break;
					}
				}
			}
			return result;
		}
	}
}
