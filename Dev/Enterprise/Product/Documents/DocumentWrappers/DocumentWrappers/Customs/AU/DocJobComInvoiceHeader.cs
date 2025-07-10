
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.AU
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

		public override ZString TariffHeading
		{
			get { return "Tariff Stat / Trt / Concession Type and Number"; }
		}

		protected override DocBaseJobDeclaration CreateJobDeclaration(Enterprise.Customs.Business.BaseJobDeclaration declarationToWrap)
		{
			return DocDeclaration.New((JobDeclaration)declarationToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(Enterprise.Customs.Business.BaseJobComInvoiceLineViewCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection((JobComInvoiceLineViewCollection)collectionToWrap, Factory);
		}

		#endregion

		#region Wrapper Fields

		public DocDeclaration Declaration
		{
			get { return (DocDeclaration)DeclarationInternal; }
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

		#region ZString Fields

		public ZString ValuationBasis
		{
			get { return JobComInvoiceHeader.JZ_ValuationBasis; }
		}

		public ZString Nature10UnitPack
		{
			get { return JobComInvoiceHeader.JZ_Nature10UnitPack; }
		}

		public ZString BondUnitPack
		{
			get { return JobComInvoiceHeader.JZ_BondUnitPack; }
		}

		public ZString CommissionType
		{
			get { return JobComInvoiceHeader.JZ_CommissionType; }
		}

		public ZString DrawbackEDN
		{
			get { return JobComInvoiceHeader.AddInfo.ZA_EDN_Hidden; }
		}

		#endregion

		#region ZInt Fields

		public ZInt Nature10PackCount
		{
			get { return JobComInvoiceHeader.JZ_Nature10PackCount; }
		}

		public ZInt PiecesForRelease
		{
			get { return JobComInvoiceHeader.JZ_PiecesForRelease; }
		}

		public ZInt PiecesToBond
		{
			get { return JobComInvoiceHeader.JZ_PiecesToBond; }
		}

		public ZInt BondPackCount
		{
			get { return JobComInvoiceHeader.JZ_BondPackCount; }
		}

		#endregion

		#region Charge Fields

		public ZDecimal IncludedBuyingCommission
		{
			get { return JobComInvoiceHeader.IncludedBuyingCommission.Amount; }
		}

		public ZDecimal ExcludedBuyingCommission
		{
			get { return JobComInvoiceHeader.ExcludedBuyingCommission.Amount; }
		}

		public ZDecimal IncludedOtherCommission
		{
			get { return JobComInvoiceHeader.IncludedOtherCommission.Amount; }
		}

		public ZDecimal ExcludedOtherCommission
		{
			get { return JobComInvoiceHeader.ExcludedOtherCommission.Amount; }
		}

		public DocCurrency IncludedBuyingCommissionCurrency
		{
			get { return DocCurrency.New(Factory, JobComInvoiceHeader.IncludedBuyingCommission.Currency); }
		}

		public DocCurrency ExcludedBuyingCommissionCurrency
		{
			get { return DocCurrency.New(Factory, JobComInvoiceHeader.ExcludedBuyingCommission.Currency); }
		}

		public DocCurrency IncludedOtherCommissionCurrency
		{
			get { return DocCurrency.New(Factory, JobComInvoiceHeader.IncludedOtherCommission.Currency); }
		}

		public DocCurrency ExcludedOtherCommissionCurrency
		{
			get { return DocCurrency.New(Factory, JobComInvoiceHeader.ExcludedOtherCommission.Currency); }
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
