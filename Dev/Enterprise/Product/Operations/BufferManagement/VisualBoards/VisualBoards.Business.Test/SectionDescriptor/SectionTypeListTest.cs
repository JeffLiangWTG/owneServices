using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.VisualBoards.Business.Test
{
	class SectionTypeListTest : VisualBoardsTestCase
	{
		public void TestList()
		{
			var descriptor1 = new DummyDescriptor { Type = "AAA", Description = "Dat Descriptor" };
			var descriptor2 = new DummyDescriptor { Type = "BBB", Description = "Dis Descriptor" };

			using (ObjectFactory.Substitute("VisualBoardSectionDescriptors", new ArrayList { descriptor1, descriptor2 }))
			{
				var list = new SectionTypeList();
				AssertEquals(2, list.Count);

				AssertEquals("AAA", list[0].Code);
				AssertEquals("Dat Descriptor", list[0].Description);

				AssertEquals("BBB", list[1].Code);
				AssertEquals("Dis Descriptor", list[1].Description);
			}
		}

		public void TestLiveList()
		{
			var list = new SectionTypeList();
			Assert("Should have at least the default section type", list.Count > 0);
		}

		class DummyDescriptor : IBoardSectionDescriptor
		{
			public string Type { get; set; }
			public string Description { get; set; }

			public IBoardSectionConfigurationBizo GetSectionConfigurationBizo(IBMBoardSection section)
			{
				throw new NotImplementedException();
			}

			public object GetSectionConfigurationControl()
			{
				throw new NotImplementedException();
			}

			public IEnumerable<TabSpec> GetAdditionalTabs()
			{
				throw new NotImplementedException();
			}

			public IBoardSectionControl GetSectionControl(IBMBoardSection section, BoardSectionViewModel sectionViewModel)
			{
				throw new NotImplementedException();
			}

			public BoardSectionViewModel GetViewModel(IBMBoardSection section, BoardViewModel boardViewModel)
			{
				throw new NotImplementedException();
			}
		}
	}
}
