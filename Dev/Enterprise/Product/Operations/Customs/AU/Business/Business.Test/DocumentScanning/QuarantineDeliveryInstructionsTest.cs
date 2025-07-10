using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineDeliveryInstructions))]
	sealed class QuarantineDeliveryInstructionsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocDeliveryContactCollection()
		{
			var instructions = new QuarantineDeliveryInstructions();
			AssertType<DocDeliveryContactCollection>(instructions.Recipients);
		}

		protected override BusinessObject GetNewBusinessObject() => new QuarantineDeliveryInstructions();
	}
}
