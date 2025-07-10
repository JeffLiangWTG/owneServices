using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin
{
	public class EURCertificateOfOriginWrapper : EU.Business.Documents.CertificateOfOrigin.EURCertificateOfOriginWrapper
	{
		public EURCertificateOfOriginWrapper(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override ZString GetReferenceDateFormat() => "dd-MM-yyyy";

		protected override EU.Business.Documents.CertificateOfOrigin.ITransportDetail GetNewTransportDetail() => new TransportDetailWrapper((JobDeclaration)Declaration);

		protected override EU.Business.Documents.CertificateOfOrigin.IEURGoodsSummary GetNewGoodsSummary() => new EURGoodsSummaryWrapper(InvoiceLines);

		protected override EU.Business.Documents.CertificateOfOrigin.ICustomsEndorsement GetNewCustomsEndorsement()
		{
			if (EntryHeader is CusEntryHeader actualEntryHeader)
			{
				return new CustomsEndorsementWrapper(actualEntryHeader);
			}
			return base.GetNewCustomsEndorsement();
		}

		protected override EU.Business.Documents.CertificateOfOrigin.IExporterDeclaration GetNewExporterDeclaration() => new ExporterDeclarationWrapper((JobDeclaration)Declaration);

		protected override ZString GetCountryDescription(RefUNLOCO unloco)
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Spanish))
			{
				return base.GetCountryDescription(unloco);
			}
		}

		protected override ZString GetCountryGroup(RefUNLOCO unloco, ZString country)
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Spanish))
			{
				return base.GetCountryGroup(unloco, country);
			}
		}
	}
}
