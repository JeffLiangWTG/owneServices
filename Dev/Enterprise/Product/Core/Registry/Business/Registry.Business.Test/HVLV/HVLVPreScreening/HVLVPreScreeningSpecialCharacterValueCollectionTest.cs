using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVPreScreeningSpecialCharacterValueCollection))]
	sealed class HVLVPreScreeningSpecialCharacterValueCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<HVLVPreScreeningSpecialCharacterValueCollection>
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

		protected override HVLVPreScreeningSpecialCharacterValueCollection GetCollectionToTest()
		{
			return new HVLVPreScreeningSpecialCharacterValueCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new HVLVPreScreeningSpecialCharacterValue();
		}

		#endregion
	}
}
