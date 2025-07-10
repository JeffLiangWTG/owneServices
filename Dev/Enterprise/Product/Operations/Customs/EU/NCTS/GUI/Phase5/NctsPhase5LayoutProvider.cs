using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	class NctsPhase5LayoutProvider : INctsPhase5LayoutProvider
	{
		public static INctsPhase5LayoutProvider GetLayoutProvider(string countryOrGroupingCode)
		{
			object provider = null;

			var providers = ObjectFactory.Get<Hashtable>("NctsPhase5LayoutProviders");
			if (!string.IsNullOrEmpty(countryOrGroupingCode))
			{
				var objectHandle = (ObjectHandle)providers[countryOrGroupingCode];
				provider = objectHandle?.GetObject();
			}
			if (provider == null)
			{
				var objectHandle = (ObjectHandle)providers["Default"];
				provider = objectHandle?.GetObject();
			}
			return (INctsPhase5LayoutProvider)provider;
		}

		public IPanelLayoutProvider TraderDetailsPanelLayout => new Phase5TraderDetailsLayout();

		public IPanelLayoutProvider DepartureDetailsPanelLayout => new Phase5DepartureDetailsLayout();

		public IPanelLayoutProvider DeclarationDetailsPanelLayout => new Phase5DeclarationDetailsLayout();

		public IPanelLayoutProvider TransportAndPackagingPanelLayout => new Phase5TransportAndPackagingLayout();

		public IPanelLayoutProvider TransportDeparturePanelLayout => new TransportDepartureLayout();

		public IPanelLayoutProvider TransportBorderPanelLayout => new TransportBorderLayout();

		public IPanelLayoutProvider SecurityAtDeparturePanelLayout => new Phase5SecurityAtDepartureLayout();

		public IPanelLayoutProvider MiscellanousOptionsPanelLayout => new Phase5MiscellaneousOptionsLayout();

		public Type CustomsOfficesUserControlType => typeof(Phase5CustomsOfficesUserControl);

		public Type GuaranteesUserControlType => typeof(Phase5GuaranteesUserControl);

		public IPanelLayoutWithGridProvider DeclarationServicePanelLayoutWithGrid => new ServiceLayoutWithGrid();

		public Type DeclarationAuthorizationsTabControlType => typeof(Phase5DeclarationAuthorizationsTabUserControl);

		public Type DeclarationSupportingDocumentsTabControlType => typeof(DeclarationSupportingDocumentsGridUserControl);

		public Type DeclarationPreviousDocumentsTabControlType => typeof(DeclarationPreviousDocumentsGridUserControl);

		public Type DeclarationAdditionalDocumentsTabControlType => typeof(DeclarationAdditionalDocumentsGridUserControl);

		public Type TransportAndPackagingTabUserControlType => typeof(Phase5TransportAndPackagingTabUserControl);

		public Type HouseConsignmentsTabUserControlType => typeof(HouseConsignmentsTabUserControl);

		public Type MessageTabUserControlType => typeof(MessagesTabUserControl);

		public IPanelLayoutWithGridProvider HouseConsignmentDetailsPanelLayoutWithGrid => new HouseConsignmentDetailsLayoutWithGrid();

		public IPanelLayoutWithGridProvider HouseConsignmentPreviousDocumentPanelLayoutWithGrid => new HouseConsignmentPreviousDocumentLayoutWithGrid();

		public IPanelLayoutWithGridProvider HouseConsignmentSupportingDocumentPanelLayoutWithGrid => new HouseConsignmentSupportingDocumentLayoutWithGrid();

		public IPanelLayoutWithGridProvider HouseConsignmentSupplyChainActorPanelLayoutWithGrid => new HouseConsignmentSupplyChainActorLayoutWithGrid();

		public IPanelLayoutWithGridProvider HouseConsignmentAdditionalDocumentPanelLayoutWithGrid => new HouseConsignmentAdditionalDocumentLayoutWithGrid();

		public IPanelLayoutWithGridProvider GoodsItemDetailsPanelLayoutWithGrid => new Phase5GoodsItemDetailsLayoutWithGrid();

		public IPanelLayoutWithGridProvider GoodsItemSupportingDocumentPanelLayoutWithGrid => new Phase5GoodsItemSupportingDocumentLayoutWithGrid();

		public IPanelLayoutWithGridProvider GoodsItemPreviousDocumentPanelLayoutWithGrid => new Phase5GoodsItemPreviousDocumentLayoutWithGrid();

		public IPanelLayoutWithGridProvider GoodsItemSupplyChainActorPanelLayoutWithGrid => new Phase5GoodsItemSupplyChainActorLayoutWithGrid();

		public IPanelLayoutProvider GoodsItemPackagesAndContainersPanelLayout => new Phase5GoodsItemPackagesAndContainersLayout();

		public IPanelLayoutProvider GoodsItemPackagePanelLayout => new Phase5GoodsItemPackageLayout();

		public IGridColumnLayoutProvider GoodsItemPackageGridPanelLayout => new Phase5GoodsItemPackageGridLayout();

		public IPanelLayoutProvider ArrivalNotificationDetailsPanelLayout => new Phase5ArrivalNotificationDetailsLayout();

		public IPanelLayoutProvider ArrivalDeclarationDetailsPanelLayout => new Phase5ArrivalDeclarationDetailsLayout();

		public IEnumerable<ITabPage> AdditionalGoodsItemTabPages => Enumerable.Empty<ITabPage>();

		public IEnumerable<string> ReorderGoodsItemTabPageNames(IEnumerable<string> defaultTabPageNames) => defaultTabPageNames;

		public Type UnloadingDifferencesTabUserControlType => typeof(Phase5UnloadingDifferencesTabUserControl);

		public IPanelLayoutProvider UnloadingDetailsPanelLayout => new UnloadingDetailsLayout();

		public IPanelLayoutProvider GuaranteeForArrivalGroupBoxPanelLayout => new GuaranteeGroupBoxWithOverrideLayout();

		public IPanelLayoutWithGridProvider HouseConsignmentAdditionalDocumentsLayout => new HouseConsignmentAdditionalDocumentsLayout();

		public IPanelLayoutWithGridProvider HouseConsignmentPreviousDocumentsLayout => new Phase5HouseConsignmentDifferencesPreviousDocumentLayoutWithGrid();

		public IPanelLayoutProvider UnloadingDifferencesDeclaredValuePanelLayout => new UnloadingDifferencesDeclaredValueLayout();

		public IPanelLayoutProvider UnloadingDifferencesUnloadedValuePanelLayout => new UnloadingDifferencesUnloadedValueLayout();

		public IPanelLayoutProvider EventPanelLayout => new Phase5EventLayout();

		public Type GoodsItemPackagesUserControlType => typeof(GoodsItemPackagesUserControl);

		public IPanelLayoutWithGridProvider NctsPackageLayoutWithGrid => new NctsPackageLayoutWithGrid();

		public IPanelLayoutWithGridProvider HouseConsignmentDifferencesLayout => new HouseConsignmentDifferencesLayout();

		public IPanelLayoutWithGridProvider IncidentDetailsPanelLayoutWithGrid => new IncidentDetailsLayoutWithGrid();

		public IPanelLayoutWithGridProvider SupportingDocumentsLayout => new Phase5HouseConsignmentDifferencesSupportingDocumentLayoutWithGrid();

		public IPanelLayoutWithGridProvider HouseConsignmentGoodItemsSupportingDocumentsLayout => new HouseConsignmentGoodItemsSupportingDocumentsLayout();

		public IPanelLayoutProvider IncidentTransportMeansLayout => new Phase5TransportMeansLayout();

		public IPanelLayoutProvider GoodsItemDifferencesDetailsLayout => new Phase5GoodsItemDifferencesDetailsLayout();

		public IPanelLayoutProvider GoodsItemDifferencesDetailsColumnLayout => new Phase5GoodsItemDifferencesDetailsColumnLayout();

		public Type DeclarationSupplyChainActorsTabControlType => typeof(Phase5DeclarationSupplyChainActorsTabUserControl);

		public Type ArrivalContainersEquipmentGrid => typeof(Phase5ArrivalContainersEquipmentGridUserControl);

		public Type ArrivalSealsGrid => typeof(Phase5ArrivalSealsGridUserControl);

		public Type UnloadingDifferencesPreviousDocumentPanelGridUserControlType => typeof(Phase5UnloadingDifferencesPreviousDocumentsGridUserControl);

		public IPanelLayoutWithGridProvider DepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid => new Phase5DepartureGoodsItemAdditionalDocumentPanelLayoutWithGrid();

		public IPanelLayoutWithGridProvider ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid => new Phase5ArrivalGoodsItemAdditionalDocumentPanelLayoutWithGrid();

		public Type ArrivalTransportInfoGridType => typeof(ArrivalTransportInfosGridUserControl);

		public IPanelLayoutProvider HouseConsignmentTransportDeparturePanelLayout => new HouseConsignmentTransportDepartureLayout();

		public Type GoodsItemsTabUserControlType => typeof(Phase5GoodsItemsTabUserControl);

		public Type ArrivalNotificationTabControlType => typeof(Phase5ArrivalNotificationTabUserControl);

		public IEnumerable<ITabPage> AdditionalDeclarationDetailTabPages => Enumerable.Empty<ITabPage>();

		public IEnumerable<string> ReorderDeclarationDetailTabPagesNames(IEnumerable<string> defaultTabPageNames) => defaultTabPageNames;

		public IEnumerable<ITabPage> AdditionalArrivalTabPages => Enumerable.Empty<ITabPage>();

		public IEnumerable<string> ReorderArrivalTabPagesNames(IEnumerable<string> defaultTabPageNames) => defaultTabPageNames;

		public IGridColumnLayoutProvider GetMessageSendingGridColumnLayout(BaseMessageSendingObjectParent messageSendingObjectParent) => null;

		public IGridColumnLayoutProvider SuportingDocumentGridColumnLayout => new SupportingDocumentsGridColumnLayout();

		public IGridColumnLayoutProvider AdditionalDocumentsGridColumnLayout => new AdditionalDocumentsGridColumnLayout();

		public IGridColumnLayoutProvider UnloadingDifferencesSuportingDocumentGridColumnLayout => new UnloadingDifferencesSupportingDocumentsGridColumnLayout();

		public IGridColumnLayoutProvider UnloadingDifferencesAdditionalDocumentsGridColumnLayout => new UnloadingDifferencesAdditionalDocumentsGridColumnLayout();

		public IGridColumnLayoutProvider ContainersGridColumnLayout => new ContainersGridColumnLayout();

		public IGridColumnLayoutProvider AdditionalSealsGridColumnLayout => new AdditionalSealsGridColumnLayout();

		public IGridColumnLayoutProvider NctsPackagesGridColumnLayout => new NctsPackagesGridColumnLayout();

		public Type DeclarationDetailsTabUserControlType => typeof(Phase5DeclarationDetailsTabUserControl);

		public IGridColumnLayoutProvider GetDepartureGoodsItemsGridColumnLayout() => new Phase5GoodsItemDetailsGridColumnsLayout();

		public IGridColumnLayoutProvider GetDepartureMovementGridColumnLayout() => new DepartureMovementDetailsGridColumnsLayout();

		public Type GoodsItemAdditionalDocumentsTabUserControlType => typeof(Phase5GoodsItemAdditionalDocumentsTabUserControl);

		public IGridColumnLayoutProvider GetGoodsItemsPreviousDocumentsGridColumnLayout() => new Phase5GoodsItemPreviousDocumentsGridColumnsLayout();

		public IGridColumnLayoutProvider GetGoodsItemDifferencesDetailsGridColumnLayout() => new Phase5GoodsItemDifferencesDetailsGridColumnsLayout();

		public IPanelLayoutProvider LiabilityDetailsLayout => new LiabilityDetailsLayout();

		public IGridColumnLayoutProvider GetHouseConsignmentDetailsGridColumnLayout() => new HouseConsignmentDetailsGridColumnLayout();
	}
}
