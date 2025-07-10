using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeableWeightRoundingCollection))]
	sealed class ChargeableWeightRoundingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ChargeableWeightRoundingCollection>
	{
		public void TestGetDefault()
		{
			ChargeableWeightRoundingCollection defaultValue = ChargeableWeightRoundingCollection.GetDefault();
			AssertEquals(1, defaultValue.Count);

			AssertEquals("None", defaultValue[0].RoundingMode);
			AssertEquals("0.5", defaultValue[0].RoundingScale);
		}

		public void TestAllowNew()
		{
			AssertEquals("Must not allow new rows", false, this.Collection.AllowNew);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ChargeableWeightRoundingCollection GetCollectionToTest()
		{
			return new ChargeableWeightRoundingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ChargeableWeightRounding();
		}

		#endregion
	}
}
