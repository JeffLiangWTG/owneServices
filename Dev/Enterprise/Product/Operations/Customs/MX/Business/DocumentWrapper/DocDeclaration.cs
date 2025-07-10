using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.MX.Business
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

		protected override DocBaseJobComInvoiceHeaderCollection CreateAllInvoiceHeaderCollection(Customs.Business.InvoiceHeaderActiveCollection collectionToWrap)
		{
			return new DocJobComInvoiceHeaderCollection((InvoiceHeaderActiveCollection)collectionToWrap, Factory);
		}

		protected override DocBaseCusContainerCollection CreateCusContainerCollection(ICusContainerCollection<BaseCusContainer> collectionToWrap)
		{
			return new DocCusContainerCollection(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeader CreateJobComInvoiceGroupHeader(BaseJobComInvoiceGroupHeader invoiceGroupToWrap)
		{
			return DocJobComInvoiceGroupHeader.New((JobComInvoiceGroupHeader)invoiceGroupToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeaderCollection CreateJobComInvoiceGroupHeaderCollection(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> collectionToWrap)
		{
			return new DocJobComInvoiceGroupHeaderCollection((IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(Customs.Business.InvoiceLineCompleteCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection(collectionToWrap, Factory);
		}

		protected override DocBaseCusEntryHeaderCollection CreateNewEntryHeadersCollection(ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> collectionToWrap)
		{
			return new DocCusEntryHeaderCollection(collectionToWrap, Factory);
		}

		#region Wrapper Fields

		public DocJobComInvoiceHeader InvoiceHeader => (DocJobComInvoiceHeader)InvoiceHeaderInternal;

		#endregion

		#region Collections

		public DocCusContainerCollection Containers => (DocCusContainerCollection)ContainersInternal;

		public DocJobComInvoiceLineCollection InvoiceLines => (DocJobComInvoiceLineCollection)InvoiceLinesInternal;

		public DocJobComInvoiceLineCollection InvoiceLinesSortedByLineNo => (DocJobComInvoiceLineCollection)InvoiceLinesSortedByLineNoInternal;

		public DocJobComInvoiceHeaderCollection InvoiceHeaders => (DocJobComInvoiceHeaderCollection)InvoiceHeadersInternal;

		public DocJobComInvoiceGroupHeader ActiveInvoiceHeader => (DocJobComInvoiceGroupHeader)ActiveInvoiceHeaderGroupInternal;

		#endregion

		#region Importer

		public ZString ImporterRFC => OrgImporter?.PrimaryRegistrationNumber.Number ?? ZString.Empty;

		public ZString ImporterName => OrgImporter?.OH_FullName ?? ZString.Empty;

		public ZString ImporterAddress => OrgImporter?.MainAddress.Address1 ?? ZString.Empty;

		public ZString ImporterPostCode => OrgImporter?.MainAddress.Postcode ?? ZString.Empty;

		public ZString ImporterState => OrgImporter?.MainAddress.State ?? ZString.Empty;

		public ZString ImporterCity => OrgImporter?.MainAddress.City ?? ZString.Empty;

		public ZString ImporterCountry
		{
			get { return OrgImporter != null ? Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, OrgImporter.MainAddress.Country.Code)?.RN_DescMultilingual ?? ZString.Empty : ZString.Empty; }
		}

		#endregion

		#region Supplier

		public ZString SupplierName => OrgSupplier?.OH_FullName ?? ZString.Empty;

		public ZString SupplierAddress => OrgSupplier?.MainAddress.Address1 ?? ZString.Empty;

		public ZString SupplierPostCode => OrgSupplier?.MainAddress.Postcode ?? ZString.Empty;

		public ZString SupplierState => OrgSupplier?.MainAddress.State ?? ZString.Empty;

		public ZString SupplierCity => OrgSupplier?.MainAddress.City ?? ZString.Empty;

		public ZString SupplierCountry
		{
			get { return OrgSupplier != null ? Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Declaration.Supplier.MainAddress.Country.Code)?.RN_DescMultilingual ?? ZString.Empty : ZString.Empty; }
		}

		#endregion

		#region Implementation

		JobDeclaration Declaration => (JobDeclaration)WrappedObject;

		OrgHeader OrgImporter => Declaration.Importer;

		OrgHeader OrgSupplier => Declaration.Supplier;

		#endregion
	}
}
