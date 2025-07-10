using System;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class PreValidateTraderSendingManager
	{
		public PreValidateTraderSendingManager(EU.EMCS.Business.EMCSJobDeclaration declaration, PreValidateTraderInfo traderInfo)
		{
			this.declaration = Argument.NotNull(declaration, nameof(EMCSJobDeclaration));
			this.traderInfo = Argument.NotNull(traderInfo, nameof(PreValidateTraderInfo));
			NotificationCollection = new MessageSendingNotificationCollection();
		}

		public void Send()
		{
			var exciseCodes = PreValidateTraderHelper.GetProductCodes(traderInfo);
			var traders = PreValidateTraderHelper.GetDistinctTraders(traderInfo);
			traders.ForEach(trader =>
			{
				exciseCodes.ForEach(codes =>
				{
					NotificationCollection.Clear();
					sender = new PreValidateTraderSendingObject(declaration, trader, codes);
					using var universalEvent = GetUniversalEvent();
					{
						if (!Deliver(declaration, universalEvent, NotificationCollection, null))
						{
							NotificationCollection?.AddError(Res.GetString("1F68D478-23D9-4DFB-AFE0-2968376085DD", "An error occurred while trying to queue Pre-Validate Trader"));
						}
						else
						{
							NotificationCollection?.AddInformation(Res.GetString("2343F63B-3252-4B9E-BD2D-BE1C4942A78F", "Pre-Validate Trader has been queued successfully"));
						}
					}
				});
			});

			declaration.Factory.Save();
			declaration.Messages.Reload(false);
			declaration.RefreshBindingIncludingChildren();
		}

		UniversalEvent GetUniversalEvent() => new PreValidateTraderUniversalEventBuilder(sender).BuildUniversalEvent();

		protected virtual bool Deliver(EU.EMCS.Business.EMCSJobDeclaration declaration, UniversalEvent universalEvent, MessageSendingNotificationCollection notificationCollection, EDIMessage message)
		{
			var isDeliverySuccessful = false;
			var context = PreValidateTraderHelper.NewDeliveryContext(declaration, notificationCollection);
			var eHubDelivery = new PreValidateTraderEHubDelivery(universalEvent);
			var mode = new NonPersistentEDICommunicationMode
			{
				EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML,
				EK_Destination = PreValidateTraderHelper.Constants.EHubID
			};

			var getMessageFunc = message != null ? new Func<IEDIMessage>(() => message) : null;
			var result = eHubDelivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(context.ParentInfo, universalEvent, new XmlWriter(), UniversalXmlInfo.Namespace_2011_11), getMessageFunc);
			if (result.Succeeded)
			{
				PreValidateTraderHelper.SetOutgoingUniversalEventInterpretation(eHubDelivery.InterchangeCreated.PK, declaration.Factory);
				isDeliverySuccessful = true;
			}
			return isDeliverySuccessful;
		}

		readonly EU.EMCS.Business.EMCSJobDeclaration declaration;
		readonly PreValidateTraderInfo traderInfo;
		PreValidateTraderSendingObject sender;
		public readonly MessageSendingNotificationCollection NotificationCollection;
	}
}
