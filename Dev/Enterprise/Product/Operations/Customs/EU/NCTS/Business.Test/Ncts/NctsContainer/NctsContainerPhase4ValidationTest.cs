using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsContainerPhase4ValidationTest : TestCaseWithFactory
	{
		public void TestCheckBC_Mode()
		{
			var expectedError = "Please enter a Container/Equipment Mode.";

			CombineAssertions(() =>
			{
				var incident = CreateArrivalIncident();
				var container = incident.IncidentContainers.AddNew();
				container.BC_ContainerNum = "CRXU1234561";
				container.Validation.ValidateBC_Mode();
				AssertHasErrorContaining(container.BC_ModeInfo, expectedError);

				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				var containerUnloadingMovementHeader = header.UnloadingMovementHeader.GoodsItems.AddNew().Containers.AddNew();
				containerUnloadingMovementHeader.BC_ContainerNum = "CRXU1234561";
				containerUnloadingMovementHeader.Validation.ValidateBC_Mode();
				AssertNoErrorContaining(containerUnloadingMovementHeader.BC_ModeInfo, expectedError);
			});
		}

		public void TestCheckContainerNumberIsUnique()
		{
			var expectedError = "Duplicate Container Number is entered.";

			var incident = CreateArrivalIncident();

			var first = incident.IncidentContainers.AddNew();
			first.BC_Mode = Core.Constants.ContainerModes.Containerised;
			first.BC_ContainerNum = "MSKU1234565";

			var second = incident.IncidentContainers.AddNew();
			second.BC_Mode = Core.Constants.ContainerModes.Containerised;
			second.BC_ContainerNum = "MSKU1234565";

			CombineAssertions(() =>
			{
				AssertHasMessageError("Container number is duplicated", second.BC_ContainerNumInfo, expectedError);

				second.BC_ContainerNum = "MSKU1234570";
				AssertNoMessageError("Container number is unique", second.BC_ContainerNumInfo, expectedError);
			});
		}

		public void TestCheckContainerNumberIsUniqueShouldBeIgnoredForNonContainerisedMode()
		{
			var expectedError = "Duplicate Container Number is entered.";

			var incident = CreateArrivalIncident();

			var first = incident.IncidentContainers.AddNew();
			first.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			first.BC_ContainerNum = "MSKU1234565";

			var second = incident.IncidentContainers.AddNew();
			second.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
			second.BC_ContainerNum = "MSKU1234565";

			CombineAssertions(() =>
			{
				AssertNoMessageError("Mode is 'NonConteinerised'", second.BC_ContainerNumInfo, expectedError);

				second.BC_Mode = Core.Constants.ContainerModes.Containerised;
				second.BC_ContainerNum = "MSKU1234565";
				AssertHasMessageError("Mode is 'Conteinerised'", second.BC_ContainerNumInfo, expectedError);
			});
		}

		public void TestCheckSealNumberIsUniqueForSeal1()
		{
			var expectedWarning = "Duplicate Seal 1 Number entered.";

			var incident = CreateArrivalIncident();
			var container = incident.IncidentContainers.AddNew();
			container.BC_Seal2 = "222";

			CombineAssertions(() =>
			{
				container.BC_Seal1 = "222";
				AssertHasWarning("Seal1 duplicates Seal2", container.BC_Seal1Info, expectedWarning);

				container.BC_Seal1 = "111";
				AssertNoWarning("Seal1 is unique", container.BC_Seal1Info, expectedWarning);
			});
		}

		public void TestCheckSealNumberIsUniqueForSeal2()
		{
			var expectedWarning = "Duplicate Seal 2 Number entered.";

			var incident = CreateArrivalIncident();
			var container = incident.IncidentContainers.AddNew();
			container.BC_Seal1 = "111";

			CombineAssertions(() =>
			{
				container.BC_Seal2 = "111";
				AssertHasWarning("Seal2 duplicates Seal1", container.BC_Seal2Info, expectedWarning);

				container.BC_Seal2 = "222";
				AssertNoWarning("Seal2 is unique", container.BC_Seal2Info, expectedWarning);
			});
		}

		EnRouteIncident CreateArrivalIncident()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			return header.EnRouteIncidents.AddNew();
		}
	}
}
