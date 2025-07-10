using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(NctsPhase5LayoutProvider))]
	sealed class NctsPhase5LayoutProviderBaseOnlyTest : NctsPhase5LayoutProviderAbstractTest
	{
		protected override Type ExpectedDepartureMovementGridColumnLayout => typeof(DepartureMovementDetailsGridColumnsLayout);

		protected override Type ExpectedTraderDetailsPanelLayoutType => typeof(Phase5TraderDetailsLayout);

		protected override Type ExpectedDepartureDetailsLayoutType => typeof(Phase5DepartureDetailsLayout);

		protected override Type ExpectedDeclarationDetailsPanelLayoutType => typeof(Phase5DeclarationDetailsLayout);

		protected override Type ExpectedMessageTabUserControlType => typeof(MessagesTabUserControl);

		protected override Type ExpectedTransportAndPackagingPanelLayoutType => typeof(Phase5TransportAndPackagingLayout);

		protected override Type ExpectedTransportDeparturePanelLayoutType => typeof(TransportDepartureLayout);

		protected override Type ExpectedTransportBorderPanelLayoutType => typeof(TransportBorderLayout);

		protected override Type ExpectedSecurityAtDeparturePanelLayoutType => typeof(Phase5SecurityAtDepartureLayout);

		protected override Type ExpectedMiscellanousOptionsPanelLayoutType => typeof(Phase5MiscellaneousOptionsLayout);

		protected override Type ExpectedCustomsOfficesUserControlType => typeof(Phase5CustomsOfficesUserControl);

		protected override Type ExpectedGuaranteesUserControlType => typeof(Phase5GuaranteesUserControl);

		protected override Type ExpectedHouseConsignmentDetailsPanelLayoutWithGridType => typeof(HouseConsignmentDetailsLayoutWithGrid);

		protected override Type ExpectedHouseConsignmentPreviousDocumentPanelLayoutWithGridType => typeof(HouseConsignmentPreviousDocumentLayoutWithGrid);

		protected override Type ExpectedHouseConsignmentSupportingDocumentPanelLayoutWithGridType => typeof(HouseConsignmentSupportingDocumentLayoutWithGrid);

		protected override Type ExpectedHouseConsignmentSupplyChainActorPanelLayoutWithGridType => typeof(HouseConsignmentSupplyChainActorLayoutWithGrid);

		protected override Type ExpectedHouseConsignmentAdditionalDocumentPanelLayoutWithGridType => typeof(HouseConsignmentAdditionalDocumentLayoutWithGrid);

		protected override Type ExpectedDeclarationSupportingDocumentsTabControlType => typeof(DeclarationSupportingDocumentsGridUserControl);

		protected override Type ExpectedDeclarationPreviousDocumentsTabControlType => typeof(DeclarationPreviousDocumentsGridUserControl);

		protected override Type ExpectedDeclarationServicePanelLayoutWithGridType => typeof(ServiceLayoutWithGrid);

		protected override Type ExpectedDeclarationAuthorizationsTabControlType => typeof(Phase5DeclarationAuthorizationsTabUserControl);

		protected override Type ExpectedGoodsItemDetailsLayoutType => typeof(Phase5GoodsItemDetailsLayoutWithGrid);

		protected override Type ExpectedGoodsItemSupportingDocumentPanelLayoutWithGridType => typeof(Phase5GoodsItemSupportingDocumentLayoutWithGrid);

		protected override Type ExpectedGoodsItemPreviousDocumentPanelLayoutWithGridType => typeof(Phase5GoodsItemPreviousDocumentLayoutWithGrid);

		protected override Type ExpectedGoodsItemSupplyChainActorPanelLayoutWithGridType => typeof(Phase5GoodsItemSupplyChainActorLayoutWithGrid);

		protected override Type ExpectedGoodsItemPackagesAndContainersPanelLayoutType => typeof(Phase5GoodsItemPackagesAndContainersLayout);

		protected override Type ExpectedGoodsItemPackagePanelLayoutType => typeof(Phase5GoodsItemPackageLayout);

		protected override Type ExpectedGoodsItemPackageGridPanelLayoutType => typeof(Phase5GoodsItemPackageGridLayout);

		protected override Type ExpectedArrivalNotificationDetailsPanelLayoutType => typeof(Phase5ArrivalNotificationDetailsLayout);

		protected override Type ExpectedArrivalDeclarationDetailsPanelLayoutType => typeof(Phase5ArrivalDeclarationDetailsLayout);

		protected override Type ExpectedEventsPanelLayoutType => typeof(Phase5EventLayout);

		protected override Type ExpectedUnloadingDetailsPanelLayoutType => typeof(UnloadingDetailsLayout);

		protected override Type ExpectedGuaranteeForArrivalGroupBoxPanelLayoutType => typeof(GuaranteeGroupBoxWithOverrideLayout);

		protected override Type ExpectedHouseConsignmentDifferencesLayoutType => typeof(HouseConsignmentDifferencesLayout);

		protected override Type ExpectedHouseConsignmentAdditionalDocumentsLayoutType => typeof(HouseConsignmentAdditionalDocumentsLayout);

		protected override Type ExpectedHouseConsignmentPreviousDocumentsLayoutType => typeof(Phase5HouseConsignmentDifferencesPreviousDocumentLayoutWithGrid);

		protected override Type ExpectedUnloadingDifferencesDeclaredValuePanelLayoutType => typeof(UnloadingDifferencesDeclaredValueLayout);

		protected override Type ExpectedUnloadingDifferencesUnloadedValuePanelLayoutType => typeof(UnloadingDifferencesUnloadedValueLayout);

		protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Latvia;

		protected override IEnumerable<ITabPage> ExpectedAdditionalGoodsItemTabPages => Enumerable.Empty<ITabPage>();

		protected override IEnumerable<string> ExpectedReorderGoodsItemTabPageNames => DefaultGoodsItemTabPageNames;

		protected override Type ExpectedNctsPackagePanelLayoutWithGridType => typeof(NctsPackageLayoutWithGrid);

		protected override Type ExpectIncidentDetailsPanelLayoutWithGridType => typeof(IncidentDetailsLayoutWithGrid);

		protected override Type ExpectedSupportingDocumentsLayoutType => typeof(Phase5HouseConsignmentDifferencesSupportingDocumentLayoutWithGrid);

		protected override Type ExpectedHouseConsignmentGoodItemsSupportingDocumentsLayoutType => typeof(HouseConsignmentGoodItemsSupportingDocumentsLayout);

		protected override Type ExpectedIncidentTransportMeansLayoutType => typeof(Phase5TransportMeansLayout);

		protected override Type ExpectedGoodsItemDifferencesDetailsLayout => typeof(Phase5GoodsItemDifferencesDetailsLayout);

		protected override Type ExpectedGoodsItemDifferencesDetailsColumnLayout => typeof(Phase5GoodsItemDifferencesDetailsColumnLayout);

		protected override Type ExpectedDeclarationSupplyChainActorsTabControlType => typeof(Phase5DeclarationSupplyChainActorsTabUserControl);

		protected override Type ExpectedArrivalContainersEquipmentGridType => typeof(Phase5ArrivalContainersEquipmentGridUserControl);

		protected override Type ExpectedArrivalSealsGridType => typeof(Phase5ArrivalSealsGridUserControl);

		protected override Type ExpectedUnloadingDifferencesPreviousDocumentPanelGridUserControlType => typeof(Phase5UnloadingDifferencesPreviousDocumentsGridUserControl);

		protected override Type ExpectedDepartureGoodItemsAdditionalDocumentsLayoutType => typeof(Phase5DepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid);

		protected override Type ExpectedArrivalGoodItemsAdditionalDocumentsLayoutType => typeof(Phase5ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid);

		protected override Type ExpectedDeclarationAdditionalDocumentsTabControlType => typeof(DeclarationAdditionalDocumentsGridUserControl);

		protected override Type ExpectedArrivalTransportInfoGridType => typeof(ArrivalTransportInfosGridUserControl);

		protected override Type ExpectedHouseConsignmentTransportDeparturePanelLayoutType => typeof(HouseConsignmentTransportDepartureLayout);

		protected override Type ExpectedGoodsItemsTabUserControlType => typeof(Phase5GoodsItemsTabUserControl);

		protected override Type ExpectedArrivalNotificationTabControlType => typeof(Phase5ArrivalNotificationTabUserControl);

		protected override IEnumerable<ITabPage> ExpectedAdditionalDeclarationDetailTabPages => Enumerable.Empty<ITabPage>();

		protected override IEnumerable<string> ExpectedReorderDeclarationDetailTabPageNames => DefaultDeclarationDetailTabPagesNames;

		protected override IEnumerable<ITabPage> ExpectedAdditionalArrivalTabPages => Enumerable.Empty<ITabPage>();

		protected override IEnumerable<string> ExpectedReorderArrivalTabPageNames => DefaultArrivalTabPagesNames;

		protected override Type ExpectedMessageSendingGridColumnLayout => null;

		protected override Type ExpectedSuportingDocumentGridColumnLayoutType => typeof(SupportingDocumentsGridColumnLayout);

		protected override Type ExpectedAdditionalDocumentsGridColumnLayoutType => typeof(AdditionalDocumentsGridColumnLayout);

		protected override Type ExpectedUnloadingDifferencesSuportingDocumentGridColumnLayoutType => typeof(UnloadingDifferencesSupportingDocumentsGridColumnLayout);

		protected override Type ExpectedUnloadingDifferencesAdditionalDocumentsGridColumnLayoutType => typeof(UnloadingDifferencesAdditionalDocumentsGridColumnLayout);

		protected override Type ExpectedContainersGridColumnLayout => typeof(ContainersGridColumnLayout);

		protected override Type ExpectedAdditionalSealsGridColumnLayout => typeof(AdditionalSealsGridColumnLayout);

		protected override Type ExpectedNctsPackagesGridColumnLayout => typeof(NctsPackagesGridColumnLayout);

		protected override Type ExpectedGetDepartureGoodsItemsGridColumnLayout => typeof(Phase5GoodsItemDetailsGridColumnsLayout);

		protected override Type ExpectedDeclarationDetailsTabUserControlType => typeof(Phase5DeclarationDetailsTabUserControl);

		protected override Type ExpectedTransportAndPackagingTabUserControlType => typeof(Phase5TransportAndPackagingTabUserControl);

		protected override Type ExpectedHouseConsignmentsTabUserControlType => typeof(HouseConsignmentsTabUserControl);

		protected override Type ExpectedGoodsItemAdditionalDocumentsTabUserControlType => typeof(Phase5GoodsItemAdditionalDocumentsTabUserControl);

		protected override Type ExpectedGoodsItemsPreviousDocumentsGridColumnLayout => typeof(Phase5GoodsItemPreviousDocumentsGridColumnsLayout);

		protected override Type ExpectedGoodsItemDifferencesDetailsGridColumnLayout => typeof(Phase5GoodsItemDifferencesDetailsGridColumnsLayout);

		protected override Type ExpectedUnloadingDifferencesTabUserControlType => typeof(Phase5UnloadingDifferencesTabUserControl);

		protected override Type LiabilityDetailsLayoutType => typeof(LiabilityDetailsLayout);

		protected override Type ExpectedGetHouseConsignmentDetailsGridColumnLayout => typeof(HouseConsignmentDetailsGridColumnLayout);

		protected override Type ExpectedGoodsItemPackagesUserControlType => typeof(GoodsItemPackagesUserControl);
	}
}
