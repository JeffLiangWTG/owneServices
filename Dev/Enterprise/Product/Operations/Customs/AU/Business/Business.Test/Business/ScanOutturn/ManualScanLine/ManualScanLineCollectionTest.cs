using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ManualScanLineCollection))]
	sealed class ManualScanLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ManualScanLineCollection>
	{
		protected override ManualScanLineCollection GetCollectionToTest()
		{
			return new ManualScanLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ManualScanLine();
		}
	}
}
