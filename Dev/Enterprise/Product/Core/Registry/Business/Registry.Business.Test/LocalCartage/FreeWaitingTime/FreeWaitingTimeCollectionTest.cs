using CargoWise.EntityFramework;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FreeWaitingTimeCollection))]
	sealed class FreeWaitingTimeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<FreeWaitingTimeCollection>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override FreeWaitingTimeCollection GetCollectionToTest()
		{
			return new FreeWaitingTimeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FreeWaitingTime();
		}

		public void TestDefaultsForNewChild()
		{
			var collection = GetCollectionToTest();
			AssertEquals(Constants.EquipmentNeeded.Any, collection.AddNew().DropMode);
		}
	}
}
