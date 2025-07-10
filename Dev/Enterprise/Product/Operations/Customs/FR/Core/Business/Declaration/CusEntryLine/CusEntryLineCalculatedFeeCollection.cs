using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryLineCalculatedFeeCollection : NonPersistentBusinessObjectCollection<CusEntryLineCalculatedFee>
	{
		public Money CL_CalcAK
		{
			get
			{
				var result = Money.Empty;

				if (!IsNat_147Applicable)
				{
					result = Charges.Where(x => IsTransportCharge(x.J7_ChargeType) && x.J7_IsDutiable && !x.J7_IsIncludedInITOT).Aggregate(Money.Empty, (m, x) => entryLine.CurrencyConverter.Add(m, x.Money));
				}

				return result;
			}
		}

		public Money CL_CalcBA
		{
			get
			{
				var result = Money.Empty;
				if (!IsNat_146Applicable)
				{
					result = Charges.Where(x => IsTransportCharge(x.J7_ChargeType) && !x.J7_IsDutiable && x.J7_IsIncludedInITOT).Aggregate(Money.Empty, (m, x) => entryLine.CurrencyConverter.Add(m, x.Money));
				}

				return result;
			}
		}

		public Money CL_CalcCA
		{
			get
			{
				var result = Money.Empty;

				if (!IsNat_294Applicable)
				{
					result = Charges.Where(x => IsTransportCharge(x.J7_ChargeType) && !x.J7_IsDutiable).Aggregate(Money.Empty, (m, x) => entryLine.CurrencyConverter.Add(m, x.Money));
				}

				return result;
			}
		}

		public CusEntryLineCalculatedFeeCollection(CusEntryLine entryLine)
		{
			this.entryLine = entryLine;
			AddAKCharge();
			AddBACharge();
			AddCACharge();
			AddOtherCharges();
			ConvertChargesCurrency();
			AddStatisticalValue();
		}

		readonly CusEntryLine entryLine;

		void AddAKCharge()
		{
			if (!CL_CalcAK.IsEmpty)
			{
				Add(new CusEntryLineCalculatedFee(entryLine.Factory, UniversalReferenceConstants.RefCusCodeList.ChargeType.AK, CL_CalcAK, !IsNat_146Applicable));
			}
		}

		void AddBACharge()
		{
			if (!CL_CalcBA.IsEmpty)
			{
				Add(new CusEntryLineCalculatedFee(entryLine.Factory, UniversalReferenceConstants.RefCusCodeList.ChargeType.BA, CL_CalcBA));
			}
		}

		void AddCACharge()
		{
			if (!CL_CalcCA.IsEmpty)
			{
				Add(new CusEntryLineCalculatedFee(entryLine.Factory, UniversalReferenceConstants.RefCusCodeList.ChargeType.CA, CL_CalcCA));
			}
		}

		void AddOtherCharges()
		{
			if (IsNat_237Applicable)
			{
				var groups = Charges.OrderBy(x => x.J7_ChargeType).GroupBy(x => new { x.J7_ChargeType, x.J7_IsApportionedCharge });
				foreach (var group in groups)
				{
					var customsCode = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(entryLine.Factory, Core.Constants.CountryCodes.France, RefCusMapTypeList.Codes.ChargeCode, group.Key.J7_ChargeType, ZDateTime.Today);

					if (IsNat_157Applicable && !Nat_157ApplicableChargeCodes.Contains(customsCode))
					{
						continue;
					}

					if (IsNat_294Applicable && Nat_294ApplicableChargeCodes.Contains(customsCode))
					{
						continue;
					}

					var sum = group.Aggregate(Money.Empty, (m, x) => entryLine.CurrencyConverter.Add(m, x.Money));
					if (!sum.IsEmpty)
					{
						var calculatedFee = new CusEntryLineCalculatedFee(entryLine.Factory, group.Key.J7_ChargeType, sum, !group.Key.J7_IsApportionedCharge);
						if (!calculatedFee.CustomsCode.IsEmpty)
						{
							Add(calculatedFee);
						}
					}
				}
			}
			else
			{
				foreach (var group in Charges.GroupBy(x => x.J7_ChargeType))
				{
					var customsCode = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(entryLine.Factory, Core.Constants.CountryCodes.France, RefCusMapTypeList.Codes.ChargeCode, group.Key, ZDateTime.Today);

					if (IsNat_157Applicable && !Nat_157ApplicableChargeCodes.Contains(customsCode))
					{
						continue;
					}

					if (IsNat_294Applicable && Nat_294ApplicableChargeCodes.Contains(customsCode))
					{
						continue;
					}

					var sum = group.Aggregate(Money.Empty, (m, x) => entryLine.CurrencyConverter.Add(m, x.Money));
					if (!sum.IsEmpty)
					{
						var calculatedFee = new CusEntryLineCalculatedFee(entryLine.Factory, group.Key, sum);
						if (!calculatedFee.CustomsCode.IsEmpty)
						{
							Add(calculatedFee);
						}
					}
				}
			}
		}

		void ConvertChargesCurrency()
		{
			ICurrency currency = RefCurrency.LoadFromCurrencyCode(GlbCompany.CurrentCompany.Factory, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
			var entryHeader = entryLine.Header;

			if (entryHeader != null && !entryHeader.IsMultiInvoiceCurrency)
			{
				currency = entryHeader.InvoiceHeaders?.FirstOrDefault()?.Invoice_Currency ?? currency;
			}

			foreach (var charge in this.Cast<CusEntryLineCalculatedFee>())
			{
				if (charge.Money.Currency != currency)
				{
					charge.Money = entryLine.CurrencyConverter.ConvertExact(charge.Money, currency);
				}
			}
		}

		void AddStatisticalValue()
		{
			var localCurrency = RefCurrency.LoadFromCurrencyCode(GlbCompany.CurrentCompany.Factory, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
			var statisticalValue = new CusEntryLineCalculatedFee(entryLine.Factory, ZString.Empty, new Money(entryLine.CL_StatisticalValue, localCurrency));
			statisticalValue.Category = Res.GetString("4c126f71-cc15-4135-a9c2-10c0b25309d6", "Stat. Value");
			Add(statisticalValue);
		}

		public bool IsNat_146Applicable
		{
			get
			{
				var incoTerm = entryLine.RandomLine.InvoiceHeader?.JZ_IncoTerm ?? ZString.Empty;

				return (incoTerm == Core.Constants.IncoTerms.ExWorks
						|| incoTerm == Core.Constants.IncoTerms.FreeAlongsideShip
						|| incoTerm == Core.Constants.IncoTerms.FreeCarrier
						|| incoTerm == Core.Constants.IncoTerms.FreeOnBoard)
						&& entryLine.RandomLine.JI_ValuationCode == Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
			}
		}

		bool IsNat_147Applicable => entryLine.RandomLine.JI_ValuationCode == Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1 && entryLine.RandomLine.InvoiceHeader.JZ_IncoTerm == Core.Constants.IncoTerms.CostInsuranceAndFreight;

		bool IsNat_157Applicable => entryLine.RandomLine.JI_ValuationCode != Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;

		bool IsNat_294Applicable => entryLine.RandomLine.SupplementaryCodes.Any(x => x.CY_Code == FRConstants.SupplementaryCodes._1277);

		bool IsNat_237Applicable => entryLine.Declaration?.IsUCC6 ?? false;
		
		static ZString[] Nat_157ApplicableChargeCodes => new ZString[]
		{
			UniversalReferenceConstants.RefCusCodeList.ChargeType.AK,
			UniversalReferenceConstants.RefCusCodeList.ChargeType.AN,
			UniversalReferenceConstants.RefCusCodeList.ChargeType.BA,
			UniversalReferenceConstants.RefCusCodeList.ChargeType.BC,
			UniversalReferenceConstants.RefCusCodeList.ChargeType.BG,
			UniversalReferenceConstants.RefCusCodeList.ChargeType.CA,
			UniversalReferenceConstants.RefCusCodeList.ChargeType.CZ
		};

		static ZString[] Nat_294ApplicableChargeCodes => new ZString[]
		{
			UniversalReferenceConstants.RefCusCodeList.ChargeType.CA,
			UniversalReferenceConstants.RefCusCodeList.ChargeType.CZ
		};

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CusEntryLineCalculatedFee(entryLine.Factory, ZString.Empty, Money.Empty);
		}

		public List<JobComInvCharge> Charges
		{
			get
			{
				return entryLine.Factory.GetValue(ref chargesCached, () =>
				{
					var charges = entryLine.InvoiceLines.Select(x => x.Charges.Cast<JobComInvCharge>()).SelectMany(a => a).ToList();
					var apportionedCharges = entryLine.InvoiceLines.Select(x => x.ApportionedCharges.Cast<JobComInvCharge>()).SelectMany(a => a).ToList();
					return charges.Concat(apportionedCharges).ToList();
				});
			}
		}

		CachedProperty<List<JobComInvCharge>> chargesCached;

		public static bool IsTransportCharge(string chargeType) => chargeType == FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge
															|| chargeType == FRCustomsChargeTypeList.Codes.AirTransportCostsCharge
															|| chargeType == FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU
															|| chargeType == FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination
															|| chargeType == FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU
															|| chargeType == FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination
															|| chargeType == FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder
															|| chargeType == FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU
															|| chargeType == FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder
															|| chargeType == FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU
															|| chargeType == FRCustomsChargeTypeList.Codes.TransportCostsCharge
															|| chargeType == FRCustomsChargeTypeList.Codes.InsuranceCostsCharge;
	}
}
