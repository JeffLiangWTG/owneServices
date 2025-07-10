namespace Enterprise.Customs.JP.Business
{
	public static class ValidationConstants
	{
		public static class Charge
		{
			public static string CurrencyIsDifferentToInvoice => Res.GetString("13A55ED0-A2B9-4EC4-80F6-82D07D34AD4A", "The charge currency is different to the invoice amount currency.");
		}

		public static string SpecialMandatoryErrorMessage(string propertyCaption) => Res.GetString("5411CDCA-3FD7-4B2C-96AE-FA2BA8BAE43B", "You have not entered {0}. You can leave it empty if you are sure it is known to customs. Or the message may be rejected.", propertyCaption);
	}
}
