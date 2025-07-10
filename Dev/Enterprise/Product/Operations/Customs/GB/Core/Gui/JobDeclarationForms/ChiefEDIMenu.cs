using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class ChiefEDIMenu : Customs.GUI.EDIMenu
	{
		public ChiefEDIMenu(bool showToggleTrainingFlag)
		{
			this.Text = "CHIEF";
		}

		protected override void AddAuditMenuItems()
		{
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set
			{
				base.Declaration = value;
				if (cnsCourierMenu != null)
				{
					cnsCourierMenu.Declaration = value;
				}
			}
		}

		protected override void SetupTopLevelMenu()
		{
			MenuItems.Clear();

			arriveDepartMenu = new ZMenuItem(ArriveAndDepartCaption);
			exportMasterFunctions = new ZMenuItem(InventoryManagementCaption);
			requestsAndReports = new ZMenuItem(ReportsAndRequestsChief);
			transferToCustoms = new ZMenuItem(TransferToCustomsCaption, ExportDeclarationToGeMSMenuItem_Click);
			MenuItem cancelRequest = new ZMenuItem(CancelDeclarationCaption, RequestCancellationAfterConfirmation_Click);

			MenuItems.Add(transferToCustoms);
			MenuItems.Add(requestsAndReports);
			MenuItems.Add(exportMasterFunctions);
			MenuItems.Add(arriveDepartMenu);

			exportMasterFunctions.MenuItems.Add(new ZMenuItem(MucrManagementClose, MucrManagementClose_Click));
			exportMasterFunctions.MenuItems.Add(new ZMenuItem(MucrManagementAssociate, MucrManagementAssociate_Click));
			exportMasterFunctions.MenuItems.Add(new ZMenuItem(MucrManagementDisassociate, MucrManagementDisassociate_Click));

			arriveDepartMenu.MenuItems.Add(new ZMenuItem(new GbInventoryManagementMessageFunction.ArrivalAnticipated(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration).FunctionHuman, inventoryAnticipateDeclaration_Click));
			arriveDepartMenu.MenuItems.Add(new ZMenuItem(new GbInventoryManagementMessageFunction.ArrivalAnticipated(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master).FunctionHuman, inventoryAnticipateMaster_Click));
			arriveDepartMenu.MenuItems.Add(new ZMenuItem(new GbInventoryManagementMessageFunction.ArrivalActual(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration).FunctionHuman, inventoryArriveDeclaration_Click));
			arriveDepartMenu.MenuItems.Add(new ZMenuItem(new GbInventoryManagementMessageFunction.ArrivalActual(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master).FunctionHuman, inventoryArriveMaster_Click));
			arriveDepartMenu.MenuItems.Add(new ZMenuItem(new GbInventoryManagementMessageFunction.Departure(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration).FunctionHuman, inventoryDepartDeclaration_Click));
			arriveDepartMenu.MenuItems.Add(new ZMenuItem(new GbInventoryManagementMessageFunction.Departure(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master).FunctionHuman, inventoryDepartMaster_Click));

			requestsAndReports.MenuItems.Add(new ZMenuItem(new Interrogate_Req().MenuCaption, interrogateChief_Req_Click));
			requestsAndReports.MenuItems.Add(new ZMenuItem(new Interrogate_DecDucr().MenuCaption, interrogateChief_DecDucr_Click));
			requestsAndReports.MenuItems.Add(new ZMenuItem(new Interrogate_Lem().MenuCaption, interrogateChief_LemDucr_Click));
			requestsAndReports.MenuItems.Add(new ZMenuItem(new Interrogate_Des().MenuCaption, interrogateChief_Des_Click));
			requestsAndReports.MenuItems.Add(new ZMenuItem(new Interrogate_DevDucr().MenuCaption, interrogateChief_DevDucr_Click));
			requestsAndReports.MenuItems.Add(new ZMenuItem(new Interrogate_DecMucr().MenuCaption, interrogateChief_DecMucr_Click));
			requestsAndReports.MenuItems.Add(new ZMenuItem(new Interrogate_Dem().MenuCaption, interrogateChief_DemDucr_Click));
			eadMenuItem = new ZMenuItem(new Interrogate_EAD().MenuCaption, interrogateChief_EAD_Click);
			requestsAndReports.MenuItems.Add(eadMenuItem);
			requestsAndReports.MenuItems.Add(cancelRequest);

			cnsCourierMenu = new GbCnsAirCourierMenu();
			MenuItems.Add(cnsCourierMenu);
		}

		public override void RefreshMenu()
		{
			var declaration = Declaration;
			var isExport = declaration != null && declaration.IsExport;
			var isCCSUK = declaration != null && declaration.IsCCSUK;

			transferToCustoms.Visible = false;
			requestsAndReports.Visible = false;
			exportMasterFunctions.Visible = isExport && isCCSUK;
			eadMenuItem.Visible = isExport;
			var isLoader = BadgeIsLoaderIncludingDEP;
			foreach (MenuItem menu in arriveDepartMenu.MenuItems)
			{
				menu.Enabled = isLoader;
			}
			arriveDepartMenu.Enabled = isLoader;
			cnsCourierMenu.Visible = declaration?.IsCNSAirImport ?? false;
		}

		void inventoryArriveDeclaration_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new GbInventoryManagementMessageFunction.ArrivalActual(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration));
		}

		void inventoryAnticipateDeclaration_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new GbInventoryManagementMessageFunction.ArrivalAnticipated(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration));
		}

		void inventoryDepartDeclaration_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new GbInventoryManagementMessageFunction.Departure(GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration));
		}

		void inventoryArriveMaster_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new GbInventoryManagementMessageFunction.ArrivalActual(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master));
		}

		void inventoryAnticipateMaster_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new GbInventoryManagementMessageFunction.ArrivalAnticipated(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master));
		}

		void inventoryDepartMaster_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new GbInventoryManagementMessageFunction.Departure(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master));
		}

		void interrogateChief_Req_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new Interrogate_Req());
		}

		void interrogateChief_DevDucr_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new Interrogate_DevDucr());
		}

		void interrogateChief_EAD_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new Interrogate_EAD());
		}

		void interrogateChief_DemDucr_Click(object sender, EventArgs e)
		{
			if (MenuOnlyAllowedForMessageTypeOrShowError(JobMessageTypeList.Codes.Export, JobMessageTypeList.Descriptions.Export))
			{
				ZString answer = Globals.Message.QueryDefaultValue("", "Enter a movement reference number to query.\r\nThis is returned in the response to a LEM (list export movements)\r\nmessage and is highlighted in green. ", "Enter movement number", 0);
				if (!answer.IsEmpty)
				{
					SendToChiefViaCsp(new Interrogate_Dem(answer));
				}
			}
		}

		void interrogateChief_LemDucr_Click(object sender, EventArgs e)
		{
			if (MenuOnlyAllowedForMessageTypeOrShowError(JobMessageTypeList.Codes.Export, JobMessageTypeList.Descriptions.Export))
			{
				SendToChiefViaCsp(new Interrogate_Lem());
			}
		}

		bool MenuOnlyAllowedForMessageTypeOrShowError(string jeMessageType, string humanMessageType)
		{
			if (Declaration != null && Declaration.JE_MessageType == jeMessageType)
			{
				return true;
			}
			else
			{
				Globals.Message.ShowError("This menu is not available for this declaration type.\r\nIt is only for declarations of type " + humanMessageType);
				return false;
			}
		}

		void interrogateChief_DecMucr_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new Interrogate_DecMucr());
		}

		void interrogateChief_DecDucr_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new Interrogate_DecDucr());
		}

		void interrogateChief_Des_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new Interrogate_Des());
		}

		void MucrManagementClose_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new GbDes242MessageFunction.MucrClose());
		}

		void MucrManagementAssociate_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new GbDes242MessageFunction.MucrAssociate());
		}

		void MucrManagementDisassociate_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new GbDes242MessageFunction.MucrDisAssociate());
		}

		void ExportDeclarationToGeMSMenuItem_Click(object sender, EventArgs e)
		{
			SendToChiefViaCsp(new CusdecMessageFunction.New());
		}

		protected virtual void SendToChiefViaCsp(CusdecMessageFunction howToSend)
		{
			if (!Declaration.IsInDatabase)
			{
				Globals.Message.Show("Please save first");
			}
			else
			{
				using (Form != null ? new CellNotificationSuspender(Form) : null)
				using (Declaration.Factory != null ? ActiveBusinessObjectCollection.DelayListChangedEvents(Declaration.Factory) : null)
				{
					IDeclarationMessageSender messageSender = (IDeclarationMessageSender)Activator.CreateInstance(ObjectFactory.GetType<Integration.Customs.GB.IDeclarationMessageSenderChooser>());
					messageSender.Send(Declaration, new Customs.GUI.SendsMessagesToCustomsGUI(), howToSend);
				}
			}
		}

		void RequestCancellationAfterConfirmation_Click(object sender, EventArgs e)
		{
			if (HasEligibleEntryToCancel)
			{
				if (ReallyCancel() == DialogResult.OK)
				{
					SendToChiefViaCsp(new CusdecMessageFunction.Deleted());
				}
			}
			else
			{
				WarnThatThereIsNothingToSend();// Cannot transmit yet as there is no entry to cancel
			}
		}

		protected virtual bool HasEligibleEntryToCancel
		{
			get
			{
				return this.Declaration != null
								&&
								this.Declaration.CustomsEntryHeaders != null
								&&
								this.Declaration.CustomsEntryHeaders.HasAnEntryWithEntryNumber;
			}
		}

		static DialogResult ReallyCancel()
		{
			return Globals.Message.ShowConfirmation("Are you sure you want to request cancellation of this declaration?  If the request is accepted, it cannot be reversed.", "Really cancel?", "yes", MessageBoxIcon.Warning);
		}

		void WarnThatThereIsNothingToSend()
		{
			Globals.Message.Show("There are no entries with an entry number, so there is nothing to cancel. No cancellation message will be sent.", "No message to cancel", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}

		bool BadgeIsLoaderIncludingDEP
		{
			get
			{
				var isDep = false;
				var isMaritimeLoader = false;
				var declaration = Declaration;
				if (declaration != null)
				{
					var badges = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty);
					if (badges != null)
					{
						var thisBadge = badges.FindByBadgeCode(declaration.JE_CustomsProfile, "EXP");
						if (thisBadge != null)
						{
							var credential = CredentialsSetting.GetCredentialsForBadge(thisBadge.BadgeCode, declaration.RegistryCompanyPK);
							if (credential != null)
							{
								isDep = credential.IsDEPOperator;
								isMaritimeLoader = credential.IsMaritimeLoader;
							}
						}
					}
				}
				return isDep || isMaritimeLoader;
			}
		}

		public const string TransferToCustomsCaption = "Send original/amendment to CHIEF";
		public const string CancelDeclarationCaption = "XTC (request cancellation)";
		public const string MucrManagementClose = "Close master";
		public const string MucrManagementAssociate = "Associate to master";
		public const string MucrManagementDisassociate = "Disassociate from master";
		public const string ReportsAndRequestsChief = "Reports && requests";
		public const string InventoryManagementCaption = "Export master functions";
		public const string ChiefMenuCaption = "CHIEF";
		public const string ArriveAndDepartCaption = "Arrive && depart freight";

		MenuItem transferToCustoms;
		MenuItem requestsAndReports;
		MenuItem exportMasterFunctions;
		MenuItem eadMenuItem;
		MenuItem arriveDepartMenu;
		GbCnsAirCourierMenu cnsCourierMenu;
	}
}
