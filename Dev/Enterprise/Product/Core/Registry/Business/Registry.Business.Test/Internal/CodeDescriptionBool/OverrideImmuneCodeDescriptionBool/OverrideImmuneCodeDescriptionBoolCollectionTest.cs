using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OverrideImmuneCodeDescriptionBoolCollection))]
	sealed class OverrideImmuneCodeDescriptionBoolCollectionTest : RegistryBusinessObjectCollectionTestCase<OverrideImmuneCodeDescriptionBoolCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override OverrideImmuneCodeDescriptionBoolCollection GetCollectionToTest()
		{
			return new OverrideImmuneCodeDescriptionBoolCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OverrideImmuneCodeDescriptionBool();
		}

		#endregion
	}
}
