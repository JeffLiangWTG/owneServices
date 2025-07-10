using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RefDocTypeEntryCollection))]
	sealed class RefDocTypeEntryCollectionTest : RegistryBusinessObjectCollectionTestCase<RefDocTypeEntryCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override RefDocTypeEntryCollection GetCollectionToTest()
		{
			return new RefDocTypeEntryCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RefDocTypeEntry();
		}

		#endregion
	}
}
