
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.NZ
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

		protected override DocBaseJobDeclaration CreateJobDeclaration(Enterprise.Customs.Business.BaseJobDeclaration declarationToWrap)
		{
			return DocDeclaration.New((JobDeclaration)declarationToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(Enterprise.Customs.Business.BaseJobComInvoiceLineViewCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection((JobComInvoiceLineViewCollection)collectionToWrap, Factory);
		}

		public DocJobComInvoiceLineCollection InvoiceLines
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesInternal; }
		}

		public DocJobComInvoiceLineCollection UnclassifiedInvoiceLines
		{
			get { return (DocJobComInvoiceLineCollection)UnclassifiedInvoiceLinesInternal; }
		}

		public DocDeclaration Declaration
		{
			get { return (DocDeclaration)DeclarationInternal; }
		}

		JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)WrappedObject; }
		}

		protected override DocOrganisation GetNewSupplier()
		{
			if (InvoiceHeader.SupplierIsMiscAndAllowedToBeMisc)
			{
				return DocOrganisation.New(MiscSupplier, Factory);
			}
			return DocOrganisation.New(InvoiceHeader.Supplier, Factory);
		}

		OrgHeader fMiscSupplier;
		OrgHeader MiscSupplier
		{
			get
			{
				if (fMiscSupplier == null)
				{
					fMiscSupplier = new BusinessObjectFactory().New<OrgHeader>();
					fMiscSupplier.OH_FullName = InvoiceHeader.MiscSupplierName.Left(OrgHeader.Schema.OH_FullNameTruncatedLength);
					fMiscSupplier.OH_Code = "MISC";
				}
				return fMiscSupplier;
			}
		}
	}
}
