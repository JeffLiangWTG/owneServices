using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.PBN.Business
{
	public sealed class PBNMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<PBNMessageSendingObject>
	{
		public PBNMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new InvalidOperationException("PBNMessageSendingObjectCollection should not support adding.");

		protected override bool AllowNewCore => false;
	}
}
