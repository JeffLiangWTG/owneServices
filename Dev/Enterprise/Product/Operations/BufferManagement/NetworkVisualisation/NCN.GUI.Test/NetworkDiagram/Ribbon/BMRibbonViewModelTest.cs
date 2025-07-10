using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	public class BMRibbonViewModelTest : NetworkRibbonViewModelTestCase
	{
#if !WINZOR
		public void TestBMRibbonViewModel_ShouldReferToCorrectKeys_WhenMergingWithDefaultRibbonViewModel()
		{
			AssertNoExceptionThrown(() =>
			{
				var diagram = NetworkTestCase.CreateDiagram(new BusinessObjectFactory());
				var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
				var viewModel = new BMRibbonViewModel(networkViewModel, control: null);
			});
		}
#endif
		protected override INetwork GetNetwork()
		{
			var factory = new BusinessObjectFactory();
			var diagram = NetworkTestCase.CreateDiagram(factory);
			return NetworkTestCase.CreateNetwork(diagram);
		}

		protected override IRibbonDataProvider GetRibbonDataProvider()
		{
			return new BMRibbonViewModelProvider();
		}

		protected override IEnumerable<string> GetSignaturesOfContextMenuActionsThatAreContainersForOtherActions()
		{
			return base.GetSignaturesOfContextMenuActionsThatAreContainersForOtherActions().Concat(GetSignaturesOfSpecificContextMenuActionsThatAreContainersForOtherActions());
		}

		IEnumerable<string> GetSignaturesOfSpecificContextMenuActionsThatAreContainersForOtherActions()
		{
			yield return nameof(LinkEntityActions);
			yield return nameof(CoreCustomActions);
			yield return nameof(PushAllEntitiesAction);
		}

		protected override IEnumerable<string> GetSignaturesOfActionsPurposelyNotIncludedIntoRibbon()
		{
			return base.GetSignaturesOfActionsPurposelyNotIncludedIntoRibbon().Concat(GetSignaturesOfSpecificActionTypesPurposelyNotIncludedIntoRibbon());
		}

		IEnumerable<string> GetSignaturesOfSpecificActionTypesPurposelyNotIncludedIntoRibbon()
		{
			yield return nameof(CreateShapeFromClipboardAction);
			yield return nameof(CreateDefaultWorkflowAction); // included into context menu as it appears for default diagrams but not shown on ribbon as ribbon is not displayed for default diagrams
			yield return nameof(CreateJobAction);
			yield return nameof(ToggleResourceDependencyVisibilityAction);
			yield return nameof(DecoupleAction); // decided to not include this action into ribbon
			yield return nameof(MoveToOtherSectionAction);
			yield return "PositionalNetworkAction: Copy An Existing Diagram";
		}

		protected override IEnumerable<string> GetSignaturesOfActionsPurposelyNotIncludedIntoContextMenu()
		{
			return base.GetSignaturesOfActionsPurposelyNotIncludedIntoContextMenu().Concat(GetSignaturesOfSpecificActionsPurposelyNotIncludedIntoContextMenu());
		}

		IEnumerable<string> GetSignaturesOfSpecificActionsPurposelyNotIncludedIntoContextMenu()
		{
			yield return nameof(ShowHideShapeInspectorAction);
		}
	}
}
