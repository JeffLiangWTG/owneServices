using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.Customs.CA.Business
{
	abstract class AdjustmentsDocumentWrapper : NonPersistentBusinessObject, IObsoleteValidation, ISourceIdentifierProvider
	{
		public AdjustmentsDocumentWrapper(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
			CheckDeclarationType(declaration);
		}
		readonly JobDeclaration declaration;

		protected abstract void CheckDeclarationType(JobDeclaration declaration);

		protected abstract AdjustmentsDocHeader CreateNewHeader(JobDeclaration declaration = null);

		protected abstract AdjustmentsDocFooter CreateNewFooter(JobDeclaration declaration = null, IEnumerable<AdjustmentsDocPage> pages = null);

		protected abstract AdjustmentsDocPage CreateNewPage();

		public ZString TransactionNumberFormated
		{
			get { return string.Format("{0} - {1}{2}", declaration.TransactionNumber.AccountSecurityCode, declaration.TransactionNumber.SequentialNumber, declaration.TransactionNumber.CheckDigit); }
		}

		public BusinessObjectCollectionWrapper<AdjustmentsDocPage> DocumentPages
		{
			get { return fDocumentPages ?? (fDocumentPages = new BusinessObjectCollectionWrapper<AdjustmentsDocPage>(GetDocumentPages())); }
		}
		BusinessObjectCollectionWrapper<AdjustmentsDocPage> fDocumentPages;

		protected virtual IEnumerable<AdjustmentsDocPage> GetDocumentPages()
		{
			var result = new List<AdjustmentsDocPage>();
			var emptyHeader = CreateNewHeader();
			var emptyFooter = CreateNewFooter();
			var tempPage = CreateNewPage();
			foreach (JobComInvoiceHeader subHeader in declaration.B2AsClaimedForInvoices.OrderBy(x => x, new B2InvoiceHeaderComparer(typeof(JobComInvoiceHeader), JobComInvoiceHeader.Schema.JZ_InvoiceNumber, ListSortDirection.Ascending)))
			{
				foreach (var page in tempPage.GetPages(subHeader))
				{
					page.Header = emptyHeader;
					page.Footer = emptyFooter;
					result.Add(page);
				}
			}

			if (result.Any())
			{
				result[0].Header = CreateNewHeader(declaration);
				result[result.Count - 1].Footer = CreateNewFooter(declaration);
			}

			return result;
		}

		ZGuid ISourceIdentifierProvider.SourceIdentifier => declaration.PK;
	}
}
