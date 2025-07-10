using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public abstract class DeclarationMessageManager<T> : DeclarationMessageManager where T : DeclarationMessageSendingObject
	{
		public DeclarationMessageManager(T messageSender) : base(messageSender)
		{
		}

		protected override BusinessObject GetBusinessObjectInNewFactory(BusinessObject businessObject)
		{
			var messageSendingObject = businessObject as DeclarationMessageSendingObject;
			return base.GetBusinessObjectInNewFactory(messageSendingObject.Header) is CusEntryHeader entryHeader
				? (DeclarationMessageSendingObject)Activator.CreateInstance(typeof(T), entryHeader) : null;
		}
	}
}
