using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	public partial class CreditTemporaryIncreaseAuthorisationSettingsControl : AmountOrPercentageAuthorisationSettingsControl
	{
		public CreditTemporaryIncreaseAuthorisationSettingsControl()
		{
			InitializeComponent();
			InitializeAdditionColumnsForTemporaryIncrease();
		}

		void InitializeAdditionColumnsForTemporaryIncrease()
		{
			var daysToExpiryColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			daysToExpiryColumnStyleInfo.BindToDecimalPlaces = null;
			daysToExpiryColumnStyleInfo.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("PaymentAuthorisationSettingsControl|89f8c862-02d0-4c98-ba43-a8829b81a084", "Days To Expiry");
			daysToExpiryColumnStyleInfo.ColumnName = CreditTemporaryIncreaseAuthorisationSettings.Schema.DaysToExpiry;
			daysToExpiryColumnStyleInfo.Decimals = 0;
			PaymentAuthorisationSettingsGrid.ColumnStyles.Add(daysToExpiryColumnStyleInfo);
		}
	}
}
