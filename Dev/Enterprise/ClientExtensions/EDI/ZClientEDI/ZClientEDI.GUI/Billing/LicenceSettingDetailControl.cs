using System;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class LicenceSettingDetailControl : ZUserControl
	{
		public LicenceSettingDetailControl()
		{
			InitializeComponent();
		}

		ZUserControl dynamicSettingControl;
		ZString currentType;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			UpdateDynamicControl();
		}

		void UpdateDynamicControl()
		{
			var setting = (EdiLicenceSetting)CurrentDataItem;

			if ((setting == null || currentType != setting.LS9_Type)
				&& dynamicSettingControl != null)
			{
				dynamicSettingControl.Dispose();
				dynamicSettingControl = null;
				currentType = ZString.Empty;
			}

			if (setting != null)
			{
				if (currentType != setting.LS9_Type)
				{
					dynamicSettingControl = CreateDynamicControl(setting);

					if (dynamicSettingControl != null)
					{
						dynamicSettingControl.SetDataBinding(setting, "");
						dynamicSettingControl.Dock = System.Windows.Forms.DockStyle.Fill;
						detailGroupBox.Controls.Add(dynamicSettingControl);
					}

					currentType = setting.LS9_Type;
				}
				else if (dynamicSettingControl != null)
				{
					dynamicSettingControl.SetDataBinding(setting, "");
				}
			}
		}

		static ZUserControl CreateDynamicControl(EdiLicenceSetting setting)
		{
			ZUserControl result;
			switch ((string)setting.LS9_Type)
			{
				case BillingConstants.LicenceSetting.Discount:
					result = new DiscountSettingControl();
					break;
				case BillingConstants.LicenceSetting.BuyingGroup:
					result = new BuyingGroupSettingControl();
					break;
				case BillingConstants.LicenceSetting.Price:
					result = new PriceSettingControl();
					break;
				case BillingConstants.LicenceSetting.PriceTier:
					result = new PriceTierSettingControl();
					break;
				case BillingConstants.LicenceSetting.Commitment:
					result = new CommitmentSettingControl();
					break;
				case BillingConstants.LicenceSetting.ConversionCredit:
					result = new ConversionCreditSettingControl();
					break;
				case BillingConstants.LicenceSetting.BorderWisePurchasedLicences:
					result = new BorderWisePurchasedLicenceControl();
					break;
				case BillingConstants.LicenceSetting.HighVolumeFeature:
					result = new HighVolumeFeatureSettingControl();
					break;
				case BillingConstants.LicenceSetting.VersionSurcharge:
					result = new VersionSurchargeSettingControl();
					break;
				case BillingConstants.LicenceSetting.MinSpend:
					result = new MinSpendSettingControl();
					break;
				case BillingConstants.LicenceSetting.DiscountSuspensionPolicy:
					result = new DiscountSuspensionPolicySettingControl();
					break;
				case BillingConstants.LicenceSetting.BillingSummaryCurrency:
					result = new BillingSummaryCurrencySettingControl();
					break;
				default:
					throw new ArgumentException("Unknown licence setting type: ", setting.LS9_Type);
			}

			return result;
		}
	}
}
