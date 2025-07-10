using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.Customs.BR.Business
{
	public class DocJobComInvoiceLine : DocBaseJobComInvoiceLine
	{
		DocJobComInvoiceLine(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
			: base(jobComInvoiceLine, factoryToWrap)
		{
		}

		public static DocJobComInvoiceLine New(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
		{
			return jobComInvoiceLine == null ? null : new DocJobComInvoiceLine(jobComInvoiceLine, factoryToWrap);
		}

		#region Overrides

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(Customs.Business.BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		protected override DocBaseCusEntryLine CreateCusEntryLine(Customs.Business.CusEntryLine entryLineToWrap)
		{
			return DocCusEntryLine.New((CusEntryLine)entryLineToWrap, Factory);
		}

		#endregion

		#region Wrapper Fields

		public DocCusEntryLine EntryLine => (DocCusEntryLine)CusEntryLineInternal;

		public DocJobComInvoiceHeader ComInvoiceHeader => (DocJobComInvoiceHeader)InvoiceHeaderInternal;

		public DocAddress ManufacturerAddress => manufacturerAddress ?? (manufacturerAddress = JobComInvoiceLine.IsImportLicense
			? DocAddress.New(JobComInvoiceLine.ManufacturerDocAddress.Address, Factory) : DocAddress.New(JobComInvoiceLine.ManufacturerAddress, Factory));

		DocAddress manufacturerAddress;

		#endregion

		#region ZString Fields

		public ZString NFENumber => JobComInvoiceLine.JI_NFeNumber;

		public ZString NetWeightUQ => JobComInvoiceLine.JI_NetWeightUQ;

		public ZString GrossWeightUQ => JobComInvoiceLine.JI_WeightUQ;

		public ZString TariffDetach => JobComInvoiceLine.TariffDetachConcatenated;

		public ZString NaladiHs => JobComInvoiceLine.NaladiHs;

		public ZString TariffDescription => JobComInvoiceLine.TariffDescription;

		public ZString DrawbackCANumber => JobComInvoiceLine.DrawbackCANumber;

		public ZString FullGoodsDescription => JobComInvoiceLine.FullGoodsDescription;

		public ZString IPITaxBenefitLegalActNumber => JobComInvoiceLine.IPITaxBenefitLegalActNumber;

		public ZString IPITaxBenefitLegalActYear => JobComInvoiceLine.IPITaxBenefitLegalActYear;

		public ZString AntidumpingLegalActNumber => JobComInvoiceLine.AntidumpingLegalActNumber;

		public ZString AntidumpingLegalActYear => JobComInvoiceLine.AntidumpingLegalActYear;

		#endregion

		#region ZDecimal Fields

		public ZDecimal NetWeight => JobComInvoiceLine.JI_NetWeight;

		public ZDecimal NetWeightInKG => JobComInvoiceLine.NetWeightInKG;

		public ZDecimal GrossWeight => JobComInvoiceLine.JI_Weight;

		public ZDecimal GrossWeightInKG => JobComInvoiceLine.GrossWeightInKG;

		public ZDecimal NFeLinePrice => JobComInvoiceLine.JI_NFeLinePrice;

		#endregion

		#region CodeAndDescriptionWrapper

		public CodeAndDescriptionWrapper UsedMaterialRegime => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.UsedMaterialRegimeList, JobComInvoiceLine.JI_UsedMaterialRegime, Factory);

		public CodeAndDescriptionWrapper UsedMaterialOperationType => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.GoodsConditionOperationTypeList, JobComInvoiceLine.JI_UsedMaterialOperationType, Factory);

		public CodeAndDescriptionWrapper DrawbackModality => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.DrawbackModalityList, JobComInvoiceLine.DrawbackModality, Factory);

		public CodeAndDescriptionWrapper TariffAgreement => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.TariffAgreementList, JobComInvoiceLine.JI_SecondaryPreference, Factory);

		public CodeAndDescriptionWrapper DutyTaxRegime => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.DutyTaxRegimeList, JobComInvoiceLine.DutyTaxRegime, Factory);

		public CodeAndDescriptionWrapper DutyLegalBase => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.DutyLegalBaseList, JobComInvoiceLine.DutyLegalBase, Factory);

		public CodeAndDescriptionWrapper PisCofinsLegalBase => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.PisCofinsLegalBaseList, JobComInvoiceLine.PisCofinsLegalBase, Factory);

		public CodeAndDescriptionWrapper IPITaxBenefitLegalActIssuingBody => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.LegalActIssuingAuthorityList, JobComInvoiceLine.IPITaxBenefitLegalActIssuingBody, Factory);

		public CodeAndDescriptionWrapper IPITaxBenefitLegalActType => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.ExTariffLegalActList, JobComInvoiceLine.IPITaxBenefitLegalActType, Factory);

		public CodeAndDescriptionWrapper AntidumpingLegalActIssuingBody => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.LegalActIssuingAuthorityList, JobComInvoiceLine.AntidumpingLegalActIssuingBody, Factory);

		public CodeAndDescriptionWrapper AntidumpingLegalActType => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.ExTariffLegalActList, JobComInvoiceLine.AntidumpingLegalActType, Factory);

		public CodeAndDescriptionWrapper IPITaxRegime => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.IPITaxRegimeList, JobComInvoiceLine.IPITaxRegime, Factory);

		public CodeAndDescriptionWrapper PisCofinsTaxRegime => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.PisCofinsTaxRegimeList, JobComInvoiceLine.PisCofinsTaxRegime, Factory);

		public CodeAndDescriptionWrapper ICMSTaxRegime => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.ICMSTaxRegimeList, JobComInvoiceLine.ICMSTaxRegime, Factory);

		public CodeAndDescriptionWrapper ManufacturerIndicator => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.ManufacturerIndicatorList, JobComInvoiceLine.JI_ManufacturerIndicator, Factory);

		public CodeAndDescriptionWrapper ICMSLegalBase => BRPairListHelper.CreateCodeAndDescriptionWrapper(JobComInvoiceLine.Lookups.ICMSLegalBaseList, JobComInvoiceLine.ICMSLegalBase, Factory);

		#endregion

		#region Implementation

		JobComInvoiceLine JobComInvoiceLine => (JobComInvoiceLine)WrappedObject;

		#endregion

	}
}
