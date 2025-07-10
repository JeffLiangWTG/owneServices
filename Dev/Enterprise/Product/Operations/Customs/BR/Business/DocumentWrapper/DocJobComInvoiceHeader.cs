using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.Customs.BR.Business
{
	public class DocJobComInvoiceHeader : DocBaseJobComInvoiceHeader
	{
		DocJobComInvoiceHeader(JobComInvoiceHeader jobComInvoiceHeader, BusinessObjectFactory factoryToWrap)
			: base(jobComInvoiceHeader, factoryToWrap)
		{
		}

		public static DocJobComInvoiceHeader New(JobComInvoiceHeader jobComInvoiceHeader, BusinessObjectFactory factoryToWrap)
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

		protected override DocBaseJobDeclaration CreateJobDeclaration(Customs.Business.BaseJobDeclaration declarationToWrap)
		{
			return DocDeclaration.New((JobDeclaration)declarationToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(Customs.Business.BaseJobComInvoiceLineViewCollection collectionToWrap)
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

		#region CodeAndDescriptionWrapper Fields

		public CodeAndDescriptionWrapper ExchangeHedge => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceHeader.Lookups.ExchangeHedgeList, JobComInvoiceHeader.ExchangeHedgeType, Factory);

		public CodeAndDescriptionWrapper ExchangeHedgeFI => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceHeader.Lookups.FinancialInstitutionList, JobComInvoiceHeader.ExchangeHedgeFinancialInstitution, Factory);

		public CodeAndDescriptionWrapper ExchangeHedgeReason => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceHeader.Lookups.ReasonTypeList, JobComInvoiceHeader.ExchangeHedgeReason, Factory);

		public CodeAndDescriptionWrapper ExchangeHedgePaymentMethod => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceHeader.Lookups.ExchangeHedgePaymentMethodList, JobComInvoiceHeader.ExchangeHedgePaymentMethod, Factory);

		public CodeAndDescriptionWrapper SupplierCountry
		{
			get
			{
				var country = SupplierAddres?.Country;
				return new CodeAndDescriptionWrapper(country?.Code ?? ZString.Empty, country?.Name.GetLocalizedValue(Core.Constants.Languages.PortugueseBrazil).ToString() ?? ZString.Empty, Factory);
			}
		}

		#endregion

		#region DocAddress

		public DocAddress SupplierAddres => supplierAddres ?? (supplierAddres = JobComInvoiceHeader.IsImportLicense
			? DocAddress.New(JobComInvoiceHeader.SupplierDocumentaryAddress.Address, Factory) : DocAddress.New(JobComInvoiceHeader.SupplierAddress, Factory));

		DocAddress supplierAddres;

		#endregion

		#region ZString

		public ZString ExchangeHedgeROFBasen => JobComInvoiceHeader.ExchangeHedgeROFBACENNumber;

		#endregion

		#region ZDecimal

		public ZDecimal ExchangeHedgeValue => JobComInvoiceHeader.ExchangeHedgeValue;

		public ZDecimal ExchangeHedgePaymentDeadline => JobComInvoiceHeader.ExchangeHedgePaymentDeadline;

		#endregion

		#region Wrapper Fields

		public DocDeclaration Declaration
		{
			get { return (DocDeclaration)DeclarationInternal; }
		}

		#endregion

		#region Implementation

		JobComInvoiceHeader JobComInvoiceHeader
		{
			get { return (JobComInvoiceHeader)WrappedObject; }
		}

		#endregion

	}
}
