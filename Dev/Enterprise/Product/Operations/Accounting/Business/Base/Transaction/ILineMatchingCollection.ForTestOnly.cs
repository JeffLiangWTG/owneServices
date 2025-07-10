#if DEBUG

using System.Collections.Generic;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public partial class ILineMatchingCollection
	{
		public List<string> WritableProperties_ForTestOnly => WritableProperties;
	}
}

#endif
