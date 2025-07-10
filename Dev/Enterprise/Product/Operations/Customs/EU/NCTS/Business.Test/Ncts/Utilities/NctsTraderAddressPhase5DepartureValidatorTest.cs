using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsTraderAddressPhase5DepartureValidatorTest : TestCaseWithFactory
	{
		public void TestNctsTraderAddressPhase5DepartureValidatorConstructor()
		{
			var address = Factory.New<JobDocAddress>();
			AssertExceptionThrown<ArgumentNullException>("When address is null", () => new NctsTraderAddressPhase5DepartureValidator(address: null, traderName: "Trader"));
			AssertExceptionThrown<ArgumentNullException>("When traderName is null", () => new NctsTraderAddressPhase5DepartureValidator(address: address, traderName: null));
			AssertExceptionThrown<ArgumentException>("When traderName is empty", () => new NctsTraderAddressPhase5DepartureValidator(address: address, traderName: ""));
			AssertNoExceptionThrown("When declaration is not null", () => new NctsTraderAddressPhase5DepartureValidator(address: address, traderName: "Trader"));
		}

		public void TestCheckRuleE1104_1HeaderPrincipalName()
		{
			AssertMaximumNameLengthInAndOutTransitionPeriod(h => h.Principal, "Principal");
		}

		public void TestCheckRuleE1104_1HeaderPrincipalAddress()
		{
			AssertMaximumAddressLengthInAndOutTransitionPeriod(h => h.Principal, "Principal");
		}

		public void TestCheckRuleE1104_1HeaderConsignorName()
		{
			AssertMaximumNameLengthInAndOutTransitionPeriod(h => h.Consignor, "Consignor");
		}

		public void TestCheckRuleE1104_1HeaderConsignorAddress()
		{
			AssertMaximumAddressLengthInAndOutTransitionPeriod(h => h.Consignor, "Consignor");
		}

		public void TestCheckRuleE1104_1HeaderConsigneeName()
		{
			AssertMaximumNameLengthInAndOutTransitionPeriod(h => h.Consignee, "Consignee");
		}

		public void TestCheckRuleE1104_1HeaderConsigneeAddress()
		{
			AssertMaximumAddressLengthInAndOutTransitionPeriod(h => h.Consignee, "Consignee");
		}

		public void TestCheckRuleE1104_1DetailConsigneeName_TransitionPeriodOFF()
		{
			const string expectedMessage = "[E1104-1] Consignee name is longer than 70 characters, it will be truncated in the message.";
			address.CompanyName = new string('C', 71);

			nctsHeader.BH_ApplicationCode = "NC5";
			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleE1104_1Active));
				CombineAssertions("For Phase 5, rule enable", () =>
				{
					goodsItem.Consignee.E2_OA_Address = address.PK;
					AssertHasWarning("When Company name exceeds max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

					address.CompanyName = new string('C', 68);
					goodsItem.Consignee.Validation.ValidateE2_OA_Address();
					AssertNoWarning("When Company name less than max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);
				});

				nctsHeader.BH_ApplicationCode = "NCT";
				CombineAssertions("For Phase 4, rule enable", () =>
				{
					address.CompanyName = new string('C', 71);
					goodsItem.Consignee.Validation.ValidateE2_OA_Address();
					AssertNoWarning("When Company name exceeds max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

					address.CompanyName = new string('C', 68);
					goodsItem.Consignee.Validation.ValidateE2_OA_Address();
					AssertNoWarning("When Company name less than max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);
				});

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleE1104_1Active));
				CombineAssertions("For Phase 5, rule disable", () =>
				{
					address.CompanyName = new string('C', 71);
					goodsItem.Consignee.Validation.ValidateE2_OA_Address();
					AssertNoWarning("When Company name exceeds max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);
				});
			}
		}

		public void TestCheckRuleE1104_1DetailConsigneeName_TransitionPeriodON()
		{
			const string expectedMessage = "[E1104-1] Consignee name is longer than 35 characters, it will be truncated in the message.";

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleE1104_1Active));

				using (TemporarilySetTransitionPeriod(isActive: true))
				{
					address.CompanyName = new string('C', 37);
					nctsHeader.BH_ApplicationCode = "NC5";
					CombineAssertions("For Phase 5, rule enable", () =>
					{
						goodsItem.Consignee.E2_OA_Address = address.PK;
						AssertHasWarning("When Company name exceeds max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

						address.CompanyName = new string('C', 33);
						goodsItem.Consignee.Validation.ValidateE2_OA_Address();
						AssertNoWarning("When Company name less than max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);
					});

					nctsHeader.BH_ApplicationCode = "NCT";
					CombineAssertions("For Phase 4, rule enable", () =>
					{
						address.CompanyName = new string('C', 36);
						goodsItem.Consignee.Validation.ValidateE2_OA_Address();
						AssertNoWarning("When Company name exceeds max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

						address.CompanyName = new string('C', 34);
						goodsItem.Consignee.Validation.ValidateE2_OA_Address();
						AssertNoWarning("When Company name less than max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);
					});

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleE1104_1Active));
					address.CompanyName = new string('C', 37);
					nctsHeader.BH_ApplicationCode = "NC5";
					CombineAssertions("For Phase 5, rule disable", () =>
					{
						goodsItem.Consignee.E2_OA_Address = address.PK;
						AssertNoWarning("When Company name exceeds max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);
					});
				}
			}
		}

		public void TestCheckRuleE1104_1DetailConsigneeAddress_TransitionPeriodOFF()
		{
			const string expectedMessage = "[E1104-1] Consignee address is longer than 70 characters, it will be truncated in the message.";
			address.OA_Address1 = new string('A', 50);
			address.OA_Address2 = new string('A', 50);

			nctsHeader.BH_ApplicationCode = "NC5";
			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleE1104_1Active));
				CombineAssertions("For Phase 5, rule enable", () =>
				{
					goodsItem.Consignee.E2_OA_Address = address.PK;
					AssertHasWarning("When Address exceeds max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

					address.OA_Address1 = new string('A', 35);
					address.OA_Address2 = new string('A', 35);
					goodsItem.Consignee.Validation.ValidateE2_OA_Address();
					AssertHasWarning("When Address exceeds max length after space added between Address1 and Address2", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

					address.OA_Address1 = $"{new string('A', 34)} ";
					address.OA_Address2 = new string('A', 35);
					goodsItem.Consignee.Validation.ValidateE2_OA_Address();
					AssertNoWarning("When Address1 have edge with white space", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

					address.OA_Address1 = new string('A', 35);
					address.OA_Address2 = $" {new string('A', 34)}";
					goodsItem.Consignee.Validation.ValidateE2_OA_Address();
					AssertNoWarning("When Address2 have edge with white space", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

					address.OA_Address2 = new string('A', 20);
					goodsItem.Consignee.Validation.ValidateE2_OA_Address();
					AssertNoWarning("When Address less than max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);
				});

				nctsHeader.BH_ApplicationCode = "NCT";
				CombineAssertions("For Phase 4, rule enable", () =>
				{
					address.OA_Address2 = new string('A', 50);
					goodsItem.Consignee.Validation.ValidateE2_OA_Address();
					AssertNoWarning("When Address exceeds max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

					address.OA_Address2 = new string('A', 15);
					goodsItem.Consignee.Validation.ValidateE2_OA_Address();
					AssertNoWarning("When Address less than max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);
				});

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleE1104_1Active));
				CombineAssertions("For Phase 5, rule disable", () =>
				{
					address.OA_Address1 = new string('A', 50);
					address.OA_Address2 = new string('A', 50);

					nctsHeader.BH_ApplicationCode = "NC5";
					AssertNoWarning("When Address exceeds max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

					address.OA_Address1 = new string('A', 35);
					address.OA_Address2 = new string('A', 35);
					goodsItem.Consignee.Validation.ValidateE2_OA_Address();
					AssertNoWarning("When Address exceeds max length after space added between Address1 and Address2", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);
				});
			}
		}

		public void TestCheckRuleE1104_1DetailConsigneeAddress_TransitionPeriodON()
		{
			const string expectedMessage = "[E1104-1] Consignee address is longer than 35 characters, it will be truncated in the message.";
			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleE1104_1Active));
				using (TemporarilySetTransitionPeriod(isActive: true))
				{
					address.OA_Address1 = new string('A', 20);
					address.OA_Address2 = new string('A', 20);

					nctsHeader.BH_ApplicationCode = "NC5";
					CombineAssertions("For Phase 5, rule enable", () =>
					{
						goodsItem.Consignee.E2_OA_Address = address.PK;
						AssertHasWarning("When Address exceeds max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

						address.OA_Address1 = new string('A', 20);
						address.OA_Address2 = new string('A', 15);
						goodsItem.Consignee.Validation.ValidateE2_OA_Address();
						AssertHasWarning("When Address exceeds max length after space added between Address1 and Address2", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

						address.OA_Address1 = $"{new string('A', 19)} ";
						address.OA_Address2 = new string('A', 15);
						goodsItem.Consignee.Validation.ValidateE2_OA_Address();
						AssertNoWarning("When Address1 have edge with white space", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

						address.OA_Address1 = new string('A', 15);
						address.OA_Address2 = $" {new string('A', 19)}";
						goodsItem.Consignee.Validation.ValidateE2_OA_Address();
						AssertNoWarning("When Address2 have edge with white space", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

						address.OA_Address2 = new string('A', 10);
						goodsItem.Consignee.Validation.ValidateE2_OA_Address();
						AssertNoWarning("When Address less than max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);
					});

					nctsHeader.BH_ApplicationCode = "NCT";
					CombineAssertions("For Phase 4, rule enable", () =>
					{
						address.OA_Address2 = new string('A', 20);
						goodsItem.Consignee.Validation.ValidateE2_OA_Address();
						AssertNoWarning("When Address exceeds max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

						address.OA_Address2 = new string('A', 15);
						goodsItem.Consignee.Validation.ValidateE2_OA_Address();
						AssertNoWarning("When Address less than max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);
					});

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleE1104_1Active));
					address.OA_Address1 = new string('A', 20);
					address.OA_Address2 = new string('A', 20);

					nctsHeader.BH_ApplicationCode = "NC5";
					CombineAssertions("For Phase 5, rule disable", () =>
					{
						goodsItem.Consignee.E2_OA_Address = address.PK;
						AssertNoWarning("When Address exceeds max length", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);

						address.OA_Address1 = new string('A', 20);
						address.OA_Address2 = new string('A', 15);
						goodsItem.Consignee.Validation.ValidateE2_OA_Address();
						AssertNoWarning("When Address exceeds max length after space added between Address1 and Address2", goodsItem.Consignee.E2_OA_AddressInfo, expectedMessage);
					});
				}
			}
		}

		void AssertMaximumNameLengthInAndOutTransitionPeriod(Func<NctsHeader, JobDocAddress> traderFunc, string traderName)
		{
			using (TemporarilySetTransitionPeriod(isActive: true))
			{
				SetupAndAssertTraderNameExceedsMaximumLength(traderFunc, 35, traderName);
			}

			using (TemporarilySetTransitionPeriod(isActive: false))
			{
				SetupAndAssertTraderNameExceedsMaximumLength(traderFunc, 70, traderName);
			}
		}

		void AssertMaximumAddressLengthInAndOutTransitionPeriod(Func<NctsHeader, JobDocAddress> traderFunc, string traderName)
		{
			using (TemporarilySetTransitionPeriod(isActive: true))
			{
				SetupAndAssertTraderAddressExceedsMaximumLength(traderFunc, 35, traderName);
			}

			using (TemporarilySetTransitionPeriod(isActive: false))
			{
				SetupAndAssertTraderAddressExceedsMaximumLength(traderFunc, 70, traderName);
			}
		}

		void SetupAndAssertTraderNameExceedsMaximumLength(Func<NctsHeader, JobDocAddress> traderFunc, int allowedStringMaxLength, string traderName)
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var allowedLengthString = new string('a', allowedStringMaxLength);
			var exceedingLengthString = new string('a', allowedStringMaxLength + 1);
			var expectedWarningError = $"[E1104-1] {traderName} name is longer than {allowedStringMaxLength} characters, it will be truncated in the message";

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.CustomsCodes.AddNew("EOR", "EOR CODE", "IT");
			organization.MainAddress.OA_RN_NKCountryCode = "IT";
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = organization.PK;

			var trader = traderFunc.Invoke(nctsHeader);

			trader.OrganisationPK = Factory.New<JobDocAddress>().PK;
			trader.E2_OA_Address = orgAddress.PK;

			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleE1104_1Active));

					orgAddress.CompanyName = string.Empty;
					trader.Validation.ValidateAll();
					AssertNoWarningContaining("NSTD5 and Name is empty", trader.E2_OA_AddressInfo, expectedWarningError);

					orgAddress.CompanyName = allowedLengthString;
					trader.Validation.ValidateAll();
					AssertNoWarningContaining("NSTD5 and Name not exceeding max length", trader.E2_OA_AddressInfo, expectedWarningError);

					orgAddress.CompanyName = exceedingLengthString;
					trader.Validation.ValidateAll();
					AssertHasWarningContaining("NSTD5 and Name exceeding max length", trader.E2_OA_AddressInfo, expectedWarningError);

					var headerArrival = CreateHeaderArrival();
					var traderArrival = InitializeTrader(headerArrival, traderFunc, orgAddress);
					traderArrival.Validation.ValidateAll();
					AssertNoWarningContaining("NSTS5 and Name exceeding max length", traderArrival.E2_OA_AddressInfo, expectedWarningError);

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					trader.Validation.ValidateAll();
					AssertNoWarningContaining("NSTD4 and Name exceeding max length", trader.E2_OA_AddressInfo, expectedWarningError);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleE1104_1Active));
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					orgAddress.CompanyName = exceedingLengthString;
					trader.Validation.ValidateAll();
					AssertNoWarningContaining("NSTD5 and Name exceeding max length, but Rule disable", trader.E2_OA_AddressInfo, expectedWarningError);
				}
			});
		}

		void SetupAndAssertTraderAddressExceedsMaximumLength(Func<NctsHeader, JobDocAddress> traderFunc, int allowedStringMaxLength, string traderName)
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var halfLength1 = (int)Math.Floor(allowedStringMaxLength * .5);
			var halfLength2 = allowedStringMaxLength - halfLength1 - 1;

			var allowedLengthString1 = new string('a', halfLength1);
			var allowedLengthString2 = new string('a', halfLength2);
			var exceedingLengthString = new string('a', halfLength2 + 1);
			var expectedWarningError = $"[E1104-1] {traderName} address is longer than {allowedStringMaxLength} characters, it will be truncated in the message";

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = organization.PK;

			var trader = InitializeTrader(nctsHeader, traderFunc, orgAddress);

			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleE1104_1Active));
					orgAddress.Address1 = string.Empty;
					orgAddress.Address2 = string.Empty;
					trader.Validation.ValidateAll();
					AssertNoWarningContaining("NSTD5 and 'Address As A Single Line' is empty", trader.E2_OA_AddressInfo, expectedWarningError);

					orgAddress.Address1 = allowedLengthString1;
					orgAddress.Address2 = allowedLengthString2;
					trader.Validation.ValidateAll();
					AssertNoWarningContaining("NSTD5 and 'Address As A Single Line' not exceeding max length", trader.E2_OA_AddressInfo, expectedWarningError);

					orgAddress.Address1 = allowedLengthString1;
					orgAddress.Address2 = exceedingLengthString;
					trader.Validation.ValidateAll();
					AssertHasWarningContaining("NSTD5 and 'Address As A Single Line' exceeding max length", trader.E2_OA_AddressInfo, expectedWarningError);

					var headerArrival = CreateHeaderArrival();
					var traderArrival = InitializeTrader(headerArrival, traderFunc, orgAddress);
					traderArrival.Validation.ValidateAll();
					AssertNoWarningContaining("NSTS5 and 'Address As A Single Line' exceeding max length", traderArrival.E2_OA_AddressInfo, expectedWarningError);

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					trader.Validation.ValidateAll();
					AssertNoWarningContaining("NSTD4 and 'Address As A Single Line' exceeding max length", trader.E2_OA_AddressInfo, expectedWarningError);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleE1104_1Active));
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					orgAddress.Address1 = allowedLengthString1;
					orgAddress.Address2 = exceedingLengthString;
					trader.Validation.ValidateAll();
					AssertNoWarningContaining("NSTD5 and 'Address As A Single Line' exceeding max length, but Rule disable", trader.E2_OA_AddressInfo, expectedWarningError);
				}
			});
		}

		JobDocAddress InitializeTrader(NctsHeader nctsHeader, Func<NctsHeader, JobDocAddress> traderFunc, OrgAddress orgAddress)
		{
			var trader = traderFunc.Invoke(nctsHeader);

			trader.OrganisationPK = Factory.New<JobDocAddress>().PK;
			trader.E2_OA_Address = orgAddress.PK;
			return trader;
		}

		NctsHeader CreateHeaderArrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			return nctsHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			address = orgHeader.MainAddress;
		}

		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;
		OrgAddress address;

		IDisposable TemporarilySetTransitionPeriod(bool isActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);
	}
}
