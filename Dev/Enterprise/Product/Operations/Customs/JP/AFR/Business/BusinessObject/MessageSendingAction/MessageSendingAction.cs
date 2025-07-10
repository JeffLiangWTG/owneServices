using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class MessageSendingAction : AutoMessageSendingAction
	{
		public MessageSendingAction(JPAFRHeader header, ActionCode actionCode)
			: base(header.Factory)
		{
			this.Header = Argument.NotNull(header, "header");
			this.HasATDBeenSent = Header.JPH_IsShippingLineEntry && Header.IsDepartureTimeRegistered;
			UpdateMessageSendingAction(actionCode);
		}

		public void UpdateMessageSendingAction()
		{
			this.JPM_MasterBillOfLadingNumber = Header.JPH_MasterBillNumber;
			this.JPM_ETA = Header.JPH_ETA;
		}

		public void UpdateMessageSendingAction(ActionCode actionCode)
		{
			this.ActionCode = actionCode;
			UpdateMessageSendingAction();
		}

		#region New Properties

		public JPAFRHeader Header { get; private set; }

		public ActionCode ActionCode { get; private set; }

		[ReadOnlyMember(nameof(HasATDBeenSent_ReadOnly))]
		public bool HasATDBeenSent
		{
			get { return hasATDBeenSent; }
			set
			{
				hasATDBeenSent = value;
				if (OnHasATDBeenSentChanged != null)
				{
					OnHasATDBeenSentChanged(this, null);
				}
			}
		}
		bool hasATDBeenSent;
		public event EventHandler OnHasATDBeenSentChanged;

		public bool HasATDBeenSent_ReadOnly
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return this.Header.JPH_IsShippingLineEntry && this.ActionCode != ActionCode.RegisterDepartureTime && this.ActionCode != ActionCode.ChangeDepartureTimeAfterATD;
			}
		}

		internal bool IsBillRegistrationCompleted
		{
			get { return this.Header.IsBillRegistrationCompleted; }
		}

		public MessageSendingObjectCollection MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					messageSendingObjects = new MessageSendingObjectCollection(Factory);
					var loader = new MessageSendingObject.Loader(Factory, this);
					foreach (var bill in Header.Bills)
					{
						var sendObject = loader.LoadOrNew(bill, ActionCode);
						if (sendObject != null)
						{
							var billActionCode = ActionCode;
							if (ActionCode == Business.ActionCode.AmendingAdd && !sendObject.IsBillAlreadyRegistered)
							{
								billActionCode = Business.ActionCode.NewBill;
							}
							sendObject.UpdateAction(billActionCode);
							messageSendingObjects.Add(sendObject);
						}
					}
					RegisterEditableChildObject(messageSendingObjects);
				}
				return messageSendingObjects;
			}
		}
		MessageSendingObjectCollection messageSendingObjects;

		public IReadOnlyList<MessageSendingObject> ObjectsToSend
		{
			get
			{
				if (objectsToSendCached == null)
				{
					objectsToSendCached = new CachedProperty<List<MessageSendingObject>>(Factory, delegate
					{
						var list = new List<MessageSendingObject>();
						foreach (MessageSendingObject msgData in MessageSendingObjects)
						{
							if (msgData.JPM_Send)
							{
								list.Add(msgData);
							}
						}
						return list;
					});
				}
				return objectsToSendCached.Value;
			}
		}
		CachedProperty<List<MessageSendingObject>> objectsToSendCached;

		#endregion

		#region Implementation

		public ZString GetWarningForBillsToSendThatAreWaitingForResponse()
		{
			var builder = new ZStringBuilder();
			foreach (var obj in ObjectsToSend)
			{
				if (MessageStatusList.IsMessagingInProgressType(obj.JPM_MessageStatus))
				{
					builder.Append(obj.JPM_BillOfLadingNumber);
				}
			}
			return builder.IsEmpty ? "" : ValidationConstants.MessageSending.BillsToSendThatAreWaitingForResponseMessage(builder.ToStringWithNewLineBetweenAppends());
		}

		public ZString GetWarningForConsolWithoutLegGoesIntoJP()
		{
			var result = ZString.Empty;
			if (Header.Consol != null)
			{
				var countryJP = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Japan);
				var foundLegEnterJP = false;
				foreach (Transport transport in Header.Consol.Transports)
				{
					if (!transport.IsDomestic && countryJP.ContainsUNLOCO(transport.DiscPort))
					{
						foundLegEnterJP = true;
						break;
					}
				}
				result = foundLegEnterJP ? string.Empty : ValidationConstants.MessageSending.ConsolDoesNotDischargeInJapan;
			}
			return result;
		}

		public void ResetEditableChildObject()
		{
			foreach (MessageSendingObject obj in MessageSendingObjects)
			{
				obj.UnRegisterBillAsEditableChildObject();
			}
		}

		#endregion
	}
}
