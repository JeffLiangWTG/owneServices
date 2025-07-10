using System;
using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Business.XmlMessaging
{
	public class XmlEDIInterchangeEDIMessageCollection : DependentBusinessObjectCollection<XmlEDIMessage, XmlEDIInterchange>
	{
		#region Constructors

		public XmlEDIInterchangeEDIMessageCollection(XmlEDIInterchange master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		#endregion

		#region Overrides

		public virtual new XmlEDIMessage AddNew(Type bizoType)
		{
			return base.AddNew(bizoType);
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			var interchange = Master;
			var messageAdded = (EDIMessage)bizOAdded;

			if (messageAdded.EM_TransportType.IsEmpty)
			{
				messageAdded.EM_TransportType = interchange.EI_TransportType;
			}
		}

		#endregion
	}
}
