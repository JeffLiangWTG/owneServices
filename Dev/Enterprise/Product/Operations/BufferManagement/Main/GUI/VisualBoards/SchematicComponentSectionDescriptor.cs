using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.GUI
{
	public class SchematicComponentSectionDescriptor : IBoardSectionDescriptor, IBoardFactoryServiceProvider
	{
		#region IBoardSectionDescriptor Members

		string IBoardSectionDescriptor.Type
		{
			get { return BMConstants.ComponentSectionType; }
		}

		string IBoardSectionDescriptor.Description
		{
			get { return Res.GetString("0b175bf5-7289-4b73-9e3e-708de4fa00ce", "Buffer Management System Component"); }
		}

		IBoardSectionConfigurationBizo IBoardSectionDescriptor.GetSectionConfigurationBizo(IBMBoardSection section)
		{
			Argument.NotNull(section, "section");

			return new BMComponentSectionConfiguration(section);
		}

		object IBoardSectionDescriptor.GetSectionConfigurationControl()
		{
			return new ComponentConfigurationControl();
		}

		public const string FilterIdentifier = "SectionFilter";

		IEnumerable<TabSpec> IBoardSectionDescriptor.GetAdditionalTabs()
		{
			yield return new TabSpec
			{
				Name = "WorkflowFilterTabControl",
				Text = Res.GetString("a957a362-54db-48f3-869d-6092df157772", "Workflow Filters"),
				TabContentControl = new FilterTabPageControl("WorkflowFilter", FilterIdentifier, "WorkflowFilterTabControl"), // This is a property name
				IsAlwaysEnabled = true
			};
			yield return new TabSpec
			{
				Name = "TaskFilterTabControl",
				Text = Res.GetString("e23adc89-f687-4422-900f-d906c22a90dd", "Task Filters"),
				TabContentControl = new FilterTabPageControl("TaskFilter", FilterIdentifier, "TaskFilterTabControl"), // This is a property name
				IsAlwaysEnabled = true
			};
			yield return new TabSpec
			{
				Name = "ColorsTabControl",
				Text = Res.GetString("dbd74e77-6b98-4ab8-91a5-36f79e19759e", "Colors"),
				TabContentControl = new ColorsTabPageControl(),
			};
			yield return new TabSpec
			{
				Name = "PrimaryChannelsTabControl",
				Text = Res.GetString("a5caa64c-602e-4de9-869a-514a09e73cd7", "Primary Channels"),
				TabContentControl = new PrimaryChannelsTabPageControl(),
			};
			yield return new TabSpec
			{
				Name = "SecondaryChannelsTabControl",
				Text = Res.GetString("cb49b279-5cbc-49ba-b111-80851290acbd", "Secondary Channels"),
				TabContentControl = new SecondaryChannelsTabPageControl(),
			};
			yield return new TabSpec
			{
				Name = "AdditionalComponentsTabControl",
				Text = Res.GetString("f9a81c3d-b0f3-4e40-bdaf-13cfe57816a8", "Additional Components"),
				TabContentControl = new AdditionalComponentsTabPageControl(),
			};
			yield return new TabSpec
			{
				Name = "AcceptabilityBandsTabPageControl",
				Text = Res.GetString("50194cda-4e70-496b-83e3-145978850085", "Acceptability Bands"),
				TabContentControl = new AcceptabilityBandsTabPageControl(),
			};
			yield return new TabSpec
			{
				Name = "CustomisedLayoutsSectionConfigControl",
				Text = Res.GetString("1a483d8e-7998-404c-a20a-12ca52715901", "Customized Layouts"),
				TabContentControl = new CustomisedLayoutsSectionConfigControl(),
			};
		}

		IBoardSectionControl IBoardSectionDescriptor.GetSectionControl(IBMBoardSection section, BoardSectionViewModel sectionViewModel)
		{
			Argument.NotNull(sectionViewModel, "sectionViewModel");

			return new BMComponentControl(((BMBoardSection)section).SectionConfiguration, (BMBoardSectionViewModel)sectionViewModel);
		}

		BoardSectionViewModel IBoardSectionDescriptor.GetViewModel(IBMBoardSection section, BoardViewModel boardViewModel)
		{
			return new BMBoardSectionViewModel((BMBoardSection)section, boardViewModel);
		}

		#endregion

		#region IBoardFactoryServiceProvider Members

		IEnumerable<IBoardFactoryService> IBoardFactoryServiceProvider.GetFactoryServices(IVisualBoardProvider source)
		{
			return GetFactoryServicesCore(source);
		}

		protected virtual IEnumerable<IBoardFactoryService> GetFactoryServicesCore(IVisualBoardProvider source)
		{
			yield return new CapacityConstrainedResourcesCacheService(source);
			yield return new RoadRunnerStatusCacheService();
		}

		#endregion
	}
}
