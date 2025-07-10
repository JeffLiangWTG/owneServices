using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.Business
{
	public class EUSupportingDocumentHelper
	{
		public EUSupportingDocumentHelper(ICanBeImportOrExport parentImportExport)
		{
			this.parentImportExport = Argument.NotNull(parentImportExport, nameof(parentImportExport));
		}
		protected readonly ICanBeImportOrExport parentImportExport;

		public IEnumerable<ZString> GetInvoiceSupportingDocumentTypes() => GetInvoiceSupportingDocumentTypesCore();

		protected virtual IEnumerable<ZString> GetInvoiceSupportingDocumentTypesCore()
		{
			var invoiceSupportingDocumentTypes = Enumerable.Empty<ZString>();
			if (parentImportExport?.IsImport ?? false)
			{
				invoiceSupportingDocumentTypes = GetInvoiceSupportingDocumentsForImport();
			}
			else if (parentImportExport?.IsExport ?? false)
			{
				invoiceSupportingDocumentTypes = GetInvoiceSupportingDocumentsForExport();
			}
			return invoiceSupportingDocumentTypes;
		}

		protected virtual IEnumerable<ZString> GetInvoiceSupportingDocumentsForImport() => UniversalReferenceConstants.SupportingDocumentTypes.InvoiceDocumentTypesForImport;
		protected virtual IEnumerable<ZString> GetInvoiceSupportingDocumentsForExport() => UniversalReferenceConstants.SupportingDocumentTypes.InvoiceDocumentTypesForExport;

		public ZBool IsInvoiceType(ZString supportingDocumentType) => GetInvoiceSupportingDocumentTypes().Contains(supportingDocumentType);
	}
}
