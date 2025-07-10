using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestsSubclassesOf(typeof(INctsPhase5LayoutProvider))]
	public abstract class NctsPhase5LayoutProviderAbstractTest : TestCaseWithFactory
	{
		public void TestTraderDetailsPanelLayout()
		{
			AssertEquals(ExpectedTraderDetailsPanelLayoutType, provider.TraderDetailsPanelLayout.GetType());
		}

		public void TestDepartureDetailsPanelLayout()
		{
			AssertEquals(ExpectedDepartureDetailsLayoutType, provider.DepartureDetailsPanelLayout.GetType());
		}

		public void TestDeclarationDetailsPanelLayout()
		{
			AssertEquals(ExpectedDeclarationDetailsPanelLayoutType, provider.DeclarationDetailsPanelLayout.GetType());
		}

		public void TestTransportAndPackagingPanelLayout()
		{
			AssertEquals(ExpectedTransportAndPackagingPanelLayoutType, provider.TransportAndPackagingPanelLayout.GetType());
		}

		public void TestTransportDeparturePanelLayout()
		{
			AssertEquals(ExpectedTransportDeparturePanelLayoutType, provider.TransportDeparturePanelLayout.GetType());
		}

		public void TestTransportBorderPanelLayout()
		{
			AssertEquals(ExpectedTransportBorderPanelLayoutType, provider.TransportBorderPanelLayout.GetType());
		}

		public void TestSecurityAtDeparturePanelLayout()
		{
			AssertEquals(ExpectedSecurityAtDeparturePanelLayoutType, provider.SecurityAtDeparturePanelLayout.GetType());
		}

		public void TestMiscellanousOptionsPanelLayout()
		{
			AssertEquals(ExpectedMiscellanousOptionsPanelLayoutType, provider.MiscellanousOptionsPanelLayout.GetType());
		}

		public void TestCustomsOfficesUserControl()
		{
			AssertEquals(ExpectedCustomsOfficesUserControlType, provider.CustomsOfficesUserControlType);
		}

		public void TestGuaranteesUserControl()
		{
			AssertEquals(ExpectedGuaranteesUserControlType, provider.GuaranteesUserControlType);
		}

		public void TestMessageTabUserControl()
		{
			AssertEquals(ExpectedMessageTabUserControlType, provider.MessageTabUserControlType);
		}

		public void TestHouseConsignmentDetailsPanelLayout()
		{
			AssertEquals(ExpectedHouseConsignmentDetailsPanelLayoutWithGridType, provider.HouseConsignmentDetailsPanelLayoutWithGrid.GetType());
		}

		public void TestGetHouseConsignmentGridColumnLayout()
		{
			AssertEquals(ExpectedGetHouseConsignmentDetailsGridColumnLayout, provider.GetHouseConsignmentDetailsGridColumnLayout().GetType());
		}

		public void TestHouseConsignmentDetailsPanelLayoutWithGridHasOnlyOneGrid()
		{
			var gridUserControlType = provider.HouseConsignmentDetailsPanelLayoutWithGrid?.GridUserControlType;
			AssertNotNull("HouseConsignmentDetailsPanelLayoutWithGrid.GridUserControlType", gridUserControlType);

			var gridUserControlInstance = Activator.CreateInstance(gridUserControlType);
			AssertNotNull("GridUserControlType instance must be a ZUserControl", gridUserControlInstance as ZUserControl);
			using (var gridUserControl = (ZUserControl)gridUserControlInstance)
			{
				var grids = gridUserControl.FindAll<ZGrid>();
				AssertEquals("Numbers of ZGrid controls in HouseConsignmentDetailsPanelLayoutWithGrid.GridUserControl", 1, grids.Count());
			}
		}

		public void TestHouseConsignmentPreviousDocumentPanelLayoutWithGrid()
		{
			AssertEquals(ExpectedHouseConsignmentPreviousDocumentPanelLayoutWithGridType, provider.HouseConsignmentPreviousDocumentPanelLayoutWithGrid.GetType());
		}

		public void TestHouseConsignmentSupportingDocumentPanelLayoutWithGrid()
		{
			AssertEquals(ExpectedHouseConsignmentSupportingDocumentPanelLayoutWithGridType, provider.HouseConsignmentSupportingDocumentPanelLayoutWithGrid.GetType());
		}

		public void TestHouseConsignmentSupplyChainActorPanelLayoutWithGrid()
		{
			AssertEquals(ExpectedHouseConsignmentSupplyChainActorPanelLayoutWithGridType, provider.HouseConsignmentSupplyChainActorPanelLayoutWithGrid.GetType());
		}

		public void TestHouseConsignmentAdditionalDocumentPanelLayoutWithGrid()
		{
			AssertEquals(ExpectedHouseConsignmentAdditionalDocumentPanelLayoutWithGridType, provider.HouseConsignmentAdditionalDocumentPanelLayoutWithGrid.GetType());
		}

		public void TestDeclarationSupportingDocumentsTabControlType()
		{
			AssertEquals(ExpectedDeclarationSupportingDocumentsTabControlType, provider.DeclarationSupportingDocumentsTabControlType);
		}

		public void TestDeclarationPreviousDocumentsTabControlType()
		{
			AssertEquals(ExpectedDeclarationPreviousDocumentsTabControlType, provider.DeclarationPreviousDocumentsTabControlType);
		}

		public void TestDeclarationServicePanelLayoutWithGrid()
		{
			AssertEquals(ExpectedDeclarationServicePanelLayoutWithGridType, provider.DeclarationServicePanelLayoutWithGrid.GetType());
		}

		public void TestDeclarationAuthorizationsTabControlType()
		{
			AssertEquals(ExpectedDeclarationAuthorizationsTabControlType, provider.DeclarationAuthorizationsTabControlType);
		}

		public void TestGoodsItemDetailsPanelLayout()
		{
			AssertEquals(ExpectedGoodsItemDetailsLayoutType, provider.GoodsItemDetailsPanelLayoutWithGrid.GetType());
		}

		public void TestGoodsItemSupportingDocumentPanelLayoutWithGrid()
		{
			AssertEquals(ExpectedGoodsItemSupportingDocumentPanelLayoutWithGridType, provider.GoodsItemSupportingDocumentPanelLayoutWithGrid.GetType());
		}

		public void TestGoodsItemPreviousDocumentPanelLayoutWithGrid()
		{
			AssertEquals(ExpectedGoodsItemPreviousDocumentPanelLayoutWithGridType, provider.GoodsItemPreviousDocumentPanelLayoutWithGrid.GetType());
		}

		public void TestGoodsItemSupplyChainActorPanelLayoutWithGrid()
		{
			AssertEquals(ExpectedGoodsItemSupplyChainActorPanelLayoutWithGridType, provider.GoodsItemSupplyChainActorPanelLayoutWithGrid.GetType());
		}

		public void TestGoodsItemPackagesAndContainersPanelLayout()
		{
			AssertEquals(ExpectedGoodsItemPackagesAndContainersPanelLayoutType, provider.GoodsItemPackagesAndContainersPanelLayout.GetType());
		}

		public void TestGoodsItemPackagePanelLayout()
		{
			AssertEquals(ExpectedGoodsItemPackagePanelLayoutType, provider.GoodsItemPackagePanelLayout.GetType());
		}

		public void TestGoodsItemPackageGridPanelLayout()
		{
			AssertEquals(ExpectedGoodsItemPackageGridPanelLayoutType, provider.GoodsItemPackageGridPanelLayout.GetType());
		}

		public void TestArrivalNotificationDetailsPanelLayout()
		{
			AssertEquals(ExpectedArrivalNotificationDetailsPanelLayoutType, provider.ArrivalNotificationDetailsPanelLayout.GetType());
		}

		public void TestArrivalDeclarationDetailsPanelLayout()
		{
			AssertEquals(ExpectedArrivalDeclarationDetailsPanelLayoutType, provider.ArrivalDeclarationDetailsPanelLayout.GetType());
		}

		public void TestEventsPanelLayout()
		{
			AssertEquals(ExpectedEventsPanelLayoutType, provider.EventPanelLayout.GetType());
		}

		public void TestAdditionalGoodsItemTabPages()
		{
			AssertContainsExactElementsInExactOrder(ExpectedAdditionalGoodsItemTabPages.Select(x => x.GetType()), provider.AdditionalGoodsItemTabPages.Select(x => x.GetType()));
		}

		public void TestReorderGoodsItemTabPageNames()
		{
			AssertContainsExactElementsInExactOrder(ExpectedReorderGoodsItemTabPageNames, provider.ReorderGoodsItemTabPageNames(DefaultGoodsItemTabPageNames.Concat(provider.AdditionalGoodsItemTabPages.Select(x => x.GetType().Name))));
		}

		protected IEnumerable<string> DefaultGoodsItemTabPageNames => Phase5GoodsItemsTabUserControlTest.ExpectedTabPageNamesInOrder;

		public void TestUnloadingDetailsPanelLayout()
		{
			AssertEquals(ExpectedUnloadingDetailsPanelLayoutType, provider.UnloadingDetailsPanelLayout.GetType());
		}

		public void TestGuaranteeForArrivalGroupBoxPanelLayout()
		{
			AssertEquals(ExpectedGuaranteeForArrivalGroupBoxPanelLayoutType, provider.GuaranteeForArrivalGroupBoxPanelLayout.GetType());
		}

		public void TestNctsPackagePanelLayoutWithGrid()
		{
			AssertEquals(ExpectedNctsPackagePanelLayoutWithGridType, provider.NctsPackageLayoutWithGrid.GetType());
		}

		public void TestHouseConsignmentDifferencesLayout()
		{
			AssertEquals(ExpectedHouseConsignmentDifferencesLayoutType, provider.HouseConsignmentDifferencesLayout.GetType());
		}

		public void TestHouseConsignmentAdditionalDocumentsLayout()
		{
			AssertEquals(ExpectedHouseConsignmentAdditionalDocumentsLayoutType, provider.HouseConsignmentAdditionalDocumentsLayout.GetType());
		}

		public void TestHouseConsignmentPreviousDocumentsLayout()
		{
			AssertEquals(ExpectedHouseConsignmentPreviousDocumentsLayoutType, provider.HouseConsignmentPreviousDocumentsLayout.GetType());
		}

		public void TestUnloadingDifferencesDeclaredValuePanelLayout()
		{
			AssertEquals(ExpectedUnloadingDifferencesDeclaredValuePanelLayoutType, provider.UnloadingDifferencesDeclaredValuePanelLayout.GetType());
		}

		public void TestUnloadingDifferencesUnloadedValuePanelLayout()
		{
			AssertEquals(ExpectedUnloadingDifferencesUnloadedValuePanelLayoutType, provider.UnloadingDifferencesUnloadedValuePanelLayout.GetType());
		}

		public void TestDepartureHouseConsignmentDifferencesAdditionalDocumentLayoutWithGrid()
		{
			AssertEquals(ExpectedDepartureGoodItemsAdditionalDocumentsLayoutType, provider.DepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid.GetType());
		}

		public void TestArrivalHouseConsignmentDifferencesAdditionalDocumentLayoutWithGrid()
		{
			AssertEquals(ExpectedArrivalGoodItemsAdditionalDocumentsLayoutType, provider.ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid.GetType());
		}

		public void TestIncidentDetailsPanelLayout()
		{
			AssertEquals(ExpectIncidentDetailsPanelLayoutWithGridType, provider.IncidentDetailsPanelLayoutWithGrid.GetType());
		}

		public void TestSupportingDocumentsLayout()
		{
			AssertEquals(ExpectedSupportingDocumentsLayoutType, provider.SupportingDocumentsLayout.GetType());
		}

		public void TestHouseConsignmentGoodItemsSupportingDocumentsLayoutType()
		{
			AssertEquals(ExpectedHouseConsignmentGoodItemsSupportingDocumentsLayoutType, provider.HouseConsignmentGoodItemsSupportingDocumentsLayout.GetType());
		}

		public void TestIncidentTransportMeansLayout()
		{
			AssertEquals(ExpectedIncidentTransportMeansLayoutType, provider.IncidentTransportMeansLayout.GetType());
		}

		public void TestGoodsItemsDifferencesDetailLayout()
		{
			AssertEquals(ExpectedGoodsItemDifferencesDetailsLayout, provider.GoodsItemDifferencesDetailsLayout.GetType());
		}

		public void TestGoodsItemsDifferencesDetailColumnLayout()
		{
			AssertEquals(ExpectedGoodsItemDifferencesDetailsColumnLayout, provider.GoodsItemDifferencesDetailsColumnLayout.GetType());
		}

		public void TestDeclarationSupplyChainActorsTabControlType()
		{
			AssertEquals(ExpectedDeclarationSupplyChainActorsTabControlType, provider.DeclarationSupplyChainActorsTabControlType);
		}

		public void TestUnloadingDifferencesPreviousDocumentPanelGridUserControl()
		{
			AssertEquals(ExpectedUnloadingDifferencesPreviousDocumentPanelGridUserControlType, provider.UnloadingDifferencesPreviousDocumentPanelGridUserControlType);
		}

		public void TestArrivalContainersEquipmentGrid()
		{
			AssertEquals(ExpectedArrivalContainersEquipmentGridType, provider.ArrivalContainersEquipmentGrid);
		}

		public void TestArrivalSealsGrid()
		{
			AssertEquals(ExpectedArrivalSealsGridType, provider.ArrivalSealsGrid);
		}

		public void TestDeclarationAdditionalDocumentsTabControlType()
		{
			AssertEquals(ExpectedDeclarationAdditionalDocumentsTabControlType, provider.DeclarationAdditionalDocumentsTabControlType);
		}

		public void TestArrivalTransportInfoGridType()
		{
			AssertEquals(ExpectedArrivalTransportInfoGridType, provider.ArrivalTransportInfoGridType);
		}

		public void TestHouseConsignmentTransportDeparturePanelLayoutType()
		{
			AssertEquals(ExpectedHouseConsignmentTransportDeparturePanelLayoutType, provider.HouseConsignmentTransportDeparturePanelLayout?.GetType());
		}

		public void TestGoodsItemsTabUserControlType()
		{
			AssertEquals(ExpectedGoodsItemsTabUserControlType, provider.GoodsItemsTabUserControlType);
		}

		public void TestArrivalNotificationTabControlType()
		{
			AssertEquals(ExpectedArrivalNotificationTabControlType, provider.ArrivalNotificationTabControlType);
		}

		public void TestNctsPhase5LayoutProviderType()
		{
			AssertType(TestedTypeHelper.GetTestedType(GetType()), provider);
		}

		public void TestAdditionalDeclarationDetailTabPages()
		{
			AssertContainsExactElementsInExactOrder(ExpectedAdditionalDeclarationDetailTabPages.Select(x => x.GetType()), provider.AdditionalDeclarationDetailTabPages.Select(x => x.GetType()));
		}

		public void TestReorderDeclarationDetailTabPagesNames()
		{
			AssertContainsExactElementsInExactOrder(ExpectedReorderDeclarationDetailTabPageNames, provider.ReorderDeclarationDetailTabPagesNames(DefaultDeclarationDetailTabPagesNames.Concat(provider.AdditionalDeclarationDetailTabPages.Select(x => x.GetType().Name))));
		}

		protected IEnumerable<string> DefaultDeclarationDetailTabPagesNames => Phase5DeclarationDetailsTabUserControlTest.ExpectedTabPageNamesInOrder;

		public void TestAdditionalArrivalTabPages()
		{
			AssertContainsExactElementsInExactOrder(ExpectedAdditionalArrivalTabPages.Select(x => x.GetType()), provider.AdditionalArrivalTabPages.Select(x => x.GetType()));
		}

		public void TestReorderArrivalTabPagesNames()
		{
			AssertContainsExactElementsInExactOrder(ExpectedReorderArrivalTabPageNames, provider.ReorderArrivalTabPagesNames(DefaultArrivalTabPagesNames.Concat(provider.AdditionalArrivalTabPages.Select(x => x.GetType().Name))));
		}

		public void TestMessageSendingGridColumnLayout()
		{
			AssertEquals(ExpectedMessageSendingGridColumnLayout, provider.GetMessageSendingGridColumnLayout(new NctsHeaderMessageSendingObjectParent(CreateNctsHeader()))?.GetType());
		}

		public void TestSuportingDocumentGridColumnLayout()
		{
			AssertEquals(ExpectedSuportingDocumentGridColumnLayoutType, provider.SuportingDocumentGridColumnLayout.GetType());
		}

		public void TestAdditionalDocumentsGridColumnLayout()
		{
			AssertEquals(ExpectedAdditionalDocumentsGridColumnLayoutType, provider.AdditionalDocumentsGridColumnLayout.GetType());
		}

		public void TestUnloadingDifferencesSuportingDocumentGridColumnLayout()
		{
			AssertEquals(ExpectedUnloadingDifferencesSuportingDocumentGridColumnLayoutType, provider.UnloadingDifferencesSuportingDocumentGridColumnLayout.GetType());
		}

		public void TestUnloadingDifferencesAdditionalDocumentsGridColumnLayout()
		{
			AssertEquals(ExpectedUnloadingDifferencesAdditionalDocumentsGridColumnLayoutType, provider.UnloadingDifferencesAdditionalDocumentsGridColumnLayout.GetType());
		}

		public void TestContainersGridColumnLayout()
		{
			AssertEquals(ExpectedContainersGridColumnLayout, provider.ContainersGridColumnLayout.GetType());
		}

		public void TestAdditionalSealsGridColumnLayout()
		{
			AssertEquals(ExpectedAdditionalSealsGridColumnLayout, provider.AdditionalSealsGridColumnLayout.GetType());
		}

		public void TestNctsPackagesGridColumnLayout()
		{
			AssertEquals(ExpectedNctsPackagesGridColumnLayout, provider.NctsPackagesGridColumnLayout.GetType());
		}

		public void TestGetDepartureGoodsItemsGridColumnLayout()
		{
			AssertEquals(ExpectedGetDepartureGoodsItemsGridColumnLayout, provider.GetDepartureGoodsItemsGridColumnLayout().GetType());
		}

		public void TestGoodsItemAdditionalDocumentsTabUserControl()
		{
			AssertEquals(ExpectedGoodsItemAdditionalDocumentsTabUserControlType, provider.GoodsItemAdditionalDocumentsTabUserControlType);
		}

		public void TestGoodsItemDetailsPanelLayoutWithGridHasOnlyOneGrid()
		{
			var gridUserControlType = provider.GoodsItemDetailsPanelLayoutWithGrid?.GridUserControlType;
			AssertNotNull("GoodsItemDetailsPanelLayoutWithGrid.GridUserControlType", gridUserControlType);

			var gridUserControlInstance = Activator.CreateInstance(gridUserControlType);
			AssertNotNull("GridUserControlType instance must be a ZUserControl", gridUserControlInstance as ZUserControl);
			using (var gridUserControl = (ZUserControl)gridUserControlInstance)
			{
				var grids = gridUserControl.FindAll<ZGrid>();
				AssertEquals("Numbers of ZGrid controls in GoodsItemDetailsPanelLayoutWithGrid.GridUserControl", 1, grids.Count());
			}
		}

		public void TestDeclarationDetailsTabUserControlType()
		{
			AssertEquals(ExpectedDeclarationDetailsTabUserControlType, provider.DeclarationDetailsTabUserControlType);
		}

		public void TestGetDepartureMovementGridColumnLayout()
		{
			AssertEquals(ExpectedDepartureMovementGridColumnLayout, provider.GetDepartureMovementGridColumnLayout().GetType());
		}

		public void TestTransportandPackagingTabUserControlType()
		{
			AssertEquals(ExpectedTransportAndPackagingTabUserControlType, provider.TransportAndPackagingTabUserControlType);
		}

		public void TestHouseConsignmentsTabUserControlType()
		{
			AssertEquals(ExpectedHouseConsignmentsTabUserControlType, provider.HouseConsignmentsTabUserControlType);
		}

		public void TestGoodsItemsPreviousDocumentsGridColumnLayout()
		{
			AssertEquals(ExpectedGoodsItemsPreviousDocumentsGridColumnLayout, provider.GetGoodsItemsPreviousDocumentsGridColumnLayout().GetType());
		}

		public void TestGoodsItemDifferencesDetailsGridColumnLayout()
		{
			AssertEquals(ExpectedGoodsItemDifferencesDetailsGridColumnLayout, provider.GetGoodsItemDifferencesDetailsGridColumnLayout().GetType());
		}

		public void TestUnloadingDifferencesTabUserControlType()
		{
			AssertEquals(ExpectedUnloadingDifferencesTabUserControlType, provider.UnloadingDifferencesTabUserControlType);
		}

		public void TestLiabilityDetailsLayout()
		{
			AssertEquals(LiabilityDetailsLayoutType, provider.LiabilityDetailsLayout.GetType());
		}

		public void TestGoodsItemPackagesUserControlType()
		{
			AssertEquals(ExpectedGoodsItemPackagesUserControlType, provider.GoodsItemPackagesUserControlType);
		}

		protected IEnumerable<string> DefaultArrivalTabPagesNames => Phase5ArrivalMovementFormBaseOnlyTest.ExpectedTabPageNamesInOrder;

		protected abstract Type ExpectedDepartureMovementGridColumnLayout { get; }

		protected abstract Type ExpectedTraderDetailsPanelLayoutType { get; }

		protected abstract Type ExpectedDepartureDetailsLayoutType { get; }

		protected abstract Type ExpectedDeclarationDetailsPanelLayoutType { get; }

		protected abstract Type ExpectedTransportAndPackagingPanelLayoutType { get; }

		protected abstract Type ExpectedTransportDeparturePanelLayoutType { get; }

		protected abstract Type ExpectedTransportBorderPanelLayoutType { get; }

		protected abstract Type ExpectedSecurityAtDeparturePanelLayoutType { get; }

		protected abstract Type ExpectedMiscellanousOptionsPanelLayoutType { get; }

		protected abstract Type ExpectedCustomsOfficesUserControlType { get; }

		protected abstract Type ExpectedGuaranteesUserControlType { get; }

		protected abstract Type ExpectedMessageTabUserControlType { get; }

		protected abstract Type ExpectedTransportAndPackagingTabUserControlType { get; }

		protected abstract Type ExpectedHouseConsignmentsTabUserControlType { get; }

		protected abstract Type ExpectedHouseConsignmentDetailsPanelLayoutWithGridType { get; }

		protected abstract Type ExpectedHouseConsignmentPreviousDocumentPanelLayoutWithGridType { get; }

		protected abstract Type ExpectedHouseConsignmentSupportingDocumentPanelLayoutWithGridType { get; }

		protected abstract Type ExpectedHouseConsignmentSupplyChainActorPanelLayoutWithGridType { get; }

		protected abstract Type ExpectedHouseConsignmentAdditionalDocumentPanelLayoutWithGridType { get; }

		protected abstract Type ExpectedDeclarationSupportingDocumentsTabControlType { get; }

		protected abstract Type ExpectedDeclarationPreviousDocumentsTabControlType { get; }

		protected abstract Type ExpectedDeclarationServicePanelLayoutWithGridType { get; }

		protected abstract Type ExpectedDeclarationAuthorizationsTabControlType { get; }

		protected abstract Type ExpectedGoodsItemDetailsLayoutType { get; }

		protected abstract Type ExpectedGoodsItemSupportingDocumentPanelLayoutWithGridType { get; }

		protected abstract Type ExpectedGoodsItemPreviousDocumentPanelLayoutWithGridType { get; }

		protected abstract Type ExpectedGoodsItemSupplyChainActorPanelLayoutWithGridType { get; }

		protected abstract Type ExpectedGoodsItemPackagesAndContainersPanelLayoutType { get; }

		protected abstract Type ExpectedGoodsItemPackagePanelLayoutType { get; }

		protected abstract Type ExpectedGoodsItemPackageGridPanelLayoutType { get; }

		protected abstract Type ExpectedArrivalNotificationDetailsPanelLayoutType { get; }

		protected abstract Type ExpectedArrivalDeclarationDetailsPanelLayoutType { get; }

		protected abstract Type ExpectedEventsPanelLayoutType { get; }

		protected abstract IEnumerable<ITabPage> ExpectedAdditionalGoodsItemTabPages { get; }

		protected abstract IEnumerable<string> ExpectedReorderGoodsItemTabPageNames { get; }

		protected abstract string CountryOrGroupingCode { get; }

		protected abstract Type ExpectedUnloadingDetailsPanelLayoutType { get; }

		protected abstract Type ExpectedGuaranteeForArrivalGroupBoxPanelLayoutType { get; }

		protected abstract Type ExpectedNctsPackagePanelLayoutWithGridType { get; }

		protected abstract Type ExpectedHouseConsignmentDifferencesLayoutType { get; }

		protected abstract Type ExpectedHouseConsignmentAdditionalDocumentsLayoutType { get; }

		protected abstract Type ExpectedHouseConsignmentPreviousDocumentsLayoutType { get; }

		protected abstract Type ExpectedUnloadingDifferencesDeclaredValuePanelLayoutType { get; }

		protected abstract Type ExpectedUnloadingDifferencesUnloadedValuePanelLayoutType { get; }

		protected abstract Type ExpectIncidentDetailsPanelLayoutWithGridType { get; }

		protected abstract Type ExpectedSupportingDocumentsLayoutType { get; }

		protected abstract Type ExpectedHouseConsignmentGoodItemsSupportingDocumentsLayoutType { get; }

		protected abstract Type ExpectedIncidentTransportMeansLayoutType { get; }

		protected abstract Type ExpectedGoodsItemDifferencesDetailsLayout { get; }

		protected abstract Type ExpectedGoodsItemDifferencesDetailsColumnLayout { get; }

		protected abstract Type ExpectedDeclarationSupplyChainActorsTabControlType { get; }

		protected abstract Type ExpectedArrivalContainersEquipmentGridType { get; }

		protected abstract Type ExpectedArrivalSealsGridType { get; }

		protected abstract Type ExpectedUnloadingDifferencesPreviousDocumentPanelGridUserControlType { get; }

		protected abstract Type ExpectedDepartureGoodItemsAdditionalDocumentsLayoutType { get; }

		protected abstract Type ExpectedArrivalGoodItemsAdditionalDocumentsLayoutType { get; }

		protected abstract Type ExpectedDeclarationAdditionalDocumentsTabControlType { get; }

		protected abstract Type ExpectedArrivalTransportInfoGridType { get; }

		protected abstract Type ExpectedHouseConsignmentTransportDeparturePanelLayoutType { get; }

		protected abstract Type ExpectedGoodsItemsTabUserControlType { get; }

		protected abstract Type ExpectedArrivalNotificationTabControlType { get; }

		protected abstract IEnumerable<ITabPage> ExpectedAdditionalDeclarationDetailTabPages { get; }

		protected abstract IEnumerable<string> ExpectedReorderDeclarationDetailTabPageNames { get; }

		protected abstract IEnumerable<ITabPage> ExpectedAdditionalArrivalTabPages { get; }

		protected abstract IEnumerable<string> ExpectedReorderArrivalTabPageNames { get; }

		protected abstract Type ExpectedMessageSendingGridColumnLayout { get; }

		protected abstract Type ExpectedSuportingDocumentGridColumnLayoutType { get; }

		protected abstract Type ExpectedAdditionalDocumentsGridColumnLayoutType { get; }

		protected abstract Type ExpectedUnloadingDifferencesSuportingDocumentGridColumnLayoutType { get; }

		protected abstract Type ExpectedUnloadingDifferencesAdditionalDocumentsGridColumnLayoutType { get; }

		protected abstract Type ExpectedContainersGridColumnLayout { get; }

		protected abstract Type ExpectedAdditionalSealsGridColumnLayout { get; }

		protected abstract Type ExpectedNctsPackagesGridColumnLayout { get; }

		protected abstract Type ExpectedGetDepartureGoodsItemsGridColumnLayout { get; }

		protected abstract Type ExpectedDeclarationDetailsTabUserControlType { get; }

		protected abstract Type ExpectedGoodsItemAdditionalDocumentsTabUserControlType { get; }

		protected abstract Type ExpectedGoodsItemsPreviousDocumentsGridColumnLayout { get; }

		protected abstract Type ExpectedGoodsItemDifferencesDetailsGridColumnLayout { get; }

		protected abstract Type ExpectedUnloadingDifferencesTabUserControlType { get; }

		protected abstract Type LiabilityDetailsLayoutType { get; }

		protected abstract Type ExpectedGetHouseConsignmentDetailsGridColumnLayout { get; }

		protected abstract Type ExpectedGoodsItemPackagesUserControlType { get;  }

		protected override void SetUp()
		{
			base.SetUp();
			provider = NctsPhase5LayoutProvider.GetLayoutProvider(CountryOrGroupingCode);
		}

		INctsPhase5LayoutProvider provider;

		NctsHeader CreateNctsHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			return nctsHeader;
		}
	}
}
