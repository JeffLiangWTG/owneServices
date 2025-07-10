using System;
using CargoWise.Cryptoki.Signing.API;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IEInvoicingSignatureBuilderProvider
	{
		IXmlDocumentSignatureBuilder GetXmlDocumentSignatureBuilder(ISignatureAlgorithm algorithm, Func<DateTime> utcNowGetter = null);
	}
}
