using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public sealed class NctsPhase5LayoutProvider : EU.NCTS.GUI.INctsPhase5LayoutProvider
	{
		public IGridColumnLayoutProvider GetDepartureMovementGridColumnLayout() => new EU.NCTS.GUI.DepartureMovementDetailsGridColumnsLayout();

		public IPanelLayoutProvider TraderDetailsPanelLayout => new Phase5TraderDetailsLayout();

		public IPanelLayoutProvider DepartureDetailsPanelLayout => new DepartureDetailsLayout();

		public IPanelLayoutProvider DeclarationDetailsPanelLayout => new Phase5DeclarationDetailsLayout();

		public IPanelLayoutProvider TransportAndPackagingPanelLayout => new Phase5TransportAndPackagingLayout();

		public IPanelLayoutProvider TransportDeparturePanelLayout => new EU.NCTS.GUI.TransportDepartureLayout();

		public IPanelLayoutProvider TransportBorderPanelLayout => new EU.NCTS.GUI.TransportBorderLayout();

		public IPanelLayoutProvider SecurityAtDeparturePanelLayout => new Phase5SecurityAtDepartureLayout();

		public IPanelLayoutProvider MiscellanousOptionsPanelLayout => new EU.NCTS.GUI.Phase5MiscellaneousOptionsLayout();

		public Type CustomsOfficesUserControlType => typeof(EU.NCTS.GUI.Phase5CustomsOfficesUserControl);

		public Type GuaranteesUserControlType => typeof(Phase5GuaranteesUserControl);

		public IPanelLayoutWithGridProvider DeclarationServicePanelLayoutWithGrid => new EU.NCTS.GUI.ServiceLayoutWithGrid();

		public IPanelLayoutWithGridProvider HouseConsignmentDetailsPanelLayoutWithGrid => new HouseConsignmentDetailsLayoutWithGrid();

		public IPanelLayoutWithGridProvider HouseConsignmentPreviousDocumentPanelLayoutWithGrid => new EU.NCTS.GUI.HouseConsignmentPreviousDocumentLayoutWithGrid();

		public IPanelLayoutWithGridProvider HouseConsignmentSupportingDocumentPanelLayoutWithGrid => new EU.NCTS.GUI.HouseConsignmentSupportingDocumentLayoutWithGrid();

		public IPanelLayoutWithGridProvider HouseConsignmentSupplyChainActorPanelLayoutWithGrid => new EU.NCTS.GUI.HouseConsignmentSupplyChainActorLayoutWithGrid();

		public IPanelLayoutWithGridProvider HouseConsignmentAdditionalDocumentPanelLayoutWithGrid => new EU.NCTS.GUI.HouseConsignmentAdditionalDocumentLayoutWithGrid();

		public IPanelLayoutWithGridProvider GoodsItemDetailsPanelLayoutWithGrid => new Phase5GoodsItemDetailsLayoutWithGrid();

		public IPanelLayoutWithGridProvider GoodsItemSupportingDocumentPanelLayoutWithGrid => new EU.NCTS.GUI.Phase5GoodsItemSupportingDocumentLayoutWithGrid();

		public IPanelLayoutWithGridProvider GoodsItemPreviousDocumentPanelLayoutWithGrid => new EU.NCTS.GUI.Phase5GoodsItemPreviousDocumentLayoutWithGrid();

		public IPanelLayoutWithGridProvider GoodsItemSupplyChainActorPanelLayoutWithGrid => new EU.NCTS.GUI.Phase5GoodsItemSupplyChainActorLayoutWithGrid();

		public IPanelLayoutProvider GoodsItemPackagesAndContainersPanelLayout => new EU.NCTS.GUI.Phase5GoodsItemPackagesAndContainersLayout();

		public IPanelLayoutProvider GoodsItemPackagePanelLayout => new Phase5GoodsItemPackageLayout();

		public IGridColumnLayoutProvider GoodsItemPackageGridPanelLayout => new Phase5GoodsItemPackageGridLayout();

		public IPanelLayoutProvider ArrivalNotificationDetailsPanelLayout => new ArrivalNotificationDetailsLayout();

		public IEnumerable<ITabPage> AdditionalGoodsItemTabPages => new List<ITabPage>();

		public IEnumerable<string> ReorderGoodsItemTabPageNames(IEnumerable<string> defaultTabPageNames) => defaultTabPageNames;

		public IPanelLayoutProvider ArrivalDeclarationDetailsPanelLayout => new Phase5ArrivalDeclarationDetailsLayout();

		public IPanelLayoutProvider UnloadingDetailsPanelLayout => new EU.NCTS.GUI.UnloadingDetailsLayout();

		public IPanelLayoutProvider GuaranteeForArrivalGroupBoxPanelLayout => new EU.GUI.GuaranteeGroupBoxWithOverrideLayout();

		public IPanelLayoutProvider UnloadingDifferencesDeclaredValuePanelLayout => new EU.NCTS.GUI.UnloadingDifferencesDeclaredValueLayout();

		public IPanelLayoutProvider UnloadingDifferencesUnloadedValuePanelLayout => new EU.NCTS.GUI.UnloadingDifferencesUnloadedValueLayout();

		public IPanelLayoutProvider EventPanelLayout => new EU.NCTS.GUI.Phase5EventLayout();

		public IPanelLayoutWithGridProvider NctsPackageLayoutWithGrid => new NctsPackageLayoutWithGrid();

		public IPanelLayoutWithGridProvider HouseConsignmentDifferencesLayout => new EU.NCTS.GUI.HouseConsignmentDifferencesLayout();

		public IPanelLayoutWithGridProvider HouseConsignmentAdditionalDocumentsLayout => new EU.NCTS.GUI.HouseConsignmentAdditionalDocumentsLayout();

		public IPanelLayoutWithGridProvider HouseConsignmentPreviousDocumentsLayout => new EU.NCTS.GUI.Phase5HouseConsignmentDifferencesPreviousDocumentLayoutWithGrid();

		public IPanelLayoutWithGridProvider IncidentDetailsPanelLayoutWithGrid => new EU.NCTS.GUI.IncidentDetailsLayoutWithGrid();

		public IPanelLayoutWithGridProvider SupportingDocumentsLayout => new EU.NCTS.GUI.Phase5HouseConsignmentDifferencesSupportingDocumentLayoutWithGrid();

		public IPanelLayoutWithGridProvider HouseConsignmentGoodItemsSupportingDocumentsLayout => new EU.NCTS.GUI.HouseConsignmentGoodItemsSupportingDocumentsLayout();

		public IPanelLayoutProvider IncidentTransportMeansLayout => new EU.NCTS.GUI.Phase5TransportMeansLayout();

		public IPanelLayoutProvider GoodsItemDifferencesDetailsLayout => new EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsLayout();

		public IPanelLayoutProvider GoodsItemDifferencesDetailsColumnLayout => new EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsColumnLayout();

		public Type DeclarationSupplyChainActorsTabControlType => typeof(EU.NCTS.GUI.Phase5DeclarationSupplyChainActorsTabUserControl);

		public Type UnloadingDifferencesPreviousDocumentPanelGridUserControlType => typeof(EU.NCTS.GUI.Phase5UnloadingDifferencesPreviousDocumentsGridUserControl);

		public Type ArrivalContainersEquipmentGrid => typeof(EU.NCTS.GUI.Phase5ArrivalContainersEquipmentGridUserControl);

		public Type ArrivalSealsGrid => typeof(EU.NCTS.GUI.Phase5ArrivalSealsGridUserControl);

		public IPanelLayoutWithGridProvider DepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid => new EU.NCTS.GUI.Phase5DepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid();

		public IPanelLayoutWithGridProvider ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid => new EU.NCTS.GUI.Phase5ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid();

		public Type ArrivalTransportInfoGridType => typeof(EU.NCTS.GUI.ArrivalTransportInfosGridUserControl);

		public IPanelLayoutProvider HouseConsignmentTransportDeparturePanelLayout => new EU.NCTS.GUI.HouseConsignmentTransportDepartureLayout();

		public Type GoodsItemsTabUserControlType => typeof(Phase5GoodsItemsTabUserControl);

		public Type DeclarationAuthorizationsTabControlType => typeof(EU.NCTS.GUI.Phase5DeclarationAuthorizationsTabUserControl);

		public Type ArrivalNotificationTabControlType => typeof(Phase5ArrivalNotificationTabUserControl);

		public Type DeclarationSupportingDocumentsTabControlType => typeof(DeclarationSupportingDocumentsGridUserControl);

		public Type DeclarationPreviousDocumentsTabControlType => typeof(EU.NCTS.GUI.DeclarationPreviousDocumentsGridUserControl);

		public Type DeclarationAdditionalDocumentsTabControlType => typeof(DeclarationAdditionalDocumentsGridUserControl);

		public IEnumerable<ITabPage> AdditionalDeclarationDetailTabPages => Enumerable.Empty<ITabPage>();

		public IEnumerable<string> ReorderDeclarationDetailTabPagesNames(IEnumerable<string> defaultTabPageNames) => defaultTabPageNames;

		public IEnumerable<ITabPage> AdditionalArrivalTabPages => Enumerable.Empty<ITabPage>();

		public IEnumerable<string> ReorderArrivalTabPagesNames(IEnumerable<string> defaultTabPageNames) => defaultTabPageNames;

		public IGridColumnLayoutProvider GetMessageSendingGridColumnLayout(BaseMessageSendingObjectParent messageSendingObjectParent) => null;

		public IGridColumnLayoutProvider SuportingDocumentGridColumnLayout => new SupportingDocumentsGridColumnLayout();

		public IGridColumnLayoutProvider AdditionalDocumentsGridColumnLayout => new AdditionalDocumentsGridColumnLayout();

		public IGridColumnLayoutProvider UnloadingDifferencesSuportingDocumentGridColumnLayout => new UnloadingDifferencesSupportingDocumentsGridColumnLayout();

		public IGridColumnLayoutProvider UnloadingDifferencesAdditionalDocumentsGridColumnLayout => new UnloadingDifferencesAdditionalDocumentsGridColumnLayout();

		public IGridColumnLayoutProvider ContainersGridColumnLayout => new EU.NCTS.GUI.ContainersGridColumnLayout();

		public IGridColumnLayoutProvider AdditionalSealsGridColumnLayout => new AdditionalSealsGridColumnLayout();

		public IGridColumnLayoutProvider NctsPackagesGridColumnLayout => new NctsPackagesGridColumnLayout();

		public Type DeclarationDetailsTabUserControlType => typeof(EU.NCTS.GUI.Phase5DeclarationDetailsTabUserControl);

		public IGridColumnLayoutProvider GetDepartureGoodsItemsGridColumnLayout() => new EU.NCTS.GUI.Phase5GoodsItemDetailsGridColumnsLayout();

		public Type HouseConsignmentsTabUserControlType => typeof(HouseConsignmentsTabUserControl);

		public Type GoodsItemAdditionalDocumentsTabUserControlType => typeof(Phase5GoodsItemAdditionalDocumentsTabUserControl);

		public Type MessageTabUserControlType => typeof(EU.NCTS.GUI.MessagesTabUserControl);

		public IGridColumnLayoutProvider GetGoodsItemsPreviousDocumentsGridColumnLayout() => new EU.NCTS.GUI.Phase5GoodsItemPreviousDocumentsGridColumnsLayout();

		public IGridColumnLayoutProvider GetGoodsItemDifferencesDetailsGridColumnLayout() => new EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsGridColumnsLayout();

		public IGridColumnLayoutProvider GetHouseConsignmentDetailsGridColumnLayout() => new EU.NCTS.GUI.HouseConsignmentDetailsGridColumnLayout();

		public Type TransportAndPackagingTabUserControlType => typeof(EU.NCTS.GUI.Phase5TransportAndPackagingTabUserControl);

		public Type UnloadingDifferencesTabUserControlType => typeof(Phase5UnloadingDifferencesTabUserControl);

		public IPanelLayoutProvider LiabilityDetailsLayout => new EU.NCTS.GUI.LiabilityDetailsLayout();

		public Type GoodsItemPackagesUserControlType => typeof(GoodsItemPackagesUserControl);
	}
}
