using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.CH.Business;

[AllowNoStaticNew]
public class DocDeclaration : DocBaseJobDeclaration
{
	public static DocDeclaration New(JobDeclaration declaration, BusinessObjectFactory factoryToWrap) => declaration == null ? null : new DocDeclaration(declaration, factoryToWrap);

	DocDeclaration(JobDeclaration declaration, BusinessObjectFactory factoryToWrap) : base(declaration, factoryToWrap)
	{
	}

	#region Collections

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

	public DocCusEntryHeaderCollection EntryHeaders
	{
		get { return (DocCusEntryHeaderCollection)RateEntryHeaders; }
	}

	protected override DocBaseCusEntryHeaderCollection CreateNewEntryHeadersCollection(ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> collectionToWrap)
	{
		var entryHeaders = new DocCusEntryHeaderCollection(collectionToWrap, Factory);
		entryHeaders.Sort(nameof(DocBaseCusEntryHeader.BGMReference), System.ComponentModel.ListSortDirection.Ascending);
		return entryHeaders;
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
