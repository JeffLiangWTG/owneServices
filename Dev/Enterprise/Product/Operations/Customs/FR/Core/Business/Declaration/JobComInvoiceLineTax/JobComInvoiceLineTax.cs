using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class JobComInvoiceLineTax : EU.Business.Declaration.JobComInvoiceLineTax
	{
		public JobComInvoiceLineTax(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Customs.Business.JobComInvoiceLineTaxLookups GetNewLookups()
		{
			return new JobComInvoiceLineTaxLookups(this);
		}

		public new JobComInvoiceLineTaxLookups Lookups => (JobComInvoiceLineTaxLookups)base.Lookups;

		public new JobComInvoiceLineTaxValidation Validation => (JobComInvoiceLineTaxValidation)base.Validation;

		protected override Customs.Business.JobComInvoiceLineTaxValidation GetNewValidation()
		{
			return new JobComInvoiceLineTaxValidation(this);
		}

		public bool IsMethodOfCalculationReadOnly => false;

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		[CargoWiseOne.ResourceStrings.ResourceStringData(JobComInvoiceLine.Schema.JI_TariffBypassCode, Caption = "Tariff Bypass Code", ShortCaption = "Tariff Bypass")]
		public ZString TariffBypassCode => InvoiceLine.JI_TariffBypassCode;
	}
}
