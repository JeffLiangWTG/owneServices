using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	class NctsCustomsDocDataObjectProviderTest : TestCaseWithFactory
	{
		public void TestGetDocDataObjectNctsHeaderDataContext()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Departure);
			nctsHeader.BH_JobReference = "NCT002300";
			var parameters = new CustomsDocDataObjectProviderParametersTest { };
			var customsDocDataObjectProvider = new NctsCustomsDocDataObjectProvider();
			var dataObject = customsDocDataObjectProvider.GetDocDataObject(nctsHeader, DataContext.FRPortsRegularizationTransitDOA, parameters);
			AssertType<DOADataObject>(dataObject);
			var doa = (DOADataObject)dataObject;
			AssertEquals("We should call DOABuilder.Build to populate the DOA object.", "NCT002300", doa.JobNumber);

			dataObject = customsDocDataObjectProvider.GetDocDataObject(nctsHeader, DataContext.FRPortsCustomsCheckCAED, parameters);
			AssertType<CAEDDataObject>(dataObject);
			var caed = (CAEDDataObject)dataObject;
			AssertEquals("We should call CAEDBuilder.Build to populate the CAED object.", "NCT002300", caed.JobNumber);
		}
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
