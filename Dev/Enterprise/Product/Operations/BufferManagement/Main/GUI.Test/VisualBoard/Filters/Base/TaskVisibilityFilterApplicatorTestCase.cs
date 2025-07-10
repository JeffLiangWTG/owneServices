using System.Collections.Generic;
using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestsSubclassesOf(typeof(IFilterApplicator))]
	public abstract class TaskVisibilityFilterApplicatorTestCase<T> : FilterApplicatorTestCase<T>
		where T : CardVisibilityFilter
	{
		protected abstract T GetFilter();

		[TestDate(2017, 10, 11)]
		public void TestDbHits()
		{
			var staff = CreateStaffInCurrentBranchDept("NOT", "Not Gone");
			var system = BMSTestHelper.GetOrCreateSystem(Factory, "ORG");
			var buffer = CreateBuffer(system, "Joomal");
			var tasks = new List<ProcessTask>();

			for (var i = 0; i < 3; i++)
			{
				var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Rilly?", buffer);
				tasks.Add(BMSTestHelper.CreateTask(workflow, staff.GS_Code, description: $"Task{i}_1"));
				tasks.Add(BMSTestHelper.CreateTask(workflow, staff.GS_Code, description: $"Task{i}_2"));
			}

			var viewModel = BMSTestHelper.CreateSectionAndViewModel(buffer).Item2;
			Factory.Save();

			var cards = tasks.Select(t => new TaskCardContent(t, viewModel)).ToArray();

			var newFactory = Factory.CreateNewFactory();
			var filter = GetFilter();
			filter.FetchForFilter(viewModel, newFactory, cards);

			var cell = new CellContent(0, 0, CellContentType.Cards);
			var applicator = filter.GetCellVisibilityApplicator(viewModel, newFactory);

			foreach (var card in cards)
			{
				applicator(card, cell);
			}

			AssertDbHits(GetExpectedDbHits(), newFactory);
		}

		protected abstract IEnumerable<KeyValuePair<string, int>> GetExpectedDbHits();
	}
}
