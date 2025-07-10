using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukMenu : EDIMenu
	{
		public CcsukMenu(CusAwbDelegateProvider cusAwbDelegateProvider, ZForm parentForm, bool showOnlyOptionsThatApplyToOnlySplitConsignments = false, bool showPleaseSelectExactlyOneRecordWhenNoAwbReturnedInsteadOfPleaseCreate = false)
		{
			SendsMessagesToCustoms = new SendsMessagesToCustomsGUI();
			this.cusAwbDelegateProvider = cusAwbDelegateProvider;
			this.showPleaseSelectExactlyOneRecordWhenNoAwbReturnedInsteadOfPleaseCreate = showPleaseSelectExactlyOneRecordWhenNoAwbReturnedInsteadOfPleaseCreate;
			showOnlyNonSplits = showOnlyOptionsThatApplyToOnlySplitConsignments;
			form = parentForm;
			SetupTopLevelMenu();
		}

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			base.Text = MenuCaption;
		}

		public const string MenuCaption = "CCSUK Messaging";
		public const string DeleteSendFrxMenuText = "Delete consignment (send FRX)";

		void BuildMenu()
		{
			while (MenuItems.Count > 0)
			{
				MenuItems.RemoveAt(0);
			}

			if (form != null && form.DisplayMode == ZArchitecture.Core.ODisplayMode.ReadOnly)
			{
				MenuItems.Add("Job is open in View mode");
				return;
			}

			if (CusAwb != null && CusAwb.HasSplits)
			{
				AddFsrFreightStatusRequestMenuItems();
				// cannot send FRC at whole-awb level, e.g. to check in all pieces.  It would be useful but CCSUK request inhinititing this to cope with a bug they have - see eDocs of WI00047335
				MaybeAddSplitManagementMenu(false);
				MaybeAddDeleteDuplicateSplitsMenu();
				return;
			}

			if (CusAwb != null && CusAwb.ReadOnlyAndPermissionHelper.IsActionAllowed(Actions.CW_Create))
			{
				var transmitFRI = new ZMenuItem("Create consignment record (send FRI)", SendFRI);
				if (!showOnlyNonSplits)
				{
					MenuItems.Add(transmitFRI);
				}
			}

			AddFsrFreightStatusRequestMenuItems();

			if (CusAwb != null && !CusAwb.ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Agent, true) && LicenceAndPimaHelper.IsSimpleAgentProfile(CusAwb))
			{
				var renominate = new ZMenuItem("Send FRN to renominate to...", AskForNewAgentAndRenominateWithCIMFRN);
				MenuItems.Add(renominate);
			}

			if (CusAwb != null
				&& CusAwb.ReadOnlyAndPermissionHelper.IsActionAllowed(Actions.Delete)
				&& CusAwb.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.NotOnCommDb
				&& CusAwb.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.NotOnCommDbDeleted
				)
			{
				var mawb = CusAwb as CusMAWB;
				if (mawb != null && !mawb.IsBasic)
				{
					// consols of houses cannot be FRX'd
				}
				else
				{
					var transmitFRX = new ZMenuItem(DeleteSendFrxMenuText, SendFRX);
					if (!showOnlyNonSplits)
					{
						MenuItems.Add(transmitFRX);
					}
				}
			}

			MenuItem transmitFRC = null;
			if (CusAwb != null && !CusAwb.IsArchivedOnCcsuk)
			{
				transmitFRC = new ZMenuItem("Send amendment request (FRC)", SendFRC);
			}
			else
			{
				transmitFRC = new ZMenuItem("FRC is disabled: job is 'archived' on CCS-UK");
			}
			MenuItems.Add(transmitFRC);

			if (CusAwb != null && CusAwb.ReadOnlyAndPermissionHelper.IsActionAllowed(Actions.CW_CanSplit))
			{
				MaybeAddSplitManagementMenu(showOnlyNonSplits);
			}

			MaybeAddCreateChiefEntryMenu();

			MaybeAddEcSetAndUnsetAndAllReleaseItems();

			AddSendGenralToOtherPartyMenuItem();

			MaybeAddCheckinAllPiecesMenu();

			MaybeAddDeactivateMenuForWtgOnly();
		}

		void MaybeAddCheckinAllPiecesMenu()
		{
			var mawb = CusAwb as CusMAWB;

			if (GBCustomsDataRegistry.Instance.CcsukAllowCheckinAtMawbLevel.Value
				&& mawb != null
				&& mawb.IsInDatabase
				&& !mawb.IsBasic
				&& mawb.Status1Date.IsEmpty
				&& mawb.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.NotOnCommDb
				&& mawb.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.NotOnCommDbDeleted
				&& LicenceAndPimaHelper.IsShedPIMA(mawb.Profile))
			{
				MenuItems.Add(new ZMenuItem("Check in all pieces", CheckinAllPiecesMenu_Click));
			}
		}

		void CheckinAllPiecesMenu_Click(object sender, EventArgs e)
		{
			if (CusAwbHasRedValidationErrors)
			{
				SendsMessagesToCustoms.NotifyUserOfAnInvalidOperation("Please fix the validation errors first");
			}
			else
			{
				ZFormModaliser.ShowDialogAndDispose(new CheckInAllChildPiecesForm(CusAwb));
			}
		}

		void MaybeAddDeactivateMenuForWtgOnly()
		{
			if (CusAwb != null && GlbStaff.CurrentUser.IsSupportUser && GBCustomsDataRegistry.Instance.CcsukTemporarilyAllowDeactivationOfCcsukAwbs.Value)
			{
				MenuItems.Add(new ZMenuItem("Deactivate (WTG only)", WtgDeactivate_Click));
			}
		}

		void WtgDeactivate_Click(object sender, EventArgs e)
		{
			if (Globals.Message.ShowConfirmation("WTG Users may deactivate records for which deletion (local and via FRX) is inhibited according to business rules. It should only be used after the client has logged a ticket asking to remove a record that they have created and/or locked in error. It is not a replacement for the client sending FRX. It cannot be undone and a log will be recorded. Deactivating a master will also deactivate its houses.", "WTG Deactivation", "deactivate", MessageBoxIcon.Question) == DialogResult.OK)
			{
				CusAwb.DeactivateByWtg();
			}
		}

		void AddSendGenralToOtherPartyMenuItem()
		{
			if (CusAwb != null)
			{
				if (LicenceAndPimaHelper.IsFullShed(CusAwb) || LicenceAndPimaHelper.IsFallbackShed(CusAwb))
				{
					MenuItems.Add(new ZMenuItem("Contact agent", ContactOtherPartyClick));
				}
				else if (LicenceAndPimaHelper.IsSimpleAgentProfile(CusAwb))
				{
					MenuItems.Add(new ZMenuItem("Contact shed", ContactOtherPartyClick));
				}
			}
		}

		void MaybeAddEcSetAndUnsetAndAllReleaseItems()
		{
			var helper = CusAwb?.ReadOnlyAndPermissionHelper;
			if (helper != null)
			{
				if (helper.IsActionAllowed(Actions.CW_UnSetEcStatusReleaseFlag))
				{
					MenuItems.Add(new ZMenuItem(UnsetEcStatusReleaseCaption, UnsetEcStatusRelease));
				}
				if (helper.IsActionAllowed(Actions.CW_SetEcStatusReleaseFlag))
				{
					MenuItems.Add(new ZMenuItem(SetEcStatusReleaseCaption, SetEcStatusRelease));
				}
				if (helper.IsActionAllowed(Actions.CW_EcStatusRelease))
				{
					MenuItems.Add(new ZMenuItem(RraMenuTitle, PrintReleaseRemovalAuthority));
				}

				if (helper.IsActionAllowed(Actions.CW_C1Release) && LicenceAndPimaHelper.IsSimpleAgentProfile(CusAwb))
				{
					MenuItems.Add(new ZMenuItem(C1MenuTitle, ReleaseAndPrintC1));
				}
				else if (LicenceAndPimaHelper.IsFullShed(CusAwb) || LicenceAndPimaHelper.IsFallbackShed(CusAwb))
				{
					MenuItems.Add(new ZMenuItem(RraMenuTitle, PrintReleaseRemovalAuthority));
				}
			}
		}

		public const string C1MenuTitle = "Release and print C1";
		public const string RraMenuTitle = "Print Release/Removal Authority";
		public const string SetEcStatusReleaseCaption = "Set 'EC Status Release'";
		public const string UnsetEcStatusReleaseCaption = "Unset 'EC Status Release'";

		void MaybeAddCreateChiefEntryMenu()
		{
			if (CusAwb != null && CusAwb.ReadOnlyAndPermissionHelper.IsActionAllowed(Actions.CW_CanCreateDeclaration))
			{
				var shouldAddMenu = false;
				if (CusAwb is CusHAWB)
				{
					var hawb = CusAwb as CusHAWB;
					shouldAddMenu = !hawb.HasDeclaration && !hawb.IsLinkedToJobShipment;
				}
				else if (CusAwb is SplitBasic)
				{
					var split = CusAwb as SplitBasic;
					shouldAddMenu = !split.HasOwnDeclaration && !split.Basic.IsLinkedToJobConsol;
				}
				else if (CusAwb is SplitHouse)
				{
					var split = CusAwb as SplitHouse;
					shouldAddMenu = !split.HasOwnDeclaration && !split.HAWB.IsLinkedToJobShipment;
				}
				else if (CusAwb is CusMAWB)
				{
					var mawbOrBasic = CusAwb as CusMAWB;
					shouldAddMenu = mawbOrBasic.IsBasic && !mawbOrBasic.MasterLevelHouseHelper.HasDeclaration && !mawbOrBasic.IsLinkedToJobConsol;
				}
				if (shouldAddMenu)
				{
					MenuItems.Add(new ZMenuItem("Create CDS Declaration", CreateCDSDeclaration));
				}
			}
		}

		void MaybeAddSplitManagementMenu(bool showOnlyNonSplits)
		{
			if (CusAwb.ReadOnlyAndPermissionHelper.AwbIsProbablyOnNetwork)
			{
				var performSplit = new ZMenuItem("Request and manage splits", PerformSplit);
				if (!showOnlyNonSplits)
				{
					MenuItems.Add(performSplit);
				}
			}
		}

		void MaybeAddDeleteDuplicateSplitsMenu()
		{
			if (GlbStaff.CurrentUser.GS_IsController && CusAwb.HasDuplicateSplits())
			{
				MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("57784CF8-5699-4DA2-A179-97655E7C021E", "Delete Duplicate Splits"), DeleteDuplicateSplits));
			}
		}

		void AddFsrFreightStatusRequestMenuItems()
		{
			var transmitFSR = new ZMenuItem("Request status update (send FSR)");
			var transmitFsrToCommdb = new ZMenuItem("To community database (CommDB)", SendCukFsrToCommdb);
			var transmitFsrToCommdbAndUpdate = new ZMenuItem("To CommDB and update using response", SendCukFsrToCommdbAndUpdate);
			var transmitFsrToCommdbAndWithoutShed = new ZMenuItem("To CommDB without specifying shed", SendCukFsrToCommdbWithoutShed);
			transmitFsrToShed = new ZMenuItem("To shed", SendCimFsrToShed);
			transmitFSR.MenuItems.Add(transmitFsrToCommdb);
			transmitFSR.MenuItems.Add(transmitFsrToShed);
			transmitFSR.MenuItems.Add(transmitFsrToCommdbAndUpdate);
			transmitFSR.MenuItems.Add(transmitFsrToCommdbAndWithoutShed);
			transmitFSR.MenuItems.Add(new ZMenuItem("Request retransmission of FSN", SendFSRFSN));
			MenuItems.Add(transmitFSR);
		}

		void CreateCDSDeclaration(object sender, EventArgs e)
		{
			if (CusAwb.HasChanges)
			{
				Globals.Message.Show("Please save first");
				return;
			}
			else
			{
				if (!CusAwb.IsThroughAwb || Globals.Message.ShowConfirmation("Through AWBs should not have a CDS Declaration.", "Through-AWB entry warning", "TAWB", MessageBoxIcon.Question, MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					if (Globals.Message.ShowConfirmation(
	@"Do you really want to create a standalone customs declaration (a 'B-job') for this AWB? 
Creating a declaration will lock certain functions/actions on this AWB.  You should 
only do this if you do not intend to later link the AWB to the Forwarding module and
if you do not need to create or add child bills (e.g. splits, houses) for the job.
Note that creating multiple declarations may mean that certain reporting and
costing functions will not run optimally.", "Create CDS Declaration?", "yes", MessageBoxIcon.Question, MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						var declaration = CusAwb.CreateNewStandaloneCDSDeclaration();
						Globals.Message.Show(declaration.HumanReadableName + " was created");
					}
				}
			}
		}

		void ContactOtherPartyClick(object sender, EventArgs e)
		{
			if (CusAwb.HasChanges)
			{
				Globals.Message.Show("Please save first");
			}
			else
			{
				var genral = new NonPersistentGenralEdiMessageForNew(CusAwb.Factory);
				genral.PrepareNewMessageSentFromAwb(CusAwb);
				var manager = new NewGenralMessageManager(genral, CusAwb);
				var form = new CcsukGenralMessageFormForNew(manager);
				ZFormModaliser.ShowDialogAndDispose(form);
			}
		}

		void ReleaseAndPrintC1(object sender, EventArgs e)
		{
			using (var form = new C1ReleaseForm(CusAwb))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		void UnsetEcStatusRelease(object sender, EventArgs e)
		{
			CusAwb.SetEcStatusRelease(false);
		}

		void SetEcStatusRelease(object sender, EventArgs e)
		{
			CusAwb.SetEcStatusRelease(true);
		}

		void PrintReleaseRemovalAuthority(object sender, EventArgs e)
		{
			if (CusAwb.HasChanges)
			{
				SendsMessagesToCustoms.NotifyUserOfAnInvalidOperation("Please save your changes first");
			}
			else
			{
				using (var form = new ErtsReleaseForm(CusAwb))
				{
					ZFormModaliser.ShowDialogWithoutDispose(form);
				}
			}
		}

		void PerformSplit(object sender, EventArgs e)
		{
			if (CusAwbHasRedValidationErrors)
			{
				SendsMessagesToCustoms.NotifyUserOfAnInvalidOperation("Please fix the validation errors first");
			}
			else if (CusAwb.ReasonForNotAllowSplit.IsEmpty)
			{
				var warnShedAboutSplittingPrearrival = CusAwb.IsPrearrival && LicenceAndPimaHelper.IsFullShed(CusAwb);
				if (!warnShedAboutSplittingPrearrival ||
					(Globals.Message.ShowConfirmation("Sheds should not manipulate splits on pre-arrivals. Proceed anyway?", "Pre-arrival split", "yes", MessageBoxIcon.Warning) == DialogResult.OK))
				{
					ZFormModaliser.ShowDialogAndDispose(new NonPersistentSplitCreatorForm(CusAwb));
				}
			}
			else
			{
				// HMRC accreditation rule 5. 
				SendsMessagesToCustoms.NotifyUserOfAnInvalidOperation("Cannot split: " + CusAwb.ReasonForNotAllowSplit);
			}
		}

		bool CusAwbHasRedValidationErrors
		{
			get
			{
				if (CusAwb.Notifications.HasErrors())
				{
					return true;
				}
				else
				{
					CusAwb.RunPreSaveValidation();
					return CusAwb.Notifications.HasErrors();
				}
			}
		}

		void DeleteDuplicateSplits(object sender, EventArgs e)
		{
			var splitsToDelete = CusAwb.FindDuplicateSplits();
			var splitsMessages = splitsToDelete.Select(x => Res.GetString("6E86D37E-FA88-4007-9DEE-77DCA7A5D8EA", "\tSplit {0}, presence {1}, created {2:dd/MM/yyyy HH:mm}", x.SplitReference, x.PresenceOnNetworkStatus, x.LocalCreationDate));
			var message = string.Join("\r\n", splitsMessages
				.Prepend(Res.GetString("7C55A201-209C-401F-AEC6-B3DCB80CEE27", "The following split record(s) are duplicated and will be deleted."))
				.Append(Res.GetString("64BD1C9E-065A-43F1-9BC9-56D6B702FC2F", "Please confirm this action.")));

			if (Globals.Message.ShowConfirmation(message, Res.GetString("B4E611DF-EBB8-42F3-8CA0-8A2CAA7B5C7C", "Delete Duplicate Splits"), Res.GetString("7C47A735-6C1F-4CE3-9190-73108BA829E4", "confirm"), MessageBoxIcon.Question) == DialogResult.OK)
			{
				CusAwb.DeleteSplits(splitsToDelete);
				Globals.Message.Show(ResString.GetMultilingualString("97D78196-B827-4445-8972-5381898CC3EE", "Duplicated splits have been deleted."));
			}
		}

		void SendFRI(object sender, EventArgs e)
		{
			SendToCcsUk(new CcsukTransmissionMessageFunction.CUSCAR.FRI());
		}

		void SendCukFsrToCommdbAndUpdate(object sender, EventArgs e)
		{
			SendToCcsUk(new CcsukTransmissionMessageFunction.CUKFSR.FsaWithUpdate());
		}

		void SendCukFsrToCommdbWithoutShed(object sender, EventArgs e)
		{
			SendToCcsUk(new CcsukTransmissionMessageFunction.CUKFSR.FsaWithoutShed());
		}

		void SendCukFsrToCommdb(object sender, EventArgs e)
		{
			SendToCcsUk(new CcsukTransmissionMessageFunction.CUKFSR.FSA());
		}

		void SendCimFsrToShed(object sender, EventArgs e)
		{
			SendToCcsUk(new CcsukTransmissionMessageFunction.CIM.FSR());
		}

		void SendFSRFSN(object sender, EventArgs e)
		{
			SendToCcsUk(new CcsukTransmissionMessageFunction.CUKFSR.FSN());
		}

		void AskForNewAgentAndRenominateWithCIMFRN(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new RenominationForm(CusAwb));
		}

		void SendFRX(object sender, EventArgs e)
		{
			bool alsoSendFsr = GBCustomsDataRegistry.Instance.CcsukAlsoSendFsrAfterFrx.Value;
			var maybeWarningAboutFsr = alsoSendFsr ? (System.Environment.NewLine + "Note that an FSR query will automatically be sent too.") : "";
			if (Globals.Message.ShowConfirmation("Are you sure you want to delete the consignment?" + maybeWarningAboutFsr, "FRX", "yes", MessageBoxIcon.Question) == DialogResult.OK)
			{
				var sentFrxOk = SendToCcsUk(new CcsukTransmissionMessageFunction.CUSCAR.FRX());
				if (sentFrxOk && alsoSendFsr)
				{
					SendToCcsUk(new CcsukTransmissionMessageFunction.CUKFSR.FSA());
				}
			}
		}

		void SendFRC(object sender, EventArgs e)
		{
			SendToCcsUk(new CcsukTransmissionMessageFunction.CUSCAR.FRC());
		}

		bool SendToCcsUk(CcsukTransmissionMessageFunction how)
		{
			CcsukInventoryICusAwbMessageSender sender = new CcsukInventoryICusAwbMessageSender();
			sender.Send(CusAwb, SendsMessagesToCustoms, how);
			return sender.CanSend;
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			if (CusAwb != null)
			{
				BuildMenu();
				if (transmitFsrToShed != null)
				{
					transmitFsrToShed.Text = "To shed " + CusAwb.CargoTerminalOperator;
				}
			}
			else
			{
				while (MenuItems.Count > 0)
				{
					MenuItems.RemoveAt(0);
				}
				var message = showPleaseSelectExactlyOneRecordWhenNoAwbReturnedInsteadOfPleaseCreate ? "Please select exactly one row" : "Please first create a CCS-UK consignment by clicking on the CCS-UK tab";
				MenuItems.Add(new ZMenuItem(message + " and/or save your changes first"));
			}
		}

		ICcsukCusAwb CusAwb
		{
			get
			{
				if (cusAwbDelegateProvider != null)
				{
					return cusAwbDelegateProvider.Awb;
				}
				return null;
			}
		}

		internal Customs.Business.ISendsMessagesToCustoms SendsMessagesToCustoms;
		internal MenuItem transmitFsrToShed;
		readonly bool showOnlyNonSplits;
		readonly CusAwbDelegateProvider cusAwbDelegateProvider;
		protected override bool DisplayGenerateEntriesMenuOption { get { return false; } }
		readonly ZForm form;
		readonly bool showPleaseSelectExactlyOneRecordWhenNoAwbReturnedInsteadOfPleaseCreate;
	}
}
