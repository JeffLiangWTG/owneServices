using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	sealed class DocBaseJobDeclarationTestClass : DocBaseJobDeclaration
	{
		DocBaseJobDeclarationTestClass(BaseJobDeclaration baseJobDeclaration, BusinessObjectFactory factoryToWrap)
			: base(baseJobDeclaration, factoryToWrap)
		{
		}

		public static new DocBaseJobDeclarationTestClass New(BaseJobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap)
		{
			if (jobDeclaration == null)
			{
				return null;
			}
			else
			{
				return new DocBaseJobDeclarationTestClass(jobDeclaration, factoryToWrap);
			}
		}

		public DocBaseCusContainerCollection ContainersInternalTestMethod
		{
			get { return base.ContainersInternal; }
		}

		public DocBaseJobComInvoiceGroupHeaderCollection InvoiceGroupHeadersInternalTestMethod
		{
			get { return base.InvoiceGroupHeadersInternal; }
		}

		public DocBaseJobComInvoiceLineCollection InvoiceLinesInternalTestMethod
		{
			get { return base.InvoiceLinesInternal; }
		}

		public DocBaseJobComInvoiceLineCollection InvoiceLinesSortedByLineNoInternalTestMethod
		{
			get { return base.InvoiceLinesSortedByLineNoInternal; }
		}

		public DocBaseJobComInvoiceLineCollection InvoiceLinesSortedByMergedLineNoInternalTestMethod
		{
			get { return base.InvoiceLinesSortedByMergedLineNoInternal; }
		}

		public DocBaseJobComInvoiceLineCollection InvoiceLinesSortedByMergedNumericLineNoInternalTestMethod
		{
			get { return base.InvoiceLinesSortedByMergedNumericLineNoInternal; }
		}

		public DocBaseJobComInvoiceHeader InvoiceHeaderInternalTestMethod
		{
			get { return base.InvoiceHeaderInternal; }
		}

		public DocBaseJobComInvoiceGroupHeader ActiveInvoiceHeaderGroupInternalTestMethod
		{
			get { return base.ActiveInvoiceHeaderGroupInternal; }
		}

		public DocBaseJobComInvoiceHeaderCollection InvoiceHeadersInternalTestMethod
		{
			get { return base.InvoiceHeadersInternal; }
		}

		#region Implementation

		protected override DocBaseCusContainerCollection CreateCusContainerCollection(ICusContainerCollection<BaseCusContainer> collectionToWrap)
		{
			return new DocBaseCusContainerCollectionTestClass(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeader CreateJobComInvoiceGroupHeader(BaseJobComInvoiceGroupHeader invoiceGroupToWrap)
		{
			return DocBaseJobComInvoiceGroupHeaderTestClass.New(invoiceGroupToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeaderCollection CreateJobComInvoiceGroupHeaderCollection(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> collectionToWrap)
		{
			return new DocBaseJobComInvoiceGroupHeaderCollectionTestClass(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocBaseJobComInvoiceHeaderTestClass.New(invoiceToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(InvoiceLineCompleteCollection collectionToWrap)
		{
			return new DocBaseJobComInvoiceLineCollectionTestClass(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeaderCollection CreateAllInvoiceHeaderCollection(InvoiceHeaderActiveCollection collectionToWrap)
		{
			return new DocBaseJobComInvoiceHeaderCollectionTestClass(collectionToWrap, Factory);
		}

		protected override DocBaseCusEntryHeaderCollection CreateNewEntryHeadersCollection(
			ICusEntryHeaderCollection<CusEntryHeader> collectionToWrap)
		{
			return null;
		}

		#endregion
	}
}
