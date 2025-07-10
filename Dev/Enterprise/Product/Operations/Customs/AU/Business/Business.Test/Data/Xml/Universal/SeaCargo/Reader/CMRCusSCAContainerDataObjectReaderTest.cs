namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	partial class CMRCusSCAOceanBillDataObjectReaderTest
	{
		protected override void AssertContainer1AllPropertiesSet(CusSCAContainer container)
		{
			base.AssertContainer1AllPropertiesSet(container);
			Assert(container.CN_ShipperOwnedContainer);
			AssertEquals("CLR", container.CN_ContainerStatus);
		}

		protected override void AssertContainer2AllPropertiesSet(CusSCAContainer container)
		{
			base.AssertContainer2AllPropertiesSet(container);
			AssertEquals(CMRContainerTypesForDataTransfer.Codes.GeneralPurposeNonVentedANonVentilatedContainerUsedToTransportCargo, container.CN_TypeOfContainer);
			AssertEquals("0000", container.CN_ContainerSizeOrISOCode);
		}
	}
}
