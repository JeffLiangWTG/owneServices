using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StmALogCollectionViewMoreTests : TestCaseWithFactory
	{
		public void TestDefaultState()
		{
			AssertEquals(2, View.BizObjNameToShowEventsFor_List.Count);
			AssertEquals("All", View.BizObjNameToShowEventsFor_List[0].Code);
			AssertEquals("This DummyBizo", View.BizObjNameToShowEventsFor_List[1].Code);
		}

		public void TestWithOneChild()
		{
			DummyChildEnterpriseBusinessObject child = CreateChild();
			AssertEquals("All", View.BizObjNameToShowEventsFor_List[0].Code);
			AssertEquals("This DummyBizo", View.BizObjNameToShowEventsFor_List[1].Code);
			AssertEquals("Child", View.BizObjNameToShowEventsFor_List[2].Code);
			AssertEquals(0, View.Count);

			child.Logs.AddNew();

			View.BizObjNameToShowEventsFor = "All";
			AssertEquals(1, View.Count);
			StmALog log = View[0];
			View.BizObjNameToShowEventsFor = "This DummyBizo";
			AssertEquals(0, View.Count);
			View.BizObjNameToShowEventsFor = "Child";
			AssertEquals(1, View.Count);
			AssertEquals(log, View[0]);
		}

		public void TestWithThreeChildren()
		{
			DummyChildEnterpriseBusinessObject[] children = CreateThreeChildren();
			SetupLogsForThreeChildren(children);

			AssertEquals(5, View.BizObjNameToShowEventsFor_List.Count);
			AssertEquals("All", View.BizObjNameToShowEventsFor_List[0].Code);
			AssertEquals("This DummyBizo", View.BizObjNameToShowEventsFor_List[1].Code);
			AssertEquals("Child", View.BizObjNameToShowEventsFor_List[2].Code);
			AssertEquals("Child [2]", View.BizObjNameToShowEventsFor_List[3].Code);
			AssertEquals("Child [3]", View.BizObjNameToShowEventsFor_List[4].Code);

			View.BizObjNameToShowEventsFor = "All";
			AssertEquals(3, View.Count);
			View.BizObjNameToShowEventsFor = "Child";
			AssertEquals(1, View.Count);
			View.BizObjNameToShowEventsFor = "Child [2]";
			AssertEquals(0, View.Count);
			View.BizObjNameToShowEventsFor = "Child [3]";
			AssertEquals(2, View.Count);
		}

		public void TestNameHandling()
		{
			DummyChildEnterpriseBusinessObject[] children = CreateThreeChildren();
			SetupLogsForThreeChildren(children);

			children[1].HumanReadableNameForTest = "Monkey";
			AssertEquals(5, View.BizObjNameToShowEventsFor_List.Count);
			AssertEquals("All", View.BizObjNameToShowEventsFor_List[0].Code);
			AssertEquals("This DummyBizo", View.BizObjNameToShowEventsFor_List[1].Code);
			AssertEquals("Child", View.BizObjNameToShowEventsFor_List[2].Code);
			AssertEquals("Monkey", View.BizObjNameToShowEventsFor_List[3].Code);
			AssertEquals("Child [2]", View.BizObjNameToShowEventsFor_List[4].Code);

			View.BizObjNameToShowEventsFor = "All";
			AssertEquals(3, View.Count);
			View.BizObjNameToShowEventsFor = "Child";
			AssertEquals(1, View.Count);
			View.BizObjNameToShowEventsFor = "Monkey";
			AssertEquals(0, View.Count);
			View.BizObjNameToShowEventsFor = "Child [2]";
			AssertEquals(2, View.Count);
		}

		#region Setup

		DummyChildEnterpriseBusinessObject CreateChild()
		{
			DummyChildEnterpriseBusinessObject child = Dummy.Collection.AddNew();
			child.HumanReadableNameForTest = "Child";
			return child;
		}

		DummyChildEnterpriseBusinessObject[] CreateThreeChildren()
		{
			return new DummyChildEnterpriseBusinessObject[] { CreateChild(), CreateChild(), CreateChild() };
		}

		void SetupLogsForThreeChildren(DummyChildEnterpriseBusinessObject[] children)
		{
			AssertEquals(3, children.Length);
			children[0].Logs.AddNew();
			children[2].Logs.AddNew(); // not a typo
			children[2].Logs.AddNew();
		}

		public DummyEnterpriseBusinessObject Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyEnterpriseBusinessObject>();
					dummy.BusinessObjectsWithRelatedEventsForTest = delegate
					{
						List<BusinessObject> result = new List<BusinessObject>();
						result.AddRange(dummy.Collection);
						return result.ToArray();
					};
				}
				return dummy;
			}
		}
		DummyEnterpriseBusinessObject dummy;

		public StmALogCollectionView View
		{
			get
			{
				if (view == null)
				{
					view = new StmALogCollectionView(Dummy);
				}
				// Trigger a RebuildAllElements on the Logs if needed, by sideffect.
				// This seems horrible, and someone should probably think about how to fix it.
				object x = Dummy.Logs.AllElements;
				return view;
			}
		}
		StmALogCollectionView view;

		#endregion
	}
}
