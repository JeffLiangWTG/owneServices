using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public class CcsukConsolMultiMawbPlugin : CcsukPlugin
	{
		public CcsukConsolMultiMawbPlugin(ForwardingConsol hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			// None if this base stuff is needed - OnSaving does it
		}

		protected override ForwardingConsol Consol
		{
			get { return (ForwardingConsol)ManifestProvider; }
		}

		void CreateCcsukHouseConsignmentsIfRequired()
		{
			if (Consol != null && PluginHelper.Mawbs.Count > 0)
			{
				var listOfHawbsToMake = new ZStringBuilder();
				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					if (!new ShipmentToManyHawbsPluginHelper(shipment, Consol).Hawbs.OfType<CusHAWB>().Any(x => !x.IsSpent))
					{
						listOfHawbsToMake.Append(string.Format("	{0} for shipment {1}", CcsukUtilities.LeftPadWithZeros(shipment.JS_HouseBill), shipment.JS_UniqueConsignRef));
					}
				}
				if (listOfHawbsToMake.Length > 0)
				{
					if (PluginHelper.Mawbs.Count > 0)
					{
						if (Env.Security.AirCcsukHouse.IsAllowed)
						{
							var dialogResult = DialogResult.Abort;
							var shipmentToHawbMatcherHeader = new NonPersistentShipmentToHawbMatcherHeader(Consol, PluginHelper.Mawbs.OfType<CusMAWB>().ToArray(), Consol.Shipments.OfType<ForwardingShipment>());
							using (var form = new CcsukShipmentAndHawbLinkerForm(shipmentToHawbMatcherHeader))
							{
								dialogResult = ZFormModaliser.ShowDialogAndDispose(form);
							}

							if (dialogResult == DialogResult.None)
							{
								var warningMessage = CompileHelpfulWarningInformationForPopup(listOfHawbsToMake);
								var result = Globals.Message.ShowConfirmation(warningMessage, TabName, Consol.JK_MasterBillNum, MessageBoxIcon.Information);
								if (result == DialogResult.OK)
								{
									foreach (ForwardingShipment shipment in Consol.Shipments)
									{
										using (var shipmentPlugin = new CcsukShipmentMultiHawbPlugin(shipment))
										{
											shipmentPlugin.CreateHawbfRequiredWhenMakingMawb();
										}
									}
								}
							}
						}
						else
						{
							Globals.Message.ShowWarning(Env.Security.AirCcsukHouse.ErrorMessageForNotAllowed, "No security right - HAWBs not created/linked");
						}
					}
				}
			}
		}

		string CompileHelpfulWarningInformationForPopup(ZStringBuilder listOfHawbsToMake)
		{
			var warningMessage = "No local HAWBs exist for these shipments. Create them?\r\n\r\n" + listOfHawbsToMake.ToStringWithNewLineBetweenAppends();
			var listOfExistingHawbs = CcsukUtilities.ListHawbsOnMawb(PluginHelper.Mawbs[0]);
			if (!listOfExistingHawbs.IsEmpty)
			{
				warningMessage = warningMessage + "\r\n\r\nThese HAWBs already exist:\r\n\r\n" + listOfExistingHawbs;
			}
			return warningMessage;
		}

		bool haveAlreadyCreatedHouses;
		public override void OnGUIShown()
		{
			if (GBCustomsDataRegistry.Instance.CcsukAutoPopulateHawbsOnConsol.Value && !haveAlreadyCreatedHouses && !Consol.IsDirect)
			{
				CreateCcsukHouseConsignmentsIfRequired();
				haveAlreadyCreatedHouses = true;
			}
			base.OnGUIShown();
		}

		#region IZPlugIn Members

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return Consol.IsValidForCcsukForPlugin();
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			var result = false;
			if (Consol != null)
			{
				var countOfMawbs = PluginHelper.ResetReloadAndCount();
				if (countOfMawbs == 0)
				{
					if (AskUserWhetherToCreateNewCcsukRecord(Consol.JK_MasterBillNum, string.Empty))
					{
						if (MutexForConsol != null && MutexForConsol.Lock())
						{
							MakeNewAwb();
							RegisterHandlerForShedLicenceLogin();
							result = true;
						}
						else
						{
							plugInNotDisplayedMessage = MutexFailureText;
						}
					}
					else
					{
						plugInNotDisplayedMessage = YouHaveChosenNotToCreateARecordNowText;
					}
				}
				else
				{
					EnsureCcsukMenu();
					PluginHelper.RegisterChildMawbsToConsol();
					RegisterHandlerForShedLicenceLogin();
					result = true;
				}
			}
			return result;
		}

		void RegisterHandlerForShedLicenceLogin()
		{
			foreach (CusMAWB mawb in PluginHelper.Mawbs)
			{
				mawb.MasterLevelHouseHelper.CcsukLicenceLoginHandler += new Customs.Business.LicenceLoginEventHandler(HandleShedLicenceLogin);
			}
		}

		void UnRegisterHandlerForShedLicenceLogin()
		{
			foreach (CusMAWB mawb in PluginHelper.Mawbs)
			{
				mawb.MasterLevelHouseHelper.CcsukLicenceLoginHandler -= new Customs.Business.LicenceLoginEventHandler(HandleShedLicenceLogin);
			}
		}

		void HandleShedLicenceLogin(object sender, Customs.Business.LicenceLoginEventArgs e)
		{
			e.LoginHasBeenAttempted = true;
			e.LicenceCheckPoint.Login(this);
		}

		void MakeNewAwb()
		{
			PluginHelper.MakeNewMawbAndPurgeCache();
			PluginHelper.RegisterChildMawbsToConsol();
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

		internal static bool AskUserWhetherToCreateNewCcsukRecord(string mainJobNumber, string extraWarningInfo)
		{
			var result = Globals.Message.ShowConfirmation(string.Format(WouldYouLikeToCreateANewRecordText, mainJobNumber, extraWarningInfo), TabName, mainJobNumber, MessageBoxIcon.Information);
			return (result == DialogResult.OK || result == DialogResult.Yes);
		}

		const string MutexFailureText = "Someone else is already in the process of creating a CCS-UK job for this consol.\r\nYou cannot process this job until they save their data. Please try later.";
		const string WouldYouLikeToCreateANewRecordText = "No local CCS-UK inventory record exists for air waybill {0}.  Would you like to create a new one?\r\n{1}";
		public const string YouHaveChosenNotToCreateARecordNowText = "You have chosen not to create a new CCSUK record now";
		const string NeedReloadText = "Someone else has modified the CCS-UK job for this consol - please reload the form.";

		ZString plugInNotDisplayedMessage = MutexFailureText;
		public override ZString PlugInNotDisplayedMessage
		{
			get { return plugInNotDisplayedMessage; }
		}

		protected override string NameCore
		{
			get { return TabName; }
		}
		const string TabName = "CCS-UK";

		protected override void SetupTopLevelMenu()
		{
			SetUpMenu(PluginHelper.Mawbs, Form, ref mainMenuItem);
		}

		internal static void SetUpMenu<T>(BusinessObjectCollection<T> collection, ZForm form, ref MenuItem mainMenuItem) where T : BusinessObject, ICcsukCusAwb
		{
			if (collection.Count == 1)
			{
				mainMenuItem = new CcsukMenu(new CusAwbDelegateProvider(delegate
				{ return collection[0]; }), form);
			}
			else if (collection.Count > 1)
			{
				mainMenuItem = new ZMenuItem("CCSUK Messaging");
				foreach (ICcsukCusAwb awb in collection)
				{
					var localCopyOfAwb = awb;
					var cukMenu = new CcsukMenu(new CusAwbDelegateProvider(delegate
					{ return localCopyOfAwb; }), form);
					cukMenu.Text = string.Format("{0} @ {1}{2} {3}", localCopyOfAwb.ReferenceNumber, localCopyOfAwb.CargoTerminalOperatorAirport, localCopyOfAwb.CargoTerminalOperator, localCopyOfAwb.CustomsActionCode);
					mainMenuItem.MenuItems.Add(cukMenu);
				}
			}
		}

		public override bool CanDelete
		{
			get { return false; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			if (PluginHelper.Mawbs.Count >= 1)
			{
				return PluginHelper;
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

		ConsolToManyMawbsPluginHelper pluginHelper;
		public ConsolToManyMawbsPluginHelper PluginHelper
		{
			get { return pluginHelper ?? (pluginHelper = new ConsolToManyMawbsPluginHelper(Consol)); }
		}

		protected override void OnIsFormEditableChanged()
		{
			base.OnIsFormEditableChanged();
			if (TopLevelMenu != null)
			{
				TopLevelMenu.Enabled = IsFormEditable;
			}
		}

		protected override Control GetNewUserControl()
		{
			return new CcsukAirConsignmentUserControlMawbMany();
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnlockMutexIfNecessary();
				UnRegisterHandlerForShedLicenceLogin();
			}

			base.Dispose(disposing);
		}

		protected void UnlockMutexIfNecessary()
		{
			if (MutexForConsol != null && MutexForConsol.HasLock)
			{
				MutexForConsol.Unlock();
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (Enabled)
			{
				foreach (CusMAWB m in PluginHelper.Mawbs)
				{
					m.SynchroniseData();
				}
			}
		}

		protected override void ChangeTheVisibilityCore()
		{
			Enabled = Consol.IsValidForCcsukForPlugin();
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			var result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes)
			{
				foreach (CusMAWB mawb in PluginHelper.Mawbs)
				{
					result = CcsukAirInventoryForm.SendAutoFrcIfNeeded(result, mawb);
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
