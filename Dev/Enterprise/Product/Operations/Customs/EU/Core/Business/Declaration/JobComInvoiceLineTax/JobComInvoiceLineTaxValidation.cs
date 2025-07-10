using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceLineTaxValidation : Customs.Business.JobComInvoiceLineTaxValidation
	{
		public JobComInvoiceLineTaxValidation(AutoJobComInvoiceLineTax parent) : base(parent)
		{
		}

		public new JobComInvoiceLineTax Parent => (JobComInvoiceLineTax)base.Parent;

		protected override void CheckJLT_MethodOfPayment()
		{
			base.CheckJLT_MethodOfPayment();
			ListValidation.MessageErrorIfInvalidCode(Parent.JLT_MethodOfPaymentInfo, Parent.Lookups.MOPList);
			CheckTaxTypeAndMop(Parent.JLT_MethodOfPaymentInfo);
			ValidateAllTypeAndMopColumnsForAllRowsOfThisType();
		}

		protected override void CheckJLT_Type()
		{
			base.CheckJLT_Type();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JLT_TypeInfo);
			ValidateAllTypeAndMopColumnsForAllRowsOfThisType();
			MandatoryValidation.CheckEntered(Parent.JLT_TypeInfo); // DB constraint demands this field
		}

		public void ValidateJLT_Calc_RateSuspension()
		{
			ValidateCalculatedProperty(Parent.JLT_Calc_RateSuspensionInfo);
		}

		protected virtual void CheckJLT_Calc_RateSuspension()
		{ }

		public void ValidateJLT_Calc_RateDuty()
		{
			ValidateCalculatedProperty(Parent.JLT_Calc_RateDutyInfo);
		}

		protected virtual void CheckJLT_Calc_RateDuty()
		{ }

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJLT_Calc_RateSuspension();
			ValidateJLT_Calc_RateDuty();
		}

		void ValidateAllTypeAndMopColumnsForAllRowsOfThisType()
		{
			var invLine = Parent?.InvoiceLine;
			if (invLine != null)
			{
				foreach (JobComInvoiceLineTax brotherTax in invLine.Taxes)
				{
					brotherTax.Validation.ValidateJLT_MethodOfPayment();
					brotherTax.Validation.ValidateJLT_Type();
				}
			}
		}

		void CheckTaxTypeAndMop(ZPropertyInfo zPropertyInfoForError)
		{
			var invLine = Parent?.InvoiceLine;
			if (invLine != null)
			{
				if (invLine.Taxes.OfType<JobComInvoiceLineTax>()
					.Any(brotherTax =>
						brotherTax.PK != Parent.PK
						&& brotherTax.JLT_Type == Parent.JLT_Type
						&& brotherTax.JLT_MethodOfPayment == Parent.JLT_MethodOfPayment)
				)
				{
					zPropertyInfoForError.AddMessageError(Res.GetString("612C712B-9DD7-46EF-89BF-96ED69BB59E7", "A row with that tax type and method of payment already exists"));
				}
			}
		}
	}
}
