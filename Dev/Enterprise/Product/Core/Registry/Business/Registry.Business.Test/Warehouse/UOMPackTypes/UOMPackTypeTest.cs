using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(UOMPackType))]
	sealed class UOMPackTypeTest : RegistryBusinessObjectTemplateTestCase<UOMPackType>
	{
		#region TestProperties

		public void TestProperties()
		{
			var packType = new UOMPackType()
			{
				Code = "CAS",
				Description = (NoResString)"Case",
				NumberOfLabels = 1
			};
			AssertEquals("CAS", packType.Code);
			AssertEquals("Case", packType.Description);
			AssertEquals(1, packType.NumberOfLabels);
		}

		#endregion

		#region TestValidationCode

		public void TestValidationNumberOfLabels()
		{
			var packTypes = new UOMPackTypeCollection();
			var packType1 = packTypes.AddNew();
			var packType2 = packTypes.AddNew();
			var packType3 = packTypes.AddNew();
			packType1.NumberOfLabels = 0;
			packType2.NumberOfLabels = 2;
			packType3.NumberOfLabels = -1;

			AssertHasErrors(packType1.NumberOfLabelsInfo);
			AssertNoErrors(packType2.NumberOfLabelsInfo);
			AssertHasErrors(packType3.NumberOfLabelsInfo);
		}

		#endregion

		#region Implementation

		protected override UOMPackType GetBusinessObjectToClone()
		{
			return new UOMPackType();
		}

		protected override UOMPackType GetBusinessObjectToSerialise()
		{
			return new UOMPackType();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
