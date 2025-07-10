using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderPhase5ValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckBM_AdditionalDeclarationType_MandatoryValidation()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_AdditionalDeclarationType = string.Empty;
			AssertHasMessageErrorContaining("Message error for empty.", movementHeader.BM_AdditionalDeclarationTypeInfo, $"{NctsConstants.ValidationRuleMessagePrefixes.TR0017}{MandatoryValidation.YouHaveNotEntered}");

			movementHeader.BM_AdditionalDeclarationType = "A";
			AssertNoMessageErrors("Validation passes.", movementHeader.BM_AdditionalDeclarationTypeInfo);
		});
	}

	public void TestCheckBM_PlaceOfLoading_MandatoryValidation_WhenPortOfPresentationIsEnteredIsOptedOut()
	{
		movementHeader.BM_PortOfPresentationCode = "ITMIL";
		movementHeader.BM_PlaceOfLoading = "";
		AssertNoMessageErrors("Base EU mandatory validation when 'Port of Presentation' is entered is opted out", movementHeader.BM_PlaceOfLoadingInfo);
	}

	public void TestCheckGoodsLocationDescription_HasNotificationsFromAddress()
	{
		var cusGoodsLocation = movementHeader.GoodsLocation;
		cusGoodsLocation.CGL_Qualifier = "Y";
		cusGoodsLocation.CGL_Type = "D";
		cusGoodsLocation.Address.AuthorisationNumber = ZString.Empty;

		movementHeader.Validation.ValidateGoodsLocationDescription();
		var addressNotificationsCount = cusGoodsLocation.Address.Notifications.Count();
		AssertNotEquals("Address should have a notification", 0, addressNotificationsCount);

		var expectedErrorMessage = "There are errors within the 'Location of Goods', please click on 'More..' to view the error information.";
		AssertHasMessageErrorContaining("GoodsLocationDescription", movementHeader.GoodsLocationDescriptionInfo, expectedErrorMessage);
	}

	public void TestBaseEUCheckBM_ForeignDestPortKCodeMandatoryIsDisabled()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_TypeOfSecurity = "EXI";
			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
			{
				movementHeader.Validation.ValidateBM_ForeignDestPortKCode();
				AssertNoMessageErrors("When TransitionPeriod OFF", movementHeader.BM_ForeignDestPortKCodeInfo);
			}

			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
			{
				movementHeader.BM_SpecificCircumstance = NctsSpecificCircumstanceIndicatorList.Codes.XXX;
				movementHeader.Validation.ValidateBM_ForeignDestPortKCode();
				AssertNoMessageErrors("When TransitionPeriod ON", movementHeader.BM_ForeignDestPortKCodeInfo);
			}
		});
	}

	public void TestValidateTirCarnetExpiryDate()
	{
		movementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
		movementHeader.Validation.ValidateTirCarnetExpiryDate();
		AssertNoNotifications(movementHeader.TirCarnetExpiryDateInfo);
	}

	public void TestCheckConditionC901()
	{
		movementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		goodsItem.Validation.ValidateAll();
		AssertNoRowMessageErrorContaining(goodsItem, "C901");
	}

	public void TestCheckBM_MethodOfPayment_WhenGoodsItemHaveValues()
	{
		const string expectedSameLineErrorMessage = "The Transport Method of Payment declared in the header will be ignored because all the lines have the same value, which differs from the value declared in the header.";
		const string expectedDifferentLineErrorMessage = "The Transport Method of Payment declared in the header will be ignored because all lines have values, which different from the header one.";

		var bill1 = movementHeader.Header.Bills.AddNew();
		var bill2 = movementHeader.Header.Bills.AddNew();
		var goodsItem1 = bill1.GoodsItems.AddNew();
		var goodsItem2 = bill2.GoodsItems.AddNew();

		using (TemporarilySetTransitionPeriod(true))
		{
			TestBM_MethodOfPayment(
				"FOR TP ON : Goods Items",
				(payment) => movementHeader.BM_MethodOfPayment = payment,
				(payment) => goodsItem1.BY_TransportChargesMethodOfPayment = payment,
				(payment) => goodsItem2.BY_TransportChargesMethodOfPayment = payment,
				() => movementHeader.Validation.ValidateBM_MethodOfPayment(),
				expectedSameLineErrorMessage,
				expectedDifferentLineErrorMessage);
		}

		using (TemporarilySetTransitionPeriod(false))
		{
			goodsItem1.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
			goodsItem2.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cheque;
			movementHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.CreditCard;
			AssertNoWarnings("TP OFF: When Header filled and all entities have different values than Header", movementHeader.BM_MethodOfPaymentInfo);

			goodsItem1.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.CreditCard;
			goodsItem2.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.CreditCard;
			movementHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
			AssertNoWarnings("TP OFF: When Header filled and all entities have same values but different from Header", movementHeader.BM_MethodOfPaymentInfo);
		}
	}

	public void TestCheckBM_MethodOfPayment_WhenHousesHaveValues()
	{
		const string expectedSameLineErrorMessage = "The Transport MoP declared in the header will be ignored because all Houses have the same value, which differs from the value declared in the header.";
		const string expectedDifferentLineErrorMessage = "The Transport MoP declared in the header will be ignored because all Houses have values different from the header one.";

		var bill1 = movementHeader.Header.Bills.AddNew();
		var bill2 = movementHeader.Header.Bills.AddNew();

		movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		AssertEquals("[PRE CONDITION] Field is read only", true, movementHeader.BM_MethodOfPaymentInfo.ReadOnly);
		AssertNoMessageErrors("No warnings on header", movementHeader.BM_MethodOfPaymentInfo);

		movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		AssertEquals("[PRE CONDITION] Field is not read only", false, movementHeader.BM_MethodOfPaymentInfo.ReadOnly);

		using (TemporarilySetTransitionPeriod(false))
		{
			TestBM_MethodOfPayment(
				"For TP OFF : Houses",
				(payment) => movementHeader.BM_MethodOfPayment = payment,
				(payment) => bill1.B0_TransportPaymentMethod = payment,
				(payment) => bill2.B0_TransportPaymentMethod = payment,
				() => movementHeader.Validation.ValidateBM_MethodOfPayment(),
				expectedSameLineErrorMessage,
				expectedDifferentLineErrorMessage);
		}

		using (TemporarilySetTransitionPeriod(true))
		{
			bill1.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cash;
			bill2.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cheque;
			movementHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.CreditCard;
			AssertNoWarnings("TP ON: When Header filled and all entities have different values than Header", movementHeader.BM_MethodOfPaymentInfo);

			bill1.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.CreditCard;
			bill2.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.CreditCard;
			movementHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
			AssertNoWarnings("TP ON: When Header filled and all entities have same values but different from Header", movementHeader.BM_MethodOfPaymentInfo);
		}
	}

	public void TestCheckBM_TransportAtDepartureTrailer1RegNo()
	{
		var targetInfo = movementHeader.BM_TransportAtDepartureTrailer1RegNoInfo;
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		var inlandTransportList = movementHeader.InlandTransportList.AddNew();

		CombineAssertions(() =>
		{
			using (TemporarilySetTransitionPeriod(false))
			{
				AssertNoMessageErrors("When transition period is inactive, CY_Code and CY_Data is empty", targetInfo);

				inlandTransportList.CY_Code = "IT";
				AssertNoMessageErrors("When transition period is inactive, CY_Code is not empty and CY_Data is empty", targetInfo);

				inlandTransportList.CY_Data = "Test";
				AssertNoMessageErrors("When transition period is inactive, CY_Code is not empty and CY_Data is not empty", targetInfo);
			}
		});
	}

	void TestBM_MethodOfPayment(string entityType, Action<string> setHeaderPayment, Action<string> setFirstLinePayment, Action<string> setSecondLinePayment, Action validateMethod, string expectedSameLineErrorMessage, string expectedDifferentLineErrorMessage)
	{
		CombineAssertions($"{entityType}", () =>
		{
			setHeaderPayment(TransportChargesModeOfPayment.Codes.CreditCard);
			AssertNoWarnings("When Header filled and no entities added", movementHeader.BM_MethodOfPaymentInfo);

			setFirstLinePayment(TransportChargesModeOfPayment.Codes.CreditCard);
			setSecondLinePayment(TransportChargesModeOfPayment.Codes.CreditCard);
			setHeaderPayment(ZString.Empty);
			AssertNoWarnings("When Header empty and all entities have same values", movementHeader.BM_MethodOfPaymentInfo);

			setHeaderPayment(TransportChargesModeOfPayment.Codes.CreditCard);
			AssertNoWarnings("When Header filled and all entities have same values as Header", movementHeader.BM_MethodOfPaymentInfo);

			setHeaderPayment(TransportChargesModeOfPayment.Codes.Cash);
			AssertHasWarning("When Header filled and all entities have same values but different from Header", movementHeader.BM_MethodOfPaymentInfo, expectedSameLineErrorMessage);
			AssertNoWarning("When Header filled and all entities have same values but different from Header - No Different Line error will come", movementHeader.BM_MethodOfPaymentInfo, expectedDifferentLineErrorMessage);

			setSecondLinePayment(ZString.Empty);
			validateMethod();
			AssertNoWarnings("When Header filled and a few entities empty", movementHeader.BM_MethodOfPaymentInfo);

			setFirstLinePayment(ZString.Empty);
			validateMethod();
			AssertNoWarnings("When Header filled and all entities empty", movementHeader.BM_MethodOfPaymentInfo);

			setFirstLinePayment(TransportChargesModeOfPayment.Codes.CreditCard);
			setSecondLinePayment(TransportChargesModeOfPayment.Codes.Cash);
			setHeaderPayment(TransportChargesModeOfPayment.Codes.Cheque);
			AssertNoWarning("When Header filled and all entities have different values than Header - No same line error will come", movementHeader.BM_MethodOfPaymentInfo, expectedSameLineErrorMessage);
			AssertHasWarning("When Header filled and all entities have different values than Header", movementHeader.BM_MethodOfPaymentInfo, expectedDifferentLineErrorMessage);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		movementHeader = nctsHeader.MovementHeader;
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;

	IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isTransitionPeriodActive);
}
