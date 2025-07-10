using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public sealed class NctsPhase5LayoutProvider : EU.NCTS.GUI.INctsPhase5LayoutProvider, EU.NCTS.GUI.INctsPhase5TransitionPeriodLayoutProvider
{
	public IGridColumnLayoutProvider GetDepartureMovementGridColumnLayout() => new EU.NCTS.GUI.DepartureMovementDetailsGridColumnsLayout();

	public IPanelLayoutProvider TraderDetailsPanelLayout => new EU.NCTS.GUI.Phase5TraderDetailsLayout();

	public IPanelLayoutProvider DepartureDetailsPanelLayout => new Phase5DepartureDetailsLayout();

	public IPanelLayoutProvider DeclarationDetailsPanelLayout => new Phase5DeclarationDetailsLayout();

	public IPanelLayoutProvider TransportAndPackagingPanelLayout => new EU.NCTS.GUI.Phase5TransportAndPackagingLayout();

	public IPanelLayoutProvider TransportDeparturePanelLayout => new EU.NCTS.GUI.TransportDepartureLayout();

	public IPanelLayoutProvider TransportBorderPanelLayout => new EU.NCTS.GUI.TransportBorderLayout();

	public IPanelLayoutProvider SecurityAtDeparturePanelLayout => new EU.NCTS.GUI.Phase5SecurityAtDepartureLayout();

	public IPanelLayoutProvider MiscellanousOptionsPanelLayout => new Phase5MiscellaneousOptionsLayout();

	public Type CustomsOfficesUserControlType => typeof(EU.NCTS.GUI.Phase5CustomsOfficesUserControl);

	public Type GuaranteesUserControlType => typeof(Phase5GuaranteesUserControl);

	public IPanelLayoutWithGridProvider DeclarationServicePanelLayoutWithGrid => new EU.NCTS.GUI.ServiceLayoutWithGrid();

	public Type DeclarationAuthorizationsTabControlType => typeof(EU.NCTS.GUI.Phase5DeclarationAuthorizationsTabUserControl);

	public IPanelLayoutWithGridProvider HouseConsignmentDetailsPanelLayoutWithGrid => new HouseConsignmentDetailsLayoutWithGrid();

	public IPanelLayoutWithGridProvider HouseConsignmentPreviousDocumentPanelLayoutWithGrid => new EU.NCTS.GUI.HouseConsignmentPreviousDocumentLayoutWithGrid();

	public IPanelLayoutWithGridProvider HouseConsignmentSupportingDocumentPanelLayoutWithGrid => new HouseConsignmentSupportingDocumentLayoutWithGrid();

	public IPanelLayoutWithGridProvider HouseConsignmentSupplyChainActorPanelLayoutWithGrid => new EU.NCTS.GUI.HouseConsignmentSupplyChainActorLayoutWithGrid();

	public IPanelLayoutWithGridProvider HouseConsignmentAdditionalDocumentPanelLayoutWithGrid => new EU.NCTS.GUI.HouseConsignmentAdditionalDocumentLayoutWithGrid();

	public IPanelLayoutWithGridProvider GoodsItemDetailsPanelLayoutWithGrid => new Phase5GoodsItemDetailsLayoutWithGrid();

	public IPanelLayoutWithGridProvider GoodsItemSupportingDocumentPanelLayoutWithGrid => new Phase5GoodsItemSupportingDocumentLayoutWithGrid();

	public IPanelLayoutWithGridProvider GoodsItemPreviousDocumentPanelLayoutWithGrid => new Phase5GoodsItemPreviousDocumentLayoutWithGrid();

	public IPanelLayoutWithGridProvider GoodsItemSupplyChainActorPanelLayoutWithGrid => new EU.NCTS.GUI.Phase5GoodsItemSupplyChainActorLayoutWithGrid();

	public IPanelLayoutProvider GoodsItemPackagesAndContainersPanelLayout => new EU.NCTS.GUI.Phase5GoodsItemPackagesAndContainersLayout();

	public IPanelLayoutProvider GoodsItemPackagePanelLayout => new EU.NCTS.GUI.Phase5GoodsItemPackageLayout();

	public IPanelLayoutProvider ArrivalNotificationDetailsPanelLayout => new EU.NCTS.GUI.Phase5ArrivalNotificationDetailsLayout();

	public IEnumerable<ITabPage> AdditionalGoodsItemTabPages => Enumerable.Empty<ITabPage>();

	public IEnumerable<string> ReorderGoodsItemTabPageNames(IEnumerable<string> defaultTabPageNames) => defaultTabPageNames;

	public IPanelLayoutProvider ArrivalDeclarationDetailsPanelLayout => new EU.NCTS.GUI.Phase5ArrivalDeclarationDetailsLayout();

	public IPanelLayoutProvider UnloadingDetailsPanelLayout => new EU.NCTS.GUI.UnloadingDetailsLayout();

	public IPanelLayoutProvider GuaranteeForArrivalGroupBoxPanelLayout => new EU.GUI.GuaranteeGroupBoxWithOverrideLayout();

	public IPanelLayoutProvider UnloadingDifferencesDeclaredValuePanelLayout => new EU.NCTS.GUI.UnloadingDifferencesDeclaredValueLayout();

	public IPanelLayoutProvider UnloadingDifferencesUnloadedValuePanelLayout => new EU.NCTS.GUI.UnloadingDifferencesUnloadedValueLayout();

	public IPanelLayoutProvider EventPanelLayout => new EU.NCTS.GUI.Phase5EventLayout();

	public IPanelLayoutWithGridProvider NctsPackageLayoutWithGrid => new EU.NCTS.GUI.NctsPackageLayoutWithGrid();

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

	public IPanelLayoutProvider HouseConsignmentTransportDeparturePanelLayout => new HouseConsignmentTransportDepartureLayout();

	public Type GoodsItemsTabUserControlType => typeof(EU.NCTS.GUI.Phase5GoodsItemsTabUserControl);

	public Type ArrivalNotificationTabControlType => typeof(EU.NCTS.GUI.Phase5ArrivalNotificationTabUserControl);

	public Type DeclarationSupportingDocumentsTabControlType => typeof(DeclarationSupportingDocumentsGridUserControl);

	public Type DeclarationPreviousDocumentsTabControlType => typeof(EU.NCTS.GUI.DeclarationPreviousDocumentsGridUserControl);

	public Type DeclarationAdditionalDocumentsTabControlType => typeof(EU.NCTS.GUI.DeclarationAdditionalDocumentsGridUserControl);

	public IEnumerable<ITabPage> AdditionalDeclarationDetailTabPages
	{
		get
		{
			yield return new EU.NCTS.GUI.DocumentsRequestedTabPage();
		}
	}

	public IEnumerable<string> ReorderDeclarationDetailTabPagesNames(IEnumerable<string> defaultTabPageNames) => defaultTabPageNames;

	public IEnumerable<ITabPage> AdditionalArrivalTabPages => Enumerable.Empty<ITabPage>();

	public IGridColumnLayoutProvider SuportingDocumentGridColumnLayout => new EU.NCTS.GUI.SupportingDocumentsGridColumnLayout();

	public IGridColumnLayoutProvider AdditionalDocumentsGridColumnLayout => new EU.NCTS.GUI.AdditionalDocumentsGridColumnLayout();

	public IGridColumnLayoutProvider UnloadingDifferencesSuportingDocumentGridColumnLayout => new EU.NCTS.GUI.UnloadingDifferencesSupportingDocumentsGridColumnLayout();

	public IGridColumnLayoutProvider UnloadingDifferencesAdditionalDocumentsGridColumnLayout => new EU.NCTS.GUI.UnloadingDifferencesAdditionalDocumentsGridColumnLayout();

	public IEnumerable<string> ReorderArrivalTabPagesNames(IEnumerable<string> defaultTabPageNames) => defaultTabPageNames;

	public IGridColumnLayoutProvider GetMessageSendingGridColumnLayout(BaseMessageSendingObjectParent messageSendingObjectParent) => null;

	public IGridColumnLayoutProvider ContainersGridColumnLayout => new EU.NCTS.GUI.ContainersGridColumnLayout();

	public IGridColumnLayoutProvider AdditionalSealsGridColumnLayout => new EU.NCTS.GUI.AdditionalSealsGridColumnLayout();

	public IGridColumnLayoutProvider NctsPackagesGridColumnLayout => new EU.NCTS.GUI.NctsPackagesGridColumnLayout();

	public IGridColumnLayoutProvider GoodsItemPackageGridPanelLayout => new EU.NCTS.GUI.Phase5GoodsItemPackageGridLayout();

	public Type DeclarationDetailsTabUserControlType => typeof(EU.NCTS.GUI.Phase5DeclarationDetailsTabUserControl);

	IGridColumnLayoutProvider EU.NCTS.GUI.INctsPhase5LayoutProvider.GetDepartureGoodsItemsGridColumnLayout() => new Phase5GoodsItemDetailsGridColumnsLayout();

	IGridColumnLayoutProvider EU.NCTS.GUI.INctsPhase5TransitionPeriodLayoutProvider.GetDepartureGoodsItemsGridColumnLayout() => new Phase5TransitionPeriodGoodsItemDetailsGridColumnsLayout();

	public Type HouseConsignmentsTabUserControlType => typeof(EU.NCTS.GUI.HouseConsignmentsTabUserControl);

	public Type GoodsItemAdditionalDocumentsTabUserControlType => typeof(EU.NCTS.GUI.Phase5GoodsItemAdditionalDocumentsTabUserControl);

	public IGridColumnLayoutProvider GetGoodsItemsPreviousDocumentsGridColumnLayout() => new Phase5GoodsItemPreviousDocumentsGridColumnsLayout();

	public IGridColumnLayoutProvider GetGoodsItemDifferencesDetailsGridColumnLayout() => new EU.NCTS.GUI.Phase5GoodsItemDifferencesDetailsGridColumnsLayout();

	IGridColumnLayoutProvider EU.NCTS.GUI.INctsPhase5LayoutProvider.GetHouseConsignmentDetailsGridColumnLayout() => new HouseConsignmentDetailsGridColumnLayout();

	IGridColumnLayoutProvider EU.NCTS.GUI.INctsPhase5TransitionPeriodLayoutProvider.GetHouseConsignmentDetailsGridColumnLayout() => new Phase5TransitionPeriodHouseConsignmentDetailsGridColumnLayout();

	public Type MessageTabUserControlType => typeof(MessagesTabUserControl);

	public Type TransportAndPackagingTabUserControlType => typeof(EU.NCTS.GUI.Phase5TransportAndPackagingTabUserControl);

	public Type UnloadingDifferencesTabUserControlType => typeof(EU.NCTS.GUI.Phase5UnloadingDifferencesTabUserControl);

	public IPanelLayoutProvider LiabilityDetailsLayout => new EU.NCTS.GUI.LiabilityDetailsLayout();

	public Type GoodsItemPackagesUserControlType => typeof(EU.NCTS.GUI.GoodsItemPackagesUserControl);
}
