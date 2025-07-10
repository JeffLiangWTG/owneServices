using System;
using Enterprise.Customs.Common;
using ChargeTypes = Enterprise.Customs.BE.Business.Declaration.BECustomsChargeTypeList.Codes;
using IncoTerms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Customs.BE.Business.Declaration;

internal class ChargesProvider
{
	public static ICustomsChargeCode[] Codes
	{
		get
		{
			return new[]
			{
				AdditionCharge,
				BuyingCommission,
				Brokerage,
				ConstructionErectionAssembly,
				Commission,
				Packing,
				DeductionCharge,
				Discount,
				Engineering,
				MaterialsConsumed,
				MaterialsIncorp,
				OverseasFreight,
				OverseasInsurance,
				Proceeds,
				Royalties,
				StatAjustment,
				Tax,
				Tools,
				Transport,
			};
		}
	}

	public static string[] ConfiguredIncoTerms
	{
		get
		{
			return new[]
			{
				IncoTerms.ExWorks,

				IncoTerms.FreeCarrierBuyer,

				IncoTerms.FreeAlongsideShip,
				IncoTerms.FreeCarrier,
				IncoTerms.FreeCarrierSeller,
				IncoTerms.FreeOnBoard,

				IncoTerms.CostAndFreight,

				IncoTerms.CostInsuranceAndFreight,

				IncoTerms.CarriagePaidTo,

				IncoTerms.CarriageAndInsurancePaidTo,
				IncoTerms.DeliveredAtPlace,
				IncoTerms.DeliveredAtPlaceUnloaded,
				IncoTerms.DeliveredAtTerminal,

				IncoTerms.DeliveredDutyPaid,
			};
		}
	}

