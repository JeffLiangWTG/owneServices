using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.ES.Business
{
	public class ESSupportingDocumentHelper : EUSupportingDocumentHelper
	{
		public ESSupportingDocumentHelper(ICanBeImportOrExport parentImportExport) : base(parentImportExport)
		{
		}

		public IEnumerable<ZString> GetInvoiceTransportSupportingDocumentTypes() => ESConstants.SupportingDocumentTypes.InvoiceTransportDocumentTypesForImport;
		public ZBool IsTransportType(ZString supportingDocumentType) => GetInvoiceTransportSupportingDocumentTypes().Contains(supportingDocumentType);

		protected override IEnumerable<ZString> GetInvoiceSupportingDocumentsForImport() => ESConstants.SupportingDocumentTypes.InvoiceDocumentTypesForImport;
	}
}
