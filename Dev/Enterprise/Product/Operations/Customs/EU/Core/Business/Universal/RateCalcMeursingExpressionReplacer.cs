using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using CusEntryLine = Enterprise.Customs.EU.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.EU.Business
{
	sealed class RateCalcMeursingExpressionReplacer	: DutyCalculator.RateCalcMeursingExpressionReplacer<CusEntryLine>
	{
		public RateCalcMeursingExpressionReplacer(CusEntryLine entryLine)
			: base(entryLine, e => e.RandomLine)
		{
		}

		protected override TariffView GetMeursingTariff() => GetMeursingTariff(Declaration, RandomLine);

		new JobComInvoiceLine RandomLine => base.RandomLine as JobComInvoiceLine;

		internal static TariffView GetMeursingTariff(BaseJobDeclaration declaration, JobComInvoiceLine invoiceLine)
		{
			var meuTariffCode = GetMeuTariffCode(invoiceLine);

			if (string.IsNullOrEmpty(meuTariffCode))
			{
				return null;
			}

			var tariffLoader = new TariffView.Loader(declaration.Factory);

			return tariffLoader.LoadMostRecentCachedTariff(
				declaration.GetDefaultDataGroupingCode(Customs.Business.DefaultDataGroupingType.Tariff)
				, Customs.Business.UniversalReferenceConstants.CusTariffTypes.MeursingTariff
				, meuTariffCode
				, invoiceLine?.EffectiveAssessmentDate ?? ZDateTime.Empty);
		}

		static string GetMeuTariffCode(JobComInvoiceLine jobComInvoiceLine)
		{
			var supplementaryCodes = jobComInvoiceLine
				?.SupplementaryCodes
				.WhereNotNull()
				.Select(x => x.CY_Code) ?? Enumerable.Empty<ZString>();

			return supplementaryCodes.FirstOrDefault(x => IsMeuTariffCode(x));

			bool IsMeuTariffCode(string code) => code.StartsWith("7", StringComparison.OrdinalIgnoreCase);
		}
	}
}
