using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business
{
	public class JobComInvoiceLineTax
		: EU.Business.Declaration.JobComInvoiceLineTax
		, Integration.Customs.GB.IJobComInvoiceLineTax
	{
		public JobComInvoiceLineTax(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new JobComInvoiceLineTaxLookups Lookups => (JobComInvoiceLineTaxLookups)base.Lookups;

		protected override Customs.Business.JobComInvoiceLineTaxLookups GetNewLookups()
		{
			return new JobComInvoiceLineTaxLookups(this);
		}

		public override ZString JLT_Calc_RateDuty
		{
			get => base.JLT_Calc_RateDuty;
			set
			{
				base.JLT_Calc_RateDuty = value;
				if (value == EU.Business.TaxRateExciseDutyListImport.Codes.ExciseACustomsDutyReliefCpcIsUsedAndASuspensionOfExciseDutyOnTiedHydrocarbonOilsIsAlsoClaimedExdMustBeEnteredAsTheTaxRateOverrideCodeAnd000EnteredInTheAmountColumn)
				{
					JLT_RateOverrideReasonCode = TaxRateExciseOverrideListImport.Codes.TheExciseDutyPayableIsBeingCalculatedByTheTrader;
				}
			}
		}

		public JobDeclaration Declaration => InvoiceLine?.Declaration as JobDeclaration;

		public override ZString JLT_MethodOfPayment
		{
			get => base.JLT_MethodOfPayment;
			set
			{
				var oldValue = JLT_MethodOfPayment;
				base.JLT_MethodOfPayment = value;
				if (oldValue != JLT_MethodOfPayment)
				{
					Declaration?.UCCHelper?.DoIfTaxLinePaymentMethodIsNOrP(this);
				}
			}
		}

		public new JobComInvoiceLineTaxValidation Validation => (JobComInvoiceLineTaxValidation)base.Validation;

		protected override Customs.Business.JobComInvoiceLineTaxValidation GetNewValidation()
		{
			return new JobComInvoiceLineTaxValidation(this);
		}

		public new JobComInvoiceLineTax Data
		{
			get { return this; }
		}
	}
}
