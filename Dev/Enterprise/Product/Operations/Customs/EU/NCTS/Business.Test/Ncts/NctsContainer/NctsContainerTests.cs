using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using NUnit.Framework;
using IContainer = Enterprise.Customs.Business.Interfaces.IContainer;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsContainer))]
	public class NctsContainerTests : Customs.Business.Testing.CusInBondContainerTest<NctsContainer>
	{
		public void TestParent()
		{
			var nctsContainer = Factory.New<NctsContainer>();
			AssertNull(nctsContainer.Parent);
		}

		public void TestParent_NctsCargoDesc()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			AssertType<NctsArrivalAndUnloadingCargoDesc>(nctsContainer.Parent);
		}

		public void TestParent_Incident()
		{
			(var header, _) = CreateNewContainer(Factory);
			var incident = header.EnRouteIncidents.AddNew();
			var nctsContainer = incident.IncidentContainers.AddNew();
			AssertType<EnRouteIncident>(nctsContainer.Parent);
		}

		public void TestParent_Transshipment()
		{
			(var header, _) = CreateNewContainer(Factory);
			var transshipment = header.EnRouteTransshipments.AddNew();
			var nctsContainer = transshipment.Containers.AddNew();
			AssertType<EnRouteTransshipment>(nctsContainer.Parent);
		}
		public void TestTypeDecider() => AssertType<NctsContainerTypeDecider>(NctsContainer.TypeDecider);

		public void TestContainerNumber()
		{
			CombineAssertions(() =>
			{
				(_, var nctsContainer) = CreateNewContainer(Factory);
				AssertEquals("GET", "TURE1235985", nctsContainer.ContainerNumber);
				nctsContainer.ContainerNumber = "Container1";
				AssertEquals("SET", "Container1", nctsContainer.BC_ContainerNum);
			});
		}

		public void TestPhase5() => CombineAssertions(() =>
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var incident = header.EnRouteIncidents.AddNew();
			var nctsIncidentContainer = incident.IncidentContainers.AddNew();
			Assert("NCTS5", nctsIncidentContainer.IsPhase5);

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("NCTS4", false, nctsIncidentContainer.IsPhase5);

			var containerCargoDesc = header.UnloadingMovementHeader.GoodsItems.AddNew().Containers.AddNew();
			AssertEquals("NCTS4Unloading", false, containerCargoDesc.IsPhase5);
		});

		public void TestContainerNumberMaxLength()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			AssertEquals(17, nctsContainer.BC_ContainerNumInfo.MaxLength);
			AssertEquals(17, nctsContainer.ContainerNumberInfo.MaxLength);
		}

		public void TestIContainer()
		{
			CombineAssertions(() =>
			{
				(_, var nctsContainer) = CreateNewContainer(Factory);
				var provider = (IContainer)nctsContainer;
				AssertEquals("ContainerNumber", "TURE1235985", provider.ContainerNumber);
				AssertEquals("IsForInvoiceLine", true, provider.IsForInvoiceLine);
			});
		}

		public void TestBC_TypeOfService()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			AssertEquals(ContainerTypeOfServiceList.Codes.DepartureContainer, nctsContainer.BC_TypeOfService);
		}

		public void TestHeader()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			AssertType<NctsHeader>(nctsContainer.Header);
		}

		public void TestCommodities()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			AssertType<NctsCommonCargoDescCollection<NctsCommonCargoDesc>>(nctsContainer.Commodities);
		}

		public void TestMoveDetail()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			AssertNull(nctsContainer.MoveDetail);
		}

		public void TestLookups()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			AssertType<NctsContainerLookups>(nctsContainer.Lookups);
		}

		public void TestSupportsClone()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			AssertEquals(true, nctsContainer.SupportsClone());
		}

		public void TestSeals()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			var seals = nctsContainer.Seals;
			CombineAssertions(() =>
			{
				AssertType<CusSealCollection>("Type", seals);
				AssertEquals("IsRegisteredEditableChildObject", true, nctsContainer.IsRegisteredEditableChildObject(seals));
				AssertSame("Cached", seals, nctsContainer.Seals);
				AssertEquals("IsLoaded", true, seals.IsLoaded);
			});
		}

		public void TestBC_ModeCaption()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			NCTSTestHelper.AssertCaptions(nctsContainer.BC_ModeInfo, "Container/Equipment Mode", "Container Mode", "Mode");
		}

		public void TestCusSealType()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			AssertType(((ICusSealTypeSupporter)nctsContainer).CusSealType, nctsContainer.Seals.AddNew());
		}

		public void TestBC_ContainerNumCaption()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			NCTSTestHelper.AssertCaptions(nctsContainer.BC_ContainerNumInfo, "Container/Equipment Number", "Container/Equipment No.", "Container/Equipment");
		}

		public void TestBC_Seal1Caption()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			NCTSTestHelper.AssertCaptions(nctsContainer.BC_Seal1Info, "Seal 1 Number", string.Empty, "Seal 1");
		}

		public void TestBC_Seal2Caption()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			NCTSTestHelper.AssertCaptions(nctsContainer.BC_Seal2Info, "Seal 2 Number", string.Empty, "Seal 2");
		}

		public void TestTotalSealCountCaption()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			NCTSTestHelper.AssertCaptions(nctsContainer.TotalSealCountInfo, "Seal Quantity", "Seal Qty.", "Seal Qty.");
		}

		public void TestTotalSealCount()
		{
			CombineAssertions(() =>
			{
				(_, var nctsContainer) = CreateNewContainer(Factory);
				nctsContainer.BC_Seal1 = "aaa";
				AssertEquals("1st seal", 1, nctsContainer.TotalSealCount);
				nctsContainer.BC_Seal2 = "aaa";
				AssertEquals("2nd seal", 2, nctsContainer.TotalSealCount);
				nctsContainer.Seals.AddNew();
				AssertEquals("Additional seal", 3, nctsContainer.TotalSealCount);
				nctsContainer.Seals.AddNew();
				AssertEquals("Additional seal", 4, nctsContainer.TotalSealCount);
			});
		}

		public void TestNctsContainerItems()
		{
			(_, var nctsContainer) = CreateNewContainer(Factory);
			var nctsContainerItems = nctsContainer.ItemNumbers;
			CombineAssertions(() =>
			{
				AssertType<NctsContainerItemCollection>("Type", nctsContainerItems);
				AssertEquals("IsRegisteredEditableChildObject", true, nctsContainer.IsRegisteredEditableChildObject(nctsContainerItems));
				AssertSame("Cached", nctsContainerItems, nctsContainer.ItemNumbers);
				AssertEquals("IsLoaded", true, nctsContainerItems.IsLoaded);
			});
		}

		public void TestValidation_Phase4()
		{
			var (header, _) = CreateNewContainer(Factory);
			header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			var incident = header.EnRouteIncidents.AddNew();
			var container = incident.IncidentContainers.AddNew();
			AssertType<NctsContainerPhase4Validation>(container.Validation);
		}

		public void TestValidation_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var incident = header.EnRouteIncidents.AddNew();
			var nctsIncidentContainer = incident.IncidentContainers.AddNew();
			AssertType<NctsContainerPhase5Validation>(nctsIncidentContainer.Validation);
		}

		public void TestContainerNumber_ReadOnly()
		{
			(var header, var nctsContainer) = CreateNewContainer(Factory);
			header.BH_ExportFlag = EventFlagList.Codes.Yes;
			var incident = header.EnRouteIncidents.AddNew();
			nctsContainer = incident.IncidentContainers.AddNew();
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(nctsContainer.ContainerNumberInfo, header);
		}

		public void TestBC_Mode_ReadOnly()
		{
			(var header, var nctsContainer) = CreateNewContainer(Factory);
			header.BH_ExportFlag = EventFlagList.Codes.Yes;
			var incident = header.EnRouteIncidents.AddNew();
			nctsContainer = incident.IncidentContainers.AddNew();
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(nctsContainer.BC_ModeInfo, header);
		}

		public void TestBC_ContainerNum_ReadOnly()
		{
			(var header, var nctsContainer) = CreateNewContainer(Factory);
			header.BH_ExportFlag = EventFlagList.Codes.Yes;
			var incident = header.EnRouteIncidents.AddNew();
			nctsContainer = incident.IncidentContainers.AddNew();
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(nctsContainer.BC_ContainerNumInfo, header);
		}

		public void TestBC_Seal1_ReadOnly()
		{
			(var header, var nctsContainer) = CreateNewContainer(Factory);
			header.BH_ExportFlag = EventFlagList.Codes.Yes;
			var incident = header.EnRouteIncidents.AddNew();
			nctsContainer = incident.IncidentContainers.AddNew();
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(nctsContainer.BC_Seal1Info, header);
		}

		public void TestBC_Seal2_ReadOnly()
		{
			(var header, var nctsContainer) = CreateNewContainer(Factory);
			header.BH_ExportFlag = EventFlagList.Codes.Yes;
			var incident = header.EnRouteIncidents.AddNew();
			nctsContainer = incident.IncidentContainers.AddNew();
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(nctsContainer.BC_Seal2Info, header);
		}

		public void TestSealsForMessaging()
		{
			(_, var container) = CreateNewContainer(Factory);
			container.BC_Seal1 = "seal1";
			container.BC_Seal2 = "seal2";
			var additionalSeal1 = container.Seals.AddNew();
			additionalSeal1.BK_SealNumber = "ADD1";
			additionalSeal1.BK_UnloadingState = "NEW";
			var additionalSeal2 = container.Seals.AddNew();
			additionalSeal2.BK_SealNumber = "ADD2";
			additionalSeal2.BK_UnloadingState = "DEC";
			var additionalSeal3 = container.Seals.AddNew();
			additionalSeal3.BK_SealNumber = "ADD3";
			additionalSeal3.BK_UnloadingState = "MIS";
			var additionalSeal4 = container.Seals.AddNew();
			additionalSeal4.BK_SealNumber = "ADD4";
			additionalSeal4.BK_UnloadingState = "DAM";

			AssertArrayEqualsByElements(new ZString[] { "ADD1", "ADD2", "ADD3" },
				container.SealsForMessaging.Select(x => (x.BK_SealNumber)).ToArray());
		}

		protected override BusinessObject GetNewBusinessObject(BusinessObjectFactory factory) => CreateNewContainer(factory).container;

		(NctsHeader header, NctsContainer container) CreateNewContainer(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var container = header.ArrivalMovementHeader.GoodsItems.AddNew().Containers.AddNew();
			container.BC_ContainerNum = "TURE1235985";
			container.BC_TypeOfService = ContainerTypeOfServiceList.Codes.DepartureContainer;
			return (header, container);
		}
	}
}
