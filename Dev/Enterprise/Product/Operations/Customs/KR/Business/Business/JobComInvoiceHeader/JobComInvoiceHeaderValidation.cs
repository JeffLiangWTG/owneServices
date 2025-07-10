using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class JobComInvoiceHeaderValidation : AutoKRJobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}
		public JobComInvoiceHeader InvoiceHeader
		{
			get { return Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTotalInvoiceLinesWeightInKG();
		}

		protected override void CheckJZ_InvoiceAmount()
		{
			base.CheckJZ_InvoiceAmount();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JZ_InvoiceAmountInfo);
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;
		protected override NotificationTypes ErrorTypeForNoOfPacks => NotificationTypes.MessageError;

		public static string NotEnteredOrgAddressMessage => Res.GetString("475BF0EC-03E7-4853-B3FC-C5A7DF9A43BD", "Please select an address for the entered organization.");
		public static string MissingCompanyNameMessage => Res.GetString("9812A613-D152-430E-BAF4-93AF97DF3CFB", "The name of this company is missing. Press F3 here and enter the company name.");
		public static string MissingRepresentativeMessage => Res.GetString("86712770-40B3-4C57-BDEC-21C8CAB3896E", "The name of this company's representative is missing. Please press F3 here and add a 'KRC' Allocation setting to this company's representative on the tab Contact > Allocated Contact.");
		public static string MissingCountryCodeMessage => Res.GetString("C33AD388-F609-4891-BEC5-0F8CD8CC1D3B", "There is no 'Country Code' for this organization. Please press F3 here and add Country/Region on the Organization form.");
		public static string GetMissingRegistrationNumberMessage(string registrationNumberType, string registrationNumberCode, string location = "here")
		{
			return Res.GetString("70F9F836-4ABF-47D5-9323-EEF72A26A984", "There is no {0} for this organization. Please press F3 {1} and add a number of type '{2}' in Config > Registration Numbers/Codes on the Organization form.", registrationNumberType, location, registrationNumberCode);
		}
		public static string GetWarningMessageAboutToSendDefaultValue(string organizationType, string registrationNumberType, string defaultValue)
		{
			return Res.GetString("FF0FE907-FE89-4F85-BAE0-3BA6F29F6BF3", "Please check if this {0} has a {1} assigned. As there is no {1}, '{2}' will be sent in a declaration.", organizationType, registrationNumberType, defaultValue);
		}

		protected override void CheckJZ_InvoiceCurrExRate()
		{
			base.CheckJZ_InvoiceCurrExRate();
			Parent.JZ_InvoiceCurrExRateInfo.CheckCurrencyAndRate(Parent.JobDeclaration, Parent.JZ_RX_NKInvoice_Currency);
		}

		public static string GetErrorMessageAboutToEnteredIndividualOrganization(string organization)
		{
			return Res.GetString("F908D1DA-83A8-4286-9CF5-10D6F6D817ED", "The {0} must not be an individual.", organization);
		}

		protected override void CheckJZ_Calc_CIFAmount_ZeroFreightInsurance()
		{
		}

		public void ValidateTotalInvoiceLinesWeightInKG()
		{
			ValidateCalculatedProperty(Parent.TotalInvoiceLinesWeightInKGInfo);
		}

		protected void CheckTotalInvoiceLinesWeightInKG()
		{
			CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.TotalInvoiceLinesWeightInKGInfo);
		}

		protected override void CheckJZ_ValuationCode()
		{
			base.CheckJZ_ValuationCode();
			if (Parent.Is5SM)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_ValuationCodeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_ValuationCodeInfo);
		}
	}
}
