using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoDepotOutturnForm : CMRMessagingForm
	{
		public SeaCargoDepotOutturnForm()
		{
			InitializeComponent();
			messageUserControl.ReplaceBindingMessagesCollectionWithAnotherCollection("SeaCargoOutturnMessages");
		}

		public SeaCargoDepotOutturnForm(CusOutturnHeader parent)
			: base(parent)
		{
			InitializeComponent();
			InitializeOutturnGridMenu();
			workflowTabPage.Initialize(parent);
			parent.RescindMessageReceived += ShowMessageWhenRescind;
			messageUserControl.ReplaceBindingMessagesCollectionWithAnotherCollection("SeaCargoOutturnMessages");
			OutturnHeader = parent;
		}

		public CusOutturnHeader OutturnHeader
		{
			get { return fOutturnHeader; }
			set { fOutturnHeader = value; }
		}
		CusOutturnHeader fOutturnHeader;

		public override string FormHeading => OutturnHeader?.HumanReadableName ?? base.FormHeading;

		void ShowMessageWhenRescind(object sender, RescindMessageEventArgs e)
		{
			Globals.Message.ShowWarning(e.MessageText);
		}

		protected override MenuItem GetMessagingMenu()
		{
			return new SeaCargoDepotOutturnMessagingMenu((CusOutturnHeaderMessageManager)Manager);
		}

		protected override Business.MultiMessageManager GetManager()
		{
			return new CusOutturnHeaderMessageManager(Header);
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes && Manager != null)
			{
				result = GetOutturnMessagingActionsController().DetermineRequiredMessagesAndSendThem(Manager);
			}
			return result;
		}

		protected SeaCargoOutturnSendMessagesToCustoms GetOutturnMessagingActionsController()
		{
			return new SeaCargoOutturnSendMessagesToCustoms();
		}

		CusOutturnHeader Header
		{
			get { return BusinessEntity as CusOutturnHeader; }
		}

		#region Outturn Details Selection

		public void DeselectAllOutturns()
		{
			if (seaCargoDepotOutturnUserControl.OutturnsGrid.ListManager != null)
			{
				for (int i = 0; i < seaCargoDepotOutturnUserControl.OutturnsGrid.ListManager.List.Count; i++)
				{
					seaCargoDepotOutturnUserControl.OutturnsGrid.UnSelect(i);
				}
			}
		}

		public void SelectOutturn(DepotCusOutturn outturn)
		{
			if (seaCargoDepotOutturnUserControl.OutturnsGrid.ListManager != null)
			{
				for (int i = 0; i < seaCargoDepotOutturnUserControl.OutturnsGrid.ListManager.List.Count; i++)
				{
					if (((DepotCusOutturn)seaCargoDepotOutturnUserControl.OutturnsGrid.ListManager.List[i]).PK == outturn.PK)
					{
						seaCargoDepotOutturnUserControl.OutturnsGrid.CurrentRowIndex = i;
						seaCargoDepotOutturnUserControl.OutturnsGrid.Select(i);
					}
				}
			}
		}

		public void AddSelectedOutturn(DepotCusOutturn outturn)
		{
			ArrayList selectedElements = new ArrayList(seaCargoDepotOutturnUserControl.OutturnsGrid.SelectedElements);
			selectedElements.Add(outturn);
			for (int i = 0; i < seaCargoDepotOutturnUserControl.OutturnsGrid.ListManager.List.Count; i++)
			{
				if (((DepotCusOutturn)seaCargoDepotOutturnUserControl.OutturnsGrid.ListManager.List[i]).PK == outturn.PK)
				{
					seaCargoDepotOutturnUserControl.OutturnsGrid.Select(i);
				}
			}
		}

		#endregion

		#region Load List from Outturn Lines

		void InitializeOutturnGridMenu()
		{
			Menu outturnMenu = seaCargoDepotOutturnUserControl.OutturnsGrid.ContextMenu;
			outturnMenu.MenuItems.Add("-");
			MenuItem createLoadListMenuItem = new ZMenuItem("Create new Load List", new EventHandler(CreateLoadListFromOutturn));
			outturnMenu.MenuItems.Add(createLoadListMenuItem);
			MenuItem attachToLoadListMenuItem = new ZMenuItem("Attach to Load List", new EventHandler(AttachToLoadListFromOutturn));
			outturnMenu.MenuItems.Add(attachToLoadListMenuItem);
			MenuItem detachFromLoadListMenuItem = new ZMenuItem("Detach from Load List", new EventHandler(DetachOutturnFromLoadList));
			outturnMenu.MenuItems.Add(detachFromLoadListMenuItem);
			MenuItem sendSEQMessageMenuItem = new ZMenuItem("Send Establishment Information Query Request to Customs", new EventHandler(SendSEQMessage));
			outturnMenu.MenuItems.Add(sendSEQMessageMenuItem);
		}

		bool CheckSelectionValid()
		{
			if (Header.HasChanges)
			{
				Globals.Message.ShowError("Please save the form before using this feature");
				return false;
			}
			if (seaCargoDepotOutturnUserControl.OutturnsGrid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowError("Please select at least one line to create a consol");
				return false;
			}
			ZString[] oceanBillNums = SelectedElementsOceanBillNumbers();
			if (oceanBillNums.Length > 1)
			{
				Globals.Message.ShowError("Multiple Oceanbills currently Selected.  Please only select one ocean Bill");
				return false;
			}
			foreach (DepotCusOutturn outturn in seaCargoDepotOutturnUserControl.OutturnsGrid.SelectedElements)
			{
				if (!outturn.C5_ParentID.IsEmpty && (outturn.C5_ParentTableCode == JobContainerSchema.Constants.Prefix || outturn.C5_ParentTableCode == JobShipmentSchema.Constants.Prefix))
				{
					Globals.Message.ShowError("Outturn for Container: " + outturn.C5_ContainerNumber + " House Bill: " + outturn.C5_HouseBill + " is already attached to a load list");
					return false;
				}
			}
			return true;
		}

		internal void CreateLoadListFromOutturn(object sender, EventArgs e)
		{
			if (CheckSelectionValid())
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				CreateOrAttachLoadListFromOutturn(factory, null);
			}
		}

		void AttachToLoadListFromOutturn(object sender, EventArgs e)
		{
			if (CheckSelectionValid())
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				CFSLoadListConsol consol = SelectConsol(factory);
				if (consol != null)
				{
					CreateOrAttachLoadListFromOutturn(factory, consol);
				}
			}
		}

		internal void DetachOutturnFromLoadList(object sender, EventArgs e)
		{
			if (seaCargoDepotOutturnUserControl.OutturnsGrid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowError("Please select an Outturn Line to detach");
			}
			else if (Globals.Message.Show("Detach any outturns that are connected to load lists?", "Confirm Detach Outturn", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				foreach (DepotCusOutturn outturn in seaCargoDepotOutturnUserControl.OutturnsGrid.SelectedElements)
				{
					outturn.C5_ParentID = ZGuid.Empty;
					outturn.C5_ParentTableCode = ZString.Empty;
				}
			}
		}

		void SendSEQMessage(object sender, EventArgs e)
		{
			if (Header.HasChanges)
			{
				Globals.Message.ShowError("Please save the form before using this feature");
			}
			else if (seaCargoDepotOutturnUserControl.OutturnsGrid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowError("Please select an Outturn Line for which you wish to send an information request");
			}
			else
			{
				string sendMessagesWarning = string.Empty;
				foreach (DepotCusOutturn outturn in seaCargoDepotOutturnUserControl.OutturnsGrid.SelectedElements)
				{
					if (outturn.C5_CargoType != CMRImportCargoTypes.Codes.FullContainerLoad && outturn.C5_CargoType != CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills)
					{
						sendMessagesWarning = @"Please note that requests at the House Bill level are only valid for Break Bulk
or for outturns on second or subsequent underbond movements. For deconsolidation
outturns please select FCL or FCX (Container Level) lines; details will be returned, by
Customs, for all house bills associated with the container.

";
						break;
					}
				}
				sendMessagesWarning = sendMessagesWarning + "Send Establishment Information Query Request(s)?";

				if (Globals.Message.Show(sendMessagesWarning, "Confirm Send Request", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					ZStringBuilder builder = new ZStringBuilder();
					int count = 0;
					foreach (DepotCusOutturn outturn in seaCargoDepotOutturnUserControl.OutturnsGrid.SelectedElements)
					{
						SendOneSEQMessage(outturn, builder);
						count++;
					}
					builder.Append(count.ToString() + " messages created.");
					Globals.Message.ShowInformation(builder.ToStringWithNewLineBetweenAppends());
				}
			}
		}

		void SendOneSEQMessage(DepotCusOutturn outturn, ZStringBuilder builder)
		{
			SeaCargoEstablishmentQueryManager manager = new SeaCargoEstablishmentQueryManager(outturn);
			if (manager.CanSendOriginal)
			{
				manager.GenerateOriginalMessages(outturn);
				try
				{
					outturn.Factory.Save();
					builder.Append("Create " + manager.MessageFriendlyName);
				}
				catch (ZSaveException e)
				{
					ZExceptionReporting.HandleSaveException(e);
				}
			}
			else
			{
				Globals.Message.Show("System cannot send " + manager.MessageFriendlyName);
			}
		}

		internal void CreateOrAttachLoadListFromOutturn(BusinessObjectFactory newConsolFactory, CFSLoadListConsol consol)
		{
			if (newConsolFactory == null)
			{
				throw new ArgumentNullException(nameof(newConsolFactory));
			}

#if DEBUG
			// Consol is allowed to be null, which is why we pass in a factory as well.
			if (consol != null && consol.Factory != newConsolFactory)
			{
				throw new ArgumentException("Consol has a different factory to the one that was passed in.");
			}
#endif

			try
			{
				foreach (DepotCusOutturn outturn in seaCargoDepotOutturnUserControl.OutturnsGrid.SelectedElements)
				{
					if (outturn.C5_CargoType == CMRImportCargoTypes.Codes.FullContainerLoad
						|| outturn.C5_CargoType == CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills)
					{
						if (consol == null)
						{
							consol = new CFSContainerCreator(outturn, newConsolFactory).Consol;
						}
						else
						{
							new CFSContainerCreator(outturn, consol, newConsolFactory);
						}
					}
					else
					{
						if (consol == null)
						{
							consol = new CFSShipmentCreator(outturn, newConsolFactory).Consol;
						}
						else
						{
							new CFSShipmentCreator(outturn, consol, newConsolFactory);
							if (consol.JK_MasterBillNum.IsEmpty)
							{
								consol.JK_MasterBillNum = outturn.C5_MasterBill;
							}
						}
					}
				}
				using (CreateOrAttachLoadListForm childForm = new CreateOrAttachLoadListForm(consol))
				{
					if (Globals.IsTest)
					{
						newConsolFactory.Save();
					}
					else
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(childForm) == DialogResult.OK)
						{
							consol.Factory.Save();
							Header.Factory.Save();
							ZControllerFactory.Create(ControllerIDs.LoadListConsol).ShowEditForm(consol);
							consol.RunPreSaveValidation();
						}
						else
						{
							foreach (DepotCusOutturn outturn in seaCargoDepotOutturnUserControl.OutturnsGrid.SelectedElements)
							{
								outturn.C5_ParentID = ZGuid.Empty;
								outturn.C5_ParentTableCode = ZString.Empty;
								if (outturn.Underbond != null && outturn.Underbond.HasChanges)
								{
									outturn.Underbond.C4_ParentID = ZGuid.Empty;
									outturn.Underbond.C4_ParentTableCode = ZString.Empty;
								}
								outturn.Factory.Save();
							}
						}
					}
				}
			}
			catch (ZSaveConcurrencyException ex)
			{
				HandleSaveException(ex);
			}
		}

		ZString[] SelectedElementsOceanBillNumbers()
		{
			ArrayList result = new ArrayList();
			foreach (DepotCusOutturn outturn in seaCargoDepotOutturnUserControl.OutturnsGrid.SelectedElements)
			{
				if (!outturn.C5_MasterBill.IsEmpty && !result.Contains(outturn.C5_MasterBill))
				{
					result.Add(outturn.C5_MasterBill);
				}
			}
			return (ZString[])result.ToArray(typeof(ZString));
		}

		CFSLoadListConsol SelectConsol(BusinessObjectFactory factory)
		{
			CFSLoadListConsol result = null;
			RefVessel vessel = RefVessel.LookupVesselByLloyds(Header.C6_LloydsIMO, Header.Factory);
			ZString vesselName = "";
			if (vessel != null)
			{
				vesselName = vessel.RV_Code;
			}
			CFSLoadListSelector selector = new CFSLoadListSelector(vesselName, Header.C6_VoyageNum);
			if (ZFormModaliser.ShowDialogAndDispose(new SelectLoadListForm(selector)) == DialogResult.OK)
			{
				result = factory.Load<CFSLoadListConsol>(selector.ConsolPK);
			}
			return result;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		public override string FormCaption
		{
			get { return "Sea Cargo Outturn"; }
		}
	}
}
