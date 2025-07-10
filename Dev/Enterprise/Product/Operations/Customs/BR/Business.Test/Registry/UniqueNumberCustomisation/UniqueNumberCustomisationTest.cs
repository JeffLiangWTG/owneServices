using System.Linq;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Registry.Testing
{
	[TestedType(typeof(UniqueNumberCustomisation))]
	sealed class UniqueNumberCustomisationTest : RegistryBusinessObjectTemplateTestCase<UniqueNumberCustomisation>
	{
		public void TestSupportsDirection()
		{
			var uniqueNumberCustomisation = new UniqueNumberCustomisation(true);
			AssertEquals("SupportDirection should be true", true, uniqueNumberCustomisation.SupportsDirection);
			AssertEquals(1, uniqueNumberCustomisation.Elements.Where(x => x.Key == BillOfLadingNumberCustomisationElement.Keys.Direction).Count());
			uniqueNumberCustomisation.SupportsDirection = false;
			AssertEquals(0, uniqueNumberCustomisation.Elements.Where(x => x.Key == BillOfLadingNumberCustomisationElement.Keys.Direction).Count());

			var uniqueNumberCustomisation2 = new UniqueNumberCustomisation(false);
			AssertEquals("SupportDirection should be false", false, uniqueNumberCustomisation2.SupportsDirection);

			AssertEquals(0, uniqueNumberCustomisation2.Elements.Where(x => x.Key == BillOfLadingNumberCustomisationElement.Keys.Direction).Count());
			uniqueNumberCustomisation2.SupportsDirection = true;
			AssertEquals(1, uniqueNumberCustomisation2.Elements.Where(x => x.Key == BillOfLadingNumberCustomisationElement.Keys.Direction).Count());
		}

		public void TestCheckDigitAlgorithm()
		{
			var uniqueNumberCustomisation = new UniqueNumberCustomisation();

			AssertEquals("CheckDigitAlgorithm should be read-only", true, uniqueNumberCustomisation.CheckDigitAlgorithmInfo.ReadOnly);
		}

		#region Implementation
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override UniqueNumberCustomisation GetBusinessObjectToClone()
		{
			return new UniqueNumberCustomisation();
		}

		protected override UniqueNumberCustomisation GetBusinessObjectToSerialise()
		{
			return new UniqueNumberCustomisation();
		}
		#endregion
	}
}
