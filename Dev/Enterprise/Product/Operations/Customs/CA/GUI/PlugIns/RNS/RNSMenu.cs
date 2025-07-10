using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using IUserNotification = Enterprise.Customs.Business.MessageManagers.IUserNotification;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI.PlugIns
{
	class RNSMenu : KMenuItem
	{
		public RNSMenu(RNSMessagingBO rnsMessaging)
			: base("RNS")
		{
			Argument.NotNull(rnsMessaging, "rnsMessaging");
			this.rnsMessaging = rnsMessaging;

			MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("611433f8-1ccb-4d67-87eb-6d2bbb02f592", "Send Release Status Query"), QueryReleaseStatus_Click));
			if (rnsMessaging.PlugInSupport is RNSPlugInSupportShipmentWrapper)
			{
				MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("99f0d384-805a-4f04-b723-80c5cdd21815", "Arrival Certification Message"), ArrivalMessage_Click));
				MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("BF2363AD-BE8F-4279-AA55-653EAE159CCD", "Enter Manual Release"), MenualRelease_Click));
			}
			else if (rnsMessaging.PlugInSupport is RNSPlugInSupportConsolWrapper)
			{
				MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("6720DF3E-2240-4D2C-B49A-85CA207078EE", "Send House Bill Release Status Query(s)"), QueryHouseBilReleaseStatus_Click));
				MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("582BD361-3EF3-48C3-BDB6-09C7CCC59585", "House Bill Arrival Certification Message(s)"), HouseBillArrivalCertification_Click));
			}
		}

		void QueryReleaseStatus_Click(object sender, EventArgs e)
		{
			var manager = new RNSMessageManager(rnsMessaging, new MessageInstructionUserNotification(), true);
			manager.SendMessage(MessageSubTypes.Request, false);
			rnsMessaging.RN_ReleaseStatusInfo.RefreshBinding();
		}

		void ArrivalMessage_Click(object sender, EventArgs e)
		{
			if (this.rnsMessaging.PlugInSupport.CargoControlNumber.IsEmpty)
			{
				Globals.Message.ShowInformation(Res.GetString("336d3997-4f30-4010-baab-90e23e6f30fb", "The CCN is required for sending an arrival certification message."));
			}
			else
			{
				var rnsRequestBO = new RNSRequestBO(rnsMessaging, RNSMessageTypes.Codes.ArrivalCertification, rnsMessaging.Factory, false, false);
				rnsRequestBO.OfficeCode = GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.CustomsOfficeCode).SubstringSafe(0, 4);

				ZFormModaliser.ShowDialogAndDispose(new RNSRequestForm(rnsRequestBO));
				rnsMessaging.ArrivalCertificationStatusInfo.RefreshBinding();
			}
		}
		readonly RNSMessagingBO rnsMessaging;

		void MenualRelease_Click(object sender, EventArgs e)
		{
			if (SaveJob())
			{
				var manualReleaseSupport = this.rnsMessaging.PlugInSupport.Master as Integration.Customs.CA.IManualReleaseSupport;
				var reasonForCannotManualRelease = manualReleaseSupport.GetReasonForCannotManualRelease();
				if (!reasonForCannotManualRelease.IsEmpty)
				{
					Globals.Message.ShowError(reasonForCannotManualRelease);
				}
				else
				{
					using (var form = new ManualReleaseForm(manualReleaseSupport, rnsMessaging.Factory))
					{
						ZFormModaliser.ShowDialogAndDispose(form);
						rnsMessaging.RN_ReleaseStatusInfo.RefreshBinding();
						rnsMessaging.RN_ReleaseDateInfo.RefreshBinding();
						rnsMessaging.RN_LoginUserInfo.RefreshBinding();
						rnsMessaging.RN_EntryDateInfo.RefreshBinding();
					}
				}
			}
		}

		void QueryHouseBilReleaseStatus_Click(object sender, EventArgs e)
		{
			if (rnsMessaging.PlugInSupport is IRNSPlugInSupport support)
			{
				var manager = support.GetRNSMultiMessageManager();
				var messages = manager.SendOriginalMessages(new RNSSendsMessagesToCustomsGUI(manager.RNSRequestParent));
				if (messages.Any())
				{
					try
					{
						manager.Factory.Save();
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}

		void HouseBillArrivalCertification_Click(object sender, EventArgs e)
		{
			var rnsRequestBos = new RNSRequestBOCollection(rnsMessaging.PlugInSupport.Master as ForwardingConsol);
			if (rnsRequestBos.Count > 0)
			{
				var continueWithSend = false;
				using (var form = new HouseBillArrivalCertificationSelectionDialog(rnsRequestBos))
				{
					continueWithSend = ZFormModaliser.ShowDialogAndDispose(form) == DialogResult.OK;
				}
				if (continueWithSend)
				{
					var manager = new ConsolRNSMessageManager(RNSParentConsolWrapper.Load(rnsMessaging.PlugInSupport.Master as ForwardingConsol), rnsRequestBos.GetSelectedRequestBOs());
					var messages = manager.SendOriginalMessages(new RNSSendsMessagesToCustomsGUI(manager.RNSRequestParent));
					if (messages.Any())
					{
						try
						{
							manager.Factory.Save();
						}
						catch (ZSaveException ex)
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("A1FA6391-5657-482D-AFF2-6802DC96F85C", "There is nothing available for sending."));
			}
		}

		ZForm MainForm
		{
			get { return (ZForm)GetMainMenu().GetForm(); }
		}

		bool SaveJob()
		{
			return !this.rnsMessaging.PlugInSupport.Master.HasChanges ||
				(Notification.ShowConfirmation(Res.GetString("FB4977C5-0953-41BA-A249-5B2B260BCBFF", "The Job has not yet been saved. Do you want to save and proceed?"), Res.GetString("4173C4BF-876E-41A4-B419-82F1537E8C05", "Save Job")) && MainForm.FireSaveButton() == ContinueWithSave.Yes);
		}

		protected virtual IUserNotification Notification
		{
			get { return fNotification; }
		}

		readonly IUserNotification fNotification = new MessageInstructionUserNotification();
	}
}
