using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public interface INctsPhase5LayoutProvider
	{
		IPanelLayoutProvider TraderDetailsPanelLayout { get; }

		IPanelLayoutProvider DepartureDetailsPanelLayout { get; }

		IPanelLayoutProvider DeclarationDetailsPanelLayout { get; }

		Type DeclarationDetailsTabUserControlType { get; }

		IPanelLayoutProvider TransportAndPackagingPanelLayout { get; }

		IPanelLayoutProvider TransportDeparturePanelLayout { get; }

		IPanelLayoutProvider HouseConsignmentTransportDeparturePanelLayout { get; }

		IPanelLayoutProvider TransportBorderPanelLayout { get; }

		IPanelLayoutProvider SecurityAtDeparturePanelLayout { get; }

		IPanelLayoutProvider MiscellanousOptionsPanelLayout { get; }

		Type CustomsOfficesUserControlType { get; }

		Type GuaranteesUserControlType { get; }

		IPanelLayoutWithGridProvider DeclarationServicePanelLayoutWithGrid { get; }

		Type DeclarationAuthorizationsTabControlType { get; }

		Type DeclarationSupportingDocumentsTabControlType { get; }

		Type DeclarationPreviousDocumentsTabControlType { get; }

		Type DeclarationAdditionalDocumentsTabControlType { get; }

		IPanelLayoutWithGridProvider GoodsItemDetailsPanelLayoutWithGrid { get; }

		Type TransportAndPackagingTabUserControlType { get; }

		Type HouseConsignmentsTabUserControlType { get; }

		Type MessageTabUserControlType { get; }

		IPanelLayoutWithGridProvider HouseConsignmentDetailsPanelLayoutWithGrid { get; }

		IPanelLayoutWithGridProvider HouseConsignmentPreviousDocumentPanelLayoutWithGrid { get; }

		IPanelLayoutWithGridProvider HouseConsignmentSupportingDocumentPanelLayoutWithGrid { get; }

		IPanelLayoutWithGridProvider HouseConsignmentSupplyChainActorPanelLayoutWithGrid { get; }

		IPanelLayoutWithGridProvider HouseConsignmentAdditionalDocumentPanelLayoutWithGrid { get; }

		IPanelLayoutWithGridProvider GoodsItemSupportingDocumentPanelLayoutWithGrid { get; }

		IPanelLayoutWithGridProvider GoodsItemPreviousDocumentPanelLayoutWithGrid { get; }

		IPanelLayoutWithGridProvider GoodsItemSupplyChainActorPanelLayoutWithGrid { get; }

		Type GoodsItemPackagesUserControlType { get; }

		IPanelLayoutProvider GoodsItemPackagesAndContainersPanelLayout { get; }

		IPanelLayoutProvider GoodsItemPackagePanelLayout { get; }

		IGridColumnLayoutProvider GoodsItemPackageGridPanelLayout { get; }

		IPanelLayoutProvider ArrivalNotificationDetailsPanelLayout { get; }

		IPanelLayoutProvider ArrivalDeclarationDetailsPanelLayout { get; }

		IEnumerable<ITabPage> AdditionalGoodsItemTabPages { get; }

		IEnumerable<string> ReorderGoodsItemTabPageNames(IEnumerable<string> defaultTabPageNames);

		Type UnloadingDifferencesTabUserControlType { get; }

		IPanelLayoutProvider UnloadingDetailsPanelLayout { get; }

		IPanelLayoutProvider GuaranteeForArrivalGroupBoxPanelLayout { get; }

		IPanelLayoutWithGridProvider HouseConsignmentAdditionalDocumentsLayout { get; }

		IPanelLayoutWithGridProvider HouseConsignmentPreviousDocumentsLayout { get; }

		IPanelLayoutProvider UnloadingDifferencesDeclaredValuePanelLayout { get; }

		IPanelLayoutProvider UnloadingDifferencesUnloadedValuePanelLayout { get; }

		IPanelLayoutProvider EventPanelLayout { get; }

		IPanelLayoutWithGridProvider NctsPackageLayoutWithGrid { get; }

		IPanelLayoutWithGridProvider HouseConsignmentDifferencesLayout { get; }

		IPanelLayoutWithGridProvider IncidentDetailsPanelLayoutWithGrid { get; }

		IPanelLayoutWithGridProvider SupportingDocumentsLayout { get; }

		IPanelLayoutWithGridProvider HouseConsignmentGoodItemsSupportingDocumentsLayout { get; }

		IPanelLayoutProvider IncidentTransportMeansLayout { get; }

		IPanelLayoutProvider GoodsItemDifferencesDetailsLayout { get; }

		IPanelLayoutProvider GoodsItemDifferencesDetailsColumnLayout { get; }

		Type DeclarationSupplyChainActorsTabControlType { get; }

		Type ArrivalContainersEquipmentGrid { get; }

		Type ArrivalSealsGrid { get; }

		Type UnloadingDifferencesPreviousDocumentPanelGridUserControlType { get; }

		IPanelLayoutWithGridProvider DepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid { get; }

		IPanelLayoutWithGridProvider ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid { get; }

		Type ArrivalTransportInfoGridType { get; }

		Type GoodsItemsTabUserControlType { get; }

		Type ArrivalNotificationTabControlType { get; }

		IEnumerable<ITabPage> AdditionalDeclarationDetailTabPages { get; }

		IEnumerable<string> ReorderDeclarationDetailTabPagesNames(IEnumerable<string> defaultTabPageNames);

		IEnumerable<ITabPage> AdditionalArrivalTabPages { get; }

		IEnumerable<string> ReorderArrivalTabPagesNames(IEnumerable<string> defaultTabPageNames);

		IGridColumnLayoutProvider GetMessageSendingGridColumnLayout(BaseMessageSendingObjectParent messageSendingObjectParent);

		IGridColumnLayoutProvider SuportingDocumentGridColumnLayout { get; }

		IGridColumnLayoutProvider AdditionalDocumentsGridColumnLayout { get; }

		IGridColumnLayoutProvider UnloadingDifferencesSuportingDocumentGridColumnLayout { get; }

		IGridColumnLayoutProvider UnloadingDifferencesAdditionalDocumentsGridColumnLayout { get; }

		IGridColumnLayoutProvider ContainersGridColumnLayout { get; }

		IGridColumnLayoutProvider AdditionalSealsGridColumnLayout { get; }

		IGridColumnLayoutProvider NctsPackagesGridColumnLayout { get; }

		IGridColumnLayoutProvider GetDepartureGoodsItemsGridColumnLayout();

		IGridColumnLayoutProvider GetDepartureMovementGridColumnLayout();

		Type GoodsItemAdditionalDocumentsTabUserControlType { get; }

		IGridColumnLayoutProvider GetGoodsItemsPreviousDocumentsGridColumnLayout();

		IGridColumnLayoutProvider GetGoodsItemDifferencesDetailsGridColumnLayout();

		IPanelLayoutProvider LiabilityDetailsLayout { get; }

		IGridColumnLayoutProvider GetHouseConsignmentDetailsGridColumnLayout();
	}
}
