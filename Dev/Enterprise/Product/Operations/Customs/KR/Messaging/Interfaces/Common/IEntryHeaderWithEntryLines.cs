using System.Collections.Generic;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IEntryHeaderWithEntryLines
	{
		IEnumerable<IEntryLine> EntryLines { get; }
	}
}
