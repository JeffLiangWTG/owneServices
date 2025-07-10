using System;
using System.Linq;
using Antlr4.Runtime;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageProcessors.Cadex.RecordParsers;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.Integration;

namespace Enterprise.Customs.CA.Business
{
	public class RefTariffSynchronizeProcessor
	{
		public RefTariffSynchronizeProcessor(ILogger serviceLogger)
		{
			logger = Argument.NotNull(serviceLogger, "serviceLogger");
		}
		readonly ILogger logger;
		const int BatchSize = 100;
		readonly ZDateTime comparisonDate = UniversalReferenceConstants.SyncCAReferenceCutOffDate;
		internal const string LastProcessedCountKey = "CA_RefTariffSynchronizeProcessor_LastProcessedCount";
		int countOfSavings;
		protected int totalCount;
		int CountOfSynchronized { get; set; }

		BusinessObjectFactory Factory => FactoryProvider.Current;
		BusinessObjectFactoryProvider FactoryProvider
		{
			get
			{
				if (factoryProvider == null)
				{
					factoryProvider = new BusinessObjectFactoryProvider();
				}
				return factoryProvider;
			}
		}
		BusinessObjectFactoryProvider factoryProvider;

		TariffViewCollection HarmonizedTariffList
		{
			get
			{
				if (fHarmonizedTariffList == null)
				{
					fHarmonizedTariffList = new TariffViewCollection(Factory, Core.Constants.CountryCodes.Canada, Constants.TariffTypes.HarmonizedSystem, ZDateTime.Today);
					fHarmonizedTariffList.Load();
				}
				return fHarmonizedTariffList;
			}
		}
		TariffViewCollection fHarmonizedTariffList;

		public void Process()
		{
			int.TryParse(ExtProperty.Database.Select(Db.Connection, LastProcessedCountKey), out var lastProcessedCount);

			this.totalCount = HarmonizedTariffList.Count;
			if (lastProcessedCount != totalCount)
			{
				logger.Log(LogType.Information, "Start Tariff Synchronization processing");

				this.countOfSavings = 1;
				CountOfSynchronized = 0;

				SynchronizeTariff();

				var countOfSynchronized = Math.Min((countOfSavings - 1) * BatchSize, totalCount);
				if (countOfSynchronized > 0)
				{
					logger.Log(LogType.Information, string.Format("Successfully synchronized {0} Tariff.", countOfSynchronized));
					ExtProperty.Database.Update(Db.Connection, LastProcessedCountKey, countOfSynchronized.ToString());
				}
				else
				{
					logger.Log(LogType.Information, "There is no Tariff ready for synchronization.");
				}
			}
			else
			{
				logger.Log(LogType.Information, "Global Tariff count has not changed, Tariff Synchronization skip.");
			}
		}

		public void SynchronizeTariff()
		{
			foreach (TariffView tariff in HarmonizedTariffList)
			{
				CountOfSynchronized++;

				var rates = tariff.Rates.Where(r => r.ZZ2_StartDate <= ZDateTime.Today && r.ZZ2_EndDate >= ZDateTime.Today
				&& (r.ZZ2_ZZR_RateTypeCode == Universal.Constants.RateTypes.Excise && r.RateCode == UniversalReferenceConstants.RefCusRate.Codes.EXS
				|| r.ZZ2_ZZR_RateTypeCode == Universal.Constants.RateTypes.Duty && r.RateCode == UniversalReferenceConstants.RefCusRate.Codes.DTY));

				var effectiveDate = tariff.ZZ1_StartDate <= comparisonDate ? comparisonDate : tariff.ZZ1_StartDate;
				if (rates.Any())
				{
					var classHeader = CadexParsersUtilities.LoadOrCreateClassHeader(Factory, effectiveDate, tariff.ZZ1_TariffCode);
					using (classHeader.GetValidationSuspender())
					{
						classHeader.ZA_AreaCode = UniversalReferenceConstants.TempAreaCode;
						classHeader.ZA_ExpiryDate = tariff.ZZ1_EndDate;
						classHeader.ZA_ExchangeDateDeterminationFlag = false;
						classHeader.ZA_InactiveInd = false;
						classHeader.ZA_PermitInd = false;
						classHeader.ZA_QuotaInd = false;
					}

					var uom = tariff.UnitsOfMeasure.FirstOrDefault(x => x.ZZ8_Type == UOMTypeList.Codes.CU1)?.ZZ8_UOM ?? ZString.Empty;
					rates.ForEach(r => SynchronizeGlobalTariffToClassHeader(r, classHeader, r.ZZ2_ZZR_RateTypeCode, uom));
				}

				if (CountOfSynchronized / BatchSize >= countOfSavings || CountOfSynchronized >= totalCount)
				{
					try
					{
						FactoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
						countOfSavings++;
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						logger.Log(LogType.Error, e.Message);
					}
				}
			}
		}

