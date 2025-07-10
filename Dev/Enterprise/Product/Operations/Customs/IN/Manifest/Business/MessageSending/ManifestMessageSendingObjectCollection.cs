using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Manifest.Business;

public class ManifestMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<ManifestMessageSendingObject>
{
	public ManifestMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		throw new NotImplementedException();
	}

	protected override bool AllowNewCore => false;
}
