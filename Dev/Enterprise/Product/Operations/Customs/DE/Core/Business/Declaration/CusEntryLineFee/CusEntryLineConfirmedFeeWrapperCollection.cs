using System.Linq;
using System.Text;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CusEntryLineConfirmedFeeWrapperCollection : EU.Business.Declaration.CusEntryLineConfirmedFeeWrapperCollection
	{
		public CusEntryLineConfirmedFeeWrapperCollection(CusEntryLine entryLine) : base(entryLine)
		{
		}

		protected override void BuildCollectionCore(EU.Business.Declaration.CusEntryLine entryLine)
		{
			var feeGroups = entryLine.ConfirmedFees
				.Cast<EU.Business.Declaration.CusEntryLineFee>()
				.GroupBy(f => f.CF_ChargeType)
				.ToArray();

			var deCulture = new System.Globalization.CultureInfo(Core.SharedConstants.Languages.German);

			foreach (var feeGroup in feeGroups)
			{
				var mainFee = feeGroup.FirstOrDefault(f => !f.CF_IsLandedCostOnly);
				var additionalFees = feeGroup.Where(f => f.CF_IsLandedCostOnly).ToArray();
				var quickViewText = string.Empty;

				var methodOfCalculation = GetFeeLine(mainFee, System.Globalization.CultureInfo.CurrentUICulture);
				var methodOfCalculationDE = GetFeeLine(mainFee, deCulture);
				if (additionalFees.Length > 0)
				{
					var sb = new StringBuilder()
						.AppendLine(methodOfCalculation);

					var sbDE = new StringBuilder()
						.AppendLine(methodOfCalculationDE);

					foreach (var addFee in additionalFees)
					{
						sb.AppendLine(GetFeeLine(addFee, System.Globalization.CultureInfo.CurrentUICulture));
						sbDE.AppendLine(GetFeeLine(addFee, deCulture));
					}

					methodOfCalculation = textForMultiple;
					quickViewText = sb.ToString();
					methodOfCalculationDE = sbDE.ToString();
				}

				Add(new CusEntryLineConfirmedFeeWrapper(mainFee, methodOfCalculation, methodOfCalculationDE, quickViewText));
			}

			string GetFeeLine(EU.Business.Declaration.CusEntryLineFee addFee, System.Globalization.CultureInfo cultureInfo)
			{
				var mop = TranslateMethodOfPayment(addFee.CF_MethodOfCalculation, cultureInfo.ToString(),
					out var showRate);
				return showRate ? $"{addFee.CF_Rate.ToString("0.00", cultureInfo)} {mop}" : mop;
			}
		}

		string TranslateMethodOfPayment(string input, string lang, out bool showRate)
		{
			showRate = true;
			var result = input;
			var parts = input.Split(' ');
			if (parts.Length == 2)
			{
				var criteriaTypeText = Factory.GetCachedValue<CustomsDutyCriteriaTypeList>().GetMultilingualDescriptionFromCode(parts[0])?.GetLocalizedValue(lang).ToString();
				var assessmentScaleText = Factory.GetCachedValue<CustomsDutyAssessmentScaleList>().GetMultilingualDescriptionFromCode(parts[1])?.GetLocalizedValue(lang).ToString();
				if (assessmentScaleText == null || criteriaTypeText == null)
				{
					showRate = false;
					result = assessmentScaleText ?? criteriaTypeText ?? input;
				}
				else
				{
					result = $"{assessmentScaleText} - {criteriaTypeText}";
				}
			}

			return result;
		}

		static string textForMultiple => Res.GetString("30e21edd-2339-4575-8c01-9a3495fd0b4e", "MUL");
	}
}
