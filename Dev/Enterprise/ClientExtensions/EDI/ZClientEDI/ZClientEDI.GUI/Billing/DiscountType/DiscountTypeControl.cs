using System;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class DiscountTypeControl : ZUserControl
	{
		public DiscountTypeControl()
		{
			InitializeComponent();
		}

		ZUserControl dynamicControl;
		ZString currentType;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			var discount = (EdiPriceHeaderDiscount)CurrentDataItem;
			if (discount != null)
			{
				discount.PHD_TypeInfo.ValueChanged -= PHD_TypeInfo_ValueChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var discount = (EdiPriceHeaderDiscount)CurrentDataItem;
			if (discount != null)
			{
				discount.PHD_TypeInfo.ValueChanged += PHD_TypeInfo_ValueChanged;
			}

			UpdateDynamicControl();
		}

		void PHD_TypeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateDynamicControl();
		}

		void UpdateDynamicControl()
		{
			var discount = (EdiPriceHeaderDiscount)CurrentDataItem;

			if ((discount == null || currentType != discount.PHD_Type)
				&& dynamicControl != null)
			{
				dynamicControl.Dispose();
				dynamicControl = null;
				currentType = ZString.Empty;
			}

			if (discount != null)
			{
				if (currentType != discount.PHD_Type)
				{
					dynamicControl = CreateDynamicControl(discount);

					if (dynamicControl != null)
					{
						dynamicControl.SetDataBinding(discount.Config, "");
						dynamicControl.Dock = System.Windows.Forms.DockStyle.Fill;
						groupBox.Controls.Add(dynamicControl);
					}

					currentType = discount.PHD_Type;
				}
				else if (dynamicControl != null)
				{
					dynamicControl.SetDataBinding(discount.Config, "");
				}
			}
		}

		static ZUserControl CreateDynamicControl(EdiPriceHeaderDiscount discount)
		{
			ZUserControl result;
			switch ((string)discount.PHD_Type)
			{
				case BillingConstants.DiscountCalculator.DevelopingCountry: result = new CountryDiscountControl(); break;
				case BillingConstants.DiscountCalculator.DomesticEntity: result = new DomesticDiscountControl(); break;
				case BillingConstants.DiscountCalculator.Volume: result = new VolumeDiscountControl(); break;
				case BillingConstants.DiscountCalculator.WiseCloud: result = new WiseCloudDiscountControl(); break;
				case BillingConstants.DiscountCalculator.SingleCountry: result = new SingleCountryDiscountControl(); break;
				case BillingConstants.DiscountCalculator.OrgMembership: result = new OrgMembershipDiscountControl(); break;
				case BillingConstants.DiscountCalculator.MasterOrgDevelopingCountry: result = new MasterOrgDevelopingCountryDiscountControl(); break;
				case BillingConstants.DiscountCalculator.ProductBundle: result = new ProductBundleDiscountControl(); break;
				default:
					result = null; break;
			}

			return result;
		}
	}
}
