using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class AddPriceCurrencyForm : ZChildForm
	{
		public AddPriceCurrencyForm()
		{
			InitializeComponent();
		}

		public AddPriceCurrencyForm(PriceCurrencyUpdater updater)
			: base(updater)
		{
			InitializeComponent();
		}

		void OkButton_Click(object sender, EventArgs e)
		{
			var bizObj = (PriceCurrencyUpdater)BusinessEntity;
			bizObj.RunPreSaveValidation();

			if (bizObj.HasErrors)
			{
				Globals.Message.ShowError(new ZNotificationCollector(bizObj, true, false, ZNotificationCollector.PropertyDescriptionType.None).ToMessageListString());
			}
			else
			{
				DialogResult = System.Windows.Forms.DialogResult.OK;
				Close();
			}
		}
	}

	public class PriceCurrencyUpdater : NonPersistentBusinessObject, IObsoleteValidation
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public PriceCurrencyUpdater(BusinessObjectFactory factory)
			: base(factory)
		{
			Rates = new PriceExchangeRateCollection(factory);
			Rounding = new PriceRoundingCollection(factory);
			Restore();

			RegisterEditableChildObject(Rates);
			RegisterEditableChildObject(Rounding);
		}

		public void ApplyTo(IEnumerable<ClientLicencePriceItem> items)
		{
			Persist();

			foreach (var item in items)
			{
				ApplyTo(item);
			}
		}

		void Persist()
		{
			SavedRates = Rates;
			SavedRounding = Rounding;
		}

		void Restore()
		{
			if (SavedRates != null)
			{
				foreach (PriceExchangeRate item in SavedRates)
				{
					AddRate(item.CurrencyCode, item.Rate, item.Uplift);
				}
			}

			if (SavedRounding != null)
			{
				foreach (PriceRounding item in SavedRounding)
				{
					AddRounding(item.PriceBreak, item.RoundingScale);
				}
			}
			else
			{
				AddRounding(5m, 0.01m);
				AddRounding(50m, 0.1m);
				AddRounding(0m, 5);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static fields in this class do not need to be thread-static")]
		static PriceExchangeRateCollection SavedRates { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static fields in this class do not need to be thread-static")]
		static PriceRoundingCollection SavedRounding { get; set; }

		void ApplyTo(ClientLicencePriceItem item)
		{
			if (item.L7_Price != 0)
			{
				foreach (PriceExchangeRate rate in Rates)
				{
					decimal price = item.L7_Price * rate.Rate * (100 + rate.Uplift) / 100m;
					price = PriceRounding.Round(Rounding.Cast<PriceRounding>(), price);

					var itemRate = item.CurrencyRates.FirstOrDefault(x => x.PIR_RX_NKCurrency.EqualsIgnoringCase(rate.CurrencyCode));
					if (itemRate == null)
					{
						itemRate = item.CurrencyRates.AddNew();
						itemRate.PIR_RX_NKCurrency = rate.CurrencyCode.ToUpperInvariant();
					}

					itemRate.PIR_Price = price;
				}
			}
		}

		[ChildEditable()]
		public PriceExchangeRateCollection Rates { get; private set; }

		[ChildEditable()]
		public PriceRoundingCollection Rounding { get; private set; }

		public ZDecimal RoundToNearest
		{
			get { return roundToNearest; }
			set { SetNonPersistentPropertyValue(RoundToNearestInfo, ref roundToNearest, value); }
		}
		ZDecimal roundToNearest;

		public ZPropertyInfo RoundToNearestInfo
		{
			get { return GetZPropertyInfo(nameof(RoundToNearest)); }
		}

		protected override void SetDefaultValues()
		{
			RoundToNearest = 5;
		}

		void AddRounding(decimal priceBreak, decimal roundingScale)
		{
			var r = Rounding.AddNew();
			r.PriceBreak = priceBreak;
			r.RoundingScale = roundingScale;
		}

		void AddRate(string currency, decimal rate, decimal uplift)
		{
			var r = Rates.AddNew();
			r.CurrencyCode = currency;
			r.Rate = rate;
			r.Uplift = uplift;
		}
	}

	public class PriceExchangeRate : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PriceExchangeRate(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZString CurrencyCode
		{
			get { return currencyCode; }
			set { SetNonPersistentPropertyValue(CurrencyCodeInfo, ref currencyCode, value); }
		}
		ZString currencyCode;

		public ZPropertyInfo CurrencyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyCode)); }
		}

		public ZDecimal Rate
		{
			get { return rate; }
			set { SetNonPersistentPropertyValue(RateInfo, ref rate, value); }
		}
		ZDecimal rate;

		public ZPropertyInfo RateInfo
		{
			get { return GetZPropertyInfo(nameof(Rate)); }
		}

		public ZDecimal Uplift
		{
			get { return uplift; }
			set { SetNonPersistentPropertyValue(UpliftInfo, ref uplift, value); }
		}
		ZDecimal uplift;

		public ZPropertyInfo UpliftInfo
		{
			get { return GetZPropertyInfo(nameof(Uplift)); }
		}

		protected override void SetDefaultValues()
		{
			uplift = 3m;
		}
	}

	public class PriceExchangeRateCollection : NonPersistentBusinessObjectCollection<PriceExchangeRate>
	{
		public PriceExchangeRateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PriceExchangeRate(Factory);
		}

		protected override bool AllowNewCore
		{
			get
			{
				return true;
			}
		}
	}

	public class PriceRoundingCollection : NonPersistentBusinessObjectCollection<PriceRounding>
	{
		public PriceRoundingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PriceRounding(Factory);
		}
	}
}


