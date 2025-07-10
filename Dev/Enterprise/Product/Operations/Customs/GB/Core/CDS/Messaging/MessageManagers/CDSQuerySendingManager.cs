using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.Query;
using Enterprise.Customs.GB.CDS.Messaging.MessageManagers;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.MessageManagers
{
	public class CDSQuerySendingManager
	{
		public CDSQuerySendingManager(CDSQuerySendingObject sender, ZGuid sourcePK) : this(sender)
		{
			this.sourcePK = sourcePK;
		}
		public CDSQuerySendingManager(CDSQuerySendingObject sender)
		{
			this.sender = Argument.NotNull(sender, "CDS Query Sending Object");
			notificationCollection = new MessageSendingNotificationCollection();
		}

		public void Send()
		{
			notificationCollection.Clear();

			if (CanSendMessages())
			{
				var cusEntryHeader = sender.GetEntryHeader(sourcePK);
				if (cusEntryHeader != null)
				{
					var successMessage = Res.GetString("B2B123B9-8A42-4BA4-AF41-E0457D95C0E3", "CDS Query has been queued successfully");
					var errorMessage = Res.GetString("3C79AA36-4FF2-46B4-BF99-0191E486A894", "An error occurred while trying to queue the query");
					if (sender.IsUniversalEvent)
					{
						using (var universalEvent = GetUniversalEvent())
						{
							if (!CDSQuerySendingHelper.Deliver(cusEntryHeader.Factory, universalEvent, cusEntryHeader, DeliveryDestination, notificationCollection, null))
							{
								notificationCollection?.AddError(errorMessage);
							}
							else
							{
								notificationCollection?.AddInformation(successMessage);
							}
						}
					}
					else
					{
						var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
						new CdsMucrInventoryLinkingMessageSender(cusEntryHeader).SendToRecipient(null, shutUp, new GbDes242MessageFunction.QueryMasterDEC(), false);
						var errors = shutUp.LastErrorsAsString;
						if (string.IsNullOrEmpty(errors))
						{
							notificationCollection?.AddInformation(successMessage);
						}
						else
						{
							notificationCollection?.AddError(errorMessage);
							notificationCollection?.AddError(errors);
						}
					}
				}

				cusEntryHeader.Factory.Save();
				cusEntryHeader.Messages.Reload(false);
				cusEntryHeader.RefreshBindingIncludingChildren();
			}

			ShowResultNotification();
		}

		public ZString GetUniversalEventContent()
		{
			using (var universalEvent = GetUniversalEvent())
			{
				return CDSQueryUniversalEventBuilder.ConvertToXml(universalEvent);
			}
		}

		public UniversalEvent GetUniversalEvent()
		{
			var builder = new CDSQueryUniversalEventBuilder(sender);
			return builder.BuildUniversalEvent(sourcePK);
		}

		bool CanSendMessages() => sender.CanSend(sourcePK, notificationCollection);

		void ShowResultNotification()
		{
			if (notificationCollection.ContainsError())
			{
				ZArchitecture.Environment.Globals.Message.ShowError(notificationCollection.ErrorNotificationsAsString(), Res.GetString("2D2775B1-27DB-47F2-9AF2-A8F73A0BB6B6", "Error in Sending CDS Query"));
			}
			else if (notificationCollection.ContainsInformation())
			{
				ZArchitecture.Environment.Globals.Message.ShowInformation(notificationCollection.InformationNotificationsAsString(), Res.GetString("69D02BFE-BCFB-432A-94A3-B7DC1EE06F29", "CDS Query Sending Result"));
			}
		}

		protected virtual string DeliveryDestination => EHubID;

		readonly CDSQuerySendingObject sender;
		readonly ZGuid sourcePK;
		readonly MessageSendingNotificationCollection notificationCollection;
		const string EHubID = "GBCustoms";
	}
}
