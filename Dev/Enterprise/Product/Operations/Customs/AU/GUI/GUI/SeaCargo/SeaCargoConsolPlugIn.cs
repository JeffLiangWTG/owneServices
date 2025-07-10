using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SeaCargoConsolPlugIn : SeaCargoPlugIn
	{
		public SeaCargoConsolPlugIn(ForwardingConsol consol)
			: this(new Declaration.Business.SeaCargo.FreightConsolWrapper(consol))
		{
			Consol = consol;
		}

		public readonly ForwardingConsol Consol;

		public SeaCargoConsolPlugIn(ISeaCargoConsolInfo hostEntity)
			: base(hostEntity.ManifestProvider)
		{
			fSeaCargoInfo = hostEntity;
			ChangeTheVisibility();
			OceanBill = hostEntity.OceanBill;
		}

		public ISeaCargoConsolInfo SeaCargoInfo
		{
			get
			{
				return fSeaCargoInfo;
			}
		}
		readonly ISeaCargoConsolInfo fSeaCargoInfo;

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get
			{
				return Env.Licence.ImportManifest;
			}
		}

		protected override CusSCAHouse CreateNewSeaCargoJobForShipment(CommonShipment shipment)
		{
			if (fSeaCargoInfo != null)
			{
				return fSeaCargoInfo.SeaCargoSynchroniser.GetHouseBill(shipment);
			}

			return null;
		}

		#region IZPlugIn Members

		protected override MenuItem GetNewTopLevelMenu()
		{
			SetExistingOceanBill();
			SeaCargoConsolMenu result = new SeaCargoConsolMenu(this, Manager);
			result.ParentForm = Form;
			return result;
		}

		protected void SetExistingOceanBill()
		{
			var existingOceanBill = SeaCargoInfo?.SeaCargoSynchroniser?.ExistingOceanBill;
			if (existingOceanBill != null)
			{
				OceanBill = existingOceanBill;
			}
		}

		void StartSynchronisingWithConsol()
		{
			if (SeaCargoInfo != null)
			{
				Synchroniser = SeaCargoInfo.SeaCargoSynchroniser;
				_ = Synchroniser.OceanBill; // This ensures house bill synchronising occurs
			}
		}

		CusSCAOceanBillMessageManager manager;
		protected internal virtual CusSCAOceanBillMessageManager Manager
		{
			get
			{
				if (manager == null)
				{
					manager = new CusSCAOceanBillMessageManager(OceanBill);
				}
				return manager;
			}
		}

		protected override IBusiness GetBusinessEntityForPlugIn() => OceanBill;

		public override bool CanDelete => true;

		protected string ModifiedHouseBillNumbers
		{
			get
			{
				string result = "";
				foreach (CusSCAHouse houseBill in ModifiedHouseBills)
				{
					if (result.Length > 0)
					{
						result += ", ";
					}
					result += houseBill.CA_HouseBill;
				}
				return result;
			}
		}

		ArrayList fModifiedHouseBills;
		protected ArrayList ModifiedHouseBills
		{
			get
			{
				if (fModifiedHouseBills == null)
				{
					fModifiedHouseBills = new ArrayList();
				}
				return fModifiedHouseBills;
			}
		}

		protected virtual ContinueWithSave ShowPreSaveDialogsCMR()
		{
			return new SendsMessagesToCustomsGUI().DetermineRequiredMessagesAndSendThem(Manager);
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			ContinueWithSave result = ContinueWithSave.Yes;
			if (OceanBill != null)
			{
				result = OceanBill is CusSCAOceanBill ? ShowPreSaveDialogsCMR() : ContinueWithSave.Yes;
			}
			if (result == ContinueWithSave.Yes)
			{
				result = base.ShowPreSaveDialogsCore();
			}
			return result;
		}

		public CusSCAOceanBill OceanBill
		{
			get
			{
				return fOceanBill;
			}
			set
			{
				fOceanBill = value;
				if (fOceanBill != null)
				{
					StartSynchronisingWithConsol();
				}
				Manager.OceanBill = value;
			}
		}
		CusSCAOceanBill fOceanBill;

		protected override Control GetNewUserControl()
		{
			return new CMRSeaCargoUserControl() { Dock = DockStyle.Fill };
		}

		protected override ZBool HasUserControl =>  true;

		public override string Name => "Sea Cargo";

		#endregion

		#region Constants
		public const string Original = "ORG";
		public const string OriginalWithUnderbond = "OWU";
		public const string Cancellation = "CAN";
		public const string CancelleationMessageSubType = "CAN";
		public const string OriginalMessageSubType = "MOH";
		public const string ReplacementMessageSubType = "MRH";
		public const string UnderbondContainerSelect = "Please select the container or containers on the Sea Cargo Containers Tab that you want to do an underbond on";
		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				SeaCargoInfo.UnlockMutexIfNeeded();
			}

			base.Dispose(disposing);
		}

		#region MenuClickEventHandlers

		public bool LoadHouseBills()
		{
			bool result = true;
			if (SeaCargoInfo.ManifestProvider.HasChanges)
			{
				Globals.Message.ShowWarning("You must save the current record before generating a message");
				result = false;
			}
			else
			{
				if (SeaCargoInfo.SeaCargoSynchroniser != null)
				{
					SeaCargoInfo.SeaCargoSynchroniser.LoadHouseBills();
				}
				OceanBill.HouseBills.Load();
			}
			return result;
		}
		#endregion

		#region Implementation

		public SeaCargoSynchroniser Synchroniser;

		protected override void ChangeTheVisibilityCore()
		{
			if (SeaCargoInfo != null)
			{
				Enabled = SeaCargoInfo.IsVisible;
			}
		}

		#region Record Locking

		protected ZGlobalMutex Mutex
		{
			get
			{
				if (fMutex == null)
				{
					fMutex = SeaCargoInfo.Mutex;
				}
				return fMutex;
			}
		}
		protected ZGlobalMutex fMutex;

		protected override void CreateSeaCargoJobIfRequired()
		{
			CoveringLabelText = string.Empty;
			if (SeaCargoInfo != null)
			{
				if (OceanBill == null && CheckForInvalidSeaCargo())
				{
					if (QueryUserToCreateSeaCargoJob())
					{
						if (FormPreSaved(Consol, Form))
						{
							if (Mutex.Lock())
							{
								try
								{
									var tmpFactory = LoadOrCreateOceanBillInTemporaryFactory();

									try
									{
										tmpFactory.Save();
									}
									catch (Exception ex) when (!ex.IsCriticalException())
									{
										if (ex is not Customs.Business.DuplicatedOceanBillException)
										{
											CoveringLabelText = Res.GetString("1a02d781-f60c-4012-a890-02101a62815b", "An error occurred when creating the sea cargo job.\r\nPlease change to another tab, then click back to this tab to create a sea cargo job for this Consol.");
											ZExceptionReporting.HandleSaveException(ex);
											return;
										}
									}
								}
								finally
								{
									Mutex.Unlock();
								}
								OceanBill = SeaCargoInfo.SeaCargoSynchroniser.OceanBill;
								OceanBill.HasChanges = false;
							}
							else
							{
								CoveringLabelText = Res.GetString("C6AF06CF-8056-4421-87F6-17FC0F92CC58", "Unable to create sea cargo job.  Someone is already in the process of creating this job.");
							}
						}
						else
						{
							CoveringLabelText = Res.GetString("7FE24C66-2362-4501-80F4-A9D71F643050", "You have to save the data first before create a sea cargo job.\r\nPlease change to another tab, then click back to this tab to create a sea cargo job for this Consol.");
						}
					}
					else
					{
						CoveringLabelText = Res.GetString("14143D86-113D-41F8-8599-1229ACBA3398", "You have chosen not to create a sea cargo job now.\r\nPlease change to another tab, then click back to this tab to create a sea cargo job for this Consol.");
					}
				}
			}
		}

		bool CheckForInvalidSeaCargo()
		{
			var loader = new Business.BaseCusSCAOceanBill.Loader(Consol.Factory);
			var legacyOceanBill = loader.LoadFromConsolAndApplicationCode(Consol, new ZString[] { string.Empty, Core.Constants.Customs.CusSCAOceanBillApplicationCodes.AustraliaLegacy });
			if (legacyOceanBill != null)
			{
				CoveringLabelText = Res.GetString("2AF00F72-D1D7-468A-8AAA-76AE4EA5BA4C", "Sea Cargo Data is Invalid.");
				return false;
			}

			return true;
		}

		protected virtual BusinessObjectFactory LoadOrCreateOceanBillInTemporaryFactory()
		{
			var newFactory = new BusinessObjectFactory() { NameForDebugging = "SeaCargoTemporaryFactory" };
			var consolInNewFactory = newFactory.Load<ForwardingConsol>(Consol.PK);
			var synchroniserInNewFactory = new CMRSeaCargoSynchroniser(consolInNewFactory);
			_ = synchroniserInNewFactory.OceanBill;
			return newFactory;
		}

		protected virtual bool QueryUserToCreateSeaCargoJob()
		{
			DialogResult result = DialogResult.Yes;
			if (!Globals.IsTest)
			{
				result = Globals.Message.Show(
					"Do you want to create a Sea Cargo Job at this time?",
					"",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Information);
			}
			return (result == DialogResult.Yes);
		}

		#endregion

		#endregion

	}
}
