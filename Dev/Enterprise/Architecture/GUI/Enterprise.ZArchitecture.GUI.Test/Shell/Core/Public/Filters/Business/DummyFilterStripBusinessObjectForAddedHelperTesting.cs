using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class DummyFilterStripBusinessObjectForAddedHelperTesting : DummyFilterStripBusinessObject
	{
		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			return helpersToAddForTest;
		}

		public void AddHelperForTest(IFilterStripsHelper helper)
		{
			helpersToAddForTest.Add(helper);
		}

		public void AddHelperTypeToExcludeFromAutomaticAddingOfFilters(Type type)
		{
			typesToExclude.Add(type);
		}

		protected override IEnumerable<Type> FilterStripsHelperTypesToExcludeFromAutomaticAddingOfFilters
		{
			get { return typesToExclude; }
		}

		readonly List<IFilterStripsHelper> helpersToAddForTest = new List<IFilterStripsHelper>();
		readonly List<Type> typesToExclude = new List<Type>();
	}
}
