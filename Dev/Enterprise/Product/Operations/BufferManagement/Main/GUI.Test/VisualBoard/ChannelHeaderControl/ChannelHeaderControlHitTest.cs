using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ChannelHeaderControlHitTest : BMSGUITestCase
	{
		public void TestChannelHeaderControlDbHits()
		{
			var expectedHits = new Dictionary<string, int>
			{
				{ BMBoardSectionSchema.Constants.TableName, 1 },
			};

			var system = CreateSystem("ORG");
			var component = CreateBuffer(system);
			var section = CreateBoardSection(component);
			var staff = CreateStaffInCurrentBranchDept("MON", "Mongo");

			Factory.Save();
			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, includeFactoryPredicate: f => f.NameForDebugging.Contains("SectionFactory")))
			{
				var newFactory = new BusinessObjectFactory { NameForDebugging = "SectionFactory" };
				var loadedSection = newFactory.Load<BMBoardSection>(section.PK);
				var cell = new CellContent(0, 0, CellContentType.ChannelHeading);
				cell.Channel = viewModel.CreateChannelForTest(staff);
				new ChannelHeaderControl(cell, viewModel).Dispose();
			}
		}
	}
}
