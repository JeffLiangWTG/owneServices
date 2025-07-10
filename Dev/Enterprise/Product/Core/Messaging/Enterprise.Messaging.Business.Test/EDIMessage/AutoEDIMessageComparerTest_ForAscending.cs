using System.Collections.Generic;
using System.ComponentModel;
using Enterprise.Messaging.Business;

namespace Enterprise.Messaging.Testing
{
	sealed class AutoEDIMessageComparerTest_ForAscending : AutoEDIMessageComparerTest<AutoEDIMessageForTest>
	{
		protected override int LessThan { get { return -1; } }
		protected override int GreaterThan { get { return 1; } }

		protected override IComparer<AutoEDIMessageForTest> GetNewComparer()
		{
			return new AutoEDIMessageComparer<AutoEDIMessageForTest>(ListSortDirection.Ascending);
		}
	}
}
