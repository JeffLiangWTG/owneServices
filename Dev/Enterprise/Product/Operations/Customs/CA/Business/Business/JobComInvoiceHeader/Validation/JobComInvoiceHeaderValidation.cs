using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class JobComInvoiceHeaderValidation : Customs.Business.InvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}
		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		protected override Customs.Business.ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new ExternalMessageValidation(Parent);
		}

		protected override void AddWarningOrMessageErrorToJZ_InvoiceCurrExRateInfoWhenExchangeRateStale(ZPropertyInfo exchangeRateInfo)
		{
			exchangeRateInfo.AddMessageError(ExchangeRateOutOfDateWarningMessage);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTotalValueForDuty();
		}

		public void ValidateTotalValueForDuty()
		{
			ValidateCalculatedProperty(Parent.TotalValueForDutyInfo);
		}

		protected virtual void CheckTotalValueForDuty()
		{
		}

		protected override void CheckJZ_InvoiceNumber()
		{
			base.CheckJZ_InvoiceNumber();
			if (Parent.JZ_InvoiceNumber.HasCharactersNotSupportedByCAMessaging())
			{
				Parent.JZ_InvoiceNumberInfo.AddWarning(Res.GetString("fde5f9a0-038f-4cd5-a342-65e10d0f3888", "The Invoice Number has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs."));
			}
		}
	}
}
