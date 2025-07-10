using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(NctsPhase5LayoutProvider))]
	sealed class NctsPhase5LayoutProviderTest : EU.NCTS.GUI.Testing.NctsPhase5LayoutProviderAbstractTest
	{
		protected override Type ExpectedDepartureMovementGridColumnLayout => typeof(EU.NCTS.GUI.DepartureMovementDetailsGridColumnsLayout);

		protected override Type ExpectedTraderDetailsPanelLayoutType => typeof(Phase5TraderDetailsLayout);

		protected override Type ExpectedDepartureDetailsLayoutType => typeof(DepartureDetailsLayout);

		protected override Type ExpectedDeclarationDetailsPanelLayoutType => typeof(Phase5DeclarationDetailsLayout);

		protected override Type ExpectedTransportAndPackagingPanelLayoutType => typeof(Phase5TransportAndPackagingLayout);

		protected override Type ExpectedTransportDeparturePanelLayoutType => typeof(EU.NCTS.GUI.TransportDepartureLayout);

		protected override Type ExpectedTransportBorderPanelLayoutType => typeof(EU.NCTS.GUI.TransportBorderLayout);

		protected override Type ExpectedSecurityAtDeparturePanelLayoutType => typeof(Phase5SecurityAtDepartureLayout);

		protected override Type ExpectedMiscellanousOptionsPanelLayoutType => typeof(EU.NCTS.GUI.Phase5MiscellaneousOptionsLayout);

		protected override Type ExpectedCustomsOfficesUserControlType => typeof(EU.NCTS.GUI.Phase5CustomsOfficesUserControl);

		protected override Type ExpectedGuaranteesUserControlType => typeof(Phase5GuaranteesUserControl);

		protected override Type ExpectedHouseConsignmentDetailsPanelLayoutWithGridType => typeof(HouseConsignmentDetailsLayoutWithGrid);

		protected override Type ExpectedHouseConsignmentPreviousDocumentPanelLayoutWithGridType => typeof(EU.NCTS.GUI.HouseConsignmentPreviousDocumentLayoutWithGrid);

		protected override Type ExpectedHouseConsignmentSupportingDocumentPanelLayoutWithGridType => typeof(EU.NCTS.GUI.HouseConsignmentSupportingDocumentLayoutWithGrid);

		protected override Type ExpectedHouseConsignmentSupplyChainActorPanelLayoutWithGridType => typeof(EU.NCTS.GUI.HouseConsignmentSupplyChainActorLayoutWithGrid);

		protected override Type ExpectedHouseConsignmentAdditionalDocumentPanelLayoutWithGridType => typeof(EU.NCTS.GUI.HouseConsignmentAdditionalDocumentLayoutWithGrid);

		protected override Type ExpectedDeclarationServicePanelLayoutWithGridType => typeof(EU.NCTS.GUI.ServiceLayoutWithGrid);

		protected override Type ExpectedGoodsItemDetailsLayoutType => typeof(Phase5GoodsItemDetailsLayoutWithGrid);

		protected override Type ExpectedGoodsItemSupportingDocumentPanelLayoutWithGridType => typeof(EU.NCTS.GUI.Phase5GoodsItemSupportingDocumentLayoutWithGrid);

		protected override Type ExpectedGoodsItemPreviousDocumentPanelLayoutWithGridType => typeof(EU.NCTS.GUI.Phase5GoodsItemPreviousDocumentLayoutWithGrid);

		protected override Type ExpectedGoodsItemSupplyChainActorPanelLayoutWithGridType => typeof(EU.NCTS.GUI.Phase5GoodsItemSupplyChainActorLayoutWithGrid);

		protected override Type ExpectedGoodsItemPackagesAndContainersPanelLayoutType => typeof(EU.NCTS.GUI.Phase5GoodsItemPackagesAndContainersLayout);

		protected override Type ExpectedGoodsItemPackagePanelLayoutType => typeof(Phase5GoodsItemPackageLayout);

		protected override Type ExpectedGoodsItemPackageGridPanelLayoutType => typeof(Phase5GoodsItemPackageGridLayout);

		protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Spain;

		protected override IEnumerable<ITabPage> ExpectedAdditionalGoodsItemTabPages => new List<ITabPage>();

		protected override IEnumerable<string> ExpectedReorderGoodsItemTabPageNames => DefaultGoodsItemTabPageNames;

		protected override Type ExpectedArrivalNotificationDetailsPanelLayoutType => typeof(ArrivalNotificationDetailsLayout);

		protected override Type ExpectedArrivalDeclarationDetailsPanelLayoutType => typeof(Phase5ArrivalDeclarationDetailsLayout);

		protected override Type ExpectedUnloadingDetailsPanelLayoutType => typeof(EU.NCTS.GUI.UnloadingDetailsLayout);

		protected override Type ExpectedGuaranteeForArrivalGroupBoxPanelLayoutType => typeof(EU.GUI.GuaranteeGroupBoxWithOverrideLayout);

		protected override Type ExpectedHouseConsignmentDifferencesLayoutType => typeof(EU.NCTS.GUI.HouseConsignmentDifferencesLayout);

		protected override Type ExpectedUnloadingDifferencesDeclaredValuePanelLayoutType => typeof(EU.NCTS.GUI.UnloadingDifferencesDeclaredValueLayout);

		protected override Type ExpectedUnloadingDifferencesUnloadedValuePanelLayoutType => typeof(EU.NCTS.GUI.UnloadingDifferencesUnloadedValueLayout);

		protected override Type ExpectedEventsPanelLayoutType => typeof(EU.NCTS.GUI.Phase5EventLayout);

		protected override Type ExpectedNctsPackagePanelLayoutWithGridType => typeof(NctsPackageLayoutWithGrid);

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

		protected override Type ExpectedDeclarationSupportingDocumentsTabControlType => typeof(DeclarationSupportingDocumentsGridUserControl);

		protected override Type ExpectedDeclarationPreviousDocumentsTabControlType => typeof(EU.NCTS.GUI.DeclarationPreviousDocumentsGridUserControl);

		protected override Type ExpectedDeclarationAdditionalDocumentsTabControlType => typeof(DeclarationAdditionalDocumentsGridUserControl);

		protected override Type ExpectedArrivalTransportInfoGridType => typeof(EU.NCTS.GUI.ArrivalTransportInfosGridUserControl);

		protected override Type ExpectedHouseConsignmentTransportDeparturePanelLayoutType => typeof(EU.NCTS.GUI.HouseConsignmentTransportDepartureLayout);

		protected override Type ExpectedGoodsItemsTabUserControlType => typeof(Phase5GoodsItemsTabUserControl);

		protected override Type ExpectedDeclarationAuthorizationsTabControlType => typeof(EU.NCTS.GUI.Phase5DeclarationAuthorizationsTabUserControl);

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

		protected override Type ExpectedContainersGridColumnLayout => typeof(EU.NCTS.GUI.ContainersGridColumnLayout);

		protected override Type ExpectedAdditionalSealsGridColumnLayout => typeof(AdditionalSealsGridColumnLayout);

		protected override Type ExpectedNctsPackagesGridColumnLayout => typeof(NctsPackagesGridColumnLayout);

		protected override Type ExpectedGetDepartureGoodsItemsGridColumnLayout => typeof(EU.NCTS.GUI.Phase5GoodsItemDetailsGridColumnsLayout);

		protected override Type ExpectedDeclarationDetailsTabUserControlType => typeof(EU.NCTS.GUI.Phase5DeclarationDetailsTabUserControl);

		protected override Type ExpectedHouseConsignmentsTabUserControlType => typeof(HouseConsignmentsTabUserControl);

		protected override Type ExpectedGoodsItemAdditionalDocumentsTabUserControlType => typeof(Phase5GoodsItemAdditionalDocumentsTabUserControl);

		protected override Type ExpectedGoodsItemsPreviousDocumentsGridColumnLayout => typeof(EU.NCTS.GUI.Phase5GoodsItemPreviousDocumentsGridColumnsLayout);

		protected override Type ExpectedGoodsItemDifferencesDetailsGridColumnLayout => typeof(EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsGridColumnsLayout);

		protected override Type ExpectedMessageTabUserControlType => typeof(EU.NCTS.GUI.MessagesTabUserControl);

		protected override Type ExpectedTransportAndPackagingTabUserControlType => typeof(EU.NCTS.GUI.Phase5TransportAndPackagingTabUserControl);

		protected override Type ExpectedUnloadingDifferencesTabUserControlType => typeof(Phase5UnloadingDifferencesTabUserControl);

		protected override Type LiabilityDetailsLayoutType => typeof(EU.NCTS.GUI.LiabilityDetailsLayout);

		protected override Type ExpectedGetHouseConsignmentDetailsGridColumnLayout => typeof(EU.NCTS.GUI.HouseConsignmentDetailsGridColumnLayout);

		protected override Type ExpectedGoodsItemPackagesUserControlType => typeof(GUI.GoodsItemPackagesUserControl);
	}
}
