using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TestSearchBoxFilter : ISearchBoxFilter
	{
		public TestSearchBoxFilter(DummyWithLookups master)
		{
			this.master = master;
		}

		readonly DummyWithLookups master;

		public void ApplySearch(BusinessObjectCollection collection, string searchPhrase)
		{
			var allDependents = new DummyDependentWithCodeBusinessObjectCollection(master, collection.Factory);
			allDependents.Reload(true);

			var selectedDependents = allDependents.ToArray<DummyDependentWithCodeBusinessObject>().Where(d => d.ZD1_Code.Contains(searchPhrase)).ToArray();
			collection.RemoveAll();
			collection.AddRange(selectedDependents);
		}

		public void ApplySearch(IList collection, string searchPhrase)
			=> ApplySearch((BusinessObjectCollection)collection, searchPhrase);

		public void ClearSearch(IList list)
		{
		}

		public int MaximumRows { get; set; }
	}
}
