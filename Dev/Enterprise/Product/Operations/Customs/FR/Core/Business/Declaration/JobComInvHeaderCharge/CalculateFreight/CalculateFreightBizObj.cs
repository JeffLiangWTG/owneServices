using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CalculateFreightBizObj : EU.Business.Declaration.CalculateFreightBizObj
	{
		public CalculateFreightBizObj(JobDeclaration declaration, IJobComInvChargeCollection<JobComInvCharge> charges, EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory, ZString currency, ZString country, ZDateTime dateOfValuation, ZString iataLoadPort)
			: base(charges, euIncoTermAndChargeFactory, currency, country, dateOfValuation, iataLoadPort)
		{
			InsuranceCurrency = currency;
			this.declaration = declaration;
		}

		IncoTermAndCustomsChargeFactory incoTermAndChargeFactory => declaration.IsImport ? (IncoTermAndCustomsChargeFactory)euIncoTermAndChargeFactory : (ExportIncoTermAndCustomsChargeFactory)euIncoTermAndChargeFactory;

		new sealed class Schema : AutoCalculateFreightBizObj.Schema
		{
			public const string PercentageInEUBorder = "PercentageInEUBorder";
			public const int PercentageInEUBorderPrecision = 3;
			public const int PercentageInEUBorderScale = 0;
			public const string PercentageDomestic = "PercentageDomestic";
			public const int PercentageDomesticPrecision = 3;
			public const int PercentageDomesticScale = 0;
			public const string AmountDomestic = "AmountDomestic";
			public const int AmountDomesticPrecision = 19;
			public const int AmountDomesticScale = 2;

			public const string InsuranceAmount = "InsuranceAmount";
			public const string InsuranceCurrency = "InsuranceCurrency";
			public const string InsuranceAmountToEUBorder = "InsuranceAmountToEUBorder";
			public const string InsuranceAmountInEUBorder = "InsuranceAmountInEUBorder";
			public const string InsuranceAmountDomestic = "InsuranceAmountDomestic";

			public const string IsInsuranceIncludedInLines = "IsInsuranceIncludedInLines";
		}

		public static CalculateFreightBizObj New(IJobComInvChargeCollection<JobComInvCharge> charges, JobDeclaration declaration)
		{
			CalculateFreightBizObj result = null;
			if (charges != null && declaration != null)
			{
				if (declaration.IncoTermAndChargeFactory is EUIncoTermAndCustomsChargeFactory euIncoTermAndChargeFactory)
				{
					var portCode = declaration.JE_IATALoadPort;
					result = new CalculateFreightBizObj(declaration, charges, euIncoTermAndChargeFactory, declaration.LocalCurrencyCode, declaration.CountryCode, declaration.DateOfValuation, portCode);

					result.Percentage = ZDecimal.Zero;
					result.PercentageInEUBorder = ZDecimal.Zero;
					result.PercentageDomestic = ZDecimal.Zero;

					if (!portCode.IsEmpty)
					{
						var port = declaration.IATALoadPort;
						if (port != null)
						{
							result.Percentage = ZDecimal.ParseSafe(port.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.PercentOutEU), ZDecimal.Zero);
							result.PercentageInEUBorder = ZDecimal.ParseSafe(port.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.PercentInEu), ZDecimal.Zero);
							result.PercentageDomestic = ZDecimal.ParseSafe(port.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.PercentDomestic), ZDecimal.Zero);
						}
					}
					result.IsFreightIncludedInLines = charges.Cast<JobComInvCharge>().FirstOrDefault(c => c.J7_ChargeType == UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge)?.J7_IsIncludedInITOT ?? false;
					result.IsInsuranceIncludedInLines = charges.Cast<JobComInvCharge>().FirstOrDefault(c => c.J7_ChargeType == FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge)?.J7_IsIncludedInITOT ?? false;

					var transportCharges = charges.Cast<JobComInvCharge>().Where(x => x.J7_ChargeType.In(result.GetChargeTypesToCalculate()) && !x.J7_ChargeType.In(result.GetChargeTypesToExcludeFromAmount()));
					if (transportCharges.Any())
					{
						var firstTransportChargeCurrency = transportCharges.First().Currency;
						result.Currency = transportCharges.All(x => x.Currency == firstTransportChargeCurrency) ? firstTransportChargeCurrency?.RX_Code ?? ZString.Empty : ZString.Empty;
					}

					var insuranceCharges = charges.Cast<JobComInvCharge>().Where(x => x.J7_ChargeType.In(result.GetChargeTypesToExcludeFromAmount()));
					if (insuranceCharges.Any())
					{
						var firstInsuranceChargeCurrency = insuranceCharges.First().Currency;
						result.InsuranceCurrency = insuranceCharges.All(x => x.Currency == firstInsuranceChargeCurrency) ? firstInsuranceChargeCurrency?.RX_Code ?? ZString.Empty : ZString.Empty;
					}
				}
			}
			return result;
		}

		(ZDecimal OutEUBorder, ZDecimal InEUBorder, ZDecimal Domestic) CalculatedPercentages => (Percentage, PercentageInEUBorder, PercentageDomestic);

		#region AmountToEUBorder

		protected override ZDecimal GetAmountToEUBorder()
		{
			return Utilities.Round(Amount * CalculatedPercentages.OutEUBorder / 100, 2);
		}
		protected override ZString[] GetChargeTypesToCalculate()
		{
			return base.GetChargeTypesToCalculate()
				.Concat(new ZString[] { FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge })
				.Distinct()
				.ToArray();
		}

		protected override ZString[] GetChargeTypesToExcludeFromAmount()
		{
			return base.GetChargeTypesToExcludeFromAmount()
				.Concat(new ZString[] { FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge })
				.Distinct()
				.ToArray();
		}

		#endregion

		ZDecimal GetAmountOfInsurance()
		{
			var insuranceCharges = charges.Cast<JobComInvCharge>().ToList().Where(x => x.J7_ChargeType == FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge);
			if (insuranceAmount == ZDecimal.Zero && insuranceCharges.Any())
			{
				var amount = ZDecimal.Zero;

				if (insuranceCharges.All(x => x.Currency == insuranceCharges.First().Currency))
				{
					foreach (JobComInvCharge charge in insuranceCharges)
					{
						amount += charge.J7_Amount;
					}
				}

				return amount;
			}
			else
			{
				return insuranceAmount;
			}
		}

		protected override bool GetCalculateResult()
		{
			incoTermAndChargeFactory.ShouldCreatedFreightChargeBeIncludedInITOT = IsFreightIncludedInLines;

			var result = base.GetCalculateResult();
			if (!AmountDomestic.IsEmpty)
			{
				incoTermAndChargeFactory.SetupDomesticCharge(charges.AddNew(), AmountDomestic, Currency);
				result = true;
			}
			if (!InsuranceAmountToEUBorder.IsEmpty)
			{
				SetupInsuranceToEUBorderCharge(charges.AddNew(), InsuranceAmountToEUBorder, InsuranceCurrency);
				result = true;
			}
			if (!InsuranceAmountInEUBorder.IsEmpty)
			{
				SetupInsuranceAfterEUBorderCharge(charges.AddNew(), InsuranceAmountInEUBorder, InsuranceCurrency);
				result = true;
			}
			if (!InsuranceAmountDomestic.IsEmpty)
			{
				SetupInsuranceDomesticCharge(charges.AddNew(), InsuranceAmountDomestic, InsuranceCurrency);
				result = true;
			}

			return result;
		}

		void SetupInsuranceToEUBorderCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
		{
			charge.J7_ChargeType = FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge;
			charge.J7_Amount = amount;
			charge.J7_RX_NKCurrency = currency;
			charge.J7_IsDutiable = true;
			charge.J7_IsGSTApplicable = true;
			charge.J7_IsStatisticalValueApplicable = true;
			charge.J7_IsIncludedInITOT = IsInsuranceIncludedInLines;
		}

		void SetupInsuranceAfterEUBorderCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
		{
			charge.J7_ChargeType = FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge;
			charge.J7_Amount = amount;
			charge.J7_RX_NKCurrency = currency;
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_IsStatisticalValueApplicable = true;
			charge.J7_IsIncludedInITOT = IsInsuranceIncludedInLines;
		}

		void SetupInsuranceDomesticCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
		{
			charge.J7_ChargeType = FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge;
			charge.J7_Amount = amount;
			charge.J7_RX_NKCurrency = currency;
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_IsStatisticalValueApplicable = false;
			charge.J7_IsIncludedInITOT = IsInsuranceIncludedInLines;
		}

		#region Validation

		public new CalculateFreightBizObjValidation Validation
		{
			get { return (CalculateFreightBizObjValidation)base.Validation; }
		}

		protected override EU.Business.Declaration.CalculateFreightBizObjValidation GetNewValidation()
		{
			return new CalculateFreightBizObjValidation(this);
		}

		#endregion

		#region PercentageInEUBorder

		[DecimalPrecision(Schema.PercentageInEUBorderPrecision)]
		[DecimalPlaces(Schema.PercentageInEUBorderScale)]
		[ResourceStringData("FRCalculateFreightBizObj|PercentageInEUBorder", Caption = "% in EU Border")]
		public ZDecimal PercentageInEUBorder
		{
			[DebuggerStepThrough]
			get
			{
				return percentageInEUBorder;
			}
			set
			{
				SetNonPersistentPropertyValue(PercentageInEUBorderInfo, ref percentageInEUBorder, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePercentageInEUBorder();
				}
			}
		}
		public virtual ZPropertyInfo PercentageInEUBorderInfo => GetZPropertyInfo(Schema.PercentageInEUBorder);

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZDecimal percentageInEUBorder;

		#endregion

		#region AmountInEUBorder

		protected override ZDecimal GetAmountAfterEUBorder()
		{
			return Utilities.Round(Amount * CalculatedPercentages.InEUBorder / 100, 2);
		}

		#endregion

		#region PercentageDomestic

		[DecimalPrecision(Schema.PercentageDomesticPrecision)]
		[DecimalPlaces(Schema.PercentageDomesticScale)]
		[ResourceStringData("FRCalculateFreightBizObj|PercentageDomestic", Caption = "% Domestic")]
		public ZDecimal PercentageDomestic
		{
			[DebuggerStepThrough]
			get
			{
				return percentageDomestic;
			}
			set
			{
				SetNonPersistentPropertyValue(PercentageDomesticInfo, ref percentageDomestic, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePercentageDomestic();
				}
			}
		}
		public virtual ZPropertyInfo PercentageDomesticInfo => GetZPropertyInfo(Schema.PercentageDomestic);

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZDecimal percentageDomestic;

		#endregion

		#region AmountDomestic

		[DecimalPrecision(Schema.AmountDomesticPrecision)]
		[DecimalPlaces(Schema.AmountDomesticScale)]
		[ResourceStringData("FRCalculateFreightBizObj|AmountDomestic", Caption = "Amount Domestic")]
		public ZDecimal AmountDomestic
		{
			get
			{
				if (amountDomestic == null)
				{
					amountDomestic = new CachedProperty<ZDecimal>(Factory, GetAmountDomestic);
				}
				return amountDomestic.Value;
			}
		}

		public ZPropertyInfo AmountDomesticInfo => GetZPropertyInfo(Schema.AmountDomestic);

		CachedProperty<ZDecimal> amountDomestic;

		ZDecimal GetAmountDomestic()
		{
			return Utilities.Round(Amount * CalculatedPercentages.Domestic / 100, 2);
		}

		#endregion

		#region Insurance

		#region InsuranceAmount

		[DecimalPrecision(Schema.AmountPrecision)]
		[DecimalPlaces(Schema.AmountScale)]
		[ResourceStringData("FRCalculateFreightBizObj|InsuranceAmount", Caption = "Insurance Amount")]
		public ZDecimal InsuranceAmount
		{
			get => GetAmountOfInsurance();
			set
			{
				SetNonPersistentPropertyValue(InsuranceAmountInfo, ref insuranceAmount, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateInsuranceAmount();
				}
			}
		}
		ZDecimal insuranceAmount;

		public ZPropertyInfo InsuranceAmountInfo => GetZPropertyInfo(Schema.InsuranceAmount);

		#endregion

		#region InsuranceCurrency

		[List(nameof(CurrencyList))]
		[MaxLength(Schema.CurrencyMaxLength)]
		[ResourceStringData("FRCalculateFreightBizObj|InsuranceCurrency", Caption = "Insurance Currency")]
		public ZString InsuranceCurrency
		{
			get => insuranceCurrency;
			set
			{
				SetNonPersistentPropertyValue(InsuranceCurrencyInfo, ref insuranceCurrency, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateInsuranceCurrency();
				}
			}
		}

		ZString insuranceCurrency;

		public ZPropertyInfo InsuranceCurrencyInfo => GetZPropertyInfo(Schema.InsuranceCurrency);

		#endregion

		public ZBool IsInsuranceOnlyForToEUBorder => Factory.GetCachedValue("FRCalculateFreightBizObj|IsInsuranceOnlyForToEUBorder|" + declaration.JE_ShipmentIncoTerm + declaration.JE_AirRouteType,
			() => (declaration.JE_ShipmentIncoTerm == Core.Constants.IncoTerms.ExWorks || declaration.JE_ShipmentIncoTerm == Core.Constants.IncoTerms.FreeCarrier) && AirRouteTypesForToEUBorder.Contains(declaration.JE_AirRouteType));

		ZString[] AirRouteTypesForToEUBorder => airRouteTypesForToEUBorder ?? (airRouteTypesForToEUBorder = new ZString[] { AirRouteTypeList.Codes._2_CountryCE, AirRouteTypeList.Codes._3_CountryMetroDir, AirRouteTypeList.Codes._6_CountryDOMDir, AirRouteTypeList.Codes._8_Other });
		ZString[] airRouteTypesForToEUBorder;

		#region InsuranceAmountToEUBorder

		[DecimalPrecision(Schema.AmountToEUBorderPrecision)]
		[DecimalPlaces(Schema.AmountToEUBorderScale)]
		[ResourceStringData("FRCalculateFreightBizObj|InsuranceAmountToEUBorder", Caption = "Insurance Amount to EU Border")]
		public ZDecimal InsuranceAmountToEUBorder
		{
			get
			{
				if (insuranceAmountToEUBorder == null)
				{
					insuranceAmountToEUBorder = new CachedProperty<ZDecimal>(Factory, GetInsuranceAmountToEUBorder);
				}
				return insuranceAmountToEUBorder.Value;
			}
		}
		CachedProperty<ZDecimal> insuranceAmountToEUBorder;

		ZDecimal GetInsuranceAmountToEUBorder()
		{
			return IsInsuranceOnlyForToEUBorder ? Utilities.Round(InsuranceAmount, 2) : Utilities.Round(InsuranceAmount * CalculatedPercentages.OutEUBorder / 100, 2);
		}

		public ZPropertyInfo InsuranceAmountToEUBorderInfo => GetZPropertyInfo(Schema.InsuranceAmountToEUBorder);

		#endregion

		#region InsuranceAmountInEUBorder

		[DecimalPrecision(Schema.AmountAfterEUBorderPrecision)]
		[DecimalPlaces(Schema.AmountAfterEUBorderScale)]
		[ResourceStringData("FRCalculateFreightBizObj|InsuranceAmountInEUBorder", Caption = "Insurance Amount in EU Border")]
		public ZDecimal InsuranceAmountInEUBorder
		{
			get
			{
				if (insuranceAmountInEUBorder == null)
				{
					insuranceAmountInEUBorder = new CachedProperty<ZDecimal>(Factory, GetInsuranceAmountInEUBorder);
				}
				return insuranceAmountInEUBorder.Value;
			}
		}

		CachedProperty<ZDecimal> insuranceAmountInEUBorder;

		ZDecimal GetInsuranceAmountInEUBorder()
		{
			return IsInsuranceOnlyForToEUBorder ? 0m : Utilities.Round(InsuranceAmount * CalculatedPercentages.InEUBorder / 100, 2);
		}

		public ZPropertyInfo InsuranceAmountInEUBorderInfo => GetZPropertyInfo(Schema.InsuranceAmountInEUBorder);

		#endregion

		#region InsuranceAmountDomestic

		[DecimalPrecision(Schema.AmountDomesticPrecision)]
		[DecimalPlaces(Schema.AmountDomesticScale)]
		[ResourceStringData("FRCalculateFreightBizObj|InsuranceAmountDomestic", Caption = "Insurance Amount Domestic")]
		public ZDecimal InsuranceAmountDomestic
		{
			get
			{
				if (insuranceAmountDomestic == null)
				{
					insuranceAmountDomestic = new CachedProperty<ZDecimal>(Factory, GetInsuranceAmountDomestic);
				}
				return insuranceAmountDomestic.Value;
			}
		}

		CachedProperty<ZDecimal> insuranceAmountDomestic;

		ZDecimal GetInsuranceAmountDomestic()
		{
			return IsInsuranceOnlyForToEUBorder ? 0m : Utilities.Round(InsuranceAmount * CalculatedPercentages.Domestic / 100, 2);
		}

		public ZPropertyInfo InsuranceAmountDomesticInfo => GetZPropertyInfo(Schema.InsuranceAmountDomestic);

		#endregion

		#endregion

		#region IsInsuranceIncludedInLines

		[ResourceStringData("FR|CalculateFreightBizObj|IsInsuranceIncludedInLines", Caption = "Insurance Included in Lines")]
		public ZBool IsInsuranceIncludedInLines
		{
			get => isInsuranceIncludedInLines;
			set
			{
				SetNonPersistentPropertyValue(IsInsuranceIncludedInLinesInfo, ref isInsuranceIncludedInLines, value);
			}
		}

		ZBool isInsuranceIncludedInLines;

		public ZPropertyInfo IsInsuranceIncludedInLinesInfo => GetZPropertyInfo(Schema.IsInsuranceIncludedInLines);

		#endregion

		readonly JobDeclaration declaration;
	}
}
