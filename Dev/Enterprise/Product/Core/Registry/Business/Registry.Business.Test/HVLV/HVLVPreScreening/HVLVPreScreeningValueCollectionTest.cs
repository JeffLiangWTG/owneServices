using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVPreScreeningValueCollection))]
	sealed class HVLVPreScreeningValueCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<HVLVPreScreeningValueCollection>
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

		protected override HVLVPreScreeningValueCollection GetCollectionToTest()
		{
			return new HVLVPreScreeningValueCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new HVLVPreScreeningValue();
		}

		#endregion
	}
}
