using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Universal.Constants.ProfileQuestion;
using ECC = Enterprise.Core.Constants;

namespace Enterprise.Customs.MX.Business.Testing
{
	public static class ReferenceTestDataHelper
	{
		public static void CreateIdentifiersRefCusProfileQuestions(BusinessObjectFactory factory)
		{
			var today = ZDateTime.Today;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(ECC.CountryCodes.Mexico);
			factory.Save();
			var tariffType1 = helper.CreateNewOrGetExistingTariffType(ECC.CountryCodes.Mexico, "IDL");
			factory.Save();
			var profileType1 = helper.CreateRefCusProfileType(tariffType1.PK, ECC.CountryCodes.Mexico, "AC", "ALMACÉN GENERAL DE DEPÓSITO CERTIFICADO");
			var profileType2 = helper.CreateRefCusProfileType(tariffType1.PK, ECC.CountryCodes.Mexico, "AI", "OPERACIONES DE COMERCIO EXTERIOR CON AMPARO");
			var profileType3 = helper.CreateRefCusProfileType(tariffType1.PK, ECC.CountryCodes.Mexico, "B2", "BIENES DEL ARTÍCULO 2 DE LA LEY DEL IEPS.");
			factory.Save();

			var question1 = helper.CreateRefCusProfileQuestion(profileType1, "Complemento 1", "CO1", "Número de registro como Almacén General de Depósito certificado.", today.AddDays(-5), today.AddDays(5), note: "Identificar a un Almacén General de Depósito certificado", answerDataType: AnswerDataTypes.String, answerMaxLength: 5);
			var question3 = helper.CreateRefCusProfileQuestion(profileType2, "Complemento 2", "CO2", "Declarar operaciones de comercio exterior que se realizan con amparo.", today.AddDays(-2), today.AddDays(5), answerDataType: AnswerDataTypes.List, answers: new[] { ("1", "desc"), ("2", "desc2"), ("3", "desc3"), ("4", "desc4"), ("5", "desc5") });
			var question4 = helper.CreateRefCusProfileQuestion(profileType2, "Complemento 1", "CO1", "Declarar operaciones de comercio exterior que se realizan con amparo.", today.AddDays(-2), today.AddDays(5), answerDataType: AnswerDataTypes.List, answers: new[] { ("1", "desc"), ("2", "desc2"), ("3", "desc3"), ("4", "desc4"), ("5", "desc5") });
			var question5 = helper.CreateRefCusProfileQuestion(profileType2, "Complemento 3", "CO3", "Declarar operaciones de comercio exterior que se realizan con amparo.", today.AddDays(-2), today.AddDays(5), answerDataType: AnswerDataTypes.List, answers: new[] { ("1", "desc"), ("2", "desc2"), ("3", "desc3"), ("4", "desc4"), ("5", "desc5") });
			var question6 = helper.CreateRefCusProfileQuestion(profileType3, "Complemento 3", "CO3", "La cuota aplicable de conformidad con el Artículo 2, fracción I, incisos D) y/o H) de la Ley del IEPS.", today.AddDays(-5), today.AddDays(5), AnswerDataTypes.Number, answerDecimalPlaces: 2, note: "Identificar las mercancías conforme al artículo 2, fracción I, incisos D) y/o H), de la Ley del IEPS.', '2024-01-01 00:00:00");
			factory.Save();
		}

		public static void CreateCustomsFacilitiesCodes(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Customs Facilities", Core.Constants.CountryCodes.Mexico);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Mexico, ECC.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "123", "Customs Facilities Test1", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Mexico, ECC.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "45", "Customs Facilities Test2", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			factory.Save();
		}

		public static void SetExchangeRate(BusinessObjectFactory factory, string currency, ZDecimal rate, ZDateTime effectiveDate, ExchangeRateType rateType = ExchangeRateType.Customs)
		{
			var refCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currency);
			SetExchangeRate(refCurrency, rate, effectiveDate, rateType);
		}

