using System.Collections.Generic;
using System.ComponentModel;
using Enterprise.Messaging.Business;

namespace Enterprise.Messaging.Testing
{
	sealed class EDIMessageComparerTest_ForAscending : EDIMessageComparerTest<EDIMessage>
	{
		protected override int LessThan { get { return -1; } }
		protected override int GreaterThan { get { return 1; } }

		protected override IComparer<EDIMessage> GetNewComparer()
		{
			return new EDIMessageComparer(ListSortDirection.Ascending);
		}
	}
}
