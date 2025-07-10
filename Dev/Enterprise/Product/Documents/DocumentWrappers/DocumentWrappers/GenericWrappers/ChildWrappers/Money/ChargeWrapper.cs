using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("Charge")]
	public sealed class ChargeWrapper : GenericWrapper
	{
		public ChargeWrapper(Charge charge, BusinessObjectFactory factory, OrgHeader carrier = null)
			: base(charge, factory)
		{
			this.charge = charge;
			Carrier = carrier ?? charge.CostAccount;
		}

		public ZString CalculationDescription
		{
			get
			{
				if (DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.Value != ShowCalculationDescriptionOnOneOffQuotesCode.Yes)
				{
					return ZString.Empty;
				}

				var descriptionText = ORtfTextUtil.RtfToText(charge.RevenueCalculationDescription).Trim();
				if (string.IsNullOrWhiteSpace(descriptionText) || !descriptionText.Contains(": ", StringComparison.OrdinalIgnoreCase))
				{
					return ZString.Empty;
				}

				var descriptionLines = descriptionText.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None);

				var descriptionBuilder = new ZStringBuilder();

				foreach (var line in descriptionLines)
				{
					if (!string.IsNullOrWhiteSpace(line))
					{
						descriptionBuilder.Append(" " + line.Trim());
					}
					else
					{
						break;
					}
				}

				descriptionLines = descriptionBuilder.ToString().Split(':');

				if (descriptionLines.Length > 1)
				{
					var result = " " + descriptionBuilder.ToString().Split(':')[1];
					return !Description.Contains(result.Trim())
						? result
						: string.Empty;
				}

				return ZString.Empty;
			}
		}

		public CodeAndDescriptionWrapper ChargeCode
		{
			get
			{
				if (chargeCode == null)
				{
					AccChargeCode ac = charge.ChargeCode;
					chargeCode = new CodeAndDescriptionWrapper(ac == null ? ZString.Empty : ac.AC_Code, charge.Lookups.ChargeCodes, Factory);
				}

				return chargeCode;
			}
		}
		CodeAndDescriptionWrapper chargeCode;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Manage the multilingual manually")]
		public ZString Description
		{
			get
			{
				switch (DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.Value)
				{
					case ShowCalculationDescriptionOnOneOffQuotesCode.Yes:
					case ShowCalculationDescriptionOnOneOffQuotesCode.No:
						return DocWrapperUtilities.GetMultilingualOrDefaultDescription(charge.JR_Desc, charge.ChargeCode);
					case ShowCalculationDescriptionOnOneOffQuotesCode.Charge:
						return charge.ChargeCode != null
							? (ZString)DocWrapperUtilities.GetMultilingualOrDefaultDescription(charge.ChargeCode.AC_Desc, charge.ChargeCode)
							: ZString.Empty;
					default:
						throw new NotSupportedException($"'{DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.Value}' is not supported.");
				}
			}
		}

		public ComponentChargeAmountWrapper LocalSell
		{
			get { return localSell ?? (localSell = new ComponentChargeAmountWrapper(charge.JR_LocalSellAmt, charge.JR_Calc_LocalSellTaxAmt, LocalCurrency, Factory)); }
		}
		ComponentChargeAmountWrapper localSell;

		public ComponentChargeAmountWrapper OSSell
		{
			get { return osSell ?? (osSell = new ComponentChargeAmountWrapper(charge.JR_OSSellAmt, charge.JR_Calc_OSSellGSTAmt, charge.SellCurrency, Factory)); }
		}
		ComponentChargeAmountWrapper osSell;

		public ComponentChargeAmountWrapper LocalCost
		{
			get { return localCost ?? (localCost = new ComponentChargeAmountWrapper(charge.JR_LocalCostAmt, charge.JR_Cost_LocalGSTAmount, LocalCurrency, Factory)); }
		}
		ComponentChargeAmountWrapper localCost;

		public ComponentChargeAmountWrapper OSCost
		{
			get { return osCost ?? (osCost = new ComponentChargeAmountWrapper(charge.JR_OSCostAmt, charge.JR_Calc_OSCostGSTAmt, charge.CostCurrency, Factory)); }
		}
		ComponentChargeAmountWrapper osCost;

		public ZString SellRate => charge.JR_OSSellExRate.ToString(charge.ExchangeRateDecimalPlaces);

		public OrgHeader Carrier { get; }

		CarrierInformation CarrierInfo => carrierInfo ?? (carrierInfo = new CarrierInformation(charge));
		CarrierInformation carrierInfo;

		public ZString CarrierTransitTime => CarrierInfo.TransitTime;

		public ZString CarrierFrequency => CarrierInfo.Frequency.ToString();

		public ZString CarrierFrequencyUnit => CarrierInfo.FrequencyUnit;

		#region Internal

		RefCurrency LocalCurrency
		{
			get { return charge.Job.Company.LocalCurrency; }
		}

		readonly Charge charge;

		#endregion

		class CarrierInformation
		{
			public CarrierInformation(Charge charge)
			{
				Carrier = charge.CostAccount;

				if (charge.ShipmentInfo is QuotedBooking quotedBooking)
				{
					if (Carrier.PK == quotedBooking.OH_Carrier || Carrier.PK == quotedBooking.Creditor)
					{
						TransitTime = quotedBooking.TransitTime;
						Frequency = quotedBooking.Frequency;
						FrequencyUnit = quotedBooking.FrequencyUnit;
					}
					else
					{
						var possibleCarrier = quotedBooking.Quote.CurrentOneOffQuote.PossibleCarriers.Cast<RateOneOffCarrier>().FirstOrDefault(x => x.TTC_OH_Carrier == Carrier.PK || x.TTC_OH_Creditor == Carrier.PK);
						if (possibleCarrier != null)
						{
							TransitTime = possibleCarrier.TTC_TransitTime;
							Frequency = possibleCarrier.TTC_Frequency;
							FrequencyUnit = possibleCarrier.TTC_FrequencyUnit;
						}
					}
				}
			}

			public OrgHeader Carrier { get; }

			public string TransitTime { get; }

			public int Frequency { get; }

			public string FrequencyUnit { get; }
		}
	}
}
