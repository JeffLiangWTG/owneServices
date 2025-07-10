using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IEntryLine
	{
		ZInt EntryLineNo { get; }

		IEnumerable<IInvoiceLine> InvoiceLines { get; }
	}
}
