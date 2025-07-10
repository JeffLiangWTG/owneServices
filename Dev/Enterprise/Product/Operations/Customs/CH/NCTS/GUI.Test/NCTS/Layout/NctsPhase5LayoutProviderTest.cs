using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(NctsPhase5LayoutProvider))]
sealed class NctsPhase5LayoutProviderTest : EU.NCTS.GUI.Testing.NctsPhase5LayoutProviderAbstractTest
{
	protected override Type ExpectedDepartureMovementGridColumnLayout => typeof(EU.NCTS.GUI.DepartureMovementDetailsGridColumnsLayout);

	protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Switzerland;

	protected override Type ExpectedTraderDetailsPanelLayoutType => typeof(EU.NCTS.GUI.Phase5TraderDetailsLayout);

	protected override Type ExpectedDepartureDetailsLayoutType => typeof(DepartureDetailsLayout);

	protected override Type ExpectedDeclarationDetailsPanelLayoutType => typeof(Phase5DeclarationDetailsLayout);

	protected override Type ExpectedTransportAndPackagingPanelLayoutType => typeof(TransportAndPackagingLayout);

	protected override Type ExpectedTransportDeparturePanelLayoutType => typeof(TransportDepartureLayout);

	protected override Type ExpectedTransportBorderPanelLayoutType => typeof(EU.NCTS.GUI.TransportBorderLayout);

	protected override Type ExpectedSecurityAtDeparturePanelLayoutType => typeof(EU.NCTS.GUI.Phase5SecurityAtDepartureLayout);

	protected override Type ExpectedMiscellanousOptionsPanelLayoutType => typeof(EU.NCTS.GUI.Phase5MiscellaneousOptionsLayout);

	protected override Type ExpectedCustomsOfficesUserControlType => typeof(CustomsOfficesUserControl);

	protected override Type ExpectedGuaranteesUserControlType => typeof(GuaranteesUserControl);

	protected override Type ExpectedHouseConsignmentDetailsPanelLayoutWithGridType => typeof(EU.NCTS.GUI.HouseConsignmentDetailsLayoutWithGrid);

	protected override Type ExpectedHouseConsignmentPreviousDocumentPanelLayoutWithGridType => typeof(EU.NCTS.GUI.HouseConsignmentPreviousDocumentLayoutWithGrid);

	protected override Type ExpectedHouseConsignmentSupportingDocumentPanelLayoutWithGridType => typeof(EU.NCTS.GUI.HouseConsignmentSupportingDocumentLayoutWithGrid);

	protected override Type ExpectedHouseConsignmentSupplyChainActorPanelLayoutWithGridType => typeof(EU.NCTS.GUI.HouseConsignmentSupplyChainActorLayoutWithGrid);

	protected override Type ExpectedHouseConsignmentAdditionalDocumentPanelLayoutWithGridType => typeof(EU.NCTS.GUI.HouseConsignmentAdditionalDocumentLayoutWithGrid);

	protected override Type ExpectedDeclarationServicePanelLayoutWithGridType => typeof(EU.NCTS.GUI.ServiceLayoutWithGrid);

	protected override Type ExpectedDeclarationAuthorizationsTabControlType => typeof(EU.NCTS.GUI.Phase5DeclarationAuthorizationsTabUserControl);

	protected override Type ExpectedGoodsItemDetailsLayoutType => typeof(Phase5GoodsItemDetailsLayoutWithGrid);

	protected override Type ExpectedGoodsItemSupportingDocumentPanelLayoutWithGridType => typeof(EU.NCTS.GUI.Phase5GoodsItemSupportingDocumentLayoutWithGrid);

	protected override Type ExpectedGoodsItemPreviousDocumentPanelLayoutWithGridType => typeof(GoodsItemPreviousDocumentLayoutWithGrid);

	protected override Type ExpectedGoodsItemSupplyChainActorPanelLayoutWithGridType => typeof(EU.NCTS.GUI.Phase5GoodsItemSupplyChainActorLayoutWithGrid);

	protected override Type ExpectedGoodsItemPackagesAndContainersPanelLayoutType => typeof(EU.NCTS.GUI.Phase5GoodsItemPackagesAndContainersLayout);

	protected override Type ExpectedGoodsItemPackagePanelLayoutType => typeof(EU.NCTS.GUI.Phase5GoodsItemPackageLayout);

	protected override Type ExpectedGoodsItemPackageGridPanelLayoutType => typeof(EU.NCTS.GUI.Phase5GoodsItemPackageGridLayout);

	protected override Type ExpectedArrivalNotificationDetailsPanelLayoutType => typeof(ArrivalNotificationDetailsLayout);

