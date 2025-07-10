using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public sealed class NctsPhase5LayoutProvider : EU.NCTS.GUI.INctsPhase5LayoutProvider
{
	public IGridColumnLayoutProvider GetDepartureMovementGridColumnLayout() => new EU.NCTS.GUI.DepartureMovementDetailsGridColumnsLayout();

	public IPanelLayoutProvider TraderDetailsPanelLayout => new EU.NCTS.GUI.Phase5TraderDetailsLayout();

	public IPanelLayoutProvider DepartureDetailsPanelLayout => new DepartureDetailsLayout();

	public IPanelLayoutProvider DeclarationDetailsPanelLayout => new Phase5DeclarationDetailsLayout();

	public IPanelLayoutProvider TransportAndPackagingPanelLayout => new TransportAndPackagingLayout();

	public IPanelLayoutProvider TransportDeparturePanelLayout => new TransportDepartureLayout();

	public IPanelLayoutProvider TransportBorderPanelLayout => new EU.NCTS.GUI.TransportBorderLayout();

	public IPanelLayoutProvider SecurityAtDeparturePanelLayout => new EU.NCTS.GUI.Phase5SecurityAtDepartureLayout();

	public IPanelLayoutProvider MiscellanousOptionsPanelLayout => new EU.NCTS.GUI.Phase5MiscellaneousOptionsLayout();

	public IPanelLayoutWithGridProvider DeclarationServicePanelLayoutWithGrid => new EU.NCTS.GUI.ServiceLayoutWithGrid();

	public Type DeclarationAuthorizationsTabControlType => typeof(EU.NCTS.GUI.Phase5DeclarationAuthorizationsTabUserControl);

	public IPanelLayoutWithGridProvider HouseConsignmentDetailsPanelLayoutWithGrid => new EU.NCTS.GUI.HouseConsignmentDetailsLayoutWithGrid();

	public IPanelLayoutWithGridProvider HouseConsignmentPreviousDocumentPanelLayoutWithGrid => new EU.NCTS.GUI.HouseConsignmentPreviousDocumentLayoutWithGrid();

	public IPanelLayoutWithGridProvider HouseConsignmentSupportingDocumentPanelLayoutWithGrid => new EU.NCTS.GUI.HouseConsignmentSupportingDocumentLayoutWithGrid();

	public IPanelLayoutWithGridProvider HouseConsignmentSupplyChainActorPanelLayoutWithGrid => new EU.NCTS.GUI.HouseConsignmentSupplyChainActorLayoutWithGrid();

	public IPanelLayoutWithGridProvider HouseConsignmentAdditionalDocumentPanelLayoutWithGrid => new EU.NCTS.GUI.HouseConsignmentAdditionalDocumentLayoutWithGrid();

	public IPanelLayoutWithGridProvider GoodsItemDetailsPanelLayoutWithGrid => new Phase5GoodsItemDetailsLayoutWithGrid();

	public IPanelLayoutWithGridProvider GoodsItemSupportingDocumentPanelLayoutWithGrid => new EU.NCTS.GUI.Phase5GoodsItemSupportingDocumentLayoutWithGrid();

	public IPanelLayoutWithGridProvider GoodsItemPreviousDocumentPanelLayoutWithGrid => new GoodsItemPreviousDocumentLayoutWithGrid();

	public IPanelLayoutWithGridProvider GoodsItemSupplyChainActorPanelLayoutWithGrid => new EU.NCTS.GUI.Phase5GoodsItemSupplyChainActorLayoutWithGrid();

	public IPanelLayoutProvider GoodsItemPackagesAndContainersPanelLayout => new EU.NCTS.GUI.Phase5GoodsItemPackagesAndContainersLayout();

	public IPanelLayoutProvider GoodsItemPackagePanelLayout => new EU.NCTS.GUI.Phase5GoodsItemPackageLayout();

	public IGridColumnLayoutProvider GoodsItemPackageGridPanelLayout => new EU.NCTS.GUI.Phase5GoodsItemPackageGridLayout();

	public IPanelLayoutProvider ArrivalNotificationDetailsPanelLayout => new ArrivalNotificationDetailsLayout();

	public IPanelLayoutProvider ArrivalDeclarationDetailsPanelLayout => new EU.NCTS.GUI.Phase5ArrivalDeclarationDetailsLayout();

	public IEnumerable<ITabPage> AdditionalGoodsItemTabPages => new[] { new RestrictionsTabPage() };

	public IEnumerable<string> ReorderGoodsItemTabPageNames(IEnumerable<string> defaultTabPageNames) => defaultTabPageNames.Take(1).Append(nameof(RestrictionsTabPage)).Union(defaultTabPageNames);

	public IPanelLayoutProvider UnloadingDetailsPanelLayout => new UnloadingDetailsLayout();

	public IPanelLayoutProvider GuaranteeForArrivalGroupBoxPanelLayout => new EU.GUI.GuaranteeGroupBoxWithOverrideLayout();

	public IPanelLayoutWithGridProvider HouseConsignmentAdditionalDocumentsLayout => new EU.NCTS.GUI.HouseConsignmentAdditionalDocumentsLayout();

	public IPanelLayoutWithGridProvider HouseConsignmentPreviousDocumentsLayout => new EU.NCTS.GUI.Phase5HouseConsignmentDifferencesPreviousDocumentLayoutWithGrid();

	public IPanelLayoutProvider UnloadingDifferencesDeclaredValuePanelLayout => new EU.NCTS.GUI.UnloadingDifferencesDeclaredValueLayout();

	public IPanelLayoutProvider UnloadingDifferencesUnloadedValuePanelLayout => new EU.NCTS.GUI.UnloadingDifferencesUnloadedValueLayout();

	public IPanelLayoutProvider EventPanelLayout => new EU.NCTS.GUI.Phase5EventLayout();

	public IPanelLayoutWithGridProvider NctsPackageLayoutWithGrid => new EU.NCTS.GUI.NctsPackageLayoutWithGrid();

	public IPanelLayoutWithGridProvider HouseConsignmentDifferencesLayout => new HouseConsignmentDifferencesLayout();

	public IPanelLayoutWithGridProvider IncidentDetailsPanelLayoutWithGrid => new EU.NCTS.GUI.IncidentDetailsLayoutWithGrid();

	public IPanelLayoutWithGridProvider SupportingDocumentsLayout => new EU.NCTS.GUI.Phase5HouseConsignmentDifferencesSupportingDocumentLayoutWithGrid();

	public IPanelLayoutWithGridProvider HouseConsignmentGoodItemsSupportingDocumentsLayout => new EU.NCTS.GUI.HouseConsignmentGoodItemsSupportingDocumentsLayout();

	public IPanelLayoutProvider IncidentTransportMeansLayout => new EU.NCTS.GUI.Phase5TransportMeansLayout();

	public IPanelLayoutProvider GoodsItemDifferencesDetailsLayout => new EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsLayout();

	public IPanelLayoutProvider GoodsItemDifferencesDetailsColumnLayout => new GoodsItemDifferencesDetailsColumnLayout();

	public IPanelLayoutWithGridProvider DepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid => new EU.NCTS.GUI.Phase5DepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid();

	public IPanelLayoutWithGridProvider ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid => new EU.NCTS.GUI.Phase5ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid();

	public Type DeclarationSupplyChainActorsTabControlType => typeof(EU.NCTS.GUI.Phase5DeclarationSupplyChainActorsTabUserControl);

	public Type CustomsOfficesUserControlType => typeof(CustomsOfficesUserControl);

	public Type GuaranteesUserControlType => typeof(GuaranteesUserControl);

	public Type ArrivalContainersEquipmentGrid => typeof(EU.NCTS.GUI.Phase5ArrivalContainersEquipmentGridUserControl);

	public Type ArrivalSealsGrid => typeof(ArrivalSealsGridUserControl);

	public Type UnloadingDifferencesPreviousDocumentPanelGridUserControlType => typeof(EU.NCTS.GUI.Phase5UnloadingDifferencesPreviousDocumentsGridUserControl);

	public Type ArrivalTransportInfoGridType => typeof(EU.NCTS.GUI.ArrivalTransportInfosGridUserControl);

	public IPanelLayoutProvider HouseConsignmentTransportDeparturePanelLayout => null;

	public Type GoodsItemsTabUserControlType => typeof(Phase5GoodsItemsTabUserControl);

	public Type ArrivalNotificationTabControlType => typeof(Phase5ArrivalNotificationTabUserControl);

	public Type DeclarationSupportingDocumentsTabControlType => typeof(EU.NCTS.GUI.DeclarationSupportingDocumentsGridUserControl);

	public Type DeclarationPreviousDocumentsTabControlType => typeof(EU.NCTS.GUI.DeclarationPreviousDocumentsGridUserControl);

	public Type DeclarationAdditionalDocumentsTabControlType => typeof(EU.NCTS.GUI.DeclarationAdditionalDocumentsGridUserControl);

	public IEnumerable<ITabPage> AdditionalDeclarationDetailTabPages => new[] { new ExportDeclarationsTabPage() };

	public IEnumerable<string> ReorderDeclarationDetailTabPagesNames(IEnumerable<string> defaultTabPageNames) => new[] { nameof(ExportDeclarationsTabPage) }.Concat(defaultTabPageNames).Except("AuthorizationsTabPage");

	public IEnumerable<ITabPage> AdditionalArrivalTabPages => new[] { new AdditionalGoodsInformationTabPage() };

	public IEnumerable<string> ReorderArrivalTabPagesNames(IEnumerable<string> defaultTabPageNames) => defaultTabPageNames.Take(1).Append(nameof(AdditionalGoodsInformationTabPage)).Union(defaultTabPageNames);

	public IGridColumnLayoutProvider GetMessageSendingGridColumnLayout(BaseMessageSendingObjectParent messageSendingObjectParent) => null;

	public IGridColumnLayoutProvider SuportingDocumentGridColumnLayout => new EU.NCTS.GUI.SupportingDocumentsGridColumnLayout();

	public IGridColumnLayoutProvider AdditionalDocumentsGridColumnLayout => new EU.NCTS.GUI.AdditionalDocumentsGridColumnLayout();

	public IGridColumnLayoutProvider UnloadingDifferencesSuportingDocumentGridColumnLayout => new EU.NCTS.GUI.UnloadingDifferencesSupportingDocumentsGridColumnLayout();

	public IGridColumnLayoutProvider UnloadingDifferencesAdditionalDocumentsGridColumnLayout => new EU.NCTS.GUI.UnloadingDifferencesAdditionalDocumentsGridColumnLayout();

	public IGridColumnLayoutProvider ContainersGridColumnLayout => new EU.NCTS.GUI.ContainersGridColumnLayout();

	public IGridColumnLayoutProvider AdditionalSealsGridColumnLayout => new EU.NCTS.GUI.AdditionalSealsGridColumnLayout();

	public IGridColumnLayoutProvider NctsPackagesGridColumnLayout => new EU.NCTS.GUI.NctsPackagesGridColumnLayout();

	public Type DeclarationDetailsTabUserControlType => typeof(DeclarationDetailsTabUserControl);

	public Type TransportAndPackagingTabUserControlType => typeof(TransportAndPackagingTabUserControl);

	public Type HouseConsignmentsTabUserControlType => typeof(HouseConsignmentsTabUserControl);

	public IGridColumnLayoutProvider GetDepartureGoodsItemsGridColumnLayout() => new GoodsItemDetailsGridColumnLayout();

	public Type GoodsItemAdditionalDocumentsTabUserControlType => typeof(EU.NCTS.GUI.Phase5GoodsItemAdditionalDocumentsTabUserControl);

	public Type UnloadingDifferencesTabUserControlType => typeof(UnloadingDifferencesTabUserControl);

	public IGridColumnLayoutProvider GetGoodsItemsPreviousDocumentsGridColumnLayout() => new GoodsItemPreviousDocumentsGridColumnsLayout();

	public IGridColumnLayoutProvider GetGoodsItemDifferencesDetailsGridColumnLayout() => new GoodsItemDifferencesDetailsGridColumnLayout();

	public Type MessageTabUserControlType => typeof(MessagesTabUserControl);

	public IPanelLayoutProvider LiabilityDetailsLayout => new EU.NCTS.GUI.LiabilityDetailsLayout();

	public Type GoodsItemPackagesUserControlType => typeof(EU.NCTS.GUI.GoodsItemPackagesUserControl);

	public IGridColumnLayoutProvider GetHouseConsignmentDetailsGridColumnLayout() => new EU.NCTS.GUI.HouseConsignmentDetailsGridColumnLayout();
}
