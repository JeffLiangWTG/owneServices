using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class SQLFilterProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			LookupField master = new LookupField(Factory);
			LookupField detail = new LookupField(Factory);

			TestFilterProvider provider = new TestFilterProvider(master, detail);

			AssertEquals(master, provider.ExposeMaster);
			AssertEquals(detail, provider.ExposeDetail);
			AssertEquals(true, provider.ValidateCalledOnConstruction);
		}

		#region TestFilterProvider

		internal class TestFilterProvider : SQLFilterProvider
		{
			public TestFilterProvider(LookupFilterFieldBase masterFilter, LookupFilterFieldBase detailFilter) : base(masterFilter, detailFilter) { }
			public bool ValidateCalledOnConstruction;
			public LookupFilterFieldBase ExposeMaster { get { return base.MasterFilter; } }
			public LookupFilterFieldBase ExposeDetail { get { return base.DetailFilter; } }
			public override ZQuery Filter { get { return new ZQuery(); } }
			public override void ValidateMasterAndDetailFilterTypes() { ValidateCalledOnConstruction = true; }
		}

		#endregion
	}
}
