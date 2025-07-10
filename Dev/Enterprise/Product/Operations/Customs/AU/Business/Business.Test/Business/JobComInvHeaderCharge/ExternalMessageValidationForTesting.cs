using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class ExternalMessageValidationForTesting : ExternalMessageValidation
	{
		public ExternalMessageValidationForTesting(BusinessObject bizObj)
			: base(bizObj)
		{
		}

		public bool ValidateEachWorkingDayHasAnExchangeRate_Exposed
		{
			get { return ValidateEachWorkingDayHasAnExchangeRate; }
		}

		public string GetAdviceHowToFixNoValidExchangeRates_Exposed()
		{
			return GetAdviceHowToFixNoValidExchangeRates();
		}
	}
}
