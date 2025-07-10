using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EntityFramework.Testing.TestCaseWithFactory;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ChargeValidationHelperTest
	{
		internal static void TestCheckJ7_ExchangeRate(CommonNonApportionedCharge charge)
		{
			RefCurrency usd = charge.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			var resultUSD = charge.Factory.New<RefExchangeRate>();
			resultUSD.RE_GC = GlbCompany.CurrentCompany.PK;
			resultUSD.RE_RX_NKExCurrency = usd.RX_Code;
			resultUSD.RE_StartDate = ZDateTime.Today;
			resultUSD.RE_ExpiryDate = ZDateTime.Today;
			resultUSD.RE_SellRate = 0.8m;
			resultUSD.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			charge.Factory.Save();

			JobDeclaration declaration = (JobDeclaration)charge.Parent.JobDeclaration;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			declaration.JE_ProcedureType = "E";
			charge.J7_ExchangeRate = 0;
			AssertNoMessageErrors(charge.J7_ExchangeRateInfo);

			declaration.JE_ProcedureType = "B";
			charge.Validation.ValidateJ7_ExchangeRate();
			AssertNoMessageErrors(charge.J7_ExchangeRateInfo);

			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge.Validation.ValidateJ7_ExchangeRate();
			AssertHasMessageErrorContaining(charge.J7_ExchangeRateInfo, ExtensionMethods.ExchangeRatePublishedMessage);

			charge.J7_ExchangeRate = 100;
			AssertNoMessageErrorContaining(charge.J7_ExchangeRateInfo, ExtensionMethods.ExchangeRatePublishedMessage);

			declaration.JE_ProcedureType = "E";
			charge.J7_ExchangeRate = 0;
			AssertNoMessageErrors(charge.J7_ExchangeRateInfo);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			charge.J7_ExchangeRate = 0;
			AssertHasMessageErrorContaining(charge.J7_ExchangeRateInfo, ExtensionMethods.ExchangeRatePublishedMessage);

			charge.J7_ExchangeRate = 100;
			AssertNoMessageErrorContaining(charge.J7_ExchangeRateInfo, ExtensionMethods.ExchangeRatePublishedMessage);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			charge.J7_ExchangeRate = 0;
			AssertNoMessageErrorContaining(charge.J7_RX_NKCurrencyInfo, ExtensionMethods.ExchangeRatePublishedMessage);

			charge.J7_ExchangeRate = 100;
			AssertNoMessageErrorContaining(charge.J7_RX_NKCurrencyInfo, ExtensionMethods.ExchangeRatePublishedMessage);

			declaration.JE_ProcedureType = "B";
			validateExchangeRateNonPublishedMessage(declaration, charge, KRJobMessageTypeList.Codes.Export);
			validateExchangeRateNonPublishedMessage(declaration, charge, KRJobMessageTypeList.Codes.Import);
			validateExchangeRateNonPublishedMessage(declaration, charge, KRJobMessageTypeList.Codes.LocalExport);
		}

		static void validateExchangeRateNonPublishedMessage(JobDeclaration declaration, CommonNonApportionedCharge charge, string messageType)
		{
			declaration.JE_MessageType = messageType;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Iceland;
			charge.J7_ExchangeRate = 0;
			//charge.Validation.ValidateJ7_RX_NKCurrency();
			if (messageType == KRJobMessageTypeList.Codes.LocalExport)
			{
				AssertNoMessageErrorContaining(charge.J7_ExchangeRateInfo, ExtensionMethods.ExchangeRateNonPublishedMessage);
			}
			else
			{
				AssertHasMessageErrorContaining(charge.J7_ExchangeRateInfo, ExtensionMethods.ExchangeRateNonPublishedMessage);
			}
			charge.J7_ExchangeRate = 100;
			//charge.Validation.ValidateJ7_RX_NKCurrency();
			AssertNoMessageErrorContaining(charge.J7_ExchangeRateInfo, ExtensionMethods.ExchangeRateNonPublishedMessage);
		}
	}
}
