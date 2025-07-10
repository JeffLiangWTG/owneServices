using System;
using CargoWise.Accounting.eInvoicing.Egypt;
using CargoWise.Cryptoki.Signing.API;
using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class EgyptAccountingCountryFactory : IAccountingCountryFactory, IEInvoicingSignatureBuilderProvider
	{
		public IXmlDocumentSignatureBuilder GetXmlDocumentSignatureBuilder(ISignatureAlgorithm algorithm, Func<DateTime> utcNowGetter = null)
			=> new EgyptSignatureBuilder(algorithm, utcNowGetter);
	}
}
