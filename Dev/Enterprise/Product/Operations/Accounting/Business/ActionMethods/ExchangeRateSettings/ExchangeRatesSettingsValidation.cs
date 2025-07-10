using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business
{
	public class ExchangeRatesSettingsValidation : AutoExchangeRatesSettingsValidation
	{
		public ExchangeRatesSettingsValidation(AutoExchangeRatesSettings parent)
			: base(parent)
		{
		}

		protected override void CheckExchangeRatesSource()
		{
			base.CheckExchangeRatesSource();
			MandatoryValidation.CheckEntered(Parent.ExchangeRatesSourceInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ExchangeRatesSourceInfo, Parent.ExchangeRatesSource_List);
		}

		#region Implementation

		public new ExchangeRatesSettings Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ExchangeRatesSettings)base.Parent; }
		}

		#endregion
	}
}

