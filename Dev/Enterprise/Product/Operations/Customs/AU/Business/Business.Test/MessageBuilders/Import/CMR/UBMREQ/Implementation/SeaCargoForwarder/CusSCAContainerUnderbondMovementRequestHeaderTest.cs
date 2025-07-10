using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAContainerUnderbondMovementRequestHeaderTest : CusSCAOceanBillUnderbondMovementRequestHeaderAbstractTest
	{
		public void TestBulkContainers()
		{
			Container.CN_ContainerMode = Core.Constants.ContainerModes.LCL;
			Underbond.C4_PackageType = "YC";
			Underbond.C4_PiecesManifested = 1;
			AssertEquals("Number Of Packages", 1, RequestHeader.NumberOfPackages);
			AssertEquals("Pack Type", "YC", RequestHeader.PackageType);
			Container.CN_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("Number Of Packages", 0, RequestHeader.NumberOfPackages);
			AssertEquals("Pack Type", ZString.Empty, RequestHeader.PackageType);
		}

		public void TestTranshipmentPort()
		{
			Underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			Underbond.C4_RL_NKTranshipDestPort = "CNSHA";
			AssertEquals("Transhipment Port comes from field on underbond", "CNSHA", RequestHeader.TranshipmentOverseasDestinationPort);
		}

		public void TestLine()
		{
			Container.CN_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("RequestHeader.Line Type", typeof(CusSCAContainerUnderbondMovementRequestLine), RequestHeader.Line.GetType());
			Container.CN_ContainerMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("RequestHeader.Line Type", typeof(CusSCABulkUnderbondMovementRequestLine), RequestHeader.Line.GetType());
			Container.CN_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("RequestHeader.Line Type", typeof(CusSCABulkUnderbondMovementRequestLine), RequestHeader.Line.GetType());
		}

		protected override IUnderbondMovementRequestHeader RequestHeader => new CusSCAContainerUnderbondMovementRequestHeader(Underbond, Container);

		CusSCAContainer container;
		CusSCAContainer Container => container ?? (container = SCAOcean.Containers.AddNew());
	}
}
