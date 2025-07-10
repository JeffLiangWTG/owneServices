using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs.TemporaryStorage;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class RegLineItemQuantityCollection : NonPersistentBusinessObjectCollection<RegLineItemQuantity>, IRegLineItemQuantityCollection<RegLineItemQuantity>
{
	public RegLineItemQuantityCollection() : base()
	{
	}

	protected override bool AllowNewCore => false;

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		throw new InvalidOperationException("This method should not be called.");
	}
}
