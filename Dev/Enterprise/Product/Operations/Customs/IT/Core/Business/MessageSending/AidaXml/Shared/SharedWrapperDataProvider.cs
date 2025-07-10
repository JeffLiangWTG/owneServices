using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public static class SharedWrapperDataProvider
{
	public static int GetNatureOfTransaction(CusEntryHeader entryHeader)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));

		var natureOfTransaction = entryHeader.RandomHeader.JZ_ValuationCode;

		if (int.TryParse(natureOfTransaction, out var result))
		{
			return result;
		}
		return 0;
	}

	public static IEnumerable<TInterface> GetAdditionalInfosFromInvoiceLinesAndHeaders<TInterface>(IEnumerable<JobComInvoiceLine> invoiceLines, string subType, Func<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo, TInterface> mappingFunc)
	{
		var invoices = invoiceLines.Select(x => x.InvoiceHeader).Distinct();
		var invoicesAdditionalInfos = invoices
			.SelectMany(x => x.AdditionalInfos.Cast<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>())
			.Where(x => x.CSI_SubType == subType);

		var invoiceLinesAdditionalInfos = invoiceLines
			.SelectMany(x => x.AdditionalInfos.Cast<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>())
			.Where(x => x.CSI_SubType == subType);

		return invoicesAdditionalInfos
			.Union(invoiceLinesAdditionalInfos)
			.Select(x => mappingFunc(x));
	}

	internal static IHeaderOrLineValueMapResolver<CusEntryHeader, CusEntryLine, ZString> GetNewTransportChargesMoPHeaderOrLineValueResolver()
	{
		return GetHeaderOrLineZStringMapResolver(
			h => h.TransportChargesMethodOfPayment,
			l => l.TransportChargesMethodOfPayment,
			useFallback: false);
	}

	internal static IHeaderOrLineValueMapResolver<CusEntryHeader, CusEntryLine, ZString> GetNewNatureOfTransactionHeaderOrLineValueResolver()
	{
		return GetHeaderOrLineZStringMapResolver(
			h => h.NatureOfTransaction,
			l => l.NatureOfTransaction,
			useFallback: false);
	}

	internal static IHeaderOrLineValueMapResolver<CusEntryHeader, CusEntryLine, ZString> GetNewCountryOfDestinationHeaderOrLineValueResolver()
	{
		return GetHeaderOrLineZStringMapResolver(
			h => h.CountryOfDestination,
			l => l.CountryOfDestination);
	}

	internal static IHeaderOrLineValueMapResolver<CusEntryHeader, CusEntryLine, IEoriTrader> GetNewConsignorHeaderOrLineValueResolver()
	{
		return GetHeaderOrLineEoriTraderMapResolver(
			h => GetTrader(h.Declaration?.SupplierDocumentaryAddress?.Address),
			l => GetTrader(l.Consignor));
	}

	internal static IHeaderOrLineValueMapResolver<CusEntryHeader, CusEntryLine, IEoriTrader> GetNewConsigneeHeaderOrLineValueResolver()
	{
		return GetHeaderOrLineEoriTraderMapResolver(
			h => GetTrader(h.Declaration?.ImporterDocumentaryAddress?.Address),
			l => GetTrader(l.Consignee));
	}

	internal static IHeaderOrLineValueMapResolver<CusEntryHeader, CusEntryLine, ZString> GetNewCountryOfExportHeaderOrLineValueResolver()
	{
		return GetHeaderOrLineZStringMapResolver(
			h => h.CountryOfExport,
			l => l.CountryOfExport);
	}

	internal static IEoriTrader GetTrader(OrgAddress address)
	{
		return address is null
			? null
			: new EoriOrTcuTraderWrapper(address);
	}

	static IHeaderOrLineValueMapResolver<CusEntryHeader, CusEntryLine, ZString> GetHeaderOrLineZStringMapResolver(
		Func<CusEntryHeader, ZString> headerValueGetter,
		Func<CusEntryLine, ZString> lineValueGetter,
		bool useFallback = true)
	{
		var builder = HeaderOrLineValueMapResolverFluent
			.Configure<CusEntryHeader, CusEntryLine, ZString>()
			.SetLineGetter(x => x.MergedLines)
			.SetHeaderGetter(x => x.Header)
			.IsEmptyWhen(x => x.IsEmpty)
			.SetLineValueGetter(lineValueGetter)
			.SetHeaderValueGetter(headerValueGetter);

		if (useFallback)
		{
			builder.UseFallBack();
		}

		return builder.Build();
	}

	static IHeaderOrLineValueMapResolver<CusEntryHeader, CusEntryLine, IEoriTrader> GetHeaderOrLineEoriTraderMapResolver(
		Func<CusEntryHeader, IEoriTrader> headerValueGetter,
		Func<CusEntryLine, IEoriTrader> lineValueGetter)
	{
		return HeaderOrLineValueMapResolverFluent
			.Configure<CusEntryHeader, CusEntryLine, IEoriTrader>()
			.SetLineGetter(x => x.MergedLines)
			.SetHeaderGetter(x => x.Header)
			.SetHeaderValueGetter(headerValueGetter)
			.SetLineValueGetter(lineValueGetter)
			.WithEqualityComparer(new TraderEqualityComparer())
			.UseFallBack()
			.Build();
	}
}
