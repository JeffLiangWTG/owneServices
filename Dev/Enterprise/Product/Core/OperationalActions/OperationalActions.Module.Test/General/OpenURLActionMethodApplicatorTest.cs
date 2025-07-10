using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	[TestedType(typeof(OpenURLActionMethodApplicator))]
	sealed class OpenURLActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestOpenURL()
		{
			var applicator = new OpenURLActionMethodApplicator(Settings, Factory);
			var log = new DummyOperationalActionSectionLog();
			AssertEquals("Precondition", 0, applicator.resolvedURLs.Count);
			applicator.Apply(log, new[] { GetDataBusinessObject("dummy1"), GetDataBusinessObject("dummy2") });
			AssertEquals(2, applicator.resolvedURLs.Count);
			AssertEquals("http://www.cargowise.com/#q=dummy1", applicator.resolvedURLs[0]);
			AssertEquals("http://www.cargowise.com/#q=dummy2", applicator.resolvedURLs[1]);
		}

		#region Implementation

		public OpenURLActionMethodSettings Settings
		{
			get { return new OpenURLActionMethodSettings { URL = "http://www.cargowise.com/#q=<Z0_NVarChar>" }; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OpenURLActionMethodApplicator(Settings, Factory);
		}

		BusinessObject GetDataBusinessObject(string name)
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			dummy.Z0_NVarChar = name;
			return dummy;
		}

		#endregion
	}
}
