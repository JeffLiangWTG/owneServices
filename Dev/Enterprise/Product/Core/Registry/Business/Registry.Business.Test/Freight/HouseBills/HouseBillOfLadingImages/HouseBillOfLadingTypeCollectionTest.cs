using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HouseBillOfLadingTypeCollection))]
	sealed class HouseBillOfLadingTypeCollectionTest : RegistryBusinessObjectCollectionTestCase<HouseBillOfLadingTypeCollection>
	{
		#region Implementation

		protected override HouseBillOfLadingTypeCollection GetCollectionToTest()
		{
			return new HouseBillOfLadingTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new HouseBillOfLadingType();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
