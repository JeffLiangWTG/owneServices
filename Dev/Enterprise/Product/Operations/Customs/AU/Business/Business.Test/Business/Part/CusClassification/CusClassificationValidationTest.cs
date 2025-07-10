using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusClassificationValidationTest : TestCaseWithFactory
	{
		public void TestValidateTariff()
		{
			Classification @class = Factory.New<Classification>();
			@class.CC_ClassificationType = Classification.ClassificationType.IMP;
			@class.CC_TariffNum = ZString.Empty;
			Assert("Has Errors", @class.CC_TariffNumInfo.HasErrors());
			@class.CC_TariffNum = "123";
			Assert("!Has MessageErrors", !@class.CC_TariffNumInfo.HasMessageErrors());
		}

		public void TestValidateInstrumentCode()
		{
			TestCaseHelper.ClearTable(CMRInstrument.Schema.TableName);

			Classification @class = Factory.New<Classification>();
			@class.CC_ClassificationType = Classification.ClassificationType.IMP;
			@class.CC_TariffNum = "2001.10.00 90";
			@class.InstrumentType = "BL";
			@class.TreatmentCode = "427";
			@class.InstrumentCode = "9840020";
			AssertEquals("!HasNotifications", true, @class.InstrumentCodeInfo.HasWarnings());

			CMRInstrument instrument = CMRInstrument.New(Factory);
			instrument.IN_Type = "BL";
			instrument.IN_Number = "9840020";
			instrument.IN_StartDate = ZDateTime.Today;

			@class.InstrumentCode = "9840020";
			AssertEquals("!HasNotifications", false, @class.InstrumentCodeInfo.HasWarnings());
		}

		public void TestCheckCC_TariffNum()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Export);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "34060000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Classification @class = Factory.New<Classification>();
				@class.CC_ClassificationType = Classification.ClassificationType.EXP;
				@class.CC_TariffNum = ZString.Empty;
				AssertHasError(@class.CC_TariffNumInfo, "Please enter a Tariff Number.");
				@class.CC_TariffNum = "3406.00.01";
				AssertNoError(@class.CC_TariffNumInfo, "Please enter a Tariff Number.");
				AssertHasMessageError(@class.CC_TariffNumInfo, "This tariff number does not exist.");
				@class.CC_TariffNum = "3406.00.00";
				AssertNoMessageError(@class.CC_TariffNumInfo, "This tariff number does not exist.");
				AssertNoError(@class.CC_TariffNumInfo, "The selected tariff is a partial tariff used only for navigation - Please select a complete tariff number.");
				@class.CC_TariffNum = "3406.00";
				AssertNoMessageError(@class.CC_TariffNumInfo, "This tariff number does not exist.");
				AssertHasError(@class.CC_TariffNumInfo, "The selected tariff is a partial tariff used only for navigation - Please select a complete tariff number.");
			}
		}

		public void TestCheckCC_TariffNum_AHECC()
		{
			var ahecc1 = Factory.New<AUCAHECC>();
			ahecc1.UA_AHECC = "3406.00";
			var ahecc2 = Factory.New<AUCAHECC>();
			ahecc2.UA_AHECC = "3406.00.00";

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Classification @class = Factory.New<Classification>();
				@class.CC_ClassificationType = Classification.ClassificationType.EXP;
				@class.CC_TariffNum = ZString.Empty;
				AssertHasError(@class.CC_TariffNumInfo, "Please enter a Tariff Number.");
				@class.CC_TariffNum = "3406.00.01";
				AssertNoError(@class.CC_TariffNumInfo, "Please enter a Tariff Number.");
				AssertHasMessageError(@class.CC_TariffNumInfo, "This tariff number does not exist.");
				@class.CC_TariffNum = "3406.00.00";
				AssertNoMessageError(@class.CC_TariffNumInfo, "This tariff number does not exist.");
				AssertNoError(@class.CC_TariffNumInfo, "The selected tariff is a partial tariff used only for navigation - Please select a complete tariff number.");
				@class.CC_TariffNum = "3406.00";
				AssertNoMessageError(@class.CC_TariffNumInfo, "This tariff number does not exist.");
				AssertHasError(@class.CC_TariffNumInfo, "The selected tariff is a partial tariff used only for navigation - Please select a complete tariff number.");
			}
		}
	}
}
