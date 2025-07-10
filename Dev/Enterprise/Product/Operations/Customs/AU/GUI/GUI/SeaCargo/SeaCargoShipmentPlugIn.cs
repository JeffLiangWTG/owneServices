using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Customs.AU.Declaration.GUI.Res;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SeaCargoShipmentPlugIn : SeaCargoPlugIn
	{
		public SeaCargoShipmentPlugIn(ForwardingShipment shipment)
			: this(new ShipmentWrapper(shipment))
		{
			this.shipment = shipment;
		}
		readonly ForwardingShipment shipment;

		public SeaCargoShipmentPlugIn(ISeaCargoShipmentInfo hostEntity)
			: base(hostEntity.ManifestProvider)
		{
			seaCargoShipmentInfo = hostEntity;
			HouseBill = hostEntity.HouseBill;
			ChangeTheVisibility();
			StartSynchronising();
		}

		public ISeaCargoShipmentInfo SeaCargoShipmentInfo => seaCargoShipmentInfo;
		ISeaCargoShipmentInfo seaCargoShipmentInfo;

		public override string Name => "Sea Cargo";

		protected override MenuItem GetNewTopLevelMenu()
		{
			return new SeaCargoShipmentMenu(this, Manager);
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return HouseBill;
		}

		protected override bool RegisterPlugInBusinessEntityAsEditable
		{
			get
			{
				bool result = SeaCargoShipmentInfo.RegisterTopLevelBusinessObjectAsEditable;
				if (result)
				{
					result = base.RegisterPlugInBusinessEntityAsEditable;
				}
				return result;
			}
		}

		public override bool CanDelete => true;

		protected override Control GetNewUserControl()
		{
			fUserControl = new SeaCargoDeclarationUserControl();
			return fUserControl;
		}

		protected override ZBool HasUserControl
		{
			get
			{
				return true;
			}
		}

		protected override CusSCAHouse CreateNewSeaCargoJobForShipment(CommonShipment shipment)
		{
			if (seaCargoShipmentInfo != null)
			{
				return seaCargoShipmentInfo.SeaCargoSynchroniser.GetHouseBill(shipment);
			}

			return null;
		}

		protected const int MinimumTimeMessageMustBeDisplay = 500;

		protected virtual ContinueWithSave ShowPreSaveDialogs_CMR()
		{
			Manager.HouseBill = HouseBill;
			return new SendsMessagesToCustomsGUI().DetermineRequiredMessagesAndSendThem(Manager);
		}

		CusSCAHouseMessageManager fManager;
		internal CusSCAHouseMessageManager Manager
		{
			get
			{
				if (fManager == null)
				{
					fManager = new CusSCAHouseMessageManager(HouseBill);
				}
				return fManager;
			}
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			ContinueWithSave result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes && HouseBill != null && !HouseBill.IsDeleted && HouseBill is CusSCAHouse)
			{
				result = ShowPreSaveDialogs_CMR();
			}
			return result;
		}

		protected override void ChangeTheVisibilityCore()
		{
			if (SeaCargoShipmentInfo != null)
			{
				Enabled = SeaCargoShipmentInfo.IsVisible;
			}
		}

		public void SaveFactory()
		{
			try
			{
				HouseBill.Factory.Save();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(e);
			}
		}

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.ImportManifest;

		#region Implementation

		SeaCargoDeclarationUserControl fUserControl;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				SeaCargoShipmentInfo.UnlockMutexIfNeeded();
			}

			base.Dispose(disposing);
		}

		public CusSCAHouse HouseBill
		{
			get
			{
				if (fHouseBill == null)
				{
					fHouseBill = SeaCargoShipmentInfo.HouseBill;
					if (fHouseBill != null)
					{
						fHouseBill.Deleted += ResetPluginEntityAfterDeletion;
					}
				}
				return fHouseBill;
			}
			set
			{
				if (fHouseBill != null)
				{
					fHouseBill.Deleted -= ResetPluginEntityAfterDeletion;
				}

				fHouseBill = value;

				if (fHouseBill != null)
				{
					fHouseBill.Deleted += ResetPluginEntityAfterDeletion;
				}

				StartSynchronising();
				Manager.HouseBill = HouseBill;
			}
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return fHouseBill != null;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			CreateSeaCargoJobIfRequired();
			return CoveringLabelText.IsEmpty;
		}

		void ResetPluginEntityAfterDeletion(object sender, EventArgs e)
		{
			if (fHouseBill != null)
			{
				fHouseBill.Deleted -= ResetPluginEntityAfterDeletion;
			}
			fHouseBill = (sender as BusinessObject).Factory.New<CusSCAHouse>();
			fHouseBill.HasChanges = false;
			SetPluginBindings();
			// Set IsNull to true so that this BO will not be saved. It has to be after the binding otherwise binding is ineffective
			fHouseBill.IsNull = true;
			fPlaceholderHouseBill = fHouseBill;
			// The wrapper is polluted with the deleted object. A new one needs to be instantiated.
			seaCargoShipmentInfo?.UnlockMutexIfNeeded();
			seaCargoShipmentInfo = new ShipmentWrapper(shipment);
			// notifiy the user about the deletion to allow entity recreation or invalidation of plugin
			SynchroniseIfTabPageVisible();
		}

		// bind current entity to plugin controls. the cached value has to be cleared in a particular order first
		void SetPluginBindings()
		{
			// 1. the cached entity held by plugin base is cleared
			ResetBusinessEntityToNull();
			// 2. make the plugin tab bindable again
			TabPage?.ResetBinding();
			// 3. make the user control bindable again
			fUserControl?.SetDataBinding(null, "");
			// 4. bind the current
			TabPage?.Bind();
		}

		IBusiness fPlaceholderHouseBill;

		void StartSynchronising()
		{
			if (OceanBill != null)
			{
				SeaCargoShipmentInfo.StartSynchronising();
			}
		}

		CusSCAHouse fHouseBill;

		public CusSCAOceanBill OceanBill
		{
			get
			{
				if (fOceanBill == null)
				{
					fOceanBill = SeaCargoShipmentInfo.OceanBill;
				}
				return fOceanBill;
			}
		}
		CusSCAOceanBill fOceanBill;

		#endregion

		protected override void CreateSeaCargoJobIfRequired()
		{
			CoveringLabelText = ZString.Empty;
			if (HouseBill == null && CheckForInvalidSeaCargo())
			{
				if (QueryUserToCreateSeaCargoJob())
				{
					if (FormPreSaved(shipment, Form))
					{
						if (SeaCargoShipmentInfo.Mutex?.Lock() ?? false)
						{
							try
							{
								var newFactory = LoadOrCreateHouseBillInTemporaryFactory();
								try
								{
									newFactory.Save();
								}
								catch (Exception ex) when (!ex.IsCriticalException())
								{
									if (ex is not DuplicateWaitObjectException && ex is not DuplicatedOceanBillException)
									{
										CoveringLabelText = Res.GetString("5b718a72-a3ce-4567-b3c3-d624cc2a1077", "An error occurred when creating the sea cargo job.\r\nPlease change to another tab, then click back to this tab to create a sea cargo job for this Shipment.");
										ZExceptionReporting.HandleSaveException(ex);
										return;
									}
								}
							}
							finally
							{
								SeaCargoShipmentInfo.Mutex.Unlock();
							}
							HouseBill = SeaCargoShipmentInfo.GetHouseBill;  //SeaCargoSynchroniser.GetHouseBill(Shipment);
							HouseBill.HasChanges = false;
							SetPluginBindings();
							RegisterPlugInAsEditable();
							// if a placeholder is present, delete it
							if (fPlaceholderHouseBill is CusSCAHouse houseBillToDelete)
							{
								// IsNull needs to be cleared, otherwise deleting handling complains
								houseBillToDelete.IsNull = false;
								houseBillToDelete.Delete();
							}
						}
						else
						{
							CoveringLabelText = Res.GetString("4EBFDDE4-3AFB-4065-B642-98A40E4A717A", "Unable to create sea cargo job.  Someone is already in the process of creating this job, or there is no arrival consol for this job.");
						}
					}
					else
					{
						CoveringLabelText = Res.GetString("A03464AB-F5E5-4307-A63A-F2B53FFC7B35", "You have to save the data first before create a sea cargo job.\r\nPlease change to another tab, then click back to this tab to create a sea cargo job for this Consol.");
					}
				}
				else
				{
					CoveringLabelText = Res.GetString("5850FDDE-E75A-415E-BFD9-5295E0F9282C", "You have chosen not to create a sea cargo job now.\r\nPlease change to another tab, then click back to this tab to create a sea cargo job for this Shipment.");
				}
			}
		}

		bool CheckForInvalidSeaCargo()
		{
			var loader = new BaseCusSCAHouse.Loader(shipment.Factory);
			var legacyHouseBill = loader.LoadFromShipmentAndApplicationCode(shipment.PK, new ZString[] { string.Empty, Core.Constants.Customs.CusSCAOceanBillApplicationCodes.AustraliaLegacy });
			if (legacyHouseBill != null)
			{
				CoveringLabelText = Res.GetString("6DD3F9D0-1438-4C36-B2C6-E340DF934185", "Sea Cargo Data is Invalid.");
				return false;
			}

			return true;
		}

		protected virtual bool QueryUserToCreateSeaCargoJob()
		{
			DialogResult result = DialogResult.Yes;
			if (SeaCargoShipmentInfo.Mutex != null)
			{
				result = Globals.Message.Show(
					"Do you want to create a Sea Cargo Job at this time?",
					"",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Information);
			}
			return (result == DialogResult.Yes);
		}

		public override void OnSaveCompletedOrAborted(bool saved)
		{
			base.OnSaveCompletedOrAborted(saved);
			if (!saved && HouseBill != null)
			{
				HouseBill.DeleteAnyNewMessages();
			}
		}

		protected virtual BusinessObjectFactory LoadOrCreateHouseBillInTemporaryFactory()
		{
			var synchroniser = SeaCargoShipmentInfo.SeaCargoSynchroniser;
			var newFactory = new BusinessObjectFactory() { NameForDebugging = "SeaCargoTemporaryFactory" };
			var consolInNewFactory = newFactory.Load<ForwardingConsol>(synchroniser.Consol.PK);
			var synchroniserInNewFactory = new CMRSeaCargoSynchroniser(consolInNewFactory, synchroniseConsol: false);
			synchroniserInNewFactory.GetHouseBill(shipment);
			return newFactory;
		}
	}
}
