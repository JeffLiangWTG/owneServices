using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public class ExitControlLayoutProvider : IExitControlLayoutProvider
	{
		public static IExitControlLayoutProvider GetLayoutProvider(string countryOrGroupingCode)
		{
			object provider = null;

			var providers = ObjectFactory.Get<Hashtable>("ExitControlLayoutProviders");
			if (!string.IsNullOrEmpty(countryOrGroupingCode))
			{
				var objectHandle = (ObjectHandle)providers[countryOrGroupingCode];
				provider = objectHandle?.GetObject();
			}
			if (provider == null)
			{
				var objectHandle = (ObjectHandle)providers["Default"];
				provider = objectHandle.GetObject();
			}
			return (IExitControlLayoutProvider)provider;
		}

		IEnumerable<ITabPage> IExitControlLayoutProvider.AdditionalDeclarationTabPages => Enumerable.Empty<ITabPage>();

		IEnumerable<ZString> IExitControlLayoutProvider.RemovableDeclarationTabPageNames => Enumerable.Empty<ZString>();

		IPanelLayoutProvider IExitControlLayoutProvider.HeaderDetailsPanelLayout => new HeaderDetailsLayout();

		IDetailsReportsGridUserControl IExitControlLayoutProvider.CreateDetailsReportGridUserControl() => new HeaderExitReportStatusGridUserControl();

		IEnumerable<ITabPage> IExitControlLayoutProvider.AdditionalConsignmentTabPages => Enumerable.Empty<ITabPage>();

		IEnumerable<ZString> IExitControlLayoutProvider.RemovableConsignmentTabPageNames => Enumerable.Empty<ZString>();

		IConsignmentsGridUserControl IExitControlLayoutProvider.ConsignmentsGridUserControl => new ConsignmentsGridUserControl();

		IPanelLayoutWithGridProvider IExitControlLayoutProvider.ConsignmentItemPanelLayoutWithGrid => new ConsignmentItemLayoutWithGrid();

		IEnumerable<ITabPageWithVisibility<CusExitReport>> IExitControlLayoutProvider.AdditionalReportTabPages => Enumerable.Empty<ITabPageWithVisibility<CusExitReport>>();

		IEnumerable<ZString> IExitControlLayoutProvider.RemovableReportTabPageNames => Enumerable.Empty<ZString>();

		IReportsGridUserControl IExitControlLayoutProvider.CreateReportsGridUserControl() => new ReportsGridUserControl();

		IReportItemsUserControl IExitControlLayoutProvider.CreateReportItemsUserControl() => null;

		BaseMessagesTabUserControl IExitControlLayoutProvider.CreateReportsMessagesUserControl() => new MessagesTabUserControl();

		Type IExitControlLayoutProvider.ContainersOrEquipmentsAndSealsUserControlType => typeof(ContainersOrEquipmentsAndSealsUserControl);

		IGridColumnLayoutProvider IExitControlLayoutProvider.ReportAdditionalDocumentsGridLayout => new ReportAdditionalDocumentGridColumnLayout();

		IGridColumnLayoutProvider IExitControlLayoutProvider.ContainersOrEquipmentsGridLayout => new ContainersOrEquipmentsGridColumnLayout();

		IGridColumnLayoutProvider IExitControlLayoutProvider.SealsGridLayout => new SealsGridColumnLayout();

		IGridColumnLayoutProvider IExitControlLayoutProvider.ReportItemAdditionalDocumentsGridLayout => new ReportItemAdditionalDocumentGridColumnLayout();

		IGridColumnLayoutProvider IExitControlLayoutProvider.ConsignmentItemPackingDetailsGridLayout => new ConsignmentItemPackingDetailsGridColumnLayout();

		IGridColumnLayoutProvider IExitControlLayoutProvider.ConsignmentItemsGridLayout => new ConsignmentItemsGridColumnLayout();

		IGridColumnLayoutProvider IExitControlLayoutProvider.AuthorizationGridColumnLayout => new AuthorizationGridColumnLayout();

		bool IExitControlLayoutProvider.AuthorizationIsActive => false;
	}
}
