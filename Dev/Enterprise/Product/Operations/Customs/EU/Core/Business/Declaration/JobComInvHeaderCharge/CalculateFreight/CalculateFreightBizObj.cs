using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CalculateFreightBizObj : AutoCalculateFreightBizObj
	{
		public CalculateFreightBizObj(IJobComInvChargeCollection<JobComInvCharge> charges, EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory, ZString currency, ZString country, ZDateTime dateOfValuation, ZString iataLoadPort)
			: base(new BusinessObjectFactory())
		{
			this.charges = Argument.NotNull(charges, nameof(charges));
			this.euIncoTermAndChargeFactory = Argument.NotNull(euIncoTermAndChargeFactory, nameof(euIncoTermAndChargeFactory));
			Currency = currency;
			Percentage = GetDefaultPercentage(charges.Factory, country, dateOfValuation, iataLoadPort);
		}

		protected readonly IJobComInvChargeCollection<JobComInvCharge> charges;
		protected readonly EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory;

		public static TCalculateFreightBizObj New<TCalculateFreightBizObj>(IJobComInvChargeCollection<JobComInvCharge> charges, JobDeclaration declaration)
			where TCalculateFreightBizObj : CalculateFreightBizObj
		{
			TCalculateFreightBizObj result = null;
			if (charges != null && declaration != null)
			{
				var euIncoTermAndChargeFactory = declaration.IncoTermAndChargeFactory as EUIncoTermAndCustomsChargeFactory;
				if (euIncoTermAndChargeFactory != null)
				{
					result = (TCalculateFreightBizObj)Activator.CreateInstance(
						typeof(TCalculateFreightBizObj),
						charges,
						euIncoTermAndChargeFactory,
						declaration.LocalCurrencyCode,
						declaration.CountryCode,
						declaration.DateOfValuation,
						declaration.JE_IATALoadPort
					);
					result.IsFreightIncludedInLines = charges.Cast<JobComInvCharge>().FirstOrDefault(c => c.J7_ChargeType == euIncoTermAndChargeFactory.FreightToEUBorderCode)?.J7_IsIncludedInITOT ?? false;

					var chargeTypesToDefault = result.GetChargeTypesToDefault();
					var transportCharges = charges.Cast<JobComInvCharge>().Where(x => x.J7_ChargeType.In(chargeTypesToDefault) && !x.J7_ChargeType.In(result.GetChargeTypesToExcludeFromAmount()));
					if (transportCharges.Any())
					{
						var firstTransportChargeCurrency = transportCharges.First().Currency;
						if (firstTransportChargeCurrency != null && transportCharges.All(x => x.Currency == firstTransportChargeCurrency))
						{
							result.Currency = firstTransportChargeCurrency.RX_Code;
						}
						else
						{
							result.Currency = string.Empty;
						}
					}
				}
			}
			return result;
		}

		public static CalculateFreightBizObj New(IJobComInvChargeCollection<JobComInvCharge> charges, JobDeclaration declaration) => New<CalculateFreightBizObj>(charges, declaration);

		protected override ZDecimal GetAmountAfterEUBorder()
		{
			return Amount - GetDutiableAmount();
		}

		protected override ZDecimal GetAmountToEUBorder()
		{
			return GetDutiableAmount();
		}

		ZDecimal GetDutiableAmount() => ZArchitecture.Core.Utilities.Round(Amount * Percentage / 100, 2);

		ZDecimal GetDefaultPercentage(BusinessObjectFactory factory, ZString country, ZDateTime dateOfValuation, ZString iataLoadPort)
		{
			ZDecimal result = 100m;
			if (!iataLoadPort.IsEmpty)
			{
				var percentages = RefCusCodeListAttributeTypes.GetAttributeValuesFor(factory, country, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, dateOfValuation, iataLoadPort, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Percentage);
				if (percentages != null && percentages.Length == 1)
				{
					result = ZDecimal.ParseSafe(percentages[0], ZDecimal.Zero);
				}
			}
			return result;
		}

		[List(nameof(CurrencyList))]
		[ResourceStringData("CalculateFreightBizObj|Currency", Caption = "Currency")]
		public override ZString Currency { get => base.Currency; set => base.Currency = value; }

		public RefCurrencyCollection CurrencyList => new RefCurrencyCollection(charges.Factory);

		[ResourceStringData("CalculateFreightBizObj|Percentage", Caption = "% before EU Border")]
		public override ZDecimal Percentage { get => base.Percentage; set => base.Percentage = value; }

		public bool Calculate()
		{
			var result = false;
			RunPreSaveValidation();
			if (!HasErrors)
			{
				var existingCharges = charges.OfType<JobComInvCharge>().Where(x => GetChargeTypesToCalculate().Contains(x.J7_ChargeType)).ToArray();
				result = GetCalculateResult();
				if (result)
				{
					existingCharges.DeleteAll();
				}
			}
			return result;
		}

		[ResourceStringData("CalculateFreightBizObj|Amount", Caption = "Amount")]
		public override ZDecimal Amount { get => GetAmountOfFreight(); set => base.Amount = value; }

		ZDecimal GetAmountOfFreight()
		{
			var chargeTypesToDefault = GetChargeTypesToDefault();
			var chargesNeeded = charges.Cast<JobComInvCharge>().ToList().Where(x => x.J7_ChargeType.In(chargeTypesToDefault) && !x.J7_ChargeType.In(GetChargeTypesToExcludeFromAmount()));
			if (base.Amount == ZDecimal.Zero && chargesNeeded.Any())
			{
				var amount = ZDecimal.Zero;

				if (chargesNeeded.All(x => x.Currency == chargesNeeded.First().Currency))
				{
					foreach (var charge in chargesNeeded)
					{
						amount += charge.J7_Amount;
					}
				}

				return amount;
			}
			else
			{
				return base.Amount;
			}
		}

		protected virtual ZString[] GetChargeTypesToDefault() => GetChargeTypesToCalculate();

		protected virtual ZString[] GetChargeTypesToCalculate()
		{
			return new ZString[] { euIncoTermAndChargeFactory.FreightToEUBorderCode, euIncoTermAndChargeFactory.FreightAfterEUBorderCode };
		}

		protected virtual ZString[] GetChargeTypesToExcludeFromAmount()
		{
			return new ZString[] { CustomsChargeTypeList.Codes.OverseasInsurance };
		}

		protected virtual bool GetCalculateResult()
		{
			euIncoTermAndChargeFactory.ShouldCreatedFreightChargeBeIncludedInITOT = IsFreightIncludedInLines;

			var result = false;
			if (!AmountToEUBorder.IsEmpty)
			{
				euIncoTermAndChargeFactory.SetupToEUBorderCharge(charges.AddNew(), AmountToEUBorder, Currency);
				result = true;
			}
			if (!AmountAfterEUBorder.IsEmpty)
			{
				euIncoTermAndChargeFactory.SetupAfterEUBorderCharge(charges.AddNew(), AmountAfterEUBorder, Currency);
				result = true;
			}
			return result;
		}

		[ResourceStringData("EU|CalculateFreightBizObj|IsFreightIncludedInLines", Caption = "Freight Included in Lines")]
		public override ZBool IsFreightIncludedInLines { get => base.IsFreightIncludedInLines; set => base.IsFreightIncludedInLines = value; }
	}
}