	public static CustomsChargeCode AdditionCharge => additionCharge ?? (additionCharge = new CustomsChargeCode(ChargeTypes.AdditionCharge, BECustomsChargeTypeList.Descriptions.AdditionCharge)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Weight,
		IsIncoTermNeutral = true,
	});
	[ThreadStatic]
	static CustomsChargeCode additionCharge;

	public static CustomsChargeCode BuyingCommission => buyingCommission ?? (buyingCommission = new CustomsChargeCode(ChargeTypes.BuyingCommission, BECustomsChargeTypeList.Descriptions.BuyingCommission)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = false,
		IsVATible = false,
		IsVATibleDeemedForThisCharge = false,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = false,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Value,
	});
	[ThreadStatic]
	static CustomsChargeCode buyingCommission;

	public static CustomsChargeCode Brokerage => brokerage ?? (brokerage = new CustomsChargeCode(ChargeTypes.Brokerage, BECustomsChargeTypeList.Descriptions.Brokerage)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Weight,
	});
	[ThreadStatic]
	static CustomsChargeCode brokerage;

	public static CustomsChargeCode ConstructionErectionAssembly => constructionErectionAssembly ?? (constructionErectionAssembly = new CustomsChargeCode(ChargeTypes.ConstructionErectionAssembly, BECustomsChargeTypeList.Descriptions.ConstructionErectionAssembly)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = false,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = false,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = false,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Weight,
	});
	[ThreadStatic]
	static CustomsChargeCode constructionErectionAssembly;

	public static CustomsChargeCode Commission => commission ?? (commission = new CustomsChargeCode(ChargeTypes.Commission, BECustomsChargeTypeList.Descriptions.Commission)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Value,
		IsIncoTermNeutral = true,
	});
	[ThreadStatic]
	static CustomsChargeCode commission;

	public static CustomsChargeCode Packing => packing ?? (packing = new CustomsChargeCode(ChargeTypes.Packing, BECustomsChargeTypeList.Descriptions.Packing)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Weight,
	});
	[ThreadStatic]
	static CustomsChargeCode packing;

	public static CustomsChargeCode DeductionCharge => deductionCharge ?? (deductionCharge = new CustomsChargeCode(ChargeTypes.DeductionCharge, BECustomsChargeTypeList.Descriptions.DeductionCharge)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = false,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = false,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = false,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Weight,
		IsIncoTermNeutral = true,
	});
	[ThreadStatic]
	static CustomsChargeCode deductionCharge;

	public static CustomsChargeCode Discount => discount ?? (discount = new CustomsChargeCode(ChargeTypes.Discount, BECustomsChargeTypeList.Descriptions.Discount)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = false,
		IsVATible = false,
		IsVATibleDeemedForThisCharge = false,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = false,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Value,
		IsIncoTermNeutral = true,
	});
	[ThreadStatic]
	static CustomsChargeCode discount;

	public static CustomsChargeCode Engineering => engineering ?? (engineering = new CustomsChargeCode(ChargeTypes.Engineering, BECustomsChargeTypeList.Descriptions.Engineering)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Weight,
	});
	[ThreadStatic]
	static CustomsChargeCode engineering;

	public static CustomsChargeCode MaterialsConsumed => materialsConsumed ?? (materialsConsumed = new CustomsChargeCode(ChargeTypes.MaterialsConsumed, BECustomsChargeTypeList.Descriptions.MaterialsConsumed)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Weight,
	});
	[ThreadStatic]
	static CustomsChargeCode materialsConsumed;

	public static CustomsChargeCode MaterialsIncorp => materialsIncorp ?? (materialsIncorp = new CustomsChargeCode(ChargeTypes.MaterialsIncorp, BECustomsChargeTypeList.Descriptions.MaterialsIncorp)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Weight,
	});
	[ThreadStatic]
	static CustomsChargeCode materialsIncorp;

	public static CustomsChargeCode OverseasFreight => overseasFreight ?? (overseasFreight = new CustomsChargeCode(ChargeTypes.OverseasFreight, BECustomsChargeTypeList.Descriptions.OverseasFreight)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Weight,
	});
	[ThreadStatic]
	static CustomsChargeCode overseasFreight;

	public static CustomsChargeCode OverseasInsurance => overseasInsurance ?? (overseasInsurance = new CustomsChargeCode(ChargeTypes.OverseasInsurance, BECustomsChargeTypeList.Descriptions.OverseasInsurance)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Value,
	});
	[ThreadStatic]
	static CustomsChargeCode overseasInsurance;

	public static CustomsChargeCode Proceeds => proceeds ?? (proceeds = new CustomsChargeCode(ChargeTypes.Proceeds, BECustomsChargeTypeList.Descriptions.Proceeds)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Weight,
	});
	[ThreadStatic]
	static CustomsChargeCode proceeds;

	public static CustomsChargeCode Royalties => royalties ?? (royalties = new CustomsChargeCode(ChargeTypes.Royalties, BECustomsChargeTypeList.Descriptions.Royalties)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Value,
	});
	[ThreadStatic]
	static CustomsChargeCode royalties;

	public static CustomsChargeCode StatAjustment => statAjustment ?? (statAjustment = new CustomsChargeCode(ChargeTypes.StatAjustment, BECustomsChargeTypeList.Descriptions.StatAjustment)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = false,
		IsVATible = false,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = false,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Weight,
	});
	[ThreadStatic]
	static CustomsChargeCode statAjustment;

	public static CustomsChargeCode Tax => tax ?? (tax = new CustomsChargeCode(ChargeTypes.Tax, BECustomsChargeTypeList.Descriptions.Tax)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = false,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = false,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = false,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Weight,
	});
	[ThreadStatic]
	static CustomsChargeCode tax;

	public static CustomsChargeCode Tools => tools ?? (tools = new CustomsChargeCode(ChargeTypes.Tools, BECustomsChargeTypeList.Descriptions.Tools)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Weight,
	});
	[ThreadStatic]
	static CustomsChargeCode tools;

	public static CustomsChargeCode Transport => transport ?? (transport = new CustomsChargeCode(ChargeTypes.Transport, BECustomsChargeTypeList.Descriptions.Transport)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = false,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = false,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = false,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		DistributeBy = ChargeDistributeByList.Codes.Weight,
	});
	[ThreadStatic]
	static CustomsChargeCode transport;

	public ChargeConfiguration GetChargeConfiguration(string incoTerm, ICustomsChargeCode charge)
	{
		var chargeCode = charge.Code;
		return new ChargeConfiguration()
		{
			IsIncludedInInvoice = includedInInvoiceIncotermCharges.IsListed(incoTerm, chargeCode),
			IsIncludedInInvoiceAmountFixed = includedInInvoiceFixedIncotermCharges.IsListed(incoTerm, chargeCode),
			IsMandatory = mandatoryIncotermCharges.IsListed(incoTerm, chargeCode),
			IsRecommended = recommendedIncotermCharges.IsListed(incoTerm, chargeCode),
		};
	}

	#region Common Charge Configuraton

	readonly (string, string)[] mandatoryIncotermCharges = new (string, string)[]
	{
		(IncoTerms.DeliveredAtPlace, ChargeTypes.OverseasFreight),
		(IncoTerms.DeliveredAtPlace, ChargeTypes.OverseasInsurance),
		(IncoTerms.DeliveredAtTerminal, ChargeTypes.OverseasFreight),
		(IncoTerms.DeliveredAtTerminal, ChargeTypes.OverseasInsurance),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.OverseasFreight),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.OverseasInsurance),
		(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.OverseasFreight),
		(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.OverseasInsurance),
		(IncoTerms.CarriagePaidTo, ChargeTypes.OverseasFreight),
		(IncoTerms.CostInsuranceAndFreight, ChargeTypes.OverseasFreight),
		(IncoTerms.CostInsuranceAndFreight, ChargeTypes.OverseasInsurance),
		(IncoTerms.CostAndFreight, ChargeTypes.OverseasFreight),
	};

	readonly (string, string)[] recommendedIncotermCharges = new (string, string)[]
	{
		(IncoTerms.DeliveredAtPlace, ChargeTypes.OverseasFreight),
		(IncoTerms.DeliveredAtPlace, ChargeTypes.OverseasInsurance),
		(IncoTerms.DeliveredAtTerminal, ChargeTypes.OverseasFreight),
		(IncoTerms.DeliveredAtTerminal, ChargeTypes.OverseasInsurance),
		(IncoTerms.ExWorks, ChargeTypes.OverseasFreight),
		(IncoTerms.ExWorks, ChargeTypes.OverseasInsurance),
		(IncoTerms.ExWorks, ChargeTypes.Packing),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.OverseasFreight),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.OverseasInsurance),
		(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.OverseasFreight),
		(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.OverseasInsurance),
		(IncoTerms.CarriagePaidTo, ChargeTypes.OverseasFreight),
		(IncoTerms.CarriagePaidTo, ChargeTypes.OverseasInsurance),
		(IncoTerms.CostInsuranceAndFreight, ChargeTypes.OverseasFreight),
		(IncoTerms.CostInsuranceAndFreight, ChargeTypes.OverseasInsurance),
		(IncoTerms.CostAndFreight, ChargeTypes.OverseasFreight),
		(IncoTerms.CostAndFreight, ChargeTypes.OverseasInsurance),
		(IncoTerms.FreeOnBoard, ChargeTypes.OverseasFreight),
		(IncoTerms.FreeOnBoard, ChargeTypes.OverseasInsurance),
		(IncoTerms.FreeCarrier, ChargeTypes.OverseasFreight),
		(IncoTerms.FreeCarrier, ChargeTypes.OverseasInsurance),
		(IncoTerms.FreeAlongsideShip, ChargeTypes.OverseasFreight),
		(IncoTerms.FreeAlongsideShip, ChargeTypes.OverseasInsurance),
	};

	readonly (string, string)[] includedInInvoiceIncotermCharges = new (string, string)[]
	{
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.BuyingCommission),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.Discount),

		(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.AdditionCharge),
		(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.Brokerage),
		(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.OverseasFreight),
		(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.OverseasInsurance),
		(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.Packing),
		(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.Transport),
		(IncoTerms.CarriagePaidTo, ChargeTypes.AdditionCharge),
		(IncoTerms.CarriagePaidTo, ChargeTypes.Brokerage),
		(IncoTerms.CarriagePaidTo, ChargeTypes.OverseasFreight),
		(IncoTerms.CarriagePaidTo, ChargeTypes.OverseasInsurance),
		(IncoTerms.CarriagePaidTo, ChargeTypes.Packing),
		(IncoTerms.CostAndFreight, ChargeTypes.Brokerage),
		(IncoTerms.CostAndFreight, ChargeTypes.OverseasFreight),
		(IncoTerms.CostAndFreight, ChargeTypes.Packing),
		(IncoTerms.CostInsuranceAndFreight, ChargeTypes.Brokerage),
		(IncoTerms.CostInsuranceAndFreight, ChargeTypes.OverseasFreight),
		(IncoTerms.CostInsuranceAndFreight, ChargeTypes.OverseasInsurance),
		(IncoTerms.CostInsuranceAndFreight, ChargeTypes.Packing),
		(IncoTerms.DeliveredAtPlace, ChargeTypes.AdditionCharge),
		(IncoTerms.DeliveredAtPlace, ChargeTypes.Brokerage),
		(IncoTerms.DeliveredAtPlace, ChargeTypes.OverseasFreight),
		(IncoTerms.DeliveredAtPlace, ChargeTypes.OverseasInsurance),
		(IncoTerms.DeliveredAtPlace, ChargeTypes.Packing),
		(IncoTerms.DeliveredAtPlace, ChargeTypes.Transport),
		(IncoTerms.DeliveredAtPlaceUnloaded, ChargeTypes.AdditionCharge),
		(IncoTerms.DeliveredAtPlaceUnloaded, ChargeTypes.Brokerage),
		(IncoTerms.DeliveredAtPlaceUnloaded, ChargeTypes.OverseasFreight),
		(IncoTerms.DeliveredAtPlaceUnloaded, ChargeTypes.OverseasInsurance),
		(IncoTerms.DeliveredAtPlaceUnloaded, ChargeTypes.Packing),
		(IncoTerms.DeliveredAtPlaceUnloaded, ChargeTypes.Transport),
		(IncoTerms.DeliveredAtTerminal, ChargeTypes.AdditionCharge),
		(IncoTerms.DeliveredAtTerminal, ChargeTypes.Brokerage),
		(IncoTerms.DeliveredAtTerminal, ChargeTypes.OverseasFreight),
		(IncoTerms.DeliveredAtTerminal, ChargeTypes.OverseasInsurance),
		(IncoTerms.DeliveredAtTerminal, ChargeTypes.Packing),
		(IncoTerms.DeliveredAtTerminal, ChargeTypes.Transport),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.AdditionCharge),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.Brokerage),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.ConstructionErectionAssembly),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.DeductionCharge),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.OverseasFreight),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.OverseasInsurance),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.Packing),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.Tax),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.Transport),
		(IncoTerms.ExWorks, ChargeTypes.Packing),
		(IncoTerms.FreeAlongsideShip, ChargeTypes.Brokerage),
		(IncoTerms.FreeAlongsideShip, ChargeTypes.Packing),
		(IncoTerms.FreeCarrier, ChargeTypes.Brokerage),
		(IncoTerms.FreeCarrier, ChargeTypes.Packing),
		(IncoTerms.FreeCarrierBuyer, ChargeTypes.Brokerage),
		(IncoTerms.FreeCarrierSeller, ChargeTypes.Brokerage),
		(IncoTerms.FreeCarrierSeller, ChargeTypes.Packing),
		(IncoTerms.FreeOnBoard, ChargeTypes.Brokerage),
		(IncoTerms.FreeOnBoard, ChargeTypes.Packing),
	};

	readonly (string, string)[] includedInInvoiceFixedIncotermCharges = new (string, string)[]
	{
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.OverseasFreight),
	};

	#endregion
}
