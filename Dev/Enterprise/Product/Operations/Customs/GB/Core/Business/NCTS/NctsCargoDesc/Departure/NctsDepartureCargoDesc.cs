using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.NctsAndDeclarationIntegration;

namespace Enterprise.Customs.GB.Business
{
	public class NctsDepartureCargoDesc : EU.NCTS.Business.NctsDepartureCargoDesc
		, Integration.Customs.GB.IDepartureCargoDesc
	{
		public NctsDepartureCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new NctsHeader Header => (NctsHeader)base.Header;

		public new EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments => (EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;

		protected override EU.NCTS.Business.INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetNewNctsSupportingDocumentCollection() => new EU.NCTS.Business.NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

		protected override Type SupportingDocumentType => typeof(NctsSupportingDocument);

		protected override IEntryNumberFormatterForNctsAndDeclarationIntegration GetEntryNumberFormatterCore() => new EntryNumberFormatterForNctsAndDeclarationIntegration();
	}
}
