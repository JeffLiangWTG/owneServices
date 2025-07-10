using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing.Hosting
{
	public class HostingUsage : SystemUsage, IPriceItemUsage
	{
		public HostingUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart, ZString code, ZString subCode, ZInt rawUnitCount)
			: base(factory, user, periodStart)
		{
			this.systemCode = code;
			this.rawUnitCount = rawUnitCount;
			this.SubCode = subCode;
		}

		public HostingUsage(BusinessObjectFactory factory, ClientChargeableUsage chargeableUsage)
			: this(factory, new UsingParty(chargeableUsage), chargeableUsage.U1_PeriodStart, chargeableUsage.U1_Code, chargeableUsage.U1_SubCode, chargeableUsage.U1_UnitCountAsInt)
		{
			ChargeableUsagePKs.Add(chargeableUsage.PK);
		}

		readonly ZInt rawUnitCount;

		public override ZString SystemCode
		{
			get { return systemCode; }
		}
		readonly ZString systemCode;

		void CalculateUnitCount()
		{
			isUnitCountCalculated = true;
			unitCount = rawUnitCount;

			if (PriceItem != null)
			{
				unitCount = ConvertToFeeTypeUnits(PriceItem.L7_FeeType, rawUnitCount);
			}
		}

		public static int ConvertToFeeTypeUnits(string feeType, int rawUnitCount)
		{
			int unitCount = rawUnitCount;

			if (feeType == BillingConstants.FeeType.Per10GBPerMonthMin1GB)
			{
				if (rawUnitCount > BillingConstants.Hosting.MBperGB)
				{
					const int MBperUnit = 10 * BillingConstants.Hosting.MBperGB;
					unitCount = (rawUnitCount + MBperUnit - 1) / MBperUnit;
				}
				else
				{
					unitCount = 0;
				}
			}
			else if (feeType == BillingConstants.FeeType.PerGBPerMonth)
			{
				unitCount = (rawUnitCount + BillingConstants.Hosting.MBperGB - 1) / BillingConstants.Hosting.MBperGB;
			}
			else if (feeType == BillingConstants.FeeType.PerMBPerMonthMin1GB)
			{
				unitCount = Math.Max(0, rawUnitCount - BillingConstants.Hosting.MBperGB);
			}
			else if (feeType == BillingConstants.FeeType.PerGBPerMonthMin1GB)
			{
				unitCount = Math.Max(0, rawUnitCount - 1);
			}

			return unitCount;
		}

		public ClientLicencePriceItem PriceItem
		{
			get { return (PriceHeader != null) ? priceItem : null; }
		}

		public override void SetPreviewOnly(ClientLicencePriceHeader previewPriceHeader)
		{
			if (BillingConstants.PriceHeaderType.ODM == previewPriceHeader.L6_SystemCode)
			{
				if (priceHeader != null && priceHeader.PK != previewPriceHeader.PK)
				{
					throw new InvalidOperationException("Attempt to set preview price header after normal price header already calculated");
				}
				priceHeader = previewPriceHeader;
				priceItem = priceHeader.LocalOrStandardItems.FindByCode(SubCode);
				isPriceCalculated = true;
			}
		}

		public override ClientLicencePriceHeader PriceHeader
		{
			get
			{
				if (!isPriceCalculated)
				{
					isPriceCalculated = true;
					if (PeriodStart.IsValid && LicCompany != null && !SubCode.IsEmpty)
					{
						ZDateTime midMonth = new DateTime(PeriodStart.Year, PeriodStart.Month, 15);
						var invoiceDelivery = InvoiceDelivery;
						if (invoiceDelivery == null || !invoiceDelivery.L9_UseParentPrices)
						{
							priceHeader = LicCompany.OnDemandPriceHeaderForDate(midMonth);
						}
						if (priceHeader != null)
						{
							priceItem = priceHeader.LocalOrStandardItems.FindByCode(SubCode);
						}
						if (priceItem == null)
						{
							if (InvoicedOrganisationPK != OrganisationPK && InvoicedLicCompany != null)
							{
								priceHeader = InvoicedLicCompany.OnDemandPriceHeaderForDate(midMonth);
								if (priceHeader != null)
								{
									priceItem = priceHeader.LocalOrStandardItems.FindByCode(SubCode);
								}
							}
						}
						if (priceItem == null)
						{
							priceHeader = null;
						}
					}
				}

				return priceHeader;
			}
		}
		ClientLicencePriceHeader priceHeader;
		ClientLicencePriceItem priceItem;
		bool isPriceCalculated;

		public override ZInt UnitCount
		{
			get
			{
				if (!isUnitCountCalculated)
				{
					CalculateUnitCount();
				}
				return unitCount;
			}
		}
		ZInt unitCount;
		bool isUnitCountCalculated;

		public override ZDecimal UnitPrice
		{
			get { return PriceItem != null ? PriceItem.L7_Price : ZDecimal.Zero; }
		}

		public override SummarySection[] GetGeneralSummarySections()
		{
			return Array.Empty<SummarySection>();
		}

		public string GetInvoiceLineDescription()
		{
			string unitAmountText;
			string unitDescription;

			if (PriceItem.L7_FeeType == BillingConstants.FeeType.Per10GBPerMonthMin1GB
				|| PriceItem.L7_FeeType == BillingConstants.FeeType.PerGBPerMonth)
			{
				decimal gb = rawUnitCount / (decimal)BillingConstants.Hosting.MBperGB;
				unitAmountText = gb.ToString("#,##0.000", CultureInfo.InvariantCulture);
				unitDescription = "GB";
			}
			else if (PriceItem.L7_FeeType == BillingConstants.FeeType.PerMBPerMonthMin1GB)
			{
				unitAmountText = rawUnitCount.ToString();
				unitDescription = "MB";
			}
			else if (PriceItem.L7_FeeType == BillingConstants.FeeType.PerGBPerMonthMin1GB)
			{
				unitAmountText = rawUnitCount.ToString();
				unitDescription = "GB";
			}
			else
			{
				unitAmountText = UnitCount.ToString();
				unitDescription = "unit(s)";
			}

			string feeDescription = SystemCode == BillingConstants.BillingSystem.WiseCloudUser
				? ""
				: " " + BillingConstants.GetCachedFeeTypeList(Factory).GetDescriptionFromCode(PriceItem.L7_FeeType);

			return PriceItem.L7_DescriptionLocalized.Trim() +
				" (" + Organisation.OH_Code + (ServerCode.IsEmpty ? "" : '-' + ServerCode) + ' ' + PeriodStartAsText + ")\r\n" +
				unitAmountText + ' ' + unitDescription + " @ " + CurrencyCode + ' ' + UnitPrice.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture) + feeDescription;
		}
	}
}

