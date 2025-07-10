using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(BMBoardSection))]
	public class BMBoardSectionTest : EnterpriseBusinessObjectTestCase
	{
		#region Clone

		public void TestCloneBMBoardSection()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(system, Factory.NewWithValidTestData<GlbGroup>());
			var section = BMSTestHelper.CreateBoardSection(buffer);

			Factory.Save();

			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.ReleaseGroupPK = releaseGroup.PK;
			section.Column = 333;
			var additionalComponent = section.SectionConfiguration.AdditionalComponents.AddNew();
			additionalComponent.BSA_FC_Component = bucket.PK;
			FilterStripsTestHelper.AddStartsWithFilter(section.WorkflowFilter, "Completion Statement", "I hate every ape I see...");
			FilterStripsTestHelper.AddStartsWithFilter(section.TaskFilter, "Description", "...from chimpan-A to chimpanzee.");

			var clone = (BMBoardSection)section.Clone();

			AssertEquals(buffer.PK, clone.MS_FC_Component);
			AssertEquals(section.Column, clone.Column);
			AssertEquals(releaseGroup.PK, clone.SectionConfiguration.ReleaseGroupPK);
			AssertEquals(ChannelTypeList.Codes.Resource, clone.SectionConfiguration.ChannelBy);
			AssertEquals(1, clone.SectionConfiguration.AdditionalComponents.Count);
			AssertEquals(bucket.PK, clone.SectionConfiguration.AdditionalComponents[0].BSA_FC_Component);

			BMSTestHelper.AssertModuleFilterDeepClone(section.WorkflowFilter, clone.WorkflowFilter, clone);
			BMSTestHelper.AssertModuleFilterDeepClone(section.TaskFilter, clone.TaskFilter, clone);
		}

		#endregion

		#region Properties

		public void TestDefaultValues()
		{
			var section = Factory.New<BMBoardSection>();
			var sectionConfiguration = section.SectionConfiguration;

			AssertEquals(1, section.ColSpan);
			AssertEquals(1, section.RowSpan);
			AssertEquals(nameof(Orientation.Vertical), sectionConfiguration.Orientation);
			AssertEquals("", section.BackgroundColor);
			AssertEquals("Black", section.ForegroundColor);
			AssertColorEquals(Color.Red, sectionConfiguration.CountdownTargetBorderColorValue.Value);
			AssertEquals(new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Solid, true), sectionConfiguration.CountdownTargetBorderStyleValue);
			AssertColorEquals(Color.Blue, sectionConfiguration.CountdownStartableBorderColorValue.Value);
			AssertEquals(new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Solid, true), sectionConfiguration.CountdownStartableBorderStyleValue);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			// This section should have no component selected to ensure validation etc can cope with it.

			var system = Factory.New<BMSystem>();
			system.FS_Name = "BLAH";
			var board = system.Boards.AddNew();
			return board.Sections.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = GetNewBusinessObject();
			return result;
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "Configuration";
				yield return "RowSpan";
				yield return "ColSpan";
				yield return "RowHeightPercent";
				yield return "ColWidthPercent";
				yield return "BackgroundColor";
				yield return "ForegroundColor";
				yield return "Row";
				yield return "Column";
			}
		}

		#endregion
	}
	
	[TestedType(typeof(BMBoardSection))]
	public class BMBoardSectionRelatedFilterTest : RelatedModuleFilterSupportableTestCase<BMBoardSection>
	{
		protected override BMBoardSection GetNewBusinessObject()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			return config.BufferSection;
		}

		protected override IEnumerable<FilterRuleTestSet> GetFilterRules(BMBoardSection businessObject)
		{
			return new[]
			{
				new FilterRuleTestSet("WFL", () => businessObject.WorkflowFilter, "BMFilterRuleFilterBusinessObject"),
				new FilterRuleTestSet("TSK", () => businessObject.TaskFilter, "ProcessTaskFilterBusinessObject")
			};
		}

		protected override void ValidateBusinessObject(BMBoardSection businessObject)
		{
			businessObject.SectionConfiguration.Validation.ValidateAll();
		}
	}
}
