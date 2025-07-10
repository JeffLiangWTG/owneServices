using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(FRNctsDepartureHeaderContainer))]
	public class FRNctsDepartureHeaderContainerTest : Customs.Business.Testing.BaseCusInBondContainerTest<FRNctsDepartureHeaderContainer>
	{
		public void TestGetTypeForLoad()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var container = header.DepartureHeaderContainers.AddNew();
			Factory.Save();

			var newFactory1 = new BusinessObjectFactory();
			AssertType<FRNctsDepartureHeaderContainer>(newFactory1.Load<EU.NCTS.Business.NctsDepartureHeaderContainer>(container.PK));

			var newFactory2 = new BusinessObjectFactory();
			AssertType<FRNctsDepartureHeaderContainer>(newFactory2.Load<EU.NCTS.Business.NctsCusInBondContainer>(container.PK));

			var newFactory3 = new BusinessObjectFactory();
			AssertType<FRNctsDepartureHeaderContainer>(newFactory3.Load<BaseCusInBondContainer>(container.PK));
		}

		public void TestLookups()
		{
			var (_, container) = GetHeaderAndContainer();
			AssertType<FRNctsDepartureHeaderContainerLookups>(container.Lookups);
		}

		public void TestValidationType_Phase4()
		{
			var (header, container) = GetHeaderAndContainer();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertType<FRNctsDepartureHeaderContainerPhase4Validation>(container.Validation);
		}

		public void TestValidationType_Phase5()
		{
			var (header, container) = GetHeaderAndContainer();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<FRNctsDepartureHeaderContainerPhase5Validation>(container.Validation);
		}

		public void TestIsContainerised()
		{
			var (_, container) = GetHeaderAndContainer();
			CombineAssertions("IsContainerised should be true only when BC_Mode is FCL, LCL or Containerised.", () =>
			{
				container.BC_Mode = Core.Constants.ContainerModes.FCL;
				AssertEquals("FCL", true, container.IsContainerised);

				container.BC_Mode = Core.Constants.ContainerModes.LCL;
				AssertEquals("LCL", true, container.IsContainerised);

				container.BC_Mode = Core.Constants.ContainerModes.Containerised;
				AssertEquals("Containerised", true, container.IsContainerised);

				container.BC_Mode = Core.Constants.ContainerModes.Liquid;
				AssertEquals("Liquid", false, container.IsContainerised);

				container.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				AssertEquals("NonContainerised", false, container.IsContainerised);
			});
		}

		(NctsHeader, FRNctsDepartureHeaderContainer) GetHeaderAndContainer()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var container = header.DepartureHeaderContainers.AddNew();
			return (header, container);
		}

		protected override BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			return header.DepartureHeaderContainers.AddNew();
		}
	}
}
