using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	public class NctsCustomsStatusMapperTest : TestCaseWithFactory
	{
		public void TestNctsCustomsStatusMapper()
		{
			CombineAssertions(() =>
			{
				var mappedStatusCode = ZString.Empty;

				AssertEquals("Mapping for Code = '' not successful", (true, NctsTransitStatusList.Codes.Unknown), NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus(""));
				AssertEquals("Mapping for Code = '12' not successful", (true, NctsTransitStatusList.Codes.DeclarationMrnAllocated), NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus("12"));
				AssertEquals("Mapping for Code = '13' not successful", (true, NctsTransitStatusList.Codes.DeclarationRejected), NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus("13"));
				AssertEquals("Mapping for Code = '14' not successful", (true, NctsTransitStatusList.Codes.DeclarationMrnAllocated), NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus("14"));
				AssertEquals("Mapping for Code = '15' not successful", (true, NctsTransitStatusList.Codes.GoodsNotReleasedForTransit), NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus("15"));
				AssertEquals("Mapping for Code = '31' not successful", (true, NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture), NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus("31"));
				AssertEquals("Mapping for Code = '34' not successful", (true, NctsTransitStatusList.Codes.GoodsWrittenOff), NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus("34"));
				AssertEquals("Mapping for Code = '35' not successful", (true, NctsTransitStatusList.Codes.GoodsWrittenOff), NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus("35"));
				AssertEquals("Mapping for Code = '38' not successful", (true, NctsTransitStatusList.Codes.DeclarationCancelled), NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus("38"));
				AssertEquals("Mapping for Code = '54' not successful", (true, NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted), NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus("54"));
				AssertEquals("Mapping for Code = '56' not successful", (true, NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease), NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus("56"));
				AssertEquals("Mapping for Code = '311' not successful", (true, NctsTransitStatusList.Codes.DeclarationDataRequested), NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus("311"));
				AssertEquals("Mapping for Code = '343' not successful", (true, NctsTransitStatusList.Codes.ControlResultCaptured), NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus("343"));
				AssertEquals("Mapping for Code = '361' not successful", (true, NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease), NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus("361"));
				AssertEquals("Mapping for non existing Code", false, NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus("1").success);
			});
		}
	}
}
