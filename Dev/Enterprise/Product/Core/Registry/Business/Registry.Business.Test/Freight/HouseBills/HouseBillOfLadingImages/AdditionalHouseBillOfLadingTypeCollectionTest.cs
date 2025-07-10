using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AdditionalHouseBillOfLadingTypeCollection))]
	sealed class AdditionalHouseBillOfLadingTypeCollectionTest : RegistryBusinessObjectCollectionTestCase<AdditionalHouseBillOfLadingTypeCollection>
	{
		#region Implementation

		protected override AdditionalHouseBillOfLadingTypeCollection GetCollectionToTest()
		{
			return new AdditionalHouseBillOfLadingTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AdditionalHouseBillOfLadingType();
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