		static void SetExchangeRate(RefCurrency currency, ZDecimal rate, ZDateTime effectiveDate, ExchangeRateType rateType = ExchangeRateType.Customs)
		{
			var rateCode = rateType == ExchangeRateType.Customs ? ECC.ExchangeRateTypes.Code.CustomsRate : ECC.ExchangeRateTypes.Code.CustomsRateSecondary;

			var filter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, SQLComparisonOperator.Equal, currency.RX_Code);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExRateType, SQLComparisonOperator.Equal, rateCode);

			var exchangeRate = currency.Factory.LoadTop1<RefExchangeRate>(filter);
			if (exchangeRate == null)
			{
				exchangeRate = currency.ExchangeRates.AddNew();
				exchangeRate.RE_RX_NKExCurrency = currency.RX_Code;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate.RE_StartDate = effectiveDate;
				exchangeRate.RE_ExpiryDate = effectiveDate.AddDays(1);
				exchangeRate.RE_ExRateType = rateCode;
			}
			exchangeRate.RE_SellRate = rate;
		}

		public static Dictionary<string, string> DefaultCustomsRegimeExport = new ()
		{
			{ MXDeclarationTypeList.Codes.A1, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.BB, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.D1, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.F4, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.G1, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.G6, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.G7, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.G9, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.H1, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.H8, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.I1, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.K1, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.K2, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.K3, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.L1, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.M3, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.RT, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.S2, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.T1, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.V1, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.V2, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.V5, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.V6, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.V7, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.V8, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.V9, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.VD, CustomsRegimeList.Codes.EXD },
			{ MXDeclarationTypeList.Codes.AJ, CustomsRegimeList.Codes.ETR },
			{ MXDeclarationTypeList.Codes.BA, CustomsRegimeList.Codes.ETR },
			{ MXDeclarationTypeList.Codes.BF, CustomsRegimeList.Codes.ETR },
			{ MXDeclarationTypeList.Codes.BR, CustomsRegimeList.Codes.ETR },
			{ MXDeclarationTypeList.Codes.V4, CustomsRegimeList.Codes.ETR },
			{ MXDeclarationTypeList.Codes.BM, CustomsRegimeList.Codes.ETE },
			{ MXDeclarationTypeList.Codes.BO, CustomsRegimeList.Codes.ETE },
			{ MXDeclarationTypeList.Codes.CT, CustomsRegimeList.Codes.ETE },
			{ MXDeclarationTypeList.Codes.A4, CustomsRegimeList.Codes.DFI },
			{ MXDeclarationTypeList.Codes.F8, CustomsRegimeList.Codes.DFI },
			{ MXDeclarationTypeList.Codes.F9, CustomsRegimeList.Codes.DFI },
			{ MXDeclarationTypeList.Codes.V3, CustomsRegimeList.Codes.DFI },
			{ MXDeclarationTypeList.Codes.J3, CustomsRegimeList.Codes.RFE },
			{ MXDeclarationTypeList.Codes.M5, CustomsRegimeList.Codes.RFE },
			{ MXDeclarationTypeList.Codes.T3, CustomsRegimeList.Codes.TRA },
			{ MXDeclarationTypeList.Codes.T6, CustomsRegimeList.Codes.TRA },
			{ MXDeclarationTypeList.Codes.T7, CustomsRegimeList.Codes.TRA },
			{ MXDeclarationTypeList.Codes.T9, CustomsRegimeList.Codes.TRA },
			{ MXDeclarationTypeList.Codes.J4, CustomsRegimeList.Codes.RFS },
			{ MXDeclarationTypeList.Codes.M4, CustomsRegimeList.Codes.RFS },
		};

		public static Dictionary<string, string> DefaultCustomsRegimeImport = new ()
		{
			{ MXDeclarationTypeList.Codes.A1, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.A3, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.BB, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.C1, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.C3, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.D1, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.F3, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.F4, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.F5, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.G1, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.G2, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.G9, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.GC, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.H1, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.H8, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.I1, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.K1, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.L1, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.P1, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.S2, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.T1, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.V2, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.V5, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.V6, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.V7, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.V9, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.VD, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.VF, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.VU, CustomsRegimeList.Codes.IMD },
			{ MXDeclarationTypeList.Codes.AD, CustomsRegimeList.Codes.ITR },
			{ MXDeclarationTypeList.Codes.AF, CustomsRegimeList.Codes.ITR },
			{ MXDeclarationTypeList.Codes.AJ, CustomsRegimeList.Codes.ITR },
			{ MXDeclarationTypeList.Codes.BA, CustomsRegimeList.Codes.ITR },
			{ MXDeclarationTypeList.Codes.BC, CustomsRegimeList.Codes.ITR },
			{ MXDeclarationTypeList.Codes.BD, CustomsRegimeList.Codes.ITR },
			{ MXDeclarationTypeList.Codes.BE, CustomsRegimeList.Codes.ITR },
			{ MXDeclarationTypeList.Codes.BH, CustomsRegimeList.Codes.ITR },
			{ MXDeclarationTypeList.Codes.BI, CustomsRegimeList.Codes.ITR },
			{ MXDeclarationTypeList.Codes.BO, CustomsRegimeList.Codes.ITR },
			{ MXDeclarationTypeList.Codes.BP, CustomsRegimeList.Codes.ITR },
			{ MXDeclarationTypeList.Codes.E2, CustomsRegimeList.Codes.ITR },
			{ MXDeclarationTypeList.Codes.E4, CustomsRegimeList.Codes.ITR },
			{ MXDeclarationTypeList.Codes.E1, CustomsRegimeList.Codes.ITE },
			{ MXDeclarationTypeList.Codes.E3, CustomsRegimeList.Codes.ITE },
			{ MXDeclarationTypeList.Codes.IN, CustomsRegimeList.Codes.ITE },
			{ MXDeclarationTypeList.Codes.V1, CustomsRegimeList.Codes.ITE },
			{ MXDeclarationTypeList.Codes.A4, CustomsRegimeList.Codes.DFI },
			{ MXDeclarationTypeList.Codes.A5, CustomsRegimeList.Codes.DFI },
			{ MXDeclarationTypeList.Codes.F2, CustomsRegimeList.Codes.DFI },
			{ MXDeclarationTypeList.Codes.F8, CustomsRegimeList.Codes.DFI },
			{ MXDeclarationTypeList.Codes.F9, CustomsRegimeList.Codes.DFI },
			{ MXDeclarationTypeList.Codes.V3, CustomsRegimeList.Codes.DFI },
			{ MXDeclarationTypeList.Codes.G8, CustomsRegimeList.Codes.RFE },
			{ MXDeclarationTypeList.Codes.M1, CustomsRegimeList.Codes.RFE },
			{ MXDeclarationTypeList.Codes.M2, CustomsRegimeList.Codes.RFE },
			{ MXDeclarationTypeList.Codes.T3, CustomsRegimeList.Codes.TRA },
			{ MXDeclarationTypeList.Codes.T6, CustomsRegimeList.Codes.TRA },
			{ MXDeclarationTypeList.Codes.T7, CustomsRegimeList.Codes.TRA },
			{ MXDeclarationTypeList.Codes.T9, CustomsRegimeList.Codes.TRA },
			{ MXDeclarationTypeList.Codes.J4, CustomsRegimeList.Codes.RFS },
			{ MXDeclarationTypeList.Codes.M3, CustomsRegimeList.Codes.RFS },
			{ MXDeclarationTypeList.Codes.M4, CustomsRegimeList.Codes.RFS }
		};
	}
}
