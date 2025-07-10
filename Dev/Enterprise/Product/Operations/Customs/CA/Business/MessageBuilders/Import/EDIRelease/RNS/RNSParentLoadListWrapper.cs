using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public class RNSParentLoadListWrapper : BusinessObjectWrapper
		, IRNSRequestParent
		, IObsoleteValidation
		, IMessageManageableBizObj
	{
		protected RNSParentLoadListWrapper(CFSLoadListConsol loadListConsol)
			: base(loadListConsol)
		{
			this.parentBusinessObject = loadListConsol;
		}

		public static RNSParentLoadListWrapper Load(CFSLoadListConsol parent)
		{
			return (RNSParentLoadListWrapper)Load(typeof(RNSParentLoadListWrapper), parent);
		}

		readonly BusinessObject parentBusinessObject;
		IUserNotification notification;
		bool isStatusQuery = true;

		public CFSLoadListConsol LoadListConsol
		{
			get
			{
				return (CFSLoadListConsol)this.parentBusinessObject;
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
			List<RNSMessagingBO> messageingBOs = new List<RNSMessagingBO>();

			foreach (CFSShipment shipment in this.LoadListConsol.Shipments)
			{
				messageingBOs.Add(new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(shipment)));
			}

			return messageingBOs;
		}

		public bool ShouldDefaultChooseToSent(SingleMessageManager messageManager)
		{
			return false;
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