		void SynchronizeGlobalTariffToClassHeader(RateView rateView, CACClassHeader classHeader, ZString rateTypeCode, ZString uom)
		{
			var rateType = rateTypeCode == Universal.Constants.RateTypes.Excise ? CACRateHeader.RateType.ExciseDutyRate : CACRateHeader.RateType.ClassificationRate;
			var effectiveDate = rateView.ZZ2_StartDate <= comparisonDate ? comparisonDate : rateView.ZZ2_StartDate;
			var rateHeader = CadexParsersUtilities.LoadOrCreateRateHeader(classHeader, effectiveDate, rateType);
			using (rateHeader.GetValidationSuspender())
			{
				rateHeader.ZB_ExpiryDate = rateView.ZZ2_EndDate;
				rateHeader.ZB_Inactive = false;
				rateHeader.ZB_FreeInd = false;

				var isContainSpecificUOM = false;
				var rate = CadexParsersUtilities.LoadOrCreateRate(Factory, rateHeader, rateType == CACRateHeader.RateType.ClassificationRate ? rateView.PreferenceCode : ZString.Empty);
				using (rate.GetValidationSuspender())
				{
					rate.ZC_FreeInd = rateView.ZZ2_RateFormula == "0";
					rate.ZC_Inactive = false;
					rate.RateLines.RemoveAndDeleteAll();

					var errorListener = new FormulaErrorListener();
					var visitor = new CARateFormulaVisitor(errorListener);
					visitor.VisitExpression(GetExpressionTree(rateView.ZZ2_RateFormula));
					foreach (var rateResult in visitor.Results)
					{
						isContainSpecificUOM |= ParseRateLines(rate.PK, rateResult);
					}

					if (errorListener.Errors.Any())
					{
						logger.Log(LogType.Error, $"Error parsing rate formula for tariff {classHeader.ZA_ClassificationNumber} - {string.Join("\n", errorListener.Errors)}");
					}
				}

				if (isContainSpecificUOM)
				{
					rateHeader.ZB_UnitOfMeasure = uom;
				}
			}
		}

		bool ParseRateLines(ZGuid ratePK, CARateFormulaVisitor.CARateResult rateResult)
		{
			var result = false;
			var isMin = rateResult.DutyRateMin > 0;
			if (isMin)
			{
				CreateRateLine(ratePK, rateResult.DutyRateMinUOM, rateResult.DutyRateMin, 0m, 0m);
				result |= rateResult.DutyRateMinUOM == RateTypes.Codes.Specific;
			}

			var isRegular = rateResult.DutyRateRegular > 0;
			if (isRegular)
			{
				CreateRateLine(ratePK, rateResult.DutyRateRegularUOM, 0m, rateResult.DutyRateRegular, 0m);
				result |= rateResult.DutyRateRegularUOM == RateTypes.Codes.Specific;
			}

			var isMax = rateResult.DutyRateMax > 0;
			if (isMax)
			{
				CreateRateLine(ratePK, rateResult.DutyRateMaxUOM, 0m, 0m, rateResult.DutyRateMax);
				result |= rateResult.DutyRateMaxUOM == RateTypes.Codes.Specific;
			}

			if (!isMin && !isRegular && !isMax)
			{
				CreateRateLine(ratePK, RateTypes.Codes.Free, 0m, 0m, 0m);
			}
			return result;
		}

		void CreateRateLine(ZGuid ratePK, string rateType, decimal rateMin, decimal rateRegular, decimal rateMax)
		{
			var formula = rateType == RateTypes.Codes.AdValorem ? 100 : 1;
			var rateLine = Factory.New<CACRateLine>();
			using (rateLine.GetValidationSuspender())
			{
				rateLine.ZR_ZC_Rate = ratePK;
				rateLine.ZR_DutyRateType = rateType;
				rateLine.ZR_DutyRateRegular = rateRegular * formula;
				rateLine.ZR_DutyRateMax = rateMax * formula;
				rateLine.ZR_DutyRateMin = rateMin * formula;
			}
		}

		RateFormulaParser.ExpressionContext GetExpressionTree(ZString formulaString)
		{
			var input = new AntlrInputStream(formulaString);
			var lexer = new RateFormulaLexer(input);
			var tokens = new CommonTokenStream(lexer);
			var parser = new RateFormulaParser(tokens);
			return parser.expression();
		}
	}
}
