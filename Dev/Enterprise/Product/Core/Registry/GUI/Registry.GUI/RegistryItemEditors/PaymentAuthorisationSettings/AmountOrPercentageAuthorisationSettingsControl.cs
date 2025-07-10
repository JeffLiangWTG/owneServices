using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	public partial class AmountOrPercentageAuthorisationSettingsControl : PaymentAuthorisationSettingsControl
	{
		public AmountOrPercentageAuthorisationSettingsControl()
		{
			InitializeComponent();
			InitializeAdditionColumnForPercentage();
		}

		void InitializeAdditionColumnForPercentage()
		{
			var percentageColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			percentageColumnStyleInfo.BindToDecimalPlaces = null;
			percentageColumnStyleInfo.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AmountOrPercentageAuthorisationSettingsControl|0abad1cf-e6f0-4f56-94ec-595528ef06fe", "Percentage");
			percentageColumnStyleInfo.ColumnName = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.Schema.Percentage;
			percentageColumnStyleInfo.Decimals = 2;
			PaymentAuthorisationSettingsGrid.ColumnStyles.Insert(2, percentageColumnStyleInfo);
		}

#if DEBUG
		public ZGrid PaymentAuthorisationSettingsGrid_ForTest { get { return PaymentAuthorisationSettingsGrid; } }
#endif
	}
}
