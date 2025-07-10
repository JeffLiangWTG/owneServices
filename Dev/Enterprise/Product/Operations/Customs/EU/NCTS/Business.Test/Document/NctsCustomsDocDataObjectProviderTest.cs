using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Documents.DocDataObjects.Testing
{
	class NctsCustomsDocDataObjectProviderTest : TestCaseWithFactory
	{
		public void TestGetDocDataObjectNctsHeaderDataContext()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var parameters = new CustomsDocDataObjectProviderParametersTest { };
			var dataObject = customsDocDataObjectProvider.GetDocDataObject(nctsHeader, "FRPortsRegularizationTransitDOA", parameters);
			AssertNull(dataObject);
		}

		protected override void SetUp()
		{
			base.SetUp();
			customsDocDataObjectProvider = new NctsCustomsDocDataObjectProvider();
		}

		NctsCustomsDocDataObjectProvider customsDocDataObjectProvider;
	}

	class CustomsDocDataObjectProviderParametersTest : IDocDataObjectParameters
	{
		public string DocumentTitle { get; set; }
		public string DataStoreName { get; set; }
		public object Data { get; set; }
		public IStmALogProvider LogProvider { get; set; }

		IStmALogProvider IDocDataObjectParameters.LogProvider => LogProvider;
	}
}
