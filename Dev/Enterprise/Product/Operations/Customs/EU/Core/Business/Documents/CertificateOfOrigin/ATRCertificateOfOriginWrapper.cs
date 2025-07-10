using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public class ATRCertificateOfOriginWrapper : CertificateOfOriginWrapper, IATRCertificateOfOrigin
	{
		public ATRCertificateOfOriginWrapper(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public ATRCertificateOfOriginWrapper(JobDeclaration declaration) : base(declaration)
		{
		}

		IATRCertificateDeclaration IATRCertificateOfOrigin.Declaration => new ATRCertificateDeclarationWrapper(EntryHeader.Declaration);

		IATRBoxItems IATRCertificateOfOrigin.ATRBoxItemBuilder => GetATRBoxItemsWrapper();

		IATRCertificateItem IATRCertificateOfOrigin.TotalATRCertificateItem => GetNewTotalATRCertificateItem();

		ZBool IATRCertificateOfOrigin.ShouldAddTotalCertificateItem => GetShouldAddTotalCertificateItem();

		ZString IATRCertificateOfOrigin.ARTNumberCaption => GetARTNumberCaption();

		ZString IATRCertificateOfOrigin.ARTEuropeanUnionCaption => GetARTEuropeanUnionCaption();

		protected virtual ZString GetARTNumberCaption() => Res.GetString("A0D2C750-6270-4DA8-9472-996673EAF584", "A.TR.No");

		protected virtual ZString GetARTEuropeanUnionCaption() => Res.GetString("57BA9E2C-EA50-4540-A9A2-0396EC5442EF", "EUROPEAN UNION");

		protected virtual ZBool GetShouldAddTotalCertificateItem() => false;

		protected virtual IATRBoxItems GetATRBoxItemsWrapper()
		{
			var invoiceLines = EntryHeader?.InvoiceLines.Cast<JobComInvoiceLine>() ?? Declaration.InvoiceLines.Cast<JobComInvoiceLine>();
			return new ATRBoxItemsWrapper(invoiceLines);
		}

		protected virtual IATRCertificateItem GetNewTotalATRCertificateItem() => null;
	}
}
