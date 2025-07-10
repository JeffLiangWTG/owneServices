using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public class RNSParentTallyWrapper : BusinessObjectWrapper
		, IRNSRequestParent
		, IObsoleteValidation
		, IMessageManageableBizObj
	{
		protected RNSParentTallyWrapper(TallyContainer tally)
			: base(tally)
		{
			this.parentBusinessObject = tally;
		}

		public static RNSParentTallyWrapper Load(TallyContainer parent)
		{
			return (RNSParentTallyWrapper)Load(typeof(RNSParentTallyWrapper), parent);
		}

		readonly BusinessObject parentBusinessObject;
		IUserNotification notification;
		bool isStatusQuery = true;

		public TallyContainer Tally
		{
			get
			{
				return (TallyContainer)this.parentBusinessObject;
			}
		}

		#region IRNSRequestParent

		public IUserNotification Notification
		{
			get
			{
				return notification;
			}
			set
			{
				notification = value;
			}
		}

		public bool IsStatusQuery
		{
			get
			{
				return isStatusQuery;
			}
			set
			{
				isStatusQuery = value;
			}
		}

		public BusinessObject ParentBusinessObject
		{
			get
			{
				return this.parentBusinessObject;
			}
		}

		public IEnumerable<IRNSRequest> GetRNSRequestCollections()
		{
			if (IsStatusQuery)
			{
				List<RNSMessagingBO> messageingBOs = new List<RNSMessagingBO>();
				foreach (CFSShipment shipment in this.Tally.PackUnpackShipments)
				{
					messageingBOs.Add(new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(shipment)));
				}
				return messageingBOs;
			}
			else
			{
				List<RNSRequestBO> requestBOs = new List<RNSRequestBO>();
				foreach (CFSShipment shipment in this.Tally.PackUnpackShipments)
				{
					var rnsMessagingBO = new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(shipment));
					var rnsRequestBO = new RNSRequestBO(rnsMessagingBO, RNSMessageTypes.Codes.ArrivalCertification, shipment.Factory, false, false);
					requestBOs.Add(rnsRequestBO);
				}
				return requestBOs;
			}
		}

		public bool ShouldDefaultChooseToSent(SingleMessageManager messageManager)
		{
			if (IsStatusQuery)
			{
				return false;
			}
			else
			{
				CFSShipment shipment = (CFSShipment)messageManager.BusinessObject;

				return (shipment.ArrivalCertificationStatusCode == MessageStatusList.Codes.NotSent || shipment.ArrivalCertificationStatusCode == EDIMessage.Status.Rejected)
					&& shipment.OuterPackLines.Any(line => ((TallyPackLine)line).JL_Outturn != 0);
			}
		}

		public bool ShouldWaitUntilResponded
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region IMessageManageableBizObj

		public bool IsInAStatusAmendmentSendable
		{
			get { return false; }
		}

		public ContinueWithDetection ProcessBeforeDetectingAmendmentAndContinue()
		{
			throw new NotSupportedException("The property is not supported.");
		}

		public IMessageManager GetMessageManagerForAmendmentDetection()
		{
			throw new NotSupportedException("The method is not supported.");
		}

		#endregion
	}
}
