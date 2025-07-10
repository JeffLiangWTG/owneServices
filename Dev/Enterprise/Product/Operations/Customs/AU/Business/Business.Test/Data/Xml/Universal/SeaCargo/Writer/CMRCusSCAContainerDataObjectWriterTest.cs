namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	partial class CMRCusSCAOceanBillDataObjectWriterTest
	{
		protected override CusSCAContainer AddContainer1(CusSCAOceanBill oceanBill)
		{
			var result = base.AddContainer1(oceanBill);
			result.CN_SealNumber = "SEAL1";
			result.CN_ShipperOwnedContainer = true;
			result.CN_ContainerStatus = "CLR";
			return result;
		}

		protected override CusSCAContainer AddContainer2(CusSCAOceanBill oceanBill)
		{
			var result = base.AddContainer2(oceanBill);
			result.CN_TypeOfContainer = CMRContainerTypesForDataTransfer.Codes.GeneralPurposeNonVentedANonVentilatedContainerUsedToTransportCargo;
			result.CN_ContainerSizeOrISOCode = "0000";
			return result;
		}
	}
}
