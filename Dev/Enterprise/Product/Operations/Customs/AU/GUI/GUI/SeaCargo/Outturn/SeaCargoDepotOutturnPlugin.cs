using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SeaCargoDepotOutturnPlugin : ZAlwaysLoadPlugIn, IFindOrCreateOutturnUI
	{
		public SeaCargoDepotOutturnPlugin(TallyContainer hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			if (hostBusinessEntity == null)
			{
				throw new ArgumentNullException(nameof(hostBusinessEntity));
			}

			this.container = hostBusinessEntity;
			this.wrapper = CFSTallyContainerWrapper.Load(container);
			this.outturn = wrapper.Outturn;

			SetupOutturn();
		}

		public override string Name
		{
			get { return "Sea Cargo Outturn"; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.SeaCargoDepot; }
		}

		protected override ZBool IsActive
		{
			get { return outturn != null; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return outturn != null ? outturn.Header : null;
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return outturn != null;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			bool getAccess = UserAccessToOutturn();

			if (MutexForOutturn.HasLock || getAccess)
			{
				if (outturn == null)
				{
					outturn = wrapper.GetOutturnForTallyPlugin(this);
				}

				SetupOutturn();

				if (outturn == null)
				{
					UnlockMutexIfNecessary();
				}
			}
			else
			{
				fUnableToCreateMessage = "Someone else is already in the process of creating sea cargo outturn for shipment.\r\nYou cannot process until they save their data. Please try later.";
			}

			return outturn != null;
		}

		bool UserAccessToOutturn()
		{
			if (!MutexForOutturn.IsLocked && MutexForOutturn.Lock())
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get
			{
				if (!MutexForOutturn.IsLocked)
				{
					return base.PlugInNotDisplayedMessage;
				}
				else
				{
					return fUnableToCreateMessage;
				}
			}
		}
		ZString fUnableToCreateMessage;

		public bool IsCurrentUserEnter
		{
			get { return fIsCurrentUserEnter; }
			set { fIsCurrentUserEnter = value; }
		}
		bool fIsCurrentUserEnter;

		public ZGlobalMutex MutexForOutturn
		{
			get
			{
				if (fMutexForOutturn == null)
				{
					fMutexForOutturn = new ZGlobalMutex(SeaCargoOutturnMutexID.Instance, container.PK.ToString() + GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString());
				}

				return fMutexForOutturn;
			}
		}
		ZGlobalMutex fMutexForOutturn;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnlockMutexIfNecessary();
			}

			base.Dispose(disposing);
		}

		protected void UnlockMutexIfNecessary()
		{
			if (MutexForOutturn != null && MutexForOutturn.HasLock)
			{
				MutexForOutturn.Unlock();
			}
		}

		protected override Control GetNewUserControl()
		{
			return new SeaCargoDepotOutturnPluginUserControl();
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			ContinueWithSave result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes && outturn != null && !outturn.IsDeleted && outturn.Header != null)
			{
				Business.IMessageManager messageManager = ((Business.IMessageManageableBizObj)outturn.Header).GetMessageManagerForAmendmentDetection();
				if (messageManager != null)
				{
					result = new SeaCargoOutturnSendMessagesToCustoms().DetermineRequiredMessagesAndSendThem(messageManager);
				}
			}

			return result;
		}

		protected override bool RegisterPlugInBusinessEntityAsEditable
		{
			get { return true; }
		}

		void SetupOutturn()
		{
			if (outturn != null && outturn.IsDeleted)
			{
				outturn = null;
			}

			if (outturn != null)
			{
				((IOutturnLinkable)container).SetOutturnLink(new TallyCustomsListener(outturn));
			}
		}

		internal TallyOutturn outturn;
		readonly TallyContainer container;
		internal readonly CFSTallyContainerWrapper wrapper;

		#region IFindOrCreateOutturnUI Members

		bool IFindOrCreateOutturnUI.ShouldCreateAndLinkWhenNoMatchesFound()
		{
			return Ask("No existing outturns have been found.\r\nDo you wish to create a new outturn and link it to this container?");
		}

		bool IFindOrCreateOutturnUI.ShouldCreateAndLinkWhenOnlyHeaderFound()
		{
			return Ask("No existing outturn lines have been found, but a matching outturn header has been found.\r\nDo you wish to create a new outturn line under this header and link it to this container?");
		}

		bool IFindOrCreateOutturnUI.ShouldLinkWhenOutturnFound()
		{
			return Ask("No outturn is linked to this container, but a matching outturn line has been found.\r\nDo you wish to link this outturn to this container?");
		}

		void IFindOrCreateOutturnUI.ShowMultipleMatchesError()
		{
			Globals.Message.ShowError("No outturn is linked to this container, but multiple possible matches were found so automatic matching cannot work.\r\nPlease manually attach an outturn line to this container.");
		}

		void IFindOrCreateOutturnUI.ShowNoConsolError()
		{
			Globals.Message.ShowError("No outturn is linked to this container, and automatic matching cannot be attempted because there is no consol attached.\r\nPlease attach a consol to this container if you want to attempt automatic matching.");
		}

		void IFindOrCreateOutturnUI.ShowOutturnAlreadyLinked()
		{
			Globals.Message.ShowError("No outturn is linked to this container, an automatic match was attempted but the outturn that was found is already linked to another container.\r\nPlease reopen form or manually attach or reattach the outturn lines.");
		}

		void IFindOrCreateOutturnUI.ShowShipmentOutturnsAlreadyLinked(int countOfAlreadyLinked)
		{
			Globals.Message.ShowWarning(string.Format("{0} of the shipment outturn lines were found to be already linked against other shipment, and weren't attached.\r\nPlease manually reattach these outturn lines.", countOfAlreadyLinked));
		}

		bool Ask(string question)
		{
			return Globals.Message.Show(question, dialogCaption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
		}

		const string dialogCaption = "Sea Cargo Outturn";

		#endregion

	}
}
