using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.General
{
	public class DocDeclaration : DocBaseJobDeclaration
	{
		DocDeclaration(BaseJobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap)
			: base(jobDeclaration, factoryToWrap)
		{
		}

		public static new DocDeclaration New(BaseJobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap)
		{
			if (jobDeclaration == null)
			{
				return null;
			}
			else
			{
				return new DocDeclaration(jobDeclaration, factoryToWrap);
			}
		}

		#region Overrides

		protected override DocBaseCusContainerCollection CreateCusContainerCollection(ICusContainerCollection<BaseCusContainer> collectionToWrap)
		{
			return new DocCusContainerCollection(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(InvoiceLineCompleteCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeaderCollection CreateJobComInvoiceGroupHeaderCollection(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> collectionToWrap)
		{
			return new DocJobComInvoiceGroupHeaderCollection(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New(invoiceToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeader CreateJobComInvoiceGroupHeader(BaseJobComInvoiceGroupHeader invoiceGroupToWrap)
		{
			return DocJobComInvoiceGroupHeader.New(invoiceGroupToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeaderCollection CreateAllInvoiceHeaderCollection(InvoiceHeaderActiveCollection collectionToWrap)
		{
			DocJobComInvoiceHeaderCollection coll = new DocJobComInvoiceHeaderCollection(JobDeclaration.Factory);
			foreach (BaseJobComInvoiceHeader header in collectionToWrap)
			{
				coll.Add(DocJobComInvoiceHeader.New(header, Factory));
			}
			return coll;
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

		#region Implementation

		BaseJobDeclaration JobDeclaration
		{
			get { return (BaseJobDeclaration)WrappedObject; }
		}

		protected override DocBaseCusEntryHeaderCollection CreateNewEntryHeadersCollection(
			ICusEntryHeaderCollection<CusEntryHeader> collectionToWrap)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
