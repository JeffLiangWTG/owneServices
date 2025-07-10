using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public interface IExitControlLayoutProvider
	{
		IEnumerable<ITabPage> AdditionalDeclarationTabPages { get; }

		IEnumerable<ZString> RemovableDeclarationTabPageNames { get; }

		IPanelLayoutProvider HeaderDetailsPanelLayout { get; }

		IEnumerable<ITabPage> AdditionalConsignmentTabPages { get; }

		IEnumerable<ZString> RemovableConsignmentTabPageNames { get; }

		IConsignmentsGridUserControl ConsignmentsGridUserControl { get; }

		IPanelLayoutWithGridProvider ConsignmentItemPanelLayoutWithGrid { get; }

		IEnumerable<ITabPageWithVisibility<Business.CusExitReport>> AdditionalReportTabPages { get; }

		IEnumerable<ZString> RemovableReportTabPageNames { get; }

		IReportsGridUserControl CreateReportsGridUserControl();

		IReportItemsUserControl CreateReportItemsUserControl();

		BaseMessagesTabUserControl CreateReportsMessagesUserControl();

		IDetailsReportsGridUserControl CreateDetailsReportGridUserControl();

		Type ContainersOrEquipmentsAndSealsUserControlType { get; }

		IGridColumnLayoutProvider ReportAdditionalDocumentsGridLayout { get; }

		IGridColumnLayoutProvider ContainersOrEquipmentsGridLayout { get; }

		IGridColumnLayoutProvider SealsGridLayout { get; }

		IGridColumnLayoutProvider ReportItemAdditionalDocumentsGridLayout { get; }

		IGridColumnLayoutProvider ConsignmentItemPackingDetailsGridLayout { get; }

		IGridColumnLayoutProvider ConsignmentItemsGridLayout { get; }

		IGridColumnLayoutProvider AuthorizationGridColumnLayout { get; }

		bool AuthorizationIsActive { get; }
	}
}
