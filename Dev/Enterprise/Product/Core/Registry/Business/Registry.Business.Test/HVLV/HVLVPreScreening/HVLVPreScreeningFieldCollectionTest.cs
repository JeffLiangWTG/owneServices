using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVPreScreeningFieldCollection))]
	sealed class HVLVPreScreeningFieldCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<HVLVPreScreeningFieldCollection>
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

		protected override HVLVPreScreeningFieldCollection GetCollectionToTest()
		{
			return new HVLVPreScreeningFieldCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new HVLVPreScreeningField(null);
		}

		#endregion
	}
}
