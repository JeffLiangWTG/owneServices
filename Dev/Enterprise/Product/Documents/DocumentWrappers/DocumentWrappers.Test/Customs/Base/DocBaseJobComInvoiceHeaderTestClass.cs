using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	sealed class DocBaseJobComInvoiceHeaderTestClass : DocBaseJobComInvoiceHeader
	{
		DocBaseJobComInvoiceHeaderTestClass(BaseJobComInvoiceHeader invoiceHeader, BusinessObjectFactory factoryToWrap)
			: base(invoiceHeader, factoryToWrap)
		{
		}

		public static DocBaseJobComInvoiceHeaderTestClass New(BaseJobComInvoiceHeader invoiceHeader, BusinessObjectFactory factoryToWrap)
		{
			if (invoiceHeader == null)
			{
				return null;
			}
			else
			{ return new DocBaseJobComInvoiceHeaderTestClass(invoiceHeader, factoryToWrap); }
		}

		public DocBaseJobDeclaration DeclarationInternalTestMethod
		{
			get { return base.DeclarationInternal; }
		}

		public DocBaseJobComInvoiceLineCollection InvoiceLinesInternalTestMethod
		{
			get { return base.InvoiceLinesInternal; }
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(BaseJobComInvoiceLineViewCollection collectionToWrap)
		{
			return new DocBaseJobComInvoiceLineCollectionTestClass(collectionToWrap, Factory);
		}

		protected override DocBaseJobDeclaration CreateJobDeclaration(BaseJobDeclaration declarationToWrap)
		{
			return DocBaseJobDeclarationTestClass.New(declarationToWrap, Factory);
		}
	}
}
