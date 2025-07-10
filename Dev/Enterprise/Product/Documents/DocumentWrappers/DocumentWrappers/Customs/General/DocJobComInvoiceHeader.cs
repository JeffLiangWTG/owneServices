using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.General
{
	public class DocJobComInvoiceHeader : DocBaseJobComInvoiceHeader
	{
		DocJobComInvoiceHeader(BaseJobComInvoiceHeader jobComInvoiceHeader, BusinessObjectFactory factoryToWrap)
			: base(jobComInvoiceHeader, factoryToWrap)
		{
		}

		public static DocJobComInvoiceHeader New(BaseJobComInvoiceHeader jobComInvoiceHeader, BusinessObjectFactory factoryToWrap)
		{
			if (jobComInvoiceHeader == null)
			{
				return null;
			}
			else
			{
				return new DocJobComInvoiceHeader(jobComInvoiceHeader, factoryToWrap);
			}
		}

		#region Overrides

		protected override DocBaseJobDeclaration CreateJobDeclaration(BaseJobDeclaration declarationToWrap)
		{
			return DocDeclaration.New(declarationToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(BaseJobComInvoiceLineViewCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection(collectionToWrap, Factory);
		}

		#endregion

		#region Collections

		public DocJobComInvoiceLineCollection InvoiceLines
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesInternal; }
		}
		public DocJobComInvoiceLineCollection UnclassifiedInvoiceLines
		{
			get { return (DocJobComInvoiceLineCollection)UnclassifiedInvoiceLinesInternal; }
		}

		#endregion

		#region Wrapper Fields

		public DocDeclaration Declaration
		{
			get { return (DocDeclaration)DeclarationInternal; }
		}

		#endregion
	}
}
