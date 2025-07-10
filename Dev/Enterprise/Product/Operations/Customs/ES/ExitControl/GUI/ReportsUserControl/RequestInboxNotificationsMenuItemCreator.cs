using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Customs.GUI.WarehouseExtensions;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.ExitControl.GUI
{
	public class RequestInboxNotificationsMenuItemCreator
	{
		public RequestInboxNotificationsMenuItemCreator(IReportsGridUserControlProvider provider)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
		}
		readonly IReportsGridUserControlProvider provider;

		public ZMenuItem Create() => new ZMenuItem(ResString.GetMultilingualString("E3C3BE86-86DB-4617-B9B9-D11742343960", "Check for Inbox Notifications"), RequestInboxNotifications_Click);

		void RequestInboxNotifications_Click(object sender, EventArgs e)
		{
			var reportsGrid = provider.UserControl.ReportsGrid;

			var selectedElements = reportsGrid.SelectedElements;
			if (selectedElements.Length == 0)
			{
				Globals.Message.Show(CommonPromptMessages.SelectARowMessage);
			}
			else
			{
				if (SaveIfRequiredAndConfirmedByUser(reportsGrid))
				{
					var continueWithSend = CheckBrokerBeforeSending(reportsGrid, out var certificateName);

					if (continueWithSend)
					{
						var messagesCreated = 0;

						var messagesTypesToSend = new List<ZString>();
						foreach (CusExitReport report in selectedElements)
						{
							foreach (var messageType in InboxRequestMessageTypes)
							{
								CreateAndSaveInboxRequestMessage(report, messageType, certificateName);
								messagesCreated++;
								if (!messagesTypesToSend.Contains(messageType))
								{
									messagesTypesToSend.Add(messageType);
								}
							}
						}

						if (messagesTypesToSend.Count > 0)
						{
							CreateAndSaveInboxListMessages(messagesTypesToSend.ToArray());
						}

						if (messagesCreated == 1)
						{
							Globals.Message.Show(ResString.GetMultilingualString("3CCCFB7E-6032-4953-BC28-858A69CB24E7", "1 In-box Notification request created"));
						}
						else
						{
							Globals.Message.Show(ResString.GetMultilingualString("68EFA50E-A942-4B60-8158-86334DA8AB3F", "{0} In-box Notification requests created", messagesCreated.ToString(Culture.Current)));
						}
					}
				}
			}
		}

		ZString[] InboxRequestMessageTypes => new ZString[] { DeclarationMessageTypeList.Codes.ExportExitClearanceNotification, DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification };

		void CreateAndSaveInboxRequestMessage(CusExitReport report, ZString messageType, ZString certName)
		{
			try
			{
				var consignment = report.Consignment;
				if (consignment != null)
				{
					var factory = new BusinessObjectFactory();

					var mrn = consignment.CXC_MovementReference;
					MessageRequest.CreateEDIMessageForInboxRequest(factory, report, report.PK, report.TablePrefix, mrn, mrn, messageType, certName);

					factory.Save();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		void CreateAndSaveInboxListMessages(ZString[] messagesTypesToSend)
		{
			try
			{
				var factory = new BusinessObjectFactory();
				var inboxListRequestSender = new InboxListRequestSender(null);
				inboxListRequestSender.CreateListPollingEDIMessages(factory, specificTypes: messagesTypesToSend);
				factory.Save();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		bool SaveIfRequiredAndConfirmedByUser(ZGrid reportsGrid)
		{
			bool okToContinue = true;
			if (reportsGrid.GetCurrent() is CusExitReport report && (report.Header?.HasChanges ?? false))
			{
				var confirmedToSave = Globals.Message.Show(
										Res.GetString("EEDECC49-DE9D-4092-9552-5C304E822921", "The Job has not yet been saved. Do you want to save and proceed?"),
										Res.GetString("54221F3F-290C-4A1B-A7D8-BE2381E73D77", "Save Job"),
										MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes;

				okToContinue = confirmedToSave && (reportsGrid.Parent.FireSaveButton() == ContinueWithSave.Yes);
			}

			return okToContinue;
		}

		bool CheckBrokerBeforeSending(ZGrid reportsGrid, out ZString certificateName)
		{
			certificateName = ZString.Empty;
			bool continueWithSend = false;
			if (reportsGrid.GetCurrent() is CusExitReport report)
			{
				var exitHeader = report.Header;
				var broker = exitHeader.CustomsAgent;
				if (broker == null || CertificateHasMessageErrors(exitHeader))
				{
					Globals.Message.ShowError(CommonPromptMessages.CredentialsErrorMessage);
				}
				else
				{
					continueWithSend = true;
					certificateName = exitHeader.CXH_CustomsProfile;
				}
			}
			return continueWithSend;

			ZBool CertificateHasMessageErrors(CusExitHeader header)
			{
				header.Validation.ValidateCXH_CustomsProfile();
				return header.CXH_CustomsProfileInfo.HasMessageErrors();
			}
		}
	}
}
