using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRCusHAWBValidationTest : AirCargoCusHAWBValidationAbstractTest
	{
		public void TestCheckCS_RX_NKGoodsCurrency()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_GoodsValue = 100;
			hawb.CS_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertHasWarning(hawb.CS_RX_NKGoodsCurrencyInfo, CMRCusHAWBValidation.MissingDepatureDate);
			mawb.CM_DepartureDate = ZDateTime.Today;
			hawb.Validation.ValidateCS_RX_NKGoodsCurrency();
			AssertNoWarning(hawb.CS_RX_NKGoodsCurrencyInfo, CMRCusHAWBValidation.MissingDepatureDate);
		}

		public void TestCheckCS_IsSpecialReporter()
		{
			hawb.CS_IsSpecialReporter = true;
			AssertHasMessageError(hawb.CS_IsSpecialReporterInfo, CMRCusHAWBValidation.GetSpecialReporterNumberEmptyMessage("HVLV"));
			Env.Registry.AUCustoms.HVLVSpecialReporterNumber = "123456";
			hawb.CS_IsSpecialReporter = false;
			hawb.CS_IsSpecialReporter = true;
			AssertNoMessageError(hawb.CS_IsSpecialReporterInfo, CMRCusHAWBValidation.GetSpecialReporterNumberEmptyMessage("HVLV"));
		}

		public void TestCheckCS_IsRemailReporter()
		{
			hawb.CS_IsRemailReporter = true;
			AssertHasMessageError(hawb.CS_IsRemailReporterInfo, CMRCusHAWBValidation.RemailReporterMustBeAtSubMawbLevel);
			AssertHasMessageError(hawb.CS_IsRemailReporterInfo, CMRCusHAWBValidation.GetSpecialReporterNumberEmptyMessage("Remail"));
			Env.Registry.AUCustoms.RemailSpecialReporterNumber = "123456";
			hawb.CS_IsMasterHouse = true;
			hawb.CS_IsRemailReporter = false;
			hawb.CS_IsRemailReporter = true;
			AssertNoMessageError(hawb.CS_IsRemailReporterInfo, CMRCusHAWBValidation.RemailReporterMustBeAtSubMawbLevel);
			AssertNoMessageError(hawb.CS_IsRemailReporterInfo, CMRCusHAWBValidation.GetSpecialReporterNumberEmptyMessage("Remail"));
			hawb.CS_IsSpecialReporter = true;
			hawb.CS_IsRemailReporter = false;
			hawb.CS_IsRemailReporter = true;
			AssertHasMessageError(hawb.CS_IsRemailReporterInfo, CMRCusHAWBValidation.RemailAndHVLVCannotBeBothReported);

			hawb.CS_IsSpecialReporter = false;
			hawb.CS_IsRemailReporter = false;
			hawb.CS_IsRemailReporter = true;
			AssertNoMessageError(hawb.CS_IsRemailReporterInfo, CMRCusHAWBValidation.RemailAndHVLVCannotBeBothReported);
		}

		public override void TestValidateHAWB()
		{
			hawb.Validation.ValidateCS_HAWB();
			AssertHasErrors("by default", hawb.CS_HAWBInfo);
			hawb.CS_HAWB = "AOEUAO";
			AssertNoNotifications("when entered", hawb.CS_HAWBInfo);
		}

		public void TestValidateHAWBWithConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			mawb.CM_JK = consol.PK;
			hawb.CS_HAWB = ZString.Empty;
			AssertHasErrors("Has error", hawb.CS_HAWBInfo);
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			hawb.Validation.ValidateCS_HAWB();
			AssertNoErrors("no error on directs", hawb.CS_HAWBInfo);
		}

		public void TestSomePartOfConsigneeAddressIsEnteredAndAlpha()
		{
			AssertConsignxxAddress(hawb.CS_ConsigneeStreetInfo);
			AssertConsignxxAddress(hawb.CS_ConsigneeStreet2Info);
			AssertConsignxxAddress(hawb.CS_ConsigneeCityInfo);
			AssertConsignxxAddress(hawb.CS_ConsigneePostcodeInfo);
			AssertConsignxxAddress(hawb.CS_ConsigneeStateInfo);
			AssertConsignxxAddress(hawb.CS_RN_NKConsigneeCountryInfo);
		}

		public void TestSomePartOfConsignorAddressIsEnteredAndAlpha()
		{
			AssertConsignxxAddress(hawb.CS_ConsignorStreetInfo);
			AssertConsignxxAddress(hawb.CS_ConsignorStreet2Info);
			AssertConsignxxAddress(hawb.CS_ConsignorCityInfo);
			AssertConsignxxAddress(hawb.CS_ConsignorPostcodeInfo);
			AssertConsignxxAddress(hawb.CS_ConsignorStateInfo);
			AssertConsignxxAddress(hawb.CS_RN_NKConsignorCountryInfo);
		}

		public void TestEmptyGoodsValueWarned()
		{
			hawb.CS_GoodsValue = 0m;
			AssertEquals("Zero amount will be declared as no commercial value consignment", true, hawb.CS_GoodsValueInfo.HasWarnings());

			hawb.CS_GoodsValue = 100m;
			AssertEquals("Zero amount will be declared as no commercial value consignment", false, hawb.CS_GoodsValueInfo.HasWarnings());
		}

		public void TestKeyMessagingFields()
		{
			string oldHAWBID = "62342";
			hawb.CS_HAWB = oldHAWBID;
			hawb.CMRMessageStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			Factory.Save();
			hawb.CS_HAWB = "23423";
			Assert("Should have an error", hawb.CS_HAWBInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", oldHAWBID)));
			hawb.CS_HAWB = oldHAWBID;
			Assert("Should have no errors", !hawb.CS_HAWBInfo.HasErrors());
		}

		public void Test1stPortIsNotRequired()
		{
			hawb.MAWB.CM_RL_NKLoadPort = "CNSHA";
			hawb.MAWB.CM_RL_NKDischargePort = "AUBNE";
			hawb.MAWB.CM_RL_NKFirstArrivalPort = "";
			hawb.CS_RL_NKOrigin = "HKHKG";
			hawb.CS_RL_NKDestination = "PGPOM";
			AssertNoMessageErrors(hawb.CS_RL_NKDestinationInfo);
		}

		public void TestIsSelfAssessedClearanceUnderThreshold()
		{
			TaxOrFeeTestHelper.SetUp();
			var deminimus = UniversalReferenceHelper.GetDeminimus(Factory);

			hawb.CS_GoodsValue = deminimus + 1;
			hawb.CS_RX_NKGoodsCurrency = JobDeclaration.LocalCurrencyConstantCode;

			hawb.CS_IsSelfAssessedClearance = true;
			hawb.MAWB.CM_DepartureDate = ZDateTime.Today;
			AssertEquals("Goods value > threshold", true, hawb.CS_IsSelfAssessedClearanceInfo.HasWarnings());

			CurrencyConverter converter = CurrencyConverter.New(Factory, ZDateTime.Now, ZArchitecture.Core.ExchangeRateType.All, 7);
			Money goodsValueInUSDCurrency = converter.ConvertExact(new Money(hawb.CS_GoodsValue, hawb.GoodsCurrency), USDCurrency);
			AssertEquals(string.Format("PreCondition: USD Amount {0} should be less than Local Amount {1}", goodsValueInUSDCurrency.Amount, hawb.CS_GoodsValue), true, goodsValueInUSDCurrency.Amount < hawb.CS_GoodsValue);

			hawb.CS_IsSelfAssessedClearance = false;
			AssertEquals("PreCondition: CS_IsSelfAssessedClearanceInfo should have not warning", false, hawb.CS_IsSelfAssessedClearanceInfo.HasWarnings());
			hawb.CS_GoodsValue = goodsValueInUSDCurrency.Amount;
			hawb.CS_IsSelfAssessedClearance = true;
			AssertEquals("Goods value > threshold", false, hawb.CS_IsSelfAssessedClearanceInfo.HasWarnings());

			hawb.CS_IsSelfAssessedClearance = false;
			AssertEquals("PreCondition: CS_IsSelfAssessedClearanceInfo should have not warning", false, hawb.CS_IsSelfAssessedClearanceInfo.HasWarnings());
			hawb.CS_RX_NKGoodsCurrency = goodsValueInUSDCurrency.Currency.Code;
			hawb.CS_IsSelfAssessedClearance = true;
			AssertEquals("Goods value > threshold", true, hawb.CS_IsSelfAssessedClearanceInfo.HasWarnings());
		}

		public void TestIsSelfAssessedClearanceWithMasterHouse()
		{
			hawb.CS_IsMasterHouse = true;
			hawb.CS_IsSelfAssessedClearance = true;
			AssertEquals("For co-load master shipment, SAC is not allowed", true, hawb.CS_IsSelfAssessedClearanceInfo.HasMessageErrors());

			hawb.CS_IsSelfAssessedClearance = false;
			AssertEquals("For co-load master shipment, SAC is not allowed", false, hawb.CS_IsSelfAssessedClearanceInfo.HasMessageErrors());
		}

		public void TestIsSelfAssessedClearanceWithThesaurusWord()
		{
			CMRReferenceFilesTestHelper.InsertThesaurusData(Factory, "Traditional medicine", "Medicine", "Pill");
			hawb.CS_GoodsDescription = "Traditional medicine";
			hawb.CS_IsSelfAssessedClearance = true;
			AssertHasWarning("Goods Description in Thesaurus", hawb.CS_IsSelfAssessedClearanceInfo, "Self-assessed clearance is not allowed as the goods description contains the following words (Medicine, Traditional medicine) which are found in the Thesaurus provided by Customs.");

			hawb.CS_IsSelfAssessedClearance = false;
			AssertNoWarnings("Goods Description in Thesaurus", hawb.CS_IsSelfAssessedClearanceInfo);

			hawb.CS_GoodsDescription = "WHAT IS THIS";
			hawb.CS_IsSelfAssessedClearance = true;
			AssertNoWarnings("Goods Description in Thesaurus", hawb.CS_IsSelfAssessedClearanceInfo);

			using (AUCustomsDataRegistry.Instance.ManifestSACOverride.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				hawb.CS_GoodsDescription = "Pills";
				hawb.CS_IsSelfAssessedClearance = true;
				AssertHasWarning(hawb.CS_IsSelfAssessedClearanceInfo, "Self-assessed clearance has been indicated here as the registry to ignore the Thesaurus check is activated.\r\nHowever, the following words in the goods description, (Pill), are found in the Thesaurus provided by Customs.");
			}
		}

		public void TestPrepaidCollectIsRequiredWhenGoodsValueIsGreaterThanZero()
		{
			hawb.CS_GoodsValue = 100m;
			hawb.CS_FreightPrepaidCollect = "";
			AssertEquals("Freight prepaid collect is mandatory if goods value > 0", true, hawb.CS_FreightPrepaidCollectInfo.HasMessageErrors());
		}

		public void TestGoodsCurrencyRequiredWhenGoodsValueIsGreaternThanZero()
		{
			hawb.CS_RX_NKGoodsCurrency = "";
			AssertNoMessageErrors("Currency is not required", hawb.CS_RX_NKGoodsCurrencyInfo);

			hawb.CS_GoodsValue = 100m;
			hawb.CS_RX_NKGoodsCurrency = "";
			AssertHasMessageErrors("Currency is required", hawb.CS_RX_NKGoodsCurrencyInfo);
		}

		public void TestGoodsCurrencyWhenNotInList()
		{
			AssertEquals("PreCondition - No message errors", false, hawb.CS_RX_NKGoodsCurrencyInfo.HasMessageErrors());
			hawb.CS_RX_NKGoodsCurrency = "BSD";
			AssertEquals("Currency is not valid", true, hawb.CS_RX_NKGoodsCurrencyInfo.HasMessageErrors());
			hawb.CS_RX_NKGoodsCurrency = "AUD";
			AssertEquals("Currency is valid", false, hawb.CS_RX_NKGoodsCurrencyInfo.HasMessageErrors());
		}

		public void TestValidateFreightPrepaidCollect()
		{
			hawb.Validation.ValidateCS_FreightPrepaidCollect();
			AssertNoNotifications("by default", hawb.CS_FreightPrepaidCollectInfo);

			hawb.CS_GoodsValue = 1;
			hawb.Validation.ValidateCS_FreightPrepaidCollect();
			AssertHasMessageErrors("when goodsvalue > 0", hawb.CS_FreightPrepaidCollectInfo);

			hawb.CS_FreightPrepaidCollect = CMRMethodsOfPayment.Codes.Collect;

			hawb.CS_FreightPrepaidCollect = "~~";
			AssertHasMessageErrors("When invalid", hawb.CS_FreightPrepaidCollectInfo);
		}

		RefCurrency USDCurrency
		{
			get
			{
				if (fUSDCurrency == null)
				{
					fUSDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
					AssertNotNull("PreCondition: USD currency should exist in database", fUSDCurrency);
					RefExchangeRate exchangeRate = fUSDCurrency.ExchangeRates.AddNew();
					exchangeRate.RE_StartDate = ZDateTime.Today;
					exchangeRate.RE_ExRateType = "CUS";
					exchangeRate.RE_SellRate = 0.64M;
				}
				return fUSDCurrency;
			}
		}

		RefCurrency fUSDCurrency;

		CusMAWB mawb;
		CusHAWB hawb;
		protected override void SetUp()
		{
			base.SetUp();
			mawb = Factory.New<CusMAWB>();
			hawb = mawb.ChildBills.AddNew();

			mawb.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			AssertEquals("House bill validation object", typeof(CMRCusHAWBValidation), hawb.Validation.GetType());
		}

		void AssertConsignxxAddress(ZPropertyInfo info)
		{
			info.Value = new ZString("12");
			AssertHasMessageErrors("Must be alpha character", info);
			info.Value = ZString.Empty;
			AssertHasMessageErrors("Some part of address must be entered", info);
			info.Value = new ZString("A1");
			AssertNoMessageErrors("Some part of address must be entered", info);
			info.Value = ZString.Empty;
		}
	}
}
