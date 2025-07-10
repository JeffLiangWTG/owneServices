using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business.CusTempStorage;

public class TemporaryStorageMessageSendingObjectCollection : EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectCollection<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>
{
	public TemporaryStorageMessageSendingObjectCollection(TemporaryStorageHeader header) : base(header)
	{
	}

	protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotSupportedException("This method is not supported");

	protected override bool AllowNewCore => false;

	protected override bool AllowRemoveCore => false;
}
