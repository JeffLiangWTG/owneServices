using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.MessageSending
{
	public class ECSExitHeaderMessageSendingObjectCollection<T> : NonPersistentBusinessObjectCollection<T> where T : ECSExitHeaderMessageSendingObject
	{
		public ECSExitHeaderMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		protected override bool AllowNewCore => false;
	}
}
