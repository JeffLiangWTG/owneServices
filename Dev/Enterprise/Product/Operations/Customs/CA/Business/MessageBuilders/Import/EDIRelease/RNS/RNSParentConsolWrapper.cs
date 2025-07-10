using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public class RNSParentConsolWrapper : BusinessObjectWrapper
		, IRNSRequestParent
		, IMessageManageableBizObj
	{
		protected RNSParentConsolWrapper(ForwardingConsol consol)
			: base(consol)
		{
			this.parentBusinessObject = consol;
		}
		readonly BusinessObject parentBusinessObject;
		IUserNotification notification;
		bool isStatusQuery = true;

		public static RNSParentConsolWrapper Load(ForwardingConsol parent) => (RNSParentConsolWrapper)Load(typeof(RNSParentConsolWrapper), parent);

		public ForwardingConsol Consol => (ForwardingConsol)this.parentBusinessObject;

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

		public BusinessObject ParentBusinessObject => this.parentBusinessObject;

		public bool ShouldWaitUntilResponded => false;

		public bool IsInAStatusAmendmentSendable => false;

		public IMessageManager GetMessageManagerForAmendmentDetection() => throw new NotSupportedException("The method is not supported.");

		public IEnumerable<IRNSRequest> GetRNSRequestCollections()
		{
			var messageingBOs = new List<IRNSRequest>();

			foreach (Freight.Business.CommonShipment shipment in this.Consol.Shipments)
			{
				messageingBOs.Add(new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(shipment)));
			}

			return messageingBOs;
		}

		public ContinueWithDetection ProcessBeforeDetectingAmendmentAndContinue() => throw new NotSupportedException("The method is not supported.");

		public bool ShouldDefaultChooseToSent(SingleMessageManager messageManager) => false;
	}
}
