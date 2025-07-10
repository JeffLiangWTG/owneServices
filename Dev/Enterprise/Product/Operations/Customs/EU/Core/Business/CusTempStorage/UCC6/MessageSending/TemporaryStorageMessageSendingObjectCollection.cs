using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageMessageSendingObjectCollection<T, THeader> : NonPersistentBusinessObjectCollection<T>
		where T : TemporaryStorageMessageSendingObject
		where THeader : TemporaryStorageHeader
	{
		public TemporaryStorageMessageSendingObjectCollection(THeader header) : base(header.Factory)
		{
			this.header = header;
		}
		readonly THeader header;

		protected override BusinessObject CreateNonPersistentBusinessObject() => (T)Activator.CreateInstance(typeof(T), header);

		protected override bool AllowNewCore => false;
	}
}
