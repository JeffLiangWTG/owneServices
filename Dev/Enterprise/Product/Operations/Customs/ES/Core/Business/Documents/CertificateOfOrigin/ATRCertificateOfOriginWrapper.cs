using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin;

public class ATRCertificateOfOriginWrapper : EU.Business.Documents.CertificateOfOrigin.ATRCertificateOfOriginWrapper
{
	public ATRCertificateOfOriginWrapper(CusEntryHeader entryHeader) : base(entryHeader)
	{
		Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
	}

	protected new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

	protected override IATRCertificateItem GetNewTotalATRCertificateItem() => new DocDataObjects.TotalATRCertificateItem(EntryHeader);

	protected override ICustomsEndorsement GetNewCustomsEndorsement() => new CustomsEndorsementWrapper(EntryHeader);

	protected override IATRBoxItems GetATRBoxItemsWrapper()
	{
		return new ATRBoxItemsWrapper(EntryHeader.InvoiceLines.Cast<JobComInvoiceLine>());
	}

	protected override IExporterDeclaration GetNewExporterDeclaration() => new ExporterDeclarationWrapper(EntryHeader.Declaration);

	protected override ZString GetReferenceDateFormat() => "dd-MM-yyyy";

	protected override ZBool GetShouldAddTotalCertificateItem() => true;

	protected override ZString GetARTNumberCaption() => Res.GetString("2756FDCE-52AD-4A65-B780-26B7D1EA477D", "A.TR.1 Nº");

	protected override ZString GetARTEuropeanUnionCaption() => Res.GetString("A20A857C-F4BA-4CC2-B4BB-71AC07D99140", "COMUNIDAD EUROPEA");
}
