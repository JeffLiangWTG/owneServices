using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(DCAParameters))]
	sealed class DCAParametersTest : RegistryBusinessObjectTemplateTestCase<DCAParameters>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override DCAParameters GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

		protected override DCAParameters GetBusinessObjectToSerialise() => new DCAParameters(null, Factory);

		public void TestCaptions()
		{
			AssertCaptions(nameof(DCAParameters.PeekWay), "Peek Way");
			AssertCaptions(nameof(DCAParameters.AllServices), "All Services");
			AssertCaptions(nameof(DCAParameters.SpecificServices), "Specific Services");
			AssertCaptions(nameof(DCAParameters.MaxMessagesPerIteration), "Maximum Messages Per Iteration");
		}

		public void TestPeekWay()
		{
			AssertEquals("3", parameters.PeekWay);
			AssertEquals(false, parameters.AllServices);

			parameters.PeekWay = "2";
			AssertEquals(true, parameters.AllServices);
		}

		public void TestPeekWayLookups()
		{
			AssertType<PeekWayList>(parameters.PeekWayLookups);
		}

		public void TestAllServices()
		{
			parameters.AllServices = true;
			AssertEquals(0, parameters.Services.Count);

			parameters.AllServices = false;
			AssertEquals(5, parameters.Services.Count);
		}

		public void TestAllServicesReadOnly()
		{
			AssertEquals("AllServices should be read-only", true, parameters.AllServicesInfo.ReadOnly);
		}

		public void TestSpecificServicesReadOnly()
		{
			parameters.PeekWay = "2";
			AssertEquals(true, parameters.SpecificServicesInfo.ReadOnly);

			parameters.PeekWay = "3";
			AssertEquals(true, parameters.SpecificServicesInfo.ReadOnly);
		}

		void AssertCaptions(string propertyName, string caption)
		{
			AssertEquals($"{propertyName} Caption", caption, DataBoundResourceStrings.GetDataForProperty(typeof(DCAParameters), propertyName).Caption);
		}

		protected override void SetUp()
		{
			base.SetUp();
			parameters = new DCAParameters(null, Factory);
		}
		DCAParameters parameters;
	}
}
