using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public class ExitControlUcc6LayoutProvider : IExitControlLayoutProvider
{
	public IEnumerable<ITabPage> AdditionalDeclarationTabPages => Enumerable.Empty<ITabPage>();

	public IEnumerable<ZString> RemovableDeclarationTabPageNames => Enumerable.Empty<ZString>();

	public IPanelLayoutProvider HeaderDetailsPanelLayout => HeaderDetailsPanelLayoutCore;
	protected virtual IPanelLayoutProvider HeaderDetailsPanelLayoutCore => new HeaderDetailsLayout();

	public IEnumerable<ITabPage> AdditionalConsignmentTabPages => Enumerable.Empty<ITabPage>();

	public IEnumerable<ZString> RemovableConsignmentTabPageNames => Enumerable.Empty<ZString>();

	public IConsignmentsGridUserControl ConsignmentsGridUserControl => new ConsignmentsGridUserControl();

	public IPanelLayoutWithGridProvider ConsignmentItemPanelLayoutWithGrid => new ConsignmentItemUcc6LayoutWithGrid();

	public IEnumerable<ITabPageWithVisibility<CusExitReport>> AdditionalReportTabPages
	{
		get
		{
			yield return new ReportAdditionalDocumentsTabPage();
			if (AuthorizationIsActive)
			{
				yield return new ReportAuthorizationsTabPage();
			}
		}
	}

	public IEnumerable<ZString> RemovableReportTabPageNames => Enumerable.Empty<ZString>();

	public Type ContainersOrEquipmentsAndSealsUserControlType => typeof(ContainersOrEquipmentsAndSealsUserControl);

	public IGridColumnLayoutProvider ReportAdditionalDocumentsGridLayout => new ReportAdditionalDocumentUcc6GridColumnLayout();

	public IGridColumnLayoutProvider ContainersOrEquipmentsGridLayout => new ContainersOrEquipmentsUcc6GridColumnLayout();

	public IGridColumnLayoutProvider SealsGridLayout => new SealsUcc6GridColumnLayout();

	public IGridColumnLayoutProvider ReportItemAdditionalDocumentsGridLayout => new ReportItemAdditionalDocumentUcc6GridColumnLayout();

	public IGridColumnLayoutProvider ConsignmentItemPackingDetailsGridLayout => new ConsignmentItemPackingDetailsUcc6GridColumnLayout();

	public IGridColumnLayoutProvider ConsignmentItemsGridLayout => new ConsignmentItemsUcc6GridColumnLayout();

	public IDetailsReportsGridUserControl CreateDetailsReportGridUserControl() => CreateDetailsReportGridUserControlCore();
	protected virtual IDetailsReportsGridUserControl CreateDetailsReportGridUserControlCore() => new HeaderExitReportStatusUcc6GridUserControl();

	public IReportItemsUserControl CreateReportItemsUserControl() => new ReportItemsUserControl();

	public IReportsGridUserControl CreateReportsGridUserControl() => CreateReportsGridUserControlCore() ;
	protected virtual IReportsGridUserControl CreateReportsGridUserControlCore() => new ReportsUcc6GridUserControl();

	public BaseMessagesTabUserControl CreateReportsMessagesUserControl() => new MessagesTabUserControl();

	public IGridColumnLayoutProvider AuthorizationGridColumnLayout => new AuthorizationGridColumnLayout();

	public bool AuthorizationIsActive => GetAuthorizationIsActiveCore();

	protected virtual bool GetAuthorizationIsActiveCore() => false;
}
