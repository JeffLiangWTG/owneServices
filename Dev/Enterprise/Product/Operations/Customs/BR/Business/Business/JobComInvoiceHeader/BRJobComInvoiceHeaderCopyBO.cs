using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class BRJobComInvoiceHeaderCopyBO : JobComInvoiceHeaderCopyBO
	{
		public BRJobComInvoiceHeaderCopyBO(JobComInvoiceHeader sourceInvoice, JobComInvoiceHeader destinationInvoice, JobComInvoiceHeaderCopyOptions copyOptions) : base(sourceInvoice, destinationInvoice)
		{
			this.copyOptions = copyOptions ?? new JobComInvoiceHeaderCopyOptions();
		}

		readonly JobComInvoiceHeaderCopyOptions copyOptions;

		protected override IEnumerable<BaseJobComInvoiceLine> GetInvoiceLinesToCopy() => base.GetInvoiceLinesToCopy().Cast<JobComInvoiceLine>().Where(x => ShouldCopy(x));

		bool ShouldCopy(JobComInvoiceLine invoiceLine) => copyOptions.AllLines || (copyOptions.OnlyLinesRequireLicense && invoiceLine.JI_RequiresImportLicense);

		protected override void AfterCopyInvoiceLine(BaseJobComInvoiceLine sourceInvoiceLine, BaseJobComInvoiceLine destinationInvoiceLine)
		{
			CopyInvoiceLine((JobComInvoiceLine)sourceInvoiceLine, (JobComInvoiceLine)destinationInvoiceLine);
			destinationInvoiceLine.JI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			destinationInvoiceLine.JI_ParentID = sourceInvoiceLine.PK;
		}

		void CopyInvoiceLine(JobComInvoiceLine sourceInvoiceLine, JobComInvoiceLine destinationInvoiceLine)
		{
			destinationInvoiceLine.NaladiHs = sourceInvoiceLine.NaladiHs;
			destinationInvoiceLine.JI_Weight = sourceInvoiceLine.JI_Weight;
			destinationInvoiceLine.JI_ManufacturerIndicator = sourceInvoiceLine.JI_ManufacturerIndicator;
			destinationInvoiceLine.EffectiveManufacturerAddressPK = sourceInvoiceLine.EffectiveManufacturerAddressPK;
			destinationInvoiceLine.DutyTaxRegime = sourceInvoiceLine.DutyTaxRegime;
			destinationInvoiceLine.DutyLegalBase = sourceInvoiceLine.DutyLegalBase;
			if (sourceInvoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.TariffAgreement) is AdditionalTariff tariffAgreement)
			{
				destinationInvoiceLine.JI_SecondaryPreference = tariffAgreement.TariffType;
			}
			destinationInvoiceLine.NVECusCodeDataCollection.CopyDataFrom(sourceInvoiceLine.NVECusCodeDataCollection);
			destinationInvoiceLine.TariffDetachs.CloneFrom(sourceInvoiceLine.TariffDetachs);
		}
	}
}
