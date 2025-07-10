using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalNCTS5TranshipmentWrapperTest : WrapperHelperTest<ArrivalNCTS5TranshipmentWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if enRouteIncident is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "enRouteIncident"), () => GetWrapper(null));
			});
		}

		public void TestContainerIndicator()
		{
			CombineAssertions(() =>
			{
				var nctsContainer2 = enRouteIncident.IncidentContainers.AddNew();

				nctsContainer.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				nctsContainer2.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				AssertEquals("Expected filled ContainerIndicator with 2 NCT", false, wrapper.ContainerIndicator);

				nctsContainer.BC_Mode = Core.Constants.ContainerModes.Containerised;
				AssertEquals("Expected filled ContainerIndicator with CNT and NCT", false, wrapper.ContainerIndicator);

				nctsContainer2.BC_Mode = Core.Constants.ContainerModes.Containerised;
				AssertEquals("Expected filled ContainerIndicator with 2 CNT", true, wrapper.ContainerIndicator);

				nctsContainer.Delete();
				nctsContainer2.Delete();
				AssertEquals("Expected filled ContainerIndicator when no container define", false, wrapper.ContainerIndicator);
			});
		}

		public void TestTransportMeans()
		{
			CombineAssertions(() =>
			{
				wrapper = GetWrapper(enRouteIncident);
				var transportMeans = wrapper.TransportMeans;

				AssertNotNull("Expected filled TransportMeans", transportMeans);
				AssertSame("Cached TransportMeans", wrapper.TransportMeans, transportMeans);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			enRouteIncident = nctsHeader.EnRouteIncidents.AddNew();
			nctsContainer = enRouteIncident.IncidentContainers.AddNew();

			wrapper = GetWrapper(enRouteIncident);
		}

		ArrivalNCTS5TranshipmentWrapper wrapper;
		EnRouteIncident enRouteIncident;
		NctsContainer nctsContainer;

		ArrivalNCTS5TranshipmentWrapper GetWrapper(EnRouteIncident enRouteIncident) => new ArrivalNCTS5TranshipmentWrapper(enRouteIncident);

		protected override ArrivalNCTS5TranshipmentWrapper GetProvider() => wrapper;
	}
}
