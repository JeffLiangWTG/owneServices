using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business
{
	public class CusLineTariffDetail : AutoCusLineTariffDetail
	{
		public new partial class Schema : AutoCusLineTariffDetail.Schema
		{
			public const string TypeDescription = "TypeDescription";
			public const string RateFormula = "RateFormula";
			public const string ExciseReferenceNumberDescription = "ExciseReferenceNumberDescription";
			public const string PaymentMethodDescription = "PaymentMethodDescription";
		}

		public CusLineTariffDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusLineTariffDetailLookups Lookups => (CusLineTariffDetailLookups)base.Lookups;

		protected override Customs.Business.CusLineTariffDetailLookups GetNewLookups() => new CusLineTariffDetailLookups(this);

		public new CusLineTariffDetailValidation Validation => (CusLineTariffDetailValidation)base.Validation;

		protected override Customs.Business.CusLineTariffDetailValidation GetNewValidation() => new CusLineTariffDetailValidation(this);

		[ResourceStringData("40A7D223-77F9-47E4-860B-DAEE108D0933", Caption = "Type")]
		[List(nameof(Lookups) + "." + nameof(CusLineTariffDetailLookups.TariffTypeList))]
		public override ZString BZ_Type
		{
			get => base.BZ_Type;
			set
			{
				var oldValue = BZ_Type;
				base.BZ_Type = value;
				if (oldValue != BZ_Type)
				{
					typeDescriptionCached = null;
				}
			}
		}

		[ResourceStringData("F6532868-8AE7-4E1D-ACC3-97645F1E41DD", Caption = "Excise Reference Number", ShortCaption = "ERN")]
		[List(nameof(Lookups) + "." + nameof(CusLineTariffDetailLookups.TariffList))]
		public override ZString BZ_Tariff
		{
			get => base.BZ_Tariff;
			set
			{
				var oldValue = BZ_Tariff;
				base.BZ_Tariff = value;
				if (!IsCopying && oldValue != BZ_Tariff)
				{
					var (firstPartUnit, secondPartUnit) = RateFormulaUnits;
					if (BZ_UQ1.IsEmpty && !firstPartUnit.IsEmpty)
					{
						BZ_UQ1 = firstPartUnit;
					}

					if (BZ_UQ2.IsEmpty && !secondPartUnit.IsEmpty)
					{
						BZ_UQ2 = secondPartUnit;
					}

					exciseReferenceNumberDescriptionCached = null;

					DefaultTariffType();
				}
			}
		}

		void DefaultTariffType()
		{
			if (UniversalTariff == null)
			{
				var candidate = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Ireland, BZ_Tariff, EffectiveAssessmentDate);
				var candidateTariffType = candidate?.ZZ1_ZZI_TariffTypeCode ?? ZString.Empty;
				var currentTariffType = BZ_Type;
				if (!candidateTariffType.IsEmpty && candidateTariffType != currentTariffType)
				{
					BZ_Type = candidateTariffType;
				}
			}
		}

		[ResourceStringData("50941D1C-1DED-433E-8473-FE05799CFC4A", Caption = "ERN Description", MediumCaption = "ERN Desc.", ShortCaption = "Desc.", FullDescription = "Excise Reference Number Description")]
		public ZString ExciseReferenceNumberDescription
		{
			get
			{
				if (!exciseReferenceNumberDescriptionCached.HasValue)
				{
					var tariffList = Lookups.TariffList;
					tariffList.Load();
					exciseReferenceNumberDescriptionCached = tariffList.Cast<ICodeDescription>().FirstOrDefault(x => BZ_Tariff.EqualsIgnoringCase(x.Code))?.Description ?? ZString.Empty;
				}
				return exciseReferenceNumberDescriptionCached.Value;
			}
		}
		ZString? exciseReferenceNumberDescriptionCached;

		public ZPropertyInfo ExciseReferenceNumberDescriptionInfo => GetZPropertyInfo(nameof(ExciseReferenceNumberDescription));

		[ResourceStringData("3E90EC85-7989-4BEE-81BB-5B789689FA47", Caption = "Quantity", ShortCaption = "Qty")]
		[DecimalPlaces(2)]
		public override ZDecimal BZ_Qty1 { get => base.BZ_Qty1; set => base.BZ_Qty1 = value; }

		[ResourceStringData("8BB20ACD-82C6-4799-96E2-C2307CCA8C32", Caption = "Quantity Unit", MediumCaption = "Qty Unit", ShortCaption = "Unit")]
		public override ZString BZ_UQ1 { get => base.BZ_UQ1; set => base.BZ_UQ1 = value; }

		[ResourceStringData("545DA492-47FB-4ABF-BD94-D7F331972BE6", Caption = "Payment Method")]
		public override ZString ZG_MethodOfPayment
		{
			get => base.ZG_MethodOfPayment;
			set
			{
				var oldValue = ZG_MethodOfPayment;
				base.ZG_MethodOfPayment = value;
				if (oldValue != ZG_MethodOfPayment)
				{
					paymentMethodDescriptionCached = null;
				}
			}
		}

		[ResourceStringData("1E543FC0-3220-43B2-935B-CBEE186497B7", Caption = "Description", ShortCaption = "Desc.", FullDescription = "Payment Method Description")]
		public ZString PaymentMethodDescription
		{
			get
			{
				if (!paymentMethodDescriptionCached.HasValue)
				{
					paymentMethodDescriptionCached = AddInfo.Lookups.PaymentMethodList.GetDescriptionFromCode(ZG_MethodOfPayment);
				}
				return paymentMethodDescriptionCached.Value;
			}
		}
		ZString? paymentMethodDescriptionCached;

		public ZPropertyInfo PaymentMethodDescriptionInfo => GetZPropertyInfo(nameof(PaymentMethodDescription));

		[ResourceStringData("C225821A-E1FD-4F2A-A3AF-32231DB34656", Caption = "Quantity 2", ShortCaption = "Qty 2")]
		public override ZDecimal BZ_Qty2 { get => base.BZ_Qty2; set => base.BZ_Qty2 = value; }

		[ResourceStringData("34CD5ECD-B72C-4512-B71E-6C130BF4D4E9", Caption = "Quantity Unit 2", ShortCaption = "Unit 2")]
		public override ZString BZ_UQ2 { get => base.BZ_UQ2; set => base.BZ_UQ2 = value; }

		[ResourceStringData("21C7F58D-325A-4C6C-977B-10E9A9FDBDC9", Caption = "Type Description", ShortCaption = "Type Desc.")]
		public ZString TypeDescription
		{
			get
			{
				if (!typeDescriptionCached.HasValue)
				{
					typeDescriptionCached = Lookups.TariffTypeList.GetDescriptionFromCode(BZ_Type);
				}
				return typeDescriptionCached.Value;
			}
		}
		ZString? typeDescriptionCached;

		public ZPropertyInfo TypeDescriptionInfo => GetZPropertyInfo(nameof(TypeDescription));

		[ResourceStringData("BD024E21-72A8-4F39-A163-C85C71EDD47D", Caption = "Rate Formula")]
		public ZString RateFormula => Factory.GetValue(ref rateFormulaCached, () => LatestRate?.ZZ2_RateFormula ?? ZString.Empty);
		CachedProperty<ZString> rateFormulaCached;

		public ZPropertyInfo RateFormulaInfo => GetZPropertyInfo(nameof(RateFormula));

		RateView LatestRate => Factory.GetValue(ref latestRateCached, () => UniversalTariff is TariffView universalTariff ?
			universalTariff.Rates.GetRatesFor(EffectiveAssessmentDate)
				.OrderByDescending(x => x.ZZ2_StartDate)
				.ThenByDescending(x => x.ZZ2_EndDate)
				.FirstOrDefault()
			: null);
		CachedProperty<RateView> latestRateCached;

		public (ZString FirstPartUnit, ZString SecondPartUnit) RateFormulaUnits
		{
			get
			{
				return Factory.GetValue(ref rateFormulaUnitsCached, () =>
				{
					var firstPartUnit = ZString.Empty;
					var secondPartUnit = ZString.Empty;

					var formulas = RateFormula.Split('+');
					var length = formulas.Length;
					if (length > 0)
					{
						firstPartUnit = ExtractUnit(formulas[0]);
					}

					if (length > 1)
					{
						secondPartUnit = ExtractUnit(formulas[1]);
					}

					return (firstPartUnit, secondPartUnit);
				});
			}
		}
		CachedProperty<(ZString, ZString)> rateFormulaUnitsCached;

		static string ExtractUnit(string formula)
		{
			var match = Regex.Match(formula, @"\[([A-Z]{3})\]", RegexOptions.IgnoreCase);
			return match.Success ? match.Groups[1].Value : string.Empty;
		}

		#region AddInfo
		protected new AddInfoCusLineTariffDetail AddInfo => (AddInfoCusLineTariffDetail)base.AddInfo;

		protected override EU.Business.AddInfoCusLineTariffDetail GetNewAddInfo() => new AddInfoCusLineTariffDetail(BZ_NAddInfoInfo);

		public new AddInfoCusLineTariffDetailValidation AddInfoValidation => AddInfo.Validation;

		public new AddInfoCusLineTariffDetailLookups AddInfoLookups => AddInfo.Lookups;
		#endregion
	}
}
