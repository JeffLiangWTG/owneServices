using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Client.EDI.Billing.Business.EdiPriceHeaderLinkVolumeCodeList;

namespace Enterprise.Client.EDI.Billing.Business
{
	[DependentBusinessObject(typeof(ClientLicencePriceHeader), "ExchangeRates")]
	[System.CodeDom.Compiler.GeneratedCode("CargoWise.EntityFramework", "1.0")]
	public class EdiPriceHeaderExchangeRate : AutoEdiPriceHeaderExchangeRate
	{
		public EdiPriceHeaderExchangeRate(BusinessObjectFactory Factory, DataRow Row) : base(Factory, Row)
		{
		}

		[List("Lookups.GroupCodes")]
		public override ZString PHE_GroupCode { get => base.PHE_GroupCode; set => base.PHE_GroupCode = value; }

		public ZString PHE_Calc_GroupCodeDescription => Lookups.GroupCodes.GetDescriptionFromCode(PHE_GroupCode);

		public ZPropertyInfo PHE_Calc_GroupCodeDescriptionInfo => GetZPropertyInfo("PHE_Calc_GroupCodeDescription");

		public ClientLicencePriceHeader PriceHeader => Factory.Load<ClientLicencePriceHeader>(PHE_L6);

		public void CalculatePriceAndLicenceUnits(ClientLicencePriceItem priceItem, EdiPriceHeaderLink link, bool isHighVolumeApplicable, out decimal price, out decimal licenceUnits)
		{
			/*
Formula is:
Adjusted USD Price = {Base USD Price} * (Volume Factor) * (Core Uplift Factor)
Unrounded Amount in {Currency} = {Adjusted USD Price} * {Currency Exchange Rate} * {Cross Currency Uplift Factor}
Rounded Amount in {Currency} = PriceRounding({Unrounded Amount})
			 */

			//{Base USD Price}
			price = priceItem.L7_Price;
			licenceUnits = priceItem.L7_LicenceUnits;

			if (link != null)
			{
				//{Volume Factor}
				if ((link.PHL_VolumeCode == Codes.LV && priceItem.L7_IsVolumeAdjustmentEligible) || (link.PHL_VolumeCode == Codes.HV && isHighVolumeApplicable))
				{
					if (link.PHL_VolumePercent != 0m && link.PHL_VolumePercent != 100m)
					{
						price *= link.PHL_VolumePercent / 100m;
						licenceUnits *= link.PHL_VolumePercent / 100m;
					}
				}

				//{Core Uplift Factor}
				if (link.PHL_CoreUpliftPercent != 0m && priceItem.L7_Code == DatabaseUsage.ActiveUsersUsageCode && link.PHL_CorePackCode == EdiPriceHeaderLinkCorePackCodeList.Codes.UP)
				{
					price *= 1m + link.PHL_CoreUpliftPercent / 100m;
					licenceUnits *= 1m + link.PHL_CoreUpliftPercent / 100m;
				}
			}

			//{Currency Exchange Rate}
			price *= PHE_Rate;

			//{Cross Currency Uplift Factor}
			if (PHE_UpliftPercent != 0m)
			{
				price *= 1m + PHE_UpliftPercent / 100m;
			}

			price = PriceRounding.Round(priceItem.Parent.PriceRoundingParams, price);

			licenceUnits = Utilities.Round(licenceUnits, 1);
		}
	}
}


