using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(DeliveryInstructionsHelper))]
	sealed class DeliveryInstructionsHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			DeliveryInstructions instruction = new DeliveryInstructions();
			return DeliveryInstructionsHelper.New(instruction);
		}
	}
}
