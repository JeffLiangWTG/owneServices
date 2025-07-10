using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Business
{
	public partial class CusLineTariffDetail
	{
		public CusLineTariffDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(CusLineTariffDetailLookups.TariffTypeList))]
		[ResourceStringData("AE43F18C-580D-41A6-A63B-B3504A77DAE9", Caption = "Type")]
		public override ZString BZ_Type
		{
			get => base.BZ_Type;
			set
			{
				if (base.BZ_Type != value && !IsCopying)
				{
					if (value.IsEmpty)
					{
						BZ_Tariff = ZString.Empty;
					}
					base.BZ_Type = value;
				}
			}
		}

		[ResourceStringData("9D1ABF52-35DD-4A56-B5D5-CBBAAB2F26D4", Caption = "Type Description", MediumCaption = "Type Desc.", ShortCaption = "Desc.")]
		public ZString BZ_TypeDescription => Lookups.TariffTypeList.GetDescriptionFromCode(BZ_Type);

		[List(nameof(Lookups) + "." + nameof(CusLineTariffDetailLookups.ExemptionReductionCodeList))]
		[ResourceStringData("F50744BC-4052-44E8-87CC-0206A41B66A2", Caption = "Exemption Reduction Code")]
		public override ZString BZ_ExemptionReductionCode
		{
			get => base.BZ_ExemptionReductionCode;
			set
			{
				if (value != base.BZ_ExemptionReductionCode && !IsCopying)
				{
					if (value.IsEmpty)
					{
						BZ_Value = 0m;
					}
					base.BZ_ExemptionReductionCode = value;
				}
			}
		}

		[ResourceStringData("4E51545B-9122-4301-B880-DFDA20A3F0E5", ShortCaption = "Desc.", MediumCaption = "Exemption Reduction Code Desc.", Caption = "Exemption Reduction Code Description")]
		public ZString BZ_ExemptionReductionCodeDescription => Lookups.ExemptionReductionCodeList.GetDescriptionFromCode(BZ_ExemptionReductionCode);

		[ReadOnlyMember(nameof(BZ_TariffReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusLineTariffDetailLookups.TariffCollection))]
		[ResourceStringData("2B9854D3-694E-4FDD-A01C-7CCD8C2D379B", Caption = "Code")]
		public override ZString BZ_Tariff { get => base.BZ_Tariff; set => base.BZ_Tariff = value; }

		bool BZ_TariffReadOnly => BZ_Type.IsEmpty;

		[ReadOnlyMember(nameof(BZ_ValueReadOnly))]
		[ResourceStringData("D45BD01B-B937-45C1-A73C-754A74AF45FE", ShortCaption = "Reduction Amt.", Caption = "Reduction Amount")]
		public override ZDecimal BZ_Value { get => base.BZ_Value; set => base.BZ_Value = RoundBZ_Value(value); }

		const decimal BZ_Value_MaximumValue = 99999999999;

		bool BZ_ValueReadOnly => BZ_ExemptionReductionCode.IsEmpty;

		[ResourceStringData("6519009A-9047-4998-A353-C3779BBA16C8", ShortCaption = "Rate", Caption = "Rate Formula")]
		public ZString RateFormula
		{
			get
			{
				var result = ZString.Empty;
				if (!BZ_Tariff.IsEmpty)
				{
					var today = ZDateTime.Today;
					result = Factory.GetCachedValue($"JP.CusLineTariffDetail.RateFormula-{BZ_Tariff}-{today.ToShortDateString()}", () =>
					{
						var tariffQuery = new ZDBOnlySubQuery(typeof(TariffView), TariffViewSchema.PK);
						tariffQuery.AddToFilter(TariffViewSchema.ZZ1_TariffCode, BZ_Tariff);
						tariffQuery.AddToFilter(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Japan);
						var rateQuery = new ZDBOnlyQuery(typeof(RefCusRate));
						rateQuery.AddToFilter(RefCusRateSchema.ZZ2_StartDate, SQLComparisonOperator.LessThanOrEqualTo, today);
						rateQuery.AddToFilter(RefCusRateSchema.ZZ2_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, today);
						rateQuery.AddSubQuery(RefCusRateSchema.ZZ2_ZZ1_Tariff, tariffQuery, JoinCondition.And);
						return Factory.LoadTop1<RefCusRate>(rateQuery)?.ZZ2_RateFormulaDerivedFrom ?? ZString.Empty;
					});
				}
				return result;
			}
		}

		public ZPropertyInfo RateFormulaInfo => GetZPropertyInfo(nameof(RateFormula));

		ZDecimal RoundBZ_Value(ZDecimal value) => value > BZ_Value_MaximumValue ? new ZDecimal(BZ_Value_MaximumValue) : value;
	}
}
