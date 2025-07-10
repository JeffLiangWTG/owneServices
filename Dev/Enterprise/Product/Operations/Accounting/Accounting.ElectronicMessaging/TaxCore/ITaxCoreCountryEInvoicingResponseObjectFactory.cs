using System;
using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public interface ITaxCoreCountryEInvoicingResponseObjectFactory
	{
		ITaxCoreEInvoiceResponseReader GetEInvoiceResponseReader();

		ITaxCoreEInvoiceAuthorisationRecordCreator GetAuthorisationRecordCreator();

		ITaxCoreEInvoiceChecker GetEInvoiceChecker();

		ZString CountryName { get; }

		ZString ResponseMessageSubTypeCode { get; }

		ZString ResponseMessageContextTypeCode { get; }

		ZString ResponseName { get; }
	}

	public abstract class TaxCoreCountryEInvoicingResponseObjectFactory : ITaxCoreCountryEInvoicingResponseObjectFactory
	{
		ZString ITaxCoreCountryEInvoicingResponseObjectFactory.CountryName => CountryName;

		ZString ITaxCoreCountryEInvoicingResponseObjectFactory.ResponseMessageSubTypeCode => InvoiceResponseMessageSubTypeCode;

		ZString ITaxCoreCountryEInvoicingResponseObjectFactory.ResponseMessageContextTypeCode => "ResponseMessage";

		ZString ITaxCoreCountryEInvoicingResponseObjectFactory.ResponseName => ResponseName;

		ITaxCoreEInvoiceAuthorisationRecordCreator ITaxCoreCountryEInvoicingResponseObjectFactory.GetAuthorisationRecordCreator() => GetAuthorisationRecordCreator();

		ITaxCoreEInvoiceChecker ITaxCoreCountryEInvoicingResponseObjectFactory.GetEInvoiceChecker() => GetEInvoiceChecker();

		ITaxCoreEInvoiceResponseReader ITaxCoreCountryEInvoicingResponseObjectFactory.GetEInvoiceResponseReader() => GetEInvoiceResponseReader();

		protected abstract string CountryName { get; }

		protected virtual string InvoiceResponseMessageSubTypeCode => FormattableString.Invariant($"{CountryName} Invoice Response");

		protected virtual string ResponseName => FormattableString.Invariant($"{CountryName} E-Invoice Response");

		protected abstract ITaxCoreEInvoiceAuthorisationRecordCreator GetAuthorisationRecordCreator();

		protected virtual ITaxCoreEInvoiceChecker GetEInvoiceChecker() => new TaxCoreEInvoiceChecker();

		protected virtual ITaxCoreEInvoiceResponseReader GetEInvoiceResponseReader() => new TaxCoreEInvoiceResponseReader();
	}
}
