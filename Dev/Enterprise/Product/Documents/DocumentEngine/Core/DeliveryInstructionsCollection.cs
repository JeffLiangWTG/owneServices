
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine
{
	public class DeliveryInstructionsCollection : NonPersistentBusinessObjectCollection<DeliveryInstructions>
	{
		public DeliveryInstructionsCollection()
			: base(new BusinessObjectFactory())
		{
		}

		public DeliveryInstructionsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DeliveryInstructions();
		}
	}
}
