using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsHeaderPhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckArrivalMrnFromUser_TR0047() => CombineAssertions(() =>
		{
			using (var ruleTestContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderArrivalPhase5ValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(x => x.IsRuleTR0047Active);
				var arrivalHeader = GetNewArrivalHeader();
				var departureHeader = Factory.New<NctsHeader>();
				departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
				departureHeader.LocalReferenceNumber = "DUPLICATE";
				departureHeader.BH_IsActive = true;
				departureHeader.ArrivalMrnFromUser = "ArrivalMrnFromUser";
				var secondArrivalHeader = GetNewArrivalHeader();
				secondArrivalHeader.ArrivalMrnFromUser = "ArrivalMrnFromUser";
				secondArrivalHeader.LocalReferenceNumber = "DUPLICATE";
				secondArrivalHeader.BH_IsActive = false;
				var thirdArrivalHeader = GetNewArrivalHeader();
				thirdArrivalHeader.BH_IsActive = true;
				thirdArrivalHeader.LocalReferenceNumber = "DUPLICATE";
				Factory.Save();

				var errorMessage = "[TR0047] An NCTS Arrival declaration with the same MRN number already exists on Job 'DUPLICATE'. Please check the MRN number to ensure that it is correct.";
				arrivalHeader.ArrivalMrnFromUser = "ArrivalMrnFromUser";
				AssertNoMessageError("expected no error, because no active Ncts Arrival exist with the MRN", arrivalHeader.ArrivalMrnFromUserInfo, errorMessage);

				thirdArrivalHeader.ArrivalMrnFromUser = "ArrivalMrnFromUser";
				Factory.Save();
				arrivalHeader.Validation.ValidateArrivalMrnFromUser();
				AssertHasMessageError("expected error, because an active Ncts Arrival exist with the MRN", arrivalHeader.ArrivalMrnFromUserInfo, errorMessage);

				ruleTestContext.DisableRule(x => x.IsRuleTR0047Active);
				arrivalHeader.Validation.ValidateArrivalMrnFromUser();
				AssertNoMessageError("expected no error, because while an active Ncts Arrival exist with the MRN, the rule is disabled", arrivalHeader.ArrivalMrnFromUserInfo, errorMessage);
			}
		});

		public void TestCheckDestinationCustomsOfficeCodeForArrival_TR0035() => CombineAssertions(() =>
		{
			using var deciderTestContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderArrivalPhase5ValidationDecider>(Factory);

			var messageError = "[TR0035] Invalid MRN Number. MRN Number should be exactly 18 Character Long.";
			deciderTestContext.EnableRule(x => x.IsRuleTR0035Active);

			var arrivalHeader = GetNewArrivalHeader();
			arrivalHeader.BH_IsActive = true;
			arrivalHeader.ArrivalMrnFromUser = "ArrivalMrnFrom";

			AssertHasMessageErrorContaining("When MRN is not empty and less then 18 chars", arrivalHeader.ArrivalMrnFromUserInfo, messageError);

			arrivalHeader.ArrivalMrnFromUser = "ArrivalMrnFromuser";
			AssertNoMessageErrorContaining("When MRN is not empty and equal to 18 chars", arrivalHeader.ArrivalMrnFromUserInfo, messageError);

			arrivalHeader.ArrivalMrnFromUser = ZString.Empty;
			AssertNoMessageErrorContaining("When MRN is empty", arrivalHeader.ArrivalMrnFromUserInfo, messageError);

			deciderTestContext.DisableRule(x => x.IsRuleTR0035Active);
			arrivalHeader.ArrivalMrnFromUser = "ArrivalMrnFrom";
			AssertNoMessageErrorContaining("No error when rule TR0035 is inactive", arrivalHeader.ArrivalMrnFromUserInfo, messageError);
		});

		public void TestCheckArrivalMrnFromUser()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			CombineAssertions(() =>
			{
				nctsHeader.ArrivalMrnFromUser = ZString.Empty;
				AssertHasMessageErrorContaining("When MRN is empty there should be an error", nctsHeader.ArrivalMrnFromUserInfo, MandatoryValidation.YouHaveNotEntered);

				nctsHeader.ArrivalMrnFromUser = "22BE10100012345678";
				AssertNoMessageErrorContaining("No message error when MRN is valid", nctsHeader.ArrivalMrnFromUserInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckArrivalMrnFromUser_Departure()
		{
			var departure = Factory.New<NctsHeader>();
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departure.ArrivalMrnFromUser = ZString.Empty;
			AssertEquals("MRN should not be validated on Departure movement", false, departure.ArrivalMrnFromUserInfo.HasMessageError("You have not entered a MRN."));
		}

		public void TestCheckArrivalMrnFromUserWhenArrivalDetailsAreReadOnly()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;

			CombineAssertions(() =>
			{
				nctsHeader.ArrivalMrnFromUser = ZString.Empty;
				AssertNoNotifications("MRN is empty", nctsHeader.ArrivalMrnFromUserInfo);

				nctsHeader.ArrivalMrnFromUser = "22BE10100012345678";
				AssertNoNotifications("MRN is valid", nctsHeader.ArrivalMrnFromUserInfo);
			});
		}

		public void TestCheckDestinationCustomsOfficeCode()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			CombineAssertions(() =>
			{
				AssertNoNotifications("No Destination Customs Office is filled", nctsHeader.DestinationCustomsOfficeCodeInfo);

				nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertNoNotifications("No validation on Customs Office because message status is SNT", nctsHeader.DestinationCustomsOfficeCodeInfo);
			});
		}

		public void TestValidateMaxGoodsItemsForAllBills()
		{
			const string error = "You are only allowed to enter a maximum of 1999 items per declaration.";
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill1 = nctsHeader.Bills.AddNew();
			var bill2 = nctsHeader.Bills.AddNew();
			var bill3 = nctsHeader.Bills.AddNew();

			for (var i = 0; i < 1998; i++)
			{
				bill1.GoodsItems.AddNew();
			}

			CombineAssertions("We are 1 under the maximum items so there should be no row errors for this.", () =>
			{
				AssertNoRowError("Bill 1", bill1, error);
				AssertNoRowError("Bill 2", bill2, error);
				AssertNoRowError("Bill 3", bill3, error);
			});

			bill3.GoodsItems.AddNew();

			CombineAssertions("We are exactly on the maximum items so there should be no row errors for this.", () =>
			{
				AssertNoRowError("Bill 1", bill1, error);
				AssertNoRowError("Bill 2", bill2, error);
				AssertNoRowError("Bill 3", bill3, error);
			});

			bill3.GoodsItems.AddNew();

			CombineAssertions("We've gone 1 over the maximum, so bills that actually have any items should show this message error.", () =>
			{
				AssertHasRowError("Bill 1 with items.", bill1, error);
				AssertNoRowError("Bill 2 without items", bill2, error);
				AssertHasRowError("Bill 3 with items.", bill3, error);
			});

			bill1.GoodsItems.Delete(bill1.GoodsItems.First());

			CombineAssertions("We are exactly on the maximum items so the row message errors should be removed.", () =>
			{
				AssertNoRowError("Bill 1", bill1, error);
				AssertNoRowError("Bill 2", bill2, error);
				AssertNoRowError("Bill 3", bill3, error);
			});

			bill2.GoodsItems.AddNew();

			CombineAssertions("We're over the limit and all 3 bills now have at least one item so the message error should appear on all of them.", () =>
			{
				AssertHasRowError("Bill 1 with items.", bill1, error);
				AssertHasRowError("Bill 2 with items.", bill2, error);
				AssertHasRowError("Bill 3 with items.", bill3, error);
			});

			nctsHeader.Bills.Delete(bill2);

			CombineAssertions("Deleting a whole bill should cause the validation to run again.", () =>
			{
				AssertNoRowError("Bill 1", bill1, error);
				AssertNoRowError("Bill 3", bill3, error);
			});
		}

		public void TestGetLocalReferenceNumber_Ncts5Arrival_Length()
		{
			const string message = "[TR0034] This field must have a value and the value must be longer than 4 characters.";
			using (var ruleTestContext = new MovementHeaderValidationDeciderTestContext<INctsArrivalMovementHeaderPhase5ValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(x => x.IsRuleTR0034Active);
				var header = GetNewArrivalHeader();
				header.LocalReferenceNumber = "1234";
				AssertHasMessageError(header.LocalReferenceNumberInfo, message);
				header.LocalReferenceNumber = "12345";
				AssertNoMessageError(header.LocalReferenceNumberInfo, message);

				ruleTestContext.DisableRule(x => x.IsRuleTR0034Active);
				header.LocalReferenceNumber = "1234";
				AssertNoMessageError(header.LocalReferenceNumberInfo, message);
			}
		}

		public void TestCheckBH_ExportFlag_RuleNR0015()
		{
			const string message = "[NR0015]";
			const string dataGroupingCode = "EUN";
			using var deciderTestContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderArrivalPhase5ValidationDecider>(Factory);

			deciderTestContext.EnableRule(x => x.IsRuleNR0015Active);
			var header = GetNewArrivalHeader();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, dataGroupingCode, ZDate.Today, true))
			{
				AssertNoMessageErrorContaining("BH_ExportFlag = N", header.BH_ExportFlagInfo, message);

				header.BH_ExportFlag = EventFlagList.Codes.Yes;
				AssertHasMessageErrorContaining("BH_ExportFlag = Y, No Incidents", header.BH_ExportFlagInfo, message);

				header.EnRouteIncidents.AddNew();
				header.Validation.ValidateBH_ExportFlag();
				AssertNoMessageErrorContaining("BH_ExportFlag = Y, Has Incident", header.BH_ExportFlagInfo, message);

				header.EnRouteIncidents.RemoveAll(e => true);
				deciderTestContext.DisableRule(x => x.IsRuleNR0015Active);

				header.Validation.ValidateBH_ExportFlag();
				AssertNoMessageErrorContaining("Rule disabled", header.BH_ExportFlagInfo, message);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, dataGroupingCode, ZDate.Today, false))
			{
				deciderTestContext.EnableRule(x => x.IsRuleNR0015Active);
				header.Validation.ValidateBH_ExportFlag();
				AssertNoMessageErrorContaining("Outside transition period", header.BH_ExportFlagInfo, message);
			}
		}

		public void TestCheckBH_OverrideFreightDefaults_Consol()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.JK_UniqueConsignRef = "C0001";
			sourceConsol.Shipments.AddNew().JS_UniqueConsignRef = "S0001";

			AssertCheckBH_OverrideFreightDefaults(sourceConsol);
		}

		public void TestCheckBH_OverrideFreightDefaults_Shipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001";

			AssertCheckBH_OverrideFreightDefaults(shipment);
		}

		void AssertCheckBH_OverrideFreightDefaults(BusinessObject source)
		{
			var message = "The 'Override Freight Defaults' must be ticked if you want to add a new house consignment, goods item, or package line.";

			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ParentID = source.PK;
			nctsHeader.BH_ParentTableCode = source.TablePrefix;
			nctsHeader.Synchroniser.Synchronise(true);

			var overrideFreightDefaultsInfo = nctsHeader.BH_OverrideFreightDefaultsInfo;
			var bills = nctsHeader.Bills;
			var goodsItems = bills[0].GoodsItems;
			var packages = (IBusinessObjectCollection)goodsItems[0].Packages;

			CombineAssertions(() =>
			{
				nctsHeader.BH_OverrideFreightDefaults = true;
				AssertNoError(overrideFreightDefaultsInfo, message);
				nctsHeader.BH_OverrideFreightDefaults = false;
				AssertNoError(overrideFreightDefaultsInfo, message);

				AssertCheckBH_OverrideFreightDefaults(nctsHeader, overrideFreightDefaultsInfo, packages, message);
				AssertCheckBH_OverrideFreightDefaults(nctsHeader, overrideFreightDefaultsInfo, goodsItems, message);
				AssertCheckBH_OverrideFreightDefaults(nctsHeader, overrideFreightDefaultsInfo, bills, message);
			});
		}

		void AssertCheckBH_OverrideFreightDefaults(NctsHeader nctsHeader, ZPropertyInfo overrideFreightDefaultsInfo, IBusinessObjectCollection collection, string message)
		{
			var validation = nctsHeader.Validation;
			var bo = collection.AddNew();

			validation.ValidateBH_OverrideFreightDefaults();
			AssertHasError(overrideFreightDefaultsInfo, message);

			nctsHeader.BH_OverrideFreightDefaults = true;
			AssertNoError(overrideFreightDefaultsInfo, message);

			collection.Delete(bo);
			validation.ValidateBH_OverrideFreightDefaults();
			AssertNoError(overrideFreightDefaultsInfo, message);

			nctsHeader.BH_OverrideFreightDefaults = false;
			AssertNoError(overrideFreightDefaultsInfo, message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		}

		NctsHeader GetNewArrivalHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			return header;
		}

		NctsHeader nctsHeader;
	}
}
