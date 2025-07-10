using System.Collections.Generic;

namespace Enterprise.Customs.EU.Business.Declaration.SupplementaryHelper
{
	public interface IEntryHeaderFilter
	{
		IEnumerable<CusEntryHeader> EntryHeaders { get; }
	}
}
