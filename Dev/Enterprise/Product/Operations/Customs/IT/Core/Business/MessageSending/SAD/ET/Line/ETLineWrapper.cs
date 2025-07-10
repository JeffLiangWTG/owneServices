using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class ETLineWrapper : SADLineCommonWrapper, IETLine
{
	public ETLineWrapper(CusEntryLine entryLine)
		: base(entryLine)
	{
		entryInstruction = Argument.NotNull(entryLine?.Header?.EntryInstruction, nameof(entryLine.Header.EntryInstruction));
		randomInvoiceLine = Argument.NotNull(entryLine?.RandomLine, nameof(entryLine.RandomLine));
	}
	readonly CusEntryInstruction entryInstruction;
	readonly JobComInvoiceLine randomInvoiceLine;

	public ZString DeclarationType => ZString.Empty;

	public ITrader Consignor
	{
		get
		{
			var invoiceHeader = randomInvoiceLine.InvoiceHeader;
			if (entryInstruction.IsBuyersConsol && invoiceHeader != null)
			{
				return new SADTraderWrapper(invoiceHeader.SupplierDocumentaryAddress);
			}
			return new SADEmptyTraderWrapper();
		}
	}

	public ITrader Consignee => new SADEmptyTraderWrapper();

	public ZString DispatchCountryCode => DispatchCountryCodeResolver.GetDispatchCountryCodeForLine(entryLine);

	public ZString DestinationCountryCode => ZString.Empty;

	public IETLineSecurityBlock SecurityBlock => new ETLineSecurityBlockWrapper(entryLine);

	public IEnumerable<IPackage> Packages => entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.PackagesPivot).Any() ? new[] { new SADLinePackageWrapper(entryLine) } : Enumerable.Empty<SADLinePackageWrapper>();

	public IETLineSpecialMentionGroup SpecialMentionGroup => new ETLineSpecialMentionGroupWrapper(entryLine);

	public ZString ComplementOfInformation => ZString.Empty;

	public ZString ComplementOfInformationLng => ZString.Empty;

	protected override ZString CountryOfOriginCore => randomInvoiceLine.JI_StateOrRegionOfOrigin;

	protected override IEnumerable<ZString> NationalProceduresCore
	{
		get
		{
			var code = entryLine.NationalProcedureCode;
			return code.IsEmpty ? new ZString[] { "0" } : new ZString[] { code };
		}
	}

	DispatchCountryCodeResolver DispatchCountryCodeResolver => dispatchCountryCodeResolver ?? (dispatchCountryCodeResolver = new DispatchCountryCodeResolver(entryHeader));
	DispatchCountryCodeResolver dispatchCountryCodeResolver;
}
