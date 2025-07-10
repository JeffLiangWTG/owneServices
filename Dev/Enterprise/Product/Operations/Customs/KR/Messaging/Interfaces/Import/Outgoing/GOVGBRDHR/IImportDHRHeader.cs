using System.Collections.Generic;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportDHRHeader : IImportFTAHeader
	{
		IEnumerable<IImportDHRInvoiceLine> DHRInvoiceLines { get; }
	}
}
