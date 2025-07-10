using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(SchematicComponentSectionDescriptor))]
	class SchematicComponentSectionDescriptorTest : BoardSectionDescriptorTestCase<SchematicComponentSectionDescriptor>
	{
		protected override string Type
		{
			get { return BMConstants.ComponentSectionType; }
		}

		protected override void TestGetSectionConfigurationBizoCore(IBoardSectionDescriptor descriptor)
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			var sectionConfiguration = descriptor.GetSectionConfigurationBizo(section);
			AssertNotNull(sectionConfiguration);
			AssertType<BMComponentSectionConfiguration>(sectionConfiguration);
		}

		protected override void TestGetSectionConfigurationControlCore(IBoardSectionDescriptor descriptor)
		{
			using (var control = (Control)descriptor.GetSectionConfigurationControl())
			{
				AssertType<ComponentConfigurationControl>(control);
			}
		}

		protected override void TestGetAdditionalTabsCore(IBoardSectionDescriptor descriptor)
		{
			var tabs = descriptor.GetAdditionalTabs().ToArray();
			AssertEquals(8, tabs.Length);

			var tabIndex = 0;
			AssertTabAndDisposeControl<FilterTabPageControl>(tabs[tabIndex++], "WorkflowFilterTabControl", "Workflow Filters");
			AssertTabAndDisposeControl<FilterTabPageControl>(tabs[tabIndex++], "TaskFilterTabControl", "Task Filters");
			AssertTabAndDisposeControl<ColorsTabPageControl>(tabs[tabIndex++], "ColorsTabControl", "Colors");
			AssertTabAndDisposeControl<PrimaryChannelsTabPageControl>(tabs[tabIndex++], "PrimaryChannelsTabControl", "Primary Channels");
			AssertTabAndDisposeControl<SecondaryChannelsTabPageControl>(tabs[tabIndex++], "SecondaryChannelsTabControl", "Secondary Channels");
			AssertTabAndDisposeControl<AdditionalComponentsTabPageControl>(tabs[tabIndex++], "AdditionalComponentsTabControl", "Additional Components");
			AssertTabAndDisposeControl<AcceptabilityBandsTabPageControl>(tabs[tabIndex++], "AcceptabilityBandsTabPageControl", "Acceptability Bands");
			AssertTabAndDisposeControl<CustomisedLayoutsSectionConfigControl>(tabs[tabIndex++], "CustomisedLayoutsSectionConfigControl", "Customized Layouts");
		}

		static void AssertTabAndDisposeControl<T>(TabSpec tabSpec, string name, string text)
			where T : Control
		{
			try
			{
				AssertEquals(name, tabSpec.Name);
				AssertEquals(text, tabSpec.Text);
				AssertType<T>(tabSpec.TabContentControl);
			}
			finally
			{
				var disposable = tabSpec.TabContentControl as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		protected override void TestGetSectionControlCore(IBoardSectionDescriptor descriptor)
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();

			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(board);
			var viewModel = descriptor.GetViewModel(section, boardViewModel);

			using (var control = (IDisposable)descriptor.GetSectionControl(section, viewModel))
			{
				AssertType<BMComponentControl>(control);
			}
		}

		protected override void TestGetViewModelCore(IBoardSectionDescriptor descriptor)
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();

			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(board);
			var viewModel = descriptor.GetViewModel(section, boardViewModel);

			AssertType<BMBoardSectionViewModel>(viewModel);
		}
	}
}
