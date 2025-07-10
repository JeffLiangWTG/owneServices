using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CalculateFreightNonAirBizObj : AutoCalculateFreightNonAirBizObj
	{
		protected CalculateFreightNonAirBizObj(IJobComInvChargeCollection<JobComInvCharge> charges, EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory)
			: base(charges.Factory)
		{
			this.charges = charges;
			this.euIncoTermAndChargeFactory = euIncoTermAndChargeFactory;
		}

		static CalculateFreightNonAirBizObj GetByCountryCode(ZString countryCode, IJobComInvChargeCollection<JobComInvCharge> charges, EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory)
		{
			CalculateFreightNonAirBizObj result;
			if (!countryCode.IsEmpty)
			{
				var types = ObjectFactory.Get<Hashtable>("CalculateFreightNonAirBizObjs");
				var objectHandle = (ObjectHandle)types[countryCode.ToString()];
				result = (CalculateFreightNonAirBizObj)objectHandle?.GetObject(charges, euIncoTermAndChargeFactory) ?? new CalculateFreightNonAirBizObj(charges, euIncoTermAndChargeFactory);
			}
			else
			{
				result = new CalculateFreightNonAirBizObj(charges, euIncoTermAndChargeFactory);
			}
			return result;
		}

		public static CalculateFreightNonAirBizObj New(IJobComInvChargeCollection<JobComInvCharge> charges, JobDeclaration declaration)
		{
			CalculateFreightNonAirBizObj result = null;
			if (declaration?.IncoTermAndChargeFactory is EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory)
			{
				result = GetByCountryCode(declaration.GetDefaultDataGroupingCode(), charges, euIncoTermAndChargeFactory);
				using (result.SuspendSettingHasChanges())
				{
					var chargesofInterest = charges.Where(x => result.ChargeTypesToCalculate.Contains(x.J7_ChargeType));
					if (chargesofInterest.Any())
					{
						var firstTransportChargeCurrency = chargesofInterest.First().Currency;
						var allChargesShareSameCurrency = chargesofInterest.All(x => x.Currency == firstTransportChargeCurrency);
						result.Currency = allChargesShareSameCurrency ? firstTransportChargeCurrency?.RX_Code ?? ZString.Empty : ZString.Empty;
						result.TotalAmount = allChargesShareSameCurrency ? (ZDecimal)chargesofInterest.Sum(x => x.J7_Amount) : ZDecimal.Zero;
					}
					else
					{
						result.Currency = declaration.LocalCurrencyCode;
					}
				}
			}
			return result;
		}

		public bool Calculate()
		{
			RunPreSaveValidation();
			var noErrors = !HasErrors;
			if (noErrors)
			{
				var existingCharges = charges.OfType<JobComInvCharge>().Where(x => ChargeTypesToCalculate.Contains(x.J7_ChargeType)).ToArray();
				var newChargesCreated = GetCalculateResult();
				if (newChargesCreated)
				{
					existingCharges.DeleteAll();
				}
			}
			return noErrors;
		}

		public ImmutableHashSet<ZString> ChargeTypesToCalculate => chargeTypesToCalculate ?? (chargeTypesToCalculate = GetChargeTypesToCalculate());
		ImmutableHashSet<ZString> chargeTypesToCalculate;

		[List(nameof(CurrencyList))]
		[ResourceStringData("CalculateFreightNonAirBizObj|Currency", Caption = "Currency")]
		public override ZString Currency { get => base.Currency; set => base.Currency = value; }

		public RefCurrencyCollection CurrencyList => new RefCurrencyCollection(charges.Factory);

		[ResourceStringData("CalculateFreightNonAirBizObj|TotalAmount", Caption = "Total Amount")]
		public override ZDecimal TotalAmount { get => base.TotalAmount; set => base.TotalAmount = value; }

		[ResourceStringData("CalculateFreightNonAirBizObj|PercentageFreightToEUBorder", Caption = "%Freight to EU Border")]
		public override ZDecimal PercentageFreightToEUBorder { get => base.PercentageFreightToEUBorder; set => base.PercentageFreightToEUBorder = value; }

		[ResourceStringData("CalculateFreightNonAirBizObj|PercentageFreightEUToDestinationCountry", Caption = "%Freight EU to Destination Country")]
		public override ZDecimal PercentageFreightEUToDestinationCountry { get => base.PercentageFreightEUToDestinationCountry; set => base.PercentageFreightEUToDestinationCountry = value; }

		[ResourceStringData("CalculateFreightNonAirBizObj|PercentageFreightToFinalDestination", Caption = "%Freight to Final Destination")]
		public ZDecimal PercentageFreightToFinalDestination => 100m - PercentageFreightToEUBorder - PercentageFreightEUToDestinationCountry;

		[ResourceStringData("CalculateFreightNonAirBizObj|AmountToEUBorder", Caption = "Amount to EU Border")]
		public override ZDecimal AmountToEUBorder => base.AmountToEUBorder;

		[ResourceStringData("CalculateFreightNonAirBizObj|AmountToDestinationCountry", Caption = "Amount to Destination Country")]
		public override ZDecimal AmountToDestinationCountry => base.AmountToDestinationCountry;

		[ResourceStringData("CalculateFreightNonAirBizObj|AmountToFinalDestination", Caption = "Amount to Final Destination")]
		public override ZDecimal AmountToFinalDestination => base.AmountToFinalDestination;

		protected override ZDecimal GetAmountToEUBorder() => Enterprise.ZArchitecture.Core.Utilities.Round(TotalAmount * PercentageFreightToEUBorder / 100, 2);

		protected override ZDecimal GetAmountToDestinationCountry() => Enterprise.ZArchitecture.Core.Utilities.Round(TotalAmount * PercentageFreightEUToDestinationCountry / 100, 2);

		protected override ZDecimal GetAmountToFinalDestination() => TotalAmount - GetAmountToEUBorder() - GetAmountToDestinationCountry();

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PercentageFreightToEUBorder = 100m;
		}

		protected virtual bool GetCalculateResult()
		{
			var result = false;
			if (!AmountToEUBorder.IsEmpty)
			{
				euIncoTermAndChargeFactory.SetupToEUBorderCharge(charges.AddNew(), AmountToEUBorder, Currency);
				result = true;
			}
			if (!AmountToDestinationCountry.IsEmpty)
			{
				euIncoTermAndChargeFactory.SetupAfterEUBorderCharge(charges.AddNew(), AmountToDestinationCountry, Currency);
				result = true;
			}
			if (!AmountToFinalDestination.IsEmpty)
			{
				euIncoTermAndChargeFactory.SetupDomesticCharge(charges.AddNew(), AmountToFinalDestination, Currency);
				result = true;
			}
			return result;
		}

		protected virtual ImmutableHashSet<ZString> GetChargeTypesToCalculate() => ImmutableHashSet.Create<ZString>(
			euIncoTermAndChargeFactory.FreightToEUBorderCode,
			euIncoTermAndChargeFactory.FreightAfterEUBorderCode,
			euIncoTermAndChargeFactory.FreightDomesticCode);

		protected readonly IJobComInvChargeCollection<JobComInvCharge> charges;
		protected readonly EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory;
	}
}
