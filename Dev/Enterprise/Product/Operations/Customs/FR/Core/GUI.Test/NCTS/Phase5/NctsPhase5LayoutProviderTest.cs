using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	[TestedType(typeof(NctsPhase5LayoutProvider))]
	sealed class NctsPhase5LayoutProviderTest : EU.NCTS.GUI.Testing.NctsPhase5LayoutProviderAbstractTest
	{
		protected override Type ExpectedDepartureMovementGridColumnLayout => typeof(EU.NCTS.GUI.DepartureMovementDetailsGridColumnsLayout);

		protected override Type ExpectedTraderDetailsPanelLayoutType => typeof(EU.NCTS.GUI.Phase5TraderDetailsLayout);

		protected override Type ExpectedDepartureDetailsLayoutType => typeof(Phase5DepartureDetailsLayout);

		protected override Type ExpectedDeclarationDetailsPanelLayoutType => typeof(EU.NCTS.GUI.Phase5DeclarationDetailsLayout);

		protected override Type ExpectedTransportAndPackagingPanelLayoutType => typeof(Phase5TransportAndPackagingLayout);

		protected override Type ExpectedTransportDeparturePanelLayoutType => typeof(EU.NCTS.GUI.TransportDepartureLayout);

		protected override Type ExpectedTransportBorderPanelLayoutType => typeof(EU.NCTS.GUI.TransportBorderLayout);

		protected override Type ExpectedSecurityAtDeparturePanelLayoutType => typeof(EU.NCTS.GUI.Phase5SecurityAtDepartureLayout);

		protected override Type ExpectedMiscellanousOptionsPanelLayoutType => typeof(EU.NCTS.GUI.Phase5MiscellaneousOptionsLayout);

		protected override Type ExpectedCustomsOfficesUserControlType => typeof(EU.NCTS.GUI.Phase5CustomsOfficesUserControl);

		protected override Type ExpectedGuaranteesUserControlType => typeof(EU.NCTS.GUI.Phase5GuaranteesUserControl);

		protected override Type ExpectedHouseConsignmentPreviousDocumentPanelLayoutWithGridType => typeof(EU.NCTS.GUI.HouseConsignmentPreviousDocumentLayoutWithGrid);

		protected override Type ExpectedHouseConsignmentSupportingDocumentPanelLayoutWithGridType => typeof(EU.NCTS.GUI.HouseConsignmentSupportingDocumentLayoutWithGrid);

		protected override Type ExpectedHouseConsignmentSupplyChainActorPanelLayoutWithGridType => typeof(EU.NCTS.GUI.HouseConsignmentSupplyChainActorLayoutWithGrid);

		protected override Type ExpectedHouseConsignmentAdditionalDocumentPanelLayoutWithGridType => typeof(EU.NCTS.GUI.HouseConsignmentAdditionalDocumentLayoutWithGrid);

		protected override Type ExpectedDeclarationServicePanelLayoutWithGridType => typeof(EU.NCTS.GUI.ServiceLayoutWithGrid);

		protected override Type ExpectedGoodsItemDetailsLayoutType => typeof(EU.NCTS.GUI.Phase5GoodsItemDetailsLayoutWithGrid);

		protected override Type ExpectedGoodsItemSupportingDocumentPanelLayoutWithGridType => typeof(EU.NCTS.GUI.Phase5GoodsItemSupportingDocumentLayoutWithGrid);

		protected override Type ExpectedGoodsItemPreviousDocumentPanelLayoutWithGridType => typeof(Phase5GoodsItemPreviousDocumentLayoutWithGrid);

		protected override Type ExpectedGoodsItemSupplyChainActorPanelLayoutWithGridType => typeof(EU.NCTS.GUI.Phase5GoodsItemSupplyChainActorLayoutWithGrid);

		protected override Type ExpectedGoodsItemPackagesAndContainersPanelLayoutType => typeof(EU.NCTS.GUI.Phase5GoodsItemPackagesAndContainersLayout);

		protected override Type ExpectedGoodsItemPackagePanelLayoutType => typeof(EU.NCTS.GUI.Phase5GoodsItemPackageLayout);

		protected override Type ExpectedGoodsItemPackageGridPanelLayoutType => typeof(EU.NCTS.GUI.Phase5GoodsItemPackageGridLayout);

		protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.France;

		protected override IEnumerable<ITabPage> ExpectedAdditionalGoodsItemTabPages => Enumerable.Empty<ITabPage>();

		protected override IEnumerable<string> ExpectedReorderGoodsItemTabPageNames => DefaultGoodsItemTabPageNames;

		protected override Type ExpectedArrivalNotificationDetailsPanelLayoutType => typeof(Phase5ArrivalNotificationDetailsLayout);

		protected override Type ExpectedArrivalDeclarationDetailsPanelLayoutType => typeof(EU.NCTS.GUI.Phase5ArrivalDeclarationDetailsLayout);

		protected override Type ExpectedUnloadingDetailsPanelLayoutType => typeof(EU.NCTS.GUI.UnloadingDetailsLayout);

		protected override Type ExpectedGuaranteeForArrivalGroupBoxPanelLayoutType => typeof(EU.GUI.GuaranteeGroupBoxWithOverrideLayout);

		protected override Type ExpectedHouseConsignmentDifferencesLayoutType => typeof(EU.NCTS.GUI.HouseConsignmentDifferencesLayout);

		protected override Type ExpectedUnloadingDifferencesDeclaredValuePanelLayoutType => typeof(EU.NCTS.GUI.UnloadingDifferencesDeclaredValueLayout);

		protected override Type ExpectedUnloadingDifferencesUnloadedValuePanelLayoutType => typeof(EU.NCTS.GUI.UnloadingDifferencesUnloadedValueLayout);

		protected override Type ExpectedEventsPanelLayoutType => typeof(EU.NCTS.GUI.Phase5EventLayout);

		protected override Type ExpectedNctsPackagePanelLayoutWithGridType => typeof(EU.NCTS.GUI.NctsPackageLayoutWithGrid);

		protected override Type ExpectedHouseConsignmentAdditionalDocumentsLayoutType => typeof(EU.NCTS.GUI.HouseConsignmentAdditionalDocumentsLayout);

		protected override Type ExpectedHouseConsignmentPreviousDocumentsLayoutType => typeof(EU.NCTS.GUI.Phase5HouseConsignmentDifferencesPreviousDocumentLayoutWithGrid);

		protected override Type ExpectIncidentDetailsPanelLayoutWithGridType => typeof(EU.NCTS.GUI.IncidentDetailsLayoutWithGrid);

		protected override Type ExpectedSupportingDocumentsLayoutType => typeof(EU.NCTS.GUI.Phase5HouseConsignmentDifferencesSupportingDocumentLayoutWithGrid);

		protected override Type ExpectedHouseConsignmentGoodItemsSupportingDocumentsLayoutType => typeof(EU.NCTS.GUI.HouseConsignmentGoodItemsSupportingDocumentsLayout);

		protected override Type ExpectedIncidentTransportMeansLayoutType => typeof(EU.NCTS.GUI.Phase5TransportMeansLayout);

		protected override Type ExpectedGoodsItemDifferencesDetailsLayout => typeof(EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsLayout);

		protected override Type ExpectedGoodsItemDifferencesDetailsColumnLayout => typeof(EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsColumnLayout);

		protected override Type ExpectedDeclarationSupplyChainActorsTabControlType => typeof(EU.NCTS.GUI.Phase5DeclarationSupplyChainActorsTabUserControl);

		protected override Type ExpectedUnloadingDifferencesPreviousDocumentPanelGridUserControlType => typeof(EU.NCTS.GUI.Phase5UnloadingDifferencesPreviousDocumentsGridUserControl);

		protected override Type ExpectedDepartureGoodItemsAdditionalDocumentsLayoutType => typeof(EU.NCTS.GUI.Phase5DepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid);

		protected override Type ExpectedArrivalGoodItemsAdditionalDocumentsLayoutType => typeof(EU.NCTS.GUI.Phase5ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid);

		protected override Type ExpectedArrivalContainersEquipmentGridType => typeof(EU.NCTS.GUI.Phase5ArrivalContainersEquipmentGridUserControl);

		protected override Type ExpectedArrivalSealsGridType => typeof(EU.NCTS.GUI.Phase5ArrivalSealsGridUserControl);

		protected override Type ExpectedDeclarationSupportingDocumentsTabControlType => typeof(EU.NCTS.GUI.DeclarationSupportingDocumentsGridUserControl);

		protected override Type ExpectedDeclarationPreviousDocumentsTabControlType => typeof(EU.NCTS.GUI.DeclarationPreviousDocumentsGridUserControl);

		protected override Type ExpectedDeclarationAdditionalDocumentsTabControlType => typeof(EU.NCTS.GUI.DeclarationAdditionalDocumentsGridUserControl);

		protected override Type ExpectedArrivalTransportInfoGridType => typeof(EU.NCTS.GUI.ArrivalTransportInfosGridUserControl);

		protected override Type ExpectedHouseConsignmentTransportDeparturePanelLayoutType => typeof(EU.NCTS.GUI.HouseConsignmentTransportDepartureLayout);

		protected override Type ExpectedGoodsItemsTabUserControlType => typeof(EU.NCTS.GUI.Phase5GoodsItemsTabUserControl);

		protected override Type ExpectedDeclarationAuthorizationsTabControlType => typeof(EU.NCTS.GUI.Phase5DeclarationAuthorizationsTabUserControl);

		protected override Type ExpectedArrivalNotificationTabControlType => typeof(EU.NCTS.GUI.Phase5ArrivalNotificationTabUserControl);

		protected override IEnumerable<ITabPage> ExpectedAdditionalDeclarationDetailTabPages => Enumerable.Empty<ITabPage>();

		protected override IEnumerable<string> ExpectedReorderDeclarationDetailTabPageNames => DefaultDeclarationDetailTabPagesNames;

		protected override IEnumerable<ITabPage> ExpectedAdditionalArrivalTabPages => Enumerable.Empty<ITabPage>();

		protected override IEnumerable<string> ExpectedReorderArrivalTabPageNames => DefaultArrivalTabPagesNames;

		protected override Type ExpectedMessageSendingGridColumnLayout => null;

		protected override Type ExpectedSuportingDocumentGridColumnLayoutType => typeof(EU.NCTS.GUI.SupportingDocumentsGridColumnLayout);

		protected override Type ExpectedAdditionalDocumentsGridColumnLayoutType => typeof(EU.NCTS.GUI.AdditionalDocumentsGridColumnLayout);

		protected override Type ExpectedUnloadingDifferencesSuportingDocumentGridColumnLayoutType => typeof(EU.NCTS.GUI.UnloadingDifferencesSupportingDocumentsGridColumnLayout);

		protected override Type ExpectedUnloadingDifferencesAdditionalDocumentsGridColumnLayoutType => typeof(EU.NCTS.GUI.UnloadingDifferencesAdditionalDocumentsGridColumnLayout);

		protected override Type ExpectedContainersGridColumnLayout => typeof(ContainersGridColumnLayout);

		protected override Type ExpectedAdditionalSealsGridColumnLayout => typeof(EU.NCTS.GUI.AdditionalSealsGridColumnLayout);

		protected override Type ExpectedNctsPackagesGridColumnLayout => typeof(EU.NCTS.GUI.NctsPackagesGridColumnLayout);

		protected override Type ExpectedGetDepartureGoodsItemsGridColumnLayout => typeof(EU.NCTS.GUI.Phase5GoodsItemDetailsGridColumnsLayout);

		protected override Type ExpectedDeclarationDetailsTabUserControlType => typeof(EU.NCTS.GUI.Phase5DeclarationDetailsTabUserControl);

		protected override Type ExpectedHouseConsignmentsTabUserControlType => typeof(EU.NCTS.GUI.HouseConsignmentsTabUserControl);

		protected override Type ExpectedGoodsItemAdditionalDocumentsTabUserControlType => typeof(EU.NCTS.GUI.Phase5GoodsItemAdditionalDocumentsTabUserControl);

		protected override Type ExpectedGoodsItemsPreviousDocumentsGridColumnLayout => typeof(Phase5GoodsItemPreviousDocumentsGridColumnsLayout);

		protected override Type ExpectedGoodsItemDifferencesDetailsGridColumnLayout => typeof(EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsGridColumnsLayout);

		protected override Type ExpectedMessageTabUserControlType => typeof(EU.NCTS.GUI.MessagesTabUserControl);

		protected override Type ExpectedTransportAndPackagingTabUserControlType => typeof(EU.NCTS.GUI.Phase5TransportAndPackagingTabUserControl);

		protected override Type ExpectedUnloadingDifferencesTabUserControlType => typeof(EU.NCTS.GUI.Phase5UnloadingDifferencesTabUserControl);

		protected override Type LiabilityDetailsLayoutType => typeof(EU.NCTS.GUI.LiabilityDetailsLayout);

		protected override Type ExpectedHouseConsignmentDetailsPanelLayoutWithGridType => typeof(EU.NCTS.GUI.HouseConsignmentDetailsLayoutWithGrid);

		protected override Type ExpectedGetHouseConsignmentDetailsGridColumnLayout => typeof(EU.NCTS.GUI.HouseConsignmentDetailsGridColumnLayout);

		protected override Type ExpectedGoodsItemPackagesUserControlType => typeof(EU.NCTS.GUI.GoodsItemPackagesUserControl);
	}
}
