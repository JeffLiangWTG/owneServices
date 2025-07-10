using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.MX.Business
{
	public class DocJobComInvoiceHeader : DocBaseJobComInvoiceHeader
	{
		DocJobComInvoiceHeader(JobComInvoiceHeader jobComInvoiceHeader, BusinessObjectFactory factoryToWrap)
			: base(jobComInvoiceHeader, factoryToWrap)
		{
		}

		public static DocJobComInvoiceHeader New(JobComInvoiceHeader jobComInvoiceHeader, BusinessObjectFactory factoryToWrap)
		{
			return jobComInvoiceHeader == null ? null : new DocJobComInvoiceHeader(jobComInvoiceHeader, factoryToWrap);
		}

		public ZString InvoiceHeaderNumber => InvoiceNumber;

		public ZString InvoiceHeaderDate => InvoiceDate.ToShortDateString();

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(BaseJobComInvoiceLineViewCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection(collectionToWrap, Factory);
		}

		protected override DocBaseJobDeclaration CreateJobDeclaration(BaseJobDeclaration declarationToWrap)
		{
			return DocDeclaration.New((JobDeclaration)declarationToWrap, Factory);
		}

		public DocJobComInvoiceLineCollection InvoiceLines => (DocJobComInvoiceLineCollection)InvoiceLinesInternal;

		public DocJobComInvoiceLineCollection UnclassifiedInvoiceLines => (DocJobComInvoiceLineCollection)UnclassifiedInvoiceLinesInternal;

		public DocDeclaration Declaration => (DocDeclaration)DeclarationInternal;
	}
}
