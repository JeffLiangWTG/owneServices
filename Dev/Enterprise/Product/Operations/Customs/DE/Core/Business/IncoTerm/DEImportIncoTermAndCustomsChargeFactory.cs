using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class DEImportIncoTermAndCustomsChargeFactory : EUIncoTermAndCustomsChargeFactory
	{
		protected override ICustomsChargeCode[] GetCharges()
		{
			var chargeList = new List<ICustomsChargeCode>(base.GetCharges());
			if (ExcludedCustomsChargeCodes != null)
			{
				chargeList.RemoveAll(x => ExcludedCustomsChargeCodes.Contains(x.Code));
			}
			chargeList.AddRange(CreateAdditionalCustomsChargeCodes());
			return SetCustomsChargeCode(chargeList.ToArray());
		}

		ICustomsChargeCode[] CreateAdditionalCustomsChargeCodes()
		{
			return new ICustomsChargeCode[]
			{
				CreateCustomsChargeCode(ImportChargeCodeList.Codes._001, ImportChargeCodeList.Descriptions._001,true, true, true, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes._002, ImportChargeCodeList.Descriptions._002,true, true, true, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes._003, ImportChargeCodeList.Descriptions._003,true, true, true, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes._004, ImportChargeCodeList.Descriptions._004,true, true, true, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes._005, ImportChargeCodeList.Descriptions._005,true, true, true, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes._006, ImportChargeCodeList.Descriptions._006,true, true, true, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes._007, ImportChargeCodeList.Descriptions._007,true, true, true, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes._008, ImportChargeCodeList.Descriptions._008,true, true, true, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes._009, ImportChargeCodeList.Descriptions._009,true, true, true, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes._011, ImportChargeCodeList.Descriptions._011,true, true, true, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes._012, ImportChargeCodeList.Descriptions._012,true, true, true, false),
				CreateFreightAfterEUBorder(),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes._015, ImportChargeCodeList.Descriptions._015, true, false, true, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes._016, ImportChargeCodeList.Descriptions._016, false, false, false, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes._017, ImportChargeCodeList.Descriptions._017, false, false, false, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes._019, ImportChargeCodeList.Descriptions._019, true, true, true, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes.AIR, ImportChargeCodeList.Descriptions.AIR, false, true, false, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes.INP, ImportChargeCodeList.Descriptions.INP, false, true, false, true),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes.OPF, ImportChargeCodeList.Descriptions.OPF, false, false, false, true),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes.TCE, ImportChargeCodeList.Descriptions.TCE, true, true, true, false),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes.SRC, ImportChargeCodeList.Descriptions.SRC, true, true, true, true),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes.SRN, ImportChargeCodeList.Descriptions.SRN, true, true, true, true),
				CreateCustomsChargeCode(ImportChargeCodeList.Codes.SRS, ImportChargeCodeList.Descriptions.SRS, true, true, true, true)
			};
		}

		HashSet<string> ExcludedCustomsChargeCodes => excludedCustomsChargeCodes ?? (excludedCustomsChargeCodes = new HashSet<string>(new[] {
			CustomsChargeTypeList.Codes.AdditionCharge,
			CustomsChargeTypeList.Codes.Commission,
			CustomsChargeTypeList.Codes.DeductionCharge,
			CustomsChargeTypeList.Codes.ExWorks,
			CustomsChargeTypeList.Codes.ForeignInlandFreight,
			CustomsChargeTypeList.Codes.LandingCharges,
			CustomsChargeTypeList.Codes.OverseasInsurance,
			CustomsChargeTypeList.Codes.OtherCharges,
			CustomsChargeTypeList.Codes.PackingCost
		}));
		HashSet<string> excludedCustomsChargeCodes;

		CustomsChargeCode CreateCustomsChargeCode(ZString code, MultilingualString description, ZBool isDutiableDeemed, ZBool isVATibleDeemed, ZBool isStatisticalDeemed, bool isInvoiceLineOnly)
		{
			var customsChargeCode = new CustomsChargeCode(code, description)
			{
				IsDutiable = ChargeHelper.NeedSetJ7_IsDutiableToTrue(code),
				IsVATible = ChargeHelper.NeedSetJ7_IsGSTApplicableToTrue(code),
				IsStatisticalValueApplicable = ChargeHelper.NeedSetJ7_IsStatisticalValueApplicableToTrue(code),
				IsDutiableDeemedForThisCharge = isDutiableDeemed,
				IsVATibleDeemedForThisCharge = isVATibleDeemed,
				IsStatisticalValueApplicableDeemed = isStatisticalDeemed,
			};
			if (isInvoiceLineOnly)
			{
				customsChargeCode.ParentTypes = ChargeParentTypes.InvoiceLine;
			}
			return customsChargeCode;
		}

		public override void SetupToEUBorderCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
		{
			charge.J7_ChargeType = FreightToEUBorderCodeCore;
			charge.J7_Amount = amount;
			charge.J7_RX_NKCurrency = currency;
		}

		public override void SetupAfterEUBorderCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
		{
			charge.J7_ChargeType = FreightAfterEUBorderCode;
			charge.J7_Amount = amount;
			charge.J7_RX_NKCurrency = currency;
		}

		protected CustomsChargeCode CreateFreightAfterEUBorder() => new CustomsChargeCode(FreightAfterEUBorderCode, FreightAfterEUBorderDesc)
		{
			IsDutiable = false,
			IsVATible = true,
			IsStatisticalValueApplicable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicableDeemed = false,
		};

		protected override void SetupIncotermChargeConfigurations()
		{
			base.SetupIncotermChargeConfigurations();

			GetConfiguration(Core.Constants.IncoTerms.CostAndFreight, CustomsChargeTypeList.Codes.OverseasInsurance).IsRecommended = false;
			GetConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, CustomsChargeTypeList.Codes.OverseasInsurance).IsRecommended = false;
			GetConfiguration(Core.Constants.IncoTerms.ExWorks, CustomsChargeTypeList.Codes.OverseasInsurance).IsRecommended = false;
			GetConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, CustomsChargeTypeList.Codes.OverseasInsurance).IsRecommended = false;
			GetConfiguration(Core.Constants.IncoTerms.FreeCarrier, CustomsChargeTypeList.Codes.OverseasInsurance).IsRecommended = false;
			GetConfiguration(Core.Constants.IncoTerms.FreeOnBoard, CustomsChargeTypeList.Codes.OverseasInsurance).IsRecommended = false;
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, CustomsChargeTypeList.Codes.OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = true });
		}

		protected override void SetupErrorConfiguration()
		{
			base.SetupErrorConfiguration();

			GetConfiguration(ErrorIncoTermCode, CustomsChargeTypeList.Codes.OverseasInsurance).IsRecommended = false;
		}

		protected override string FreightToEUBorderCodeCore => ImportChargeCodeList.Codes._010;

		protected override MultilingualString FreightToEUBorderDesc => ImportChargeCodeList.Descriptions._010;

		protected override string InsuranceChargeCodeCore => ImportChargeCodeList.Codes._012;

		protected override string FreightAfterEUBorderCodeCore => ImportChargeCodeList.Codes._014;

		protected override MultilingualString FreightAfterEUBorderDesc => ImportChargeCodeList.Descriptions._014;
	}
}
