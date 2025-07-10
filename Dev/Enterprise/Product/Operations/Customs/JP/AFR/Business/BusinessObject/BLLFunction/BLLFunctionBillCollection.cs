using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class BLLFunctionBillCollection : NonPersistentBusinessObjectCollection<BLLFunctionBill>
	{
		public BLLFunctionBillCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new System.NotImplementedException();
	}
}
