using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.BR.Business
{
	public class DocDeclaration : DocBaseJobDeclaration
	{
		DocDeclaration(JobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap) : base(jobDeclaration, factoryToWrap)
		{
		}

		public static DocDeclaration New(JobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap)
		{
			return jobDeclaration == null ? null : new DocDeclaration(jobDeclaration, factoryToWrap);
		}

		#region Overrides

		protected override DocBaseCusContainerCollection CreateCusContainerCollection(ICusContainerCollection<BaseCusContainer> collectionToWrap)
		{
			return new DocCusContainerCollection(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(Customs.Business.InvoiceLineCompleteCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeaderCollection CreateJobComInvoiceGroupHeaderCollection(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> collectionToWrap)
		{
			return new DocJobComInvoiceGroupHeaderCollection((IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeader CreateJobComInvoiceGroupHeader(BaseJobComInvoiceGroupHeader invoiceGroupToWrap)
		{
			return DocJobComInvoiceGroupHeader.New((JobComInvoiceGroupHeader)invoiceGroupToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeaderCollection CreateAllInvoiceHeaderCollection(Customs.Business.InvoiceHeaderActiveCollection collectionToWrap)
		{
			return new DocJobComInvoiceHeaderCollection((InvoiceHeaderActiveCollection)collectionToWrap, Factory);
		}

		protected override DocBaseCusEntryHeaderCollection CreateNewEntryHeadersCollection(
			ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> collectionToWrap)
		{
			return new DocCusEntryHeaderCollection(collectionToWrap, Factory);
		}

		#endregion

		#region Wrapper Fields

		public DocJobComInvoiceHeader InvoiceHeader
		{
			get { return (DocJobComInvoiceHeader)InvoiceHeaderInternal; }
		}

		public DocJobComInvoiceGroupHeader ActiveInvoiceHeader
		{
			get { return (DocJobComInvoiceGroupHeader)ActiveInvoiceHeaderGroupInternal; }
		}

		#endregion

		#region Collections

		public DocCusContainerCollection Containers
		{
			get { return (DocCusContainerCollection)ContainersInternal; }
		}

		public DocJobComInvoiceLineCollection InvoiceLines
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesInternal; }
		}

		public DocJobComInvoiceLineCollection InvoiceLinesSortedByLineNo
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesSortedByLineNoInternal; }
		}

		#endregion
	}
}