	protected override Type ExpectedArrivalDeclarationDetailsPanelLayoutType => typeof(EU.NCTS.GUI.Phase5ArrivalDeclarationDetailsLayout);

	protected override Type ExpectedEventsPanelLayoutType => typeof(EU.NCTS.GUI.Phase5EventLayout);

	protected override Type ExpectedUnloadingDetailsPanelLayoutType => typeof(UnloadingDetailsLayout);

	protected override Type ExpectedGuaranteeForArrivalGroupBoxPanelLayoutType => typeof(EU.GUI.GuaranteeGroupBoxWithOverrideLayout);

	protected override Type ExpectedHouseConsignmentDifferencesLayoutType => typeof(HouseConsignmentDifferencesLayout);

	protected override Type ExpectedHouseConsignmentAdditionalDocumentsLayoutType => typeof(EU.NCTS.GUI.HouseConsignmentAdditionalDocumentsLayout);

	protected override Type ExpectedHouseConsignmentPreviousDocumentsLayoutType => typeof(EU.NCTS.GUI.Phase5HouseConsignmentDifferencesPreviousDocumentLayoutWithGrid);

	protected override Type ExpectedUnloadingDifferencesDeclaredValuePanelLayoutType => typeof(EU.NCTS.GUI.UnloadingDifferencesDeclaredValueLayout);

	protected override Type ExpectedUnloadingDifferencesUnloadedValuePanelLayoutType => typeof(EU.NCTS.GUI.UnloadingDifferencesUnloadedValueLayout);

	protected override IEnumerable<ITabPage> ExpectedAdditionalGoodsItemTabPages => new[] { new RestrictionsTabPage() };

	protected override IEnumerable<string> ExpectedReorderGoodsItemTabPageNames => new []
	{
		"GoodsItemDetailsTabPage",
		"RestrictionsTabPage",
		"GoodsItemPackagesAndContainersTabPage",
		"GoodsItemSupportingDocumentsTabPage",
		"GoodsItemAdditionalDocumentsTabPage",
		"GoodsItemPreviousDocumentsTabPage",
		"GoodsItemSupplyChainActorsTabPage"
	};

	protected override Type ExpectedNctsPackagePanelLayoutWithGridType => typeof(EU.NCTS.GUI.NctsPackageLayoutWithGrid);

	protected override Type ExpectIncidentDetailsPanelLayoutWithGridType => typeof(EU.NCTS.GUI.IncidentDetailsLayoutWithGrid);

	protected override Type ExpectedSupportingDocumentsLayoutType => typeof(EU.NCTS.GUI.Phase5HouseConsignmentDifferencesSupportingDocumentLayoutWithGrid);

	protected override Type ExpectedHouseConsignmentGoodItemsSupportingDocumentsLayoutType => typeof(EU.NCTS.GUI.HouseConsignmentGoodItemsSupportingDocumentsLayout);

	protected override Type ExpectedIncidentTransportMeansLayoutType => typeof(EU.NCTS.GUI.Phase5TransportMeansLayout);

	protected override Type ExpectedGoodsItemDifferencesDetailsLayout => typeof(EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsLayout);

	protected override Type ExpectedGoodsItemDifferencesDetailsColumnLayout => typeof(GoodsItemDifferencesDetailsColumnLayout);

	protected override Type ExpectedDeclarationSupplyChainActorsTabControlType => typeof(EU.NCTS.GUI.Phase5DeclarationSupplyChainActorsTabUserControl);

	protected override Type ExpectedArrivalContainersEquipmentGridType => typeof(EU.NCTS.GUI.Phase5ArrivalContainersEquipmentGridUserControl);

	protected override Type ExpectedArrivalSealsGridType => typeof(ArrivalSealsGridUserControl);

	protected override Type ExpectedUnloadingDifferencesPreviousDocumentPanelGridUserControlType => typeof(EU.NCTS.GUI.Phase5UnloadingDifferencesPreviousDocumentsGridUserControl);

	protected override Type ExpectedDepartureGoodItemsAdditionalDocumentsLayoutType => typeof(EU.NCTS.GUI.Phase5DepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid);

	protected override Type ExpectedArrivalGoodItemsAdditionalDocumentsLayoutType => typeof(EU.NCTS.GUI.Phase5ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid);

	protected override Type ExpectedDeclarationSupportingDocumentsTabControlType => typeof(EU.NCTS.GUI.DeclarationSupportingDocumentsGridUserControl);

	protected override Type ExpectedDeclarationPreviousDocumentsTabControlType => typeof(EU.NCTS.GUI.DeclarationPreviousDocumentsGridUserControl);

	protected override Type ExpectedDeclarationAdditionalDocumentsTabControlType => typeof(EU.NCTS.GUI.DeclarationAdditionalDocumentsGridUserControl);

	protected override Type ExpectedArrivalTransportInfoGridType => typeof(EU.NCTS.GUI.ArrivalTransportInfosGridUserControl);

	protected override Type ExpectedHouseConsignmentTransportDeparturePanelLayoutType => null;

	protected override Type ExpectedGoodsItemsTabUserControlType => typeof(Phase5GoodsItemsTabUserControl);

	protected override Type ExpectedArrivalNotificationTabControlType => typeof(Phase5ArrivalNotificationTabUserControl);

	protected override IEnumerable<ITabPage> ExpectedAdditionalDeclarationDetailTabPages => new[] { new ExportDeclarationsTabPage(), };

	protected override IEnumerable<string> ExpectedReorderDeclarationDetailTabPageNames => new[]
	{
			// Please talk with Swiss Customs team PM if you're adding/removing Declaration Detail Tabs in EU.
			"ExportDeclarationsTabPage",
			"SupportingDocumentsTabPage",
			"AdditionalDocumentsTabPage",
			"PreviousDocumentsTabPage",
			"CountryOfRoutingTabPage",
			"SupplyChainActorTabPage",
	};

	protected override IEnumerable<ITabPage> ExpectedAdditionalArrivalTabPages => new[] { new AdditionalGoodsInformationTabPage() };

	protected override IEnumerable<string> ExpectedReorderArrivalTabPageNames
	{
		get
		{
			var tabPageNames = DefaultArrivalTabPagesNames.ToList();
			tabPageNames.Insert(1, nameof(AdditionalGoodsInformationTabPage));
			return tabPageNames;
		}
	}

	protected override Type ExpectedMessageSendingGridColumnLayout => null;

	protected override Type ExpectedSuportingDocumentGridColumnLayoutType => typeof(EU.NCTS.GUI.SupportingDocumentsGridColumnLayout);

	protected override Type ExpectedAdditionalDocumentsGridColumnLayoutType => typeof(EU.NCTS.GUI.AdditionalDocumentsGridColumnLayout);

	protected override Type ExpectedUnloadingDifferencesSuportingDocumentGridColumnLayoutType => typeof(EU.NCTS.GUI.UnloadingDifferencesSupportingDocumentsGridColumnLayout);

	protected override Type ExpectedUnloadingDifferencesAdditionalDocumentsGridColumnLayoutType => typeof(EU.NCTS.GUI.UnloadingDifferencesAdditionalDocumentsGridColumnLayout);

	protected override Type ExpectedContainersGridColumnLayout => typeof(EU.NCTS.GUI.ContainersGridColumnLayout);

	protected override Type ExpectedAdditionalSealsGridColumnLayout => typeof(EU.NCTS.GUI.AdditionalSealsGridColumnLayout);

	protected override Type ExpectedNctsPackagesGridColumnLayout => typeof(EU.NCTS.GUI.NctsPackagesGridColumnLayout);

	protected override Type ExpectedGetDepartureGoodsItemsGridColumnLayout => typeof(GoodsItemDetailsGridColumnLayout);

	protected override Type ExpectedDeclarationDetailsTabUserControlType => typeof(DeclarationDetailsTabUserControl);

	protected override Type ExpectedTransportAndPackagingTabUserControlType => typeof(TransportAndPackagingTabUserControl);

	protected override Type ExpectedHouseConsignmentsTabUserControlType => typeof(HouseConsignmentsTabUserControl);

	protected override Type ExpectedGoodsItemAdditionalDocumentsTabUserControlType => typeof(EU.NCTS.GUI.Phase5GoodsItemAdditionalDocumentsTabUserControl);

	protected override Type ExpectedGoodsItemsPreviousDocumentsGridColumnLayout => typeof(GoodsItemPreviousDocumentsGridColumnsLayout);

	protected override Type ExpectedGoodsItemDifferencesDetailsGridColumnLayout => typeof(GoodsItemDifferencesDetailsGridColumnLayout);

	protected override Type ExpectedMessageTabUserControlType => typeof(MessagesTabUserControl);

	protected override Type ExpectedUnloadingDifferencesTabUserControlType => typeof(UnloadingDifferencesTabUserControl);

	protected override Type LiabilityDetailsLayoutType => typeof(EU.NCTS.GUI.LiabilityDetailsLayout);

	protected override Type ExpectedGetHouseConsignmentDetailsGridColumnLayout => typeof(EU.NCTS.GUI.HouseConsignmentDetailsGridColumnLayout);

	protected override Type ExpectedGoodsItemPackagesUserControlType => typeof(EU.NCTS.GUI.GoodsItemPackagesUserControl);
}
