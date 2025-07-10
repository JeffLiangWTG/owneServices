using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(JPAFRBillsValidation))]
	sealed class JPAFRBillsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJPB_DG()
		{
			ResetAFRHeaderBill();

			var undg1 = DGSubstanceTestHelper.Create("1234", "a", "IMO");
			var undg2 = DGSubstanceTestHelper.Create("5678", "b", "IMO");

			var expectedError = ValidationConstants.Bill.DuplicatedUNDG(undg1);

			Bill.JPB_DG = undg1.PK;
			AssertNoMessageError(Bill.JPB_DGInfo, expectedError);

			Bill.UNDGs.AddNew().DI_DG = undg2.PK;
			Bill.Validation.ValidateJPB_DG();
			AssertNoMessageError(Bill.JPB_DGInfo, expectedError);

			Bill.UNDGs.AddNew().DI_DG = undg1.PK;
			Bill.Validation.ValidateJPB_DG();
			AssertHasMessageError(Bill.JPB_DGInfo, expectedError);
		}

		public void TestJPB_SpecialCargoCode()
		{
			ResetAFRHeaderBill();

			Bill.JPB_DG_NKSubstance = "XXX";
			Bill.JPB_SpecialCargoCode = string.Empty;
			AssertHasMessageErrorContaining(Bill.JPB_SpecialCargoCodeInfo, ValidationConstants.Bill.SpecialCargoCodeIsRequiredForDangerousGoods);
			Bill.JPB_SpecialCargoCode = "XXX";
			AssertNoMessageErrorContaining(Bill.JPB_SpecialCargoCodeInfo, ValidationConstants.Bill.SpecialCargoCodeIsRequiredForDangerousGoods);
			Bill.JPB_DG_NKSubstance = string.Empty;
			Bill.JPB_SpecialCargoCode = string.Empty;
			AssertNoMessageErrorContaining(Bill.JPB_SpecialCargoCodeInfo, ValidationConstants.Bill.SpecialCargoCodeIsRequiredForDangerousGoods);
			Bill.JPB_DG_NKSubstance = string.Empty;
			Bill.JPB_SpecialCargoCode = "XXX";
			AssertNoMessageErrorContaining(Bill.JPB_SpecialCargoCodeInfo, ValidationConstants.Bill.SpecialCargoCodeIsRequiredForDangerousGoods);
		}

		public void TestCheckJPB_BillNumber_NVOCC()
		{
			ResetAFRHeaderBill();
			Header.JPH_IsShippingLineEntry = false;

			Bill.JPB_BillNumber = ZString.Empty;
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.JPB_BillNumber = "Bill";
			AssertNoMessageErrorContaining(Bill.JPB_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Bill.BOLNumMissingNVOCCCode);

			Bill.JPB_BillNumber = "Bill,Number";
			AssertNoMessageErrorContaining(Bill.JPB_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill|Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill[Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill]Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill{Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill}Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill`Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill~Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill^Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill_Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			CombineAssertions(() =>
			{
				JPAFRRegistry.Instance.AFRNVOCCIDtoAddtoBillDuringSync.SetValue(Header.RegistryCompanyPK, Guid.Empty, Guid.Empty, "J07");

				Bill.JPB_BillNumber = "BillNumber";
				AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Bill.BOLNumberNotStartWithNVOCCCodeInRegistry("J07"));

				Bill.JPB_BillNumber = "J07BillNumber";
				AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Bill.BOLNumberNotStartWithNVOCCCodeInRegistry("J07"));

				Bill.JPB_BillNumber = "J07-BillNumber";
				AssertNoMessageErrors(Bill.JPB_BillNumberInfo);
				AssertNoErrors(Bill.JPB_BillNumberInfo);
				AssertNoWarnings(Bill.JPB_BillNumberInfo);
			});

			CombineAssertions(() =>
			{
				JPAFRRegistry.Instance.AFRNVOCCIDtoAddtoBillDuringSync.SetValue(Header.RegistryCompanyPK, Guid.Empty, Guid.Empty, "J0-7");

				Bill.JPB_BillNumber = "J0-7BillNumber";
				AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Bill.BOLNumberStartWithInvalidNVOCCCode);
			});

			CombineAssertions(() =>
			{
				JPAFRRegistry.Instance.AFRNVOCCIDtoAddtoBillDuringSync.SetValue(Header.RegistryCompanyPK, Guid.Empty, Guid.Empty, "");

				Bill.JPB_BillNumber = "J0-7BillNumber";
				AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Bill.BOLNumberStartWithInvalidNVOCCCode);

				Bill.JPB_BillNumber = "AAAABillNumber";
				AssertNoMessageErrors(Bill.JPB_BillNumberInfo);
				AssertNoErrors(Bill.JPB_BillNumberInfo);
				AssertNoWarnings(Bill.JPB_BillNumberInfo);

				Bill.JPB_BillNumber = "AAA-BillNumber";
				AssertNoMessageErrors(Bill.JPB_BillNumberInfo);
				AssertNoErrors(Bill.JPB_BillNumberInfo);
				AssertNoWarnings(Bill.JPB_BillNumberInfo);

				Bill.JPB_BillNumber = "AA--BillNumber";
				AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Bill.BOLNumberStartWithInvalidNVOCCCode);

				Bill.JPB_BillNumber = "A---BillNumber";
				AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Bill.BOLNumberStartWithInvalidNVOCCCode);
			});

			Bill.JPB_BillNumber = "BillNumber";
			AssertNoMessageErrors(Bill.JPB_BillNumberInfo);
			AssertNoErrors(Bill.JPB_BillNumberInfo);
			AssertNoWarnings(Bill.JPB_BillNumberInfo);

			Bill.Factory.Save();

			Bill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			Bill.JPB_ReleaseStatus = ZString.Empty;
			Bill.JPB_BillNumber = "NewBillNumber";
			AssertHasErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.ChangingFieldWhenMessagingIsInProgress("Bill Of Lading Number", "BillNumber", "NewBillNumber"));

			Bill.JPB_MessageStatus = ZString.Empty;
			Bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			Bill.JPB_BillNumber = "NewBillNumber";
			AssertHasErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.ChangingFieldWhenBillIsRegistered("Bill Of Lading Number", "BillNumber", "NewBillNumber"));

			Bill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			Bill.JPB_ReleaseStatus = ZString.Empty;
			Bill.JPB_BillNumber = "NewB";
			AssertHasErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.ChangingFieldWhenMessagingIsInProgress("Bill Of Lading Number", "BillNumber", "NewB"));

			Bill.JPB_MessageStatus = ZString.Empty;
			Bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			Bill.JPB_BillNumber = "NewB";
			AssertHasErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.ChangingFieldWhenBillIsRegistered("Bill Of Lading Number", "BillNumber", "NewB"));

			Bill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			Bill.JPB_ReleaseStatus = ZString.Empty;
			Bill.JPB_BillNumber = "New,BillNumber";
			AssertHasErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.ChangingFieldWhenMessagingIsInProgress("Bill Of Lading Number", "BillNumber", "New,BillNumber"));

			Bill.JPB_MessageStatus = ZString.Empty;
			Bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			Bill.JPB_BillNumber = "New,BillNumber";
			AssertHasErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.ChangingFieldWhenBillIsRegistered("Bill Of Lading Number", "BillNumber", "New,BillNumber"));
		}

		public void TestCheckJPB_BillNumber_VOCC()
		{
			ResetAFRHeaderBill();
			Header.JPH_IsShippingLineEntry = true;

			Bill.JPB_BillNumber = ZString.Empty;
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.JPB_BillNumber = "Bill";
			AssertNoMessageErrorContaining(Bill.JPB_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Bill.BOLNumMissingNVOCCCode);

			Bill.JPB_BillNumber = "Bill,Number";
			AssertNoMessageErrorContaining(Bill.JPB_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill|Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill[Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill]Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill{Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill}Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill`Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill~Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill^Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			Bill.JPB_BillNumber = "Bill_Number";
			AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);

			CombineAssertions(() =>
			{
				JPAFRRegistry.Instance.AFRNVOCCIDtoAddtoBillDuringSync.SetValue(Header.RegistryCompanyPK, Guid.Empty, Guid.Empty, "J08");
				Header.JPH_CarrierCode = "J07";

				Bill.JPB_BillNumber = "BillNumber";
				AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Header.MBOLNumDoesntMatchCarrierCode);

				Bill.JPB_BillNumber = "J07BillNumber";
				AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Header.MBOLNumDoesntMatchCarrierCode);

				Bill.JPB_BillNumber = "J07-BillNumber";
				AssertNoMessageErrors(Bill.JPB_BillNumberInfo);
				AssertNoErrors(Bill.JPB_BillNumberInfo);
				AssertNoWarnings(Bill.JPB_BillNumberInfo);
			});

			CombineAssertions(() =>
			{
				Header.JPH_CarrierCode = "J0-7";

				Bill.JPB_BillNumber = "J0-7BillNumber";
				AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Header.MBOLNumStartWithInvalidCarrierCode);
			});

			CombineAssertions(() =>
			{
				Header.JPH_CarrierCode = "";

				Bill.JPB_BillNumber = "J0-7BillNumber";
				AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Header.MBOLNumStartWithInvalidCarrierCode);

				Bill.JPB_BillNumber = "AAAABillNumber";
				AssertNoMessageErrors(Bill.JPB_BillNumberInfo);
				AssertNoErrors(Bill.JPB_BillNumberInfo);
				AssertNoWarnings(Bill.JPB_BillNumberInfo);

				Bill.JPB_BillNumber = "AAA-BillNumber";
				AssertNoMessageErrors(Bill.JPB_BillNumberInfo);
				AssertNoErrors(Bill.JPB_BillNumberInfo);
				AssertNoWarnings(Bill.JPB_BillNumberInfo);

				Bill.JPB_BillNumber = "AA--BillNumber";
				AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Header.MBOLNumStartWithInvalidCarrierCode);

				Bill.JPB_BillNumber = "A---BillNumber";
				AssertHasMessageErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Header.MBOLNumStartWithInvalidCarrierCode);
			});

			Bill.JPB_BillNumber = "BillNumber";
			AssertNoMessageErrors(Bill.JPB_BillNumberInfo);
			AssertNoErrors(Bill.JPB_BillNumberInfo);
			AssertNoWarnings(Bill.JPB_BillNumberInfo);

			Bill.Factory.Save();

			Bill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			Bill.JPB_ReleaseStatus = ZString.Empty;
			Bill.JPB_BillNumber = "NewBillNumber";
			AssertHasErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.ChangingFieldWhenMessagingIsInProgress("Bill Of Lading Number", "BillNumber", "NewBillNumber"));

			Bill.JPB_MessageStatus = ZString.Empty;
			Bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			Bill.JPB_BillNumber = "NewBillNumber";
			AssertHasErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.ChangingFieldWhenBillIsRegistered("Bill Of Lading Number", "BillNumber", "NewBillNumber"));

			Bill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			Bill.JPB_ReleaseStatus = ZString.Empty;
			Bill.JPB_BillNumber = "NewB";
			AssertHasErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.ChangingFieldWhenMessagingIsInProgress("Bill Of Lading Number", "BillNumber", "NewB"));

			Bill.JPB_MessageStatus = ZString.Empty;
			Bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			Bill.JPB_BillNumber = "NewB";
			AssertHasErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.ChangingFieldWhenBillIsRegistered("Bill Of Lading Number", "BillNumber", "NewB"));

			Bill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			Bill.JPB_ReleaseStatus = ZString.Empty;
			Bill.JPB_BillNumber = "New,BillNumber";
			AssertHasErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.ChangingFieldWhenMessagingIsInProgress("Bill Of Lading Number", "BillNumber", "New,BillNumber"));

			Bill.JPB_MessageStatus = ZString.Empty;
			Bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			Bill.JPB_BillNumber = "New,BillNumber";
			AssertHasErrorContaining(Bill.JPB_BillNumberInfo, ValidationConstants.Shared.ChangingFieldWhenBillIsRegistered("Bill Of Lading Number", "BillNumber", "New,BillNumber"));
		}

		public void TestCheckNotificationForwardingParties()
		{
			ResetAFRHeaderBill();
			Bill.NotificationForwardingParties.AddNew();
			Bill.NotificationForwardingParties.Last().CY_Data = "NP1";
			Bill.NotificationForwardingParties.AddNew();
			Bill.NotificationForwardingParties.Last().CY_Data = "NP2";
			Bill.NotificationForwardingParties.AddNew();
			Bill.NotificationForwardingParties.Last().CY_Data = "NP3";
			Bill.NotificationForwardingParties.AddNew();
			Bill.NotificationForwardingParties.Last().CY_Data = "NP4";

			AssertNoRowWarningContaining(Bill, ValidationConstants.Bill.MaximumNotificationForwardingPartyExceeded);

			Bill.Validation.ValidateAll();
			AssertHasRowWarning(Bill, ValidationConstants.Bill.MaximumNotificationForwardingPartyExceeded);

			Bill.NotificationForwardingParties.Last().CY_Data = string.Empty;
			Bill.Validation.ValidateAll();

			Bill.NotificationForwardingParties.Last().Delete();
			Bill.Validation.ValidateAll();
			AssertNoRowWarningContaining(Bill, ValidationConstants.Bill.MaximumNotificationForwardingPartyExceeded);
		}

		public void TestCheckJPB_RL_NKOrigin()
		{
			ResetAFRHeaderBill();

			Bill.JPB_RL_NKOrigin = string.Empty;
			AssertHasMessageErrorContaining(Bill.JPB_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.JPB_RL_NKOrigin = "XXXXX";
			AssertHasMessageErrorContaining(Bill.JPB_RL_NKOriginInfo, ListValidation.InvalidCodeMessageError);

			var testUNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			testUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			testUNLOCO.RL_PortName = "123456789012345678901234567890";
			Bill.JPB_RL_NKOrigin = testUNLOCO.Code;
			AssertHasWarningContaining(Bill.JPB_RL_NKOriginInfo, ValidationConstants.Shared.PortNameLengthExceeded);

			testUNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			Bill.JPB_RL_NKOrigin = testUNLOCO.Code;
			AssertNoMessageErrors(Bill.JPB_RL_NKOriginInfo);
			AssertNoWarnings(Bill.JPB_RL_NKOriginInfo);

			testUNLOCO.Code = "JPxxx";
			testUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Bill.JPB_RL_NKOrigin = testUNLOCO.Code;
			AssertNoMessageErrorContaining(Bill.JPB_RL_NKOriginInfo, ValidationConstants.Bill.InvalidPortOfOrigin);
			AssertNoWarnings(Bill.JPB_RL_NKOriginInfo);

			testUNLOCO.Code = "AUxxx";
			testUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			Bill.JPB_RL_NKOrigin = testUNLOCO.Code;
			AssertNoMessageErrors(Bill.JPB_RL_NKOriginInfo);
			AssertHasWarningContaining(Bill.JPB_RL_NKOriginInfo, ValidationConstants.Bill.InvalidPortOfOrigin);
		}

		public void TestCheckJPB_RL_NKFinalDestination()
		{
			ResetAFRHeaderBill();

			Bill.JPB_RL_NKFinalDestination = string.Empty;
			AssertNoMessageErrors(Bill.JPB_RL_NKFinalDestinationInfo);
			AssertNoWarnings(Bill.JPB_RL_NKFinalDestinationInfo);

			Bill.JPB_RL_NKFinalDestination = "XXXXX";
			AssertHasMessageErrorContaining(Bill.JPB_RL_NKFinalDestinationInfo, ListValidation.InvalidCodeMessageError);

			var testUNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			testUNLOCO.RL_PortName = "123456789012345678901234567890";
			Bill.JPB_RL_NKFinalDestination = testUNLOCO.Code;
			AssertHasWarningContaining(Bill.JPB_RL_NKFinalDestinationInfo, ValidationConstants.Shared.PortNameLengthExceeded);

			Bill.JPB_RL_NKFinalDestination = Factory.NewWithValidTestData<RefUNLOCO>().Code;
			AssertNoMessageErrors(Bill.JPB_RL_NKFinalDestinationInfo);
			AssertNoWarnings(Bill.JPB_RL_NKFinalDestinationInfo);
		}

		public void TestCheckJPB_RL_NKDelivery()
		{
			ResetAFRHeaderBill();

			Bill.JPB_RL_NKDelivery = string.Empty;
			AssertHasMessageErrorContaining(Bill.JPB_RL_NKDeliveryInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.JPB_RL_NKDelivery = "XXXXX";
			AssertHasMessageErrorContaining(Bill.JPB_RL_NKDeliveryInfo, ListValidation.InvalidCodeMessageError);

			var testUNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			testUNLOCO.RL_PortName = "123456789012345678901234567890";
			Bill.JPB_RL_NKDelivery = testUNLOCO.Code;
			AssertHasWarningContaining(Bill.JPB_RL_NKDeliveryInfo, ValidationConstants.Shared.PortNameLengthExceeded);

			using (var inbondDetailInitiator = new InBondDetailInitiatorTestHelper())
			{
				Bill.InBondDetailInitiator = inbondDetailInitiator;
				Bill.JPB_Calc_TransportMode = TransportModeList.Codes.Ship;
				Bill.Header.JPH_RL_NKDischarge = Factory.NewWithValidTestData<RefUNLOCO>().Code;
				Bill.JPB_RL_NKDelivery = Bill.Header.JPH_RL_NKDischarge;
				AssertHasMessageErrorContaining(Bill.JPB_RL_NKDeliveryInfo, ValidationConstants.Bill.DeliveryPortCannotEqualDischargePortForTranshipment);
			}

			Bill.JPB_RL_NKDelivery = Factory.NewWithValidTestData<RefUNLOCO>().Code;
			AssertNoMessageErrors(Bill.JPB_RL_NKDeliveryInfo);
			AssertNoWarnings(Bill.JPB_RL_NKDeliveryInfo);
		}

		public void TestCheckJPB_GoodsDescription()
		{
			ResetAFRHeaderBill();

			Bill.JPB_GoodsDescription = ZString.Empty;
			AssertHasMessageErrorContaining(Bill.JPB_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.JPB_GoodsDescription = "GoodsDescription";
			AssertNoMessageErrorContaining(Bill.JPB_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.JPB_GoodsDescription = "GoodsDescriptio[n";
			AssertHasMessageErrorContaining(Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Shared.InvalidNACCSChar(Bill.JPB_GoodsDescriptionInfo.HumanReadableName));

			CombineAssertions("Inappropriate good description Warnings", () =>
			{
				Bill.JPB_GoodsDescription = "machines";
				AssertHasWarning("TST: checking machines", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("machines"));

				Bill.JPB_GoodsDescription = "pArts";
				AssertHasWarning("TST: checking pArts", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("pArts"));

				Bill.JPB_GoodsDescription = "NA";
				AssertHasWarning("TST: checking NA", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("NA"));

				Bill.JPB_GoodsDescription = "NM";
				AssertHasWarning("TST: checking NM", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("NM"));

				Bill.JPB_GoodsDescription = "na";
				AssertHasWarning("TST: checking na", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("na"));

				Bill.JPB_GoodsDescription = "nm";
				AssertHasWarning("TST: checking nm", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("nm"));

				Bill.JPB_GoodsDescription = "nA";
				AssertHasWarning("TST: checking nA", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("nA"));

				Bill.JPB_GoodsDescription = "nM";
				AssertHasWarning("TST: checking nM", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("nM"));

				Bill.JPB_GoodsDescription = "n/M";
				AssertHasWarning("TST: checking n/M", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("n/M"));

				Bill.JPB_GoodsDescription = "N/A";
				AssertHasWarning("TST: checking N/A", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("N/A"));

				Bill.JPB_GoodsDescription = "Unknown";
				AssertHasWarning("TST: checking Unknown", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("Unknown"));

				Bill.JPB_GoodsDescription = "House Hold Goods";
				AssertHasWarning("TST: checking House Hold Goods", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("House Hold Goods"));

				Bill.JPB_GoodsDescription = "S";
				AssertHasWarning("TST: checking S", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("S"));

				Bill.JPB_GoodsDescription = " S";
				AssertHasWarning("TST: checking  S", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods(" S"));

				Bill.JPB_GoodsDescription = "V";
				AssertHasWarning("TST: checking V", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("V"));

				Bill.JPB_GoodsDescription = "CN";
				AssertHasWarning("TST: checking CN", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("CN"));

				Bill.JPB_GoodsDescription = " CN";
				AssertHasWarning("TST: checking  CN", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods(" CN"));

				Bill.JPB_GoodsDescription = "PP";
				AssertHasWarning("TST: checking PP ", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("PP"));

				Bill.JPB_GoodsDescription = "1";
				AssertHasWarning("TST: checking 1", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("1"));

				Bill.JPB_GoodsDescription = "2";
				AssertHasWarning("TST: checking 2", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("2"));

				Bill.JPB_GoodsDescription = "11";
				AssertHasWarning("TST: checking 11", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("11"));

				Bill.JPB_GoodsDescription = "35";
				AssertHasWarning("TST: checking 35", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("35"));

				Bill.JPB_GoodsDescription = "1C";
				AssertHasWarning("TST: checking 1C", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("1C"));

				Bill.JPB_GoodsDescription = "2D";
				AssertHasWarning("TST: checking 2D", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("2D"));

				Bill.JPB_GoodsDescription = "4S";
				AssertHasWarning("TST: checking 4S", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("4S"));

				Bill.JPB_GoodsDescription = "AS per Attached";
				AssertHasWarning("TST: checking AS per Attached", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("AS per Attached"));

				Bill.JPB_GoodsDescription = " XXX ";
				AssertHasWarning("TST: checking XXX", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods(" XXX"));

				Bill.JPB_GoodsDescription = "ZZZZZ";
				AssertHasWarning("TST: checking ZZZZZ", Bill.JPB_GoodsDescriptionInfo, ValidationConstants.Bill.InappropriateDescriptionOfGoods("ZZZZZ"));

				Bill.JPB_GoodsDescription = "AXXX ";
				AssertNoWarnings("TST: checking AXXX ", Bill.JPB_GoodsDescriptionInfo);

				Bill.JPB_GoodsDescription = "partss";
				AssertNoWarnings("TST: checking partss", Bill.JPB_GoodsDescriptionInfo);

				Bill.JPB_GoodsDescription = " N A";
				AssertNoWarnings("TST: checking  N A", Bill.JPB_GoodsDescriptionInfo);
			});
		}

		public void TestCheckJPB_Tariff()
		{
			ResetAFRHeaderBill();

			Bill.JPB_Tariff = ZString.Empty;
			AssertHasMessageErrorContaining(Bill.JPB_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Bill.JPB_TariffInfo, ValidationConstants.Bill.HSCodeInvalid);

			Bill.JPB_Tariff = "Tariff";
			AssertNoMessageErrorContaining(Bill.JPB_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Bill.JPB_TariffInfo, ValidationConstants.Bill.HSCodeInvalid);

			Bill.JPB_Tariff = "0123T5";
			AssertNoMessageErrorContaining(Bill.JPB_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Bill.JPB_TariffInfo, ValidationConstants.Bill.HSCodeInvalid);

			Bill.JPB_Tariff = "12345";
			AssertNoMessageErrorContaining(Bill.JPB_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Bill.JPB_TariffInfo, ValidationConstants.Bill.HSCodeInvalid);

			Bill.JPB_Tariff = "123456";
			AssertNoMessageErrorContaining(Bill.JPB_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(Bill.JPB_TariffInfo, ValidationConstants.Bill.HSCodeInvalid);

			Bill.JPB_Tariff = "980100";
			AssertNoMessageErrors(Bill.JPB_TariffInfo);
			AssertHasWarningContaining(Bill.JPB_TariffInfo, ValidationConstants.Bill.HSCodeNonUniversal);

			Bill.JPB_Tariff = "989100";
			AssertNoMessageErrors(Bill.JPB_TariffInfo);
			AssertHasWarningContaining(Bill.JPB_TariffInfo, ValidationConstants.Bill.HSCodeNonUniversal);

			Bill.JPB_Tariff = "990100";
			AssertNoMessageErrors(Bill.JPB_TariffInfo);
			AssertHasWarningContaining(Bill.JPB_TariffInfo, ValidationConstants.Bill.HSCodeNonUniversal);

			Bill.JPB_Tariff = "020110";
			AssertNoMessageErrors(Bill.JPB_TariffInfo);
			AssertNoWarnings(Bill.JPB_TariffInfo);
		}

		public void TestCheckJPB_MarksAndNumbers()
		{
			ResetAFRHeaderBill();

			Bill.JPB_MarksAndNumbers = ZString.Empty;
			AssertHasMessageErrorContaining(Bill.JPB_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.JPB_MarksAndNumbers = "MarksAndNumbers";
			AssertNoMessageErrorContaining(Bill.JPB_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.JPB_MarksAndNumbers = "MarksAndNumber[s";
			AssertHasMessageErrorContaining(Bill.JPB_MarksAndNumbersInfo, ValidationConstants.Shared.InvalidNACCSChar(Bill.JPB_MarksAndNumbersInfo.HumanReadableName));
		}

		public void TestCheckJPB_Remarks()
		{
			ResetAFRHeaderBill();

			Bill.JPB_Remarks = ZString.Empty;
			AssertNoMessageErrors(Bill.JPB_RemarksInfo);

			Bill.JPB_Remarks = "remarks";
			AssertNoMessageErrors(Bill.JPB_RemarksInfo);

			Bill.JPB_Remarks = "remarks[s";
			AssertHasMessageErrorContaining(Bill.JPB_RemarksInfo, ValidationConstants.Shared.InvalidNACCSChar(Bill.JPB_RemarksInfo.HumanReadableName));
		}

		public void TestCheckJPB_ManifestQty()
		{
			ResetAFRHeaderBill();

			Bill.JPB_ManifestQty = -55000;
			AssertHasErrorContaining(Bill.JPB_ManifestQtyInfo, ValidationConstants.Bill.ValueCannotBeNegative);

			Bill.JPB_ManifestQty = 0;
			AssertHasMessageErrorContaining(Bill.JPB_ManifestQtyInfo, ValidationConstants.Bill.ManifestQtyUseOneForUndescribable);

			Bill.JPB_ManifestQty = 1000000000;
			AssertHasMessageErrorContaining(Bill.JPB_ManifestQtyInfo, ValidationConstants.Bill.ManifestQtyExceedingMaximumNumber);

			Bill.JPB_ManifestQty = 500;
			AssertNoErrors(Bill.JPB_ManifestQtyInfo);
			AssertNoMessageErrors(Bill.JPB_ManifestQtyInfo);
			AssertNoWarnings(Bill.JPB_ManifestQtyInfo);
		}

		public void TestCheckJPB_ManifestUQ()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var universalReferenceTestHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, "Package types");
			universalReferenceTestHelper.CreateCusCodeList(CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, "BG", "Bag", startDate, endDate);
			Factory.Save();

			ResetAFRHeaderBill();

			Bill.JPB_ManifestUQ = ZString.Empty;
			AssertHasMessageErrorContaining(Bill.JPB_ManifestUQInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.JPB_ManifestUQ = "XX";
			AssertHasMessageErrorContaining(Bill.JPB_ManifestUQInfo, ListValidation.InvalidCodeMessageError);

			Bill.JPB_ManifestUQ = PackageTypeList.Codes.Bag;
			AssertNoErrors(Bill.JPB_ManifestUQInfo);
			AssertNoMessageErrors(Bill.JPB_ManifestUQInfo);
			AssertNoWarnings(Bill.JPB_ManifestUQInfo);
		}

		public void TestCheckJPB_GrossWeight()
		{
			ResetAFRHeaderBill();

			AssertNoMessageErrors(Bill.JPB_GrossWeightInfo);
			AssertNoErrors(Bill.JPB_GrossWeightInfo);
			AssertNoWarnings(Bill.JPB_GrossWeightInfo);

			Bill.JPB_GrossWeight = 0;
			AssertHasMessageErrorContaining(Bill.JPB_GrossWeightInfo, ValidationConstants.Bill.ValueEmpty(Bill.JPB_GrossWeightInfo.HumanReadableName));
			AssertNoErrors(Bill.JPB_GrossWeightInfo);
			AssertNoWarnings(Bill.JPB_GrossWeightInfo);

			Bill.JPB_GrossWeight = 40.1234;
			AssertNoMessageErrors(Bill.JPB_GrossWeightInfo);
			AssertNoErrors(Bill.JPB_GrossWeightInfo);
			AssertNoWarnings(Bill.JPB_GrossWeightInfo);

			Bill.JPB_GrossWeight = -40.1234;
			AssertNoMessageErrors(Bill.JPB_GrossWeightInfo);
			AssertHasErrorContaining(Bill.JPB_GrossWeightInfo, ValidationConstants.Bill.ValueCannotBeNegative);
			AssertNoWarnings(Bill.JPB_GrossWeightInfo);
		}

		public void TestCheckJPB_GrossWeightUQ()
		{
			ResetAFRHeaderBill();

			Bill.JPB_GrossWeightUQ = ZString.Empty;
			AssertHasMessageErrorContaining(Bill.JPB_GrossWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.JPB_GrossWeightUQ = "XX";
			AssertHasMessageErrorContaining(Bill.JPB_GrossWeightUQInfo, ListValidation.InvalidCodeMessageError);

			Bill.JPB_GrossWeightUQ = "KGM";
			AssertHasMessageErrorContaining(Bill.JPB_GrossWeightUQInfo, ListValidation.InvalidCodeMessageError);

			Bill.JPB_GrossWeightUQ = WeightUnitCodeList.Codes.Kilogram;
			AssertNoMessageErrors(Bill.JPB_GrossWeightUQInfo);
		}

		public void TestCheckJPB_Volume()
		{
			ResetAFRHeaderBill();

			AssertNoMessageErrors(Bill.JPB_VolumeInfo);
			AssertNoErrors(Bill.JPB_VolumeInfo);
			AssertNoWarnings(Bill.JPB_VolumeInfo);

			Bill.JPB_Volume = 0;
			AssertHasMessageErrorContaining(Bill.JPB_VolumeInfo, ValidationConstants.Bill.ValueEmpty(Bill.JPB_VolumeInfo.HumanReadableName));
			AssertNoErrors(Bill.JPB_VolumeInfo);
			AssertNoWarnings(Bill.JPB_VolumeInfo);

			Bill.JPB_Volume = 40.1234;
			AssertNoMessageErrors(Bill.JPB_VolumeInfo);
			AssertNoErrors(Bill.JPB_VolumeInfo);
			AssertNoWarnings(Bill.JPB_VolumeInfo);

			Bill.JPB_Volume = -1;
			AssertNoMessageErrors(Bill.JPB_VolumeInfo);
			AssertHasErrorContaining(Bill.JPB_VolumeInfo, ValidationConstants.Bill.ValueCannotBeNegative);
			AssertNoWarnings(Bill.JPB_VolumeInfo);
		}

		public void TestCheckJPB_VolumeUQ()
		{
			ResetAFRHeaderBill();

			Bill.JPB_VolumeUQ = ZString.Empty;
			AssertHasMessageErrorContaining(Bill.JPB_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.JPB_VolumeUQ = "XX";
			AssertHasMessageErrorContaining(Bill.JPB_VolumeUQInfo, ListValidation.InvalidCodeMessageError);

			Bill.JPB_VolumeUQ = VolumeUnitCodeList.Codes.CubicMeter;
			AssertNoMessageErrors(Bill.JPB_VolumeUQInfo);
		}

		public void TestCheckJPB_RN_NKGoodsOrigin()
		{
			ResetAFRHeaderBill();

			Bill.JPB_RN_NKGoodsOrigin = string.Empty;
			AssertNoMessageErrors(Bill.JPB_RN_NKGoodsOriginInfo);

			Bill.JPB_RN_NKGoodsOrigin = "XX";
			AssertHasMessageErrorContaining(Bill.JPB_RN_NKGoodsOriginInfo, ListValidation.InvalidCodeMessageError);

			Bill.JPB_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrors(Bill.JPB_RN_NKGoodsOriginInfo);

			var testCountry = Factory.NewWithValidTestData<RefCountry>();
			testCountry.RN_Code = "A]";
			Bill.JPB_RN_NKGoodsOrigin = testCountry.Code;
			AssertHasMessageErrorContaining(Bill.JPB_RN_NKGoodsOriginInfo, ValidationConstants.Shared.InvalidNACCSChar(Bill.JPB_RN_NKGoodsOriginInfo.HumanReadableName));
		}

		public void TestCheckJPB_FreightValue()
		{
			ResetAFRHeaderBill();

			Bill.JPB_FreightValue = -100;
			AssertHasErrorContaining(Bill.JPB_FreightValueInfo, ValidationConstants.Bill.ValueCannotBeNegative);

			Bill.JPB_RX_NKFreightValueCurrency = CurrencyCodes.Japan;
			Bill.JPB_FreightValue = 100.5;
			AssertHasWarningContaining(Bill.JPB_FreightValueInfo, ValidationConstants.Bill.FrightValueNoDecimalPartAllowedForJPY);

			Bill.JPB_RX_NKFreightValueCurrency = CurrencyCodes.Japan;
			Bill.JPB_FreightValue = 100;
			AssertNoErrors(Bill.JPB_FreightValueInfo);
			AssertNoMessageErrors(Bill.JPB_FreightValueInfo);
			AssertNoWarnings(Bill.JPB_FreightValueInfo);

			Bill.JPB_RX_NKFreightValueCurrency = CurrencyCodes.Australia;
			Bill.JPB_FreightValue = 100.555;
			AssertHasWarningContaining(Bill.JPB_FreightValueInfo, ValidationConstants.Bill.FrightValueDecimalPartLimitForOtherCurrency);

			Bill.JPB_RX_NKFreightValueCurrency = CurrencyCodes.Australia;
			Bill.JPB_FreightValue = 100.55;
			AssertNoErrors(Bill.JPB_FreightValueInfo);
			AssertNoMessageErrors(Bill.JPB_FreightValueInfo);
			AssertNoWarnings(Bill.JPB_FreightValueInfo);
		}

		public void TestCheckJPB_RX_NKFreightValueCurrency()
		{
			ResetAFRHeaderBill();

			Bill.JPB_FreightValue = 0;
			Bill.JPB_RX_NKFreightValueCurrency = "XXX";
			AssertHasMessageErrorContaining(Bill.JPB_RX_NKFreightValueCurrencyInfo, ListValidation.InvalidCodeMessageError);

			Bill.JPB_RX_NKFreightValueCurrency = CurrencyCodes.Japan;
			AssertNoErrors(Bill.JPB_RX_NKFreightValueCurrencyInfo);
			AssertNoMessageErrors(Bill.JPB_RX_NKFreightValueCurrencyInfo);
			AssertNoWarnings(Bill.JPB_RX_NKFreightValueCurrencyInfo);

			Bill.JPB_FreightValue = 100.55;
			Bill.JPB_RX_NKFreightValueCurrency = "XXX";
			AssertHasMessageErrorContaining(Bill.JPB_RX_NKFreightValueCurrencyInfo, ListValidation.InvalidCodeMessageError);

			Bill.JPB_FreightValue = 100.55;
			Bill.JPB_RX_NKFreightValueCurrency = string.Empty;
			AssertHasMessageErrorContaining(Bill.JPB_RX_NKFreightValueCurrencyInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.JPB_RX_NKFreightValueCurrency = "AU]";
			AssertHasMessageErrorContaining(Bill.JPB_RX_NKFreightValueCurrencyInfo, ValidationConstants.Shared.InvalidNACCSChar(Bill.JPB_RX_NKFreightValueCurrencyInfo.HumanReadableName));
		}

		public void TestCheckJPB_ContainerOperatorCode()
		{
			CombineAssertions("Test for NVOCC", () =>
			{
				ResetAFRHeaderBill();
				using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
				{
					Header.JPH_IsShippingLineEntry = false;
					Header.JPH_RL_NKDischarge = "JPABA";
					Bill.InBondDetailInitiator = inbondInitiator;
					Header.InBondDetailInitiator = inbondInitiator;
					AssertExceptionThrown(typeof(MaxLengthExceededException), () => { Bill.JPB_ContainerOperatorCode = "1ASAAa"; });
					ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();

					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoErrors(Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoWarnings(Bill.JPB_ContainerOperatorCodeInfo);

					Bill.JPB_ContainerOperatorCode = "";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoErrors(Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoWarnings(Bill.JPB_ContainerOperatorCodeInfo);

					Bill.JPB_ContainerOperatorCode = "1ASAA";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoErrors(Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoWarnings(Bill.JPB_ContainerOperatorCodeInfo);

					Bill.JPB_ContainerOperatorCode = "1AS[A";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoErrors(Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoWarnings(Bill.JPB_ContainerOperatorCodeInfo);

					Header.JPH_RL_NKDischarge = "AUABA";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoErrors(Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoWarnings(Bill.JPB_ContainerOperatorCodeInfo);
				}
			});

			CombineAssertions("Test for VOCC", () =>
			{
				ResetAFRHeaderBill();
				using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
				{
					ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
					Header.JPH_IsShippingLineEntry = true;
					Header.JPH_RL_NKDischarge = "JPABA";
					Bill.InBondDetailInitiator = inbondInitiator;
					Header.InBondDetailInitiator = inbondInitiator;
					AssertExceptionThrown(typeof(MaxLengthExceededException), () => { Bill.JPB_ContainerOperatorCode = "1ASAAa"; });
					ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();

					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoErrors(Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoWarnings(Bill.JPB_ContainerOperatorCodeInfo);

					Bill.JPB_ContainerOperatorCode = "";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors("TST: Empty MessageErrors", Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoErrors("TST: Empty Error", Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoWarnings("TST: Empty Warnings", Bill.JPB_ContainerOperatorCodeInfo);

					Bill.JPB_ContainerOperatorCode = "1AS[A";
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_ContainerOperatorCodeInfo, ValidationConstants.Shared.InvalidNACCSChar(Bill.JPB_ContainerOperatorCodeInfo.HumanReadableName));
					AssertNoErrors("TST: Normal Error", Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoWarnings("TST: Normal Warnings", Bill.JPB_ContainerOperatorCodeInfo);

					Bill.JPB_ContainerOperatorCode = "1ASAA";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors("TST: Normal MessageErrors", Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoErrors("TST: Normal Error", Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoWarnings("TST: Normal Warnings", Bill.JPB_ContainerOperatorCodeInfo);

					Header.JPH_RL_NKDischarge = "AUABA";
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_ContainerOperatorCodeInfo, ValidationConstants.InbondDetails.EntryInvalidAsNotDischargeInJapan(Bill.JPB_ContainerOperatorCodeInfo.HumanReadableName));
					AssertNoErrors("TST: Normal outsideJapan Error", Bill.JPB_ContainerOperatorCodeInfo);
					AssertNoWarnings("TST: Normal outsideJapan Warnings", Bill.JPB_ContainerOperatorCodeInfo);
				}
			});
		}

		public void TestCheckUNDGDataItem()
		{
			CombineAssertions(() =>
			{
				ResetAFRHeaderBill();

				DGSubstanceTestHelper.Create("1233", "a", "IMO", additionalInitialisation: (subs) => subs.DG_Class = "][");
				var testUNDGSub = UNDGSubstanceLoader.LoadSubstances(Factory, "1233", "a", "IMO").First();
				Bill.JPB_DG_NKSubstance = "XX";
				AssertHasMessageErrorContaining(bill.JPB_DG_NKSubstanceInfo, "The code you have selected is not in the list.");

				Bill.JPB_DG_NKSubstance = testUNDGSub.DG_Code;
				AssertNoErrors(Bill.JPB_DG_NKSubstanceInfo);
				AssertHasMessageErrorContaining(Bill.JPB_DG_NKSubstanceInfo, ValidationConstants.Shared.InvalidNACCSChar(testUNDGSub.DG_ClassInfo.HumanReadableName));

				testUNDGSub.DG_Class = "1233";
				testUNDGSub.DG_Code = "[]";
				Bill.JPB_DG_NKSubstance = testUNDGSub.DG_Code;
				AssertNoErrors(Bill.JPB_DG_NKSubstanceInfo);
				AssertNoMessageErrorContaining(Bill.JPB_DG_NKSubstanceInfo, ValidationConstants.Shared.InvalidNACCSChar(testUNDGSub.DG_ClassInfo.HumanReadableName));
				AssertHasMessageErrorContaining(Bill.JPB_DG_NKSubstanceInfo, ValidationConstants.Shared.InvalidNACCSChar(Bill.JPB_DG_NKSubstanceInfo.HumanReadableName));

				testUNDGSub.DG_Class = "1233";
				testUNDGSub.DG_Code = "XXXX";
				Bill.JPB_DG_NKSubstance = testUNDGSub.DG_Code;
				AssertNoErrors(Bill.JPB_DG_NKSubstanceInfo);
				AssertNoMessageErrors(Bill.JPB_DG_NKSubstanceInfo);
				AssertNoWarnings(Bill.JPB_DG_NKSubstanceInfo);
			});
		}

		#region Calculated properties

		public void TestCheckJPB_Calc_GoodsValue()
		{
			ResetAFRHeaderBill();
			using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
			{
				Header.JPH_RL_NKDischarge = "JPABA";
				Bill.InBondDetailInitiator = inbondInitiator;
				Header.InBondDetailInitiator = inbondInitiator;

				AssertNoMessageErrors(Bill.JPB_Calc_GoodsValueInfo);
				AssertNoErrors(Bill.JPB_Calc_GoodsValueInfo);
				AssertNoWarnings(Bill.JPB_Calc_GoodsValueInfo);

				Bill.JPB_Calc_TransportMode = TransportModeList.Codes.Aircraft;
				Bill.Validation.ValidateAll();
				AssertHasMessageErrorContaining(Bill.JPB_Calc_GoodsValueInfo, ValidationConstants.InbondDetails.GoodValueCannotBeEmptyForTranshipment);

				Bill.JPB_Calc_GoodsValue = -100;
				Bill.Validation.ValidateAll();
				AssertHasErrorContaining(Bill.JPB_Calc_GoodsValueInfo, ValidationConstants.Bill.ValueCannotBeNegative);

				Bill.JPB_Calc_RX_NKGoodsValueCurrency = CurrencyCodes.Japan;
				Bill.JPB_Calc_GoodsValue = 100.5;
				Bill.Validation.ValidateAll();
				AssertHasWarningContaining(Bill.JPB_Calc_GoodsValueInfo, ValidationConstants.InbondDetails.GoodValueNoDecimalPartAllowedFotJPY);

				Bill.JPB_Calc_RX_NKGoodsValueCurrency = CurrencyCodes.Japan;
				Bill.JPB_Calc_GoodsValue = 100;
				Bill.Validation.ValidateAll();
				AssertNoErrors(Bill.JPB_Calc_GoodsValueInfo);
				AssertNoMessageErrors(Bill.JPB_Calc_GoodsValueInfo);
				AssertNoWarnings(Bill.JPB_Calc_GoodsValueInfo);

				Bill.JPB_Calc_RX_NKGoodsValueCurrency = CurrencyCodes.Australia;
				Bill.JPB_Calc_GoodsValue = 100.555;
				Bill.Validation.ValidateAll();
				AssertHasWarningContaining(Bill.JPB_Calc_GoodsValueInfo, ValidationConstants.InbondDetails.GoodValueDecimalPartLimitForOtherCurrency);

				Bill.JPB_Calc_RX_NKGoodsValueCurrency = CurrencyCodes.Australia;
				Bill.JPB_Calc_GoodsValue = 100.0;
				Bill.Validation.ValidateAll();
				AssertNoErrors(Bill.JPB_Calc_GoodsValueInfo);
				AssertNoMessageErrors(Bill.JPB_Calc_GoodsValueInfo);
				AssertNoWarnings(Bill.JPB_Calc_GoodsValueInfo);

				Header.JPH_RL_NKDischarge = "AUSYD";
				Bill.Validation.ValidateAll();
				AssertNoErrors(Bill.JPB_Calc_GoodsValueInfo);
				AssertHasMessageErrorContaining(Bill.JPB_Calc_GoodsValueInfo, ValidationConstants.InbondDetails.EntryInvalidAsNotDischargeInJapan(Bill.JPB_Calc_GoodsValueInfo.HumanReadableName));
				AssertNoWarnings(Bill.JPB_Calc_GoodsValueInfo);
			}
		}

		public void TestCheckJPB_Calc_RX_NKGoodsValueCurrency()
		{
			ResetAFRHeaderBill();
			using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
			{
				Bill.InBondDetailInitiator = inbondInitiator;
				Header.InBondDetailInitiator = inbondInitiator;

				AssertNoErrors(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo);
				AssertNoMessageErrors(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo);
				AssertNoWarnings(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo);

				Bill.JPB_Calc_GoodsValue = 0;
				Bill.JPB_Calc_RX_NKGoodsValueCurrency = "XXX";
				Bill.Validation.ValidateAll();
				AssertHasMessageErrorContaining(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo, ListValidation.InvalidCodeMessageError);

				Bill.JPB_Calc_RX_NKGoodsValueCurrency = CurrencyCodes.Japan;
				Bill.Validation.ValidateAll();
				AssertNoErrors(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo);
				AssertNoMessageErrors(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo);
				AssertNoWarnings(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo);

				Bill.JPB_Calc_GoodsValue = 100.55;
				Bill.JPB_Calc_RX_NKGoodsValueCurrency = "XXX";
				Bill.Validation.ValidateAll();
				AssertHasMessageErrorContaining(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo, ListValidation.InvalidCodeMessageError);

				Bill.JPB_Calc_GoodsValue = 100.55;
				Bill.JPB_Calc_RX_NKGoodsValueCurrency = string.Empty;
				Bill.Validation.ValidateAll();
				AssertHasMessageErrorContaining(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo, MandatoryValidation.YouHaveNotEntered);

				Bill.JPB_Calc_GoodsValue = 100.55;
				Bill.JPB_Calc_RX_NKGoodsValueCurrency = CurrencyCodes.Japan;
				Bill.Validation.ValidateAll();
				AssertNoErrors(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo);
				AssertNoMessageErrors(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo);
				AssertNoWarnings(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo);

				Bill.JPB_Calc_GoodsValue = 100.55;
				Bill.JPB_Calc_RX_NKGoodsValueCurrency = CurrencyCodes.Australia;
				Bill.Validation.ValidateAll();
				AssertNoErrors(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo);
				AssertNoMessageErrors(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo);
				AssertNoWarnings(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo);

				Bill.JPB_Calc_GoodsValue = 100.55;
				Bill.JPB_Calc_RX_NKGoodsValueCurrency = "AU]";
				Bill.Validation.ValidateAll();
				AssertNoErrors(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo);
				AssertHasMessageErrorContaining(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo, ValidationConstants.Shared.InvalidNACCSChar(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo.HumanReadableName));
				AssertNoWarnings(Bill.JPB_Calc_RX_NKGoodsValueCurrencyInfo);
			}
		}

		public void TestCheckJPB_Calc_TransportMode()
		{
			ResetAFRHeaderBill();
			using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
			{
				Header.JPH_RL_NKDischarge = "JPABA";
				Bill.InBondDetailInitiator = inbondInitiator;
				Header.InBondDetailInitiator = inbondInitiator;

				Bill.JPB_Calc_TransportMode = string.Empty;
				Bill.Validation.ValidateAll();
				AssertNoMessageErrors(Bill.JPB_Calc_TransportModeInfo);
				AssertNoErrors(Bill.JPB_Calc_TransportModeInfo);
				AssertNoWarnings(Bill.JPB_Calc_TransportModeInfo);

				Bill.JPB_Calc_ESDT = ZDateTime.Now;
				Bill.JPB_Calc_TransportMode = string.Empty;
				Bill.Validation.ValidateAll();
				AssertHasMessageErrorContaining(Bill.JPB_Calc_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

				Bill.JPB_Calc_ESDT = ZDateTime.Now;
				Bill.JPB_Calc_TransportMode = "XX";
				Bill.Validation.ValidateAll();
				AssertHasMessageErrorContaining(Bill.JPB_Calc_TransportModeInfo, ListValidation.InvalidCodeMessageError);

				Bill.JPB_Calc_TemporaryLandingDuration = 1;
				Bill.JPB_Calc_TransportMode = TransportModeList.Codes.Ship;
				Bill.Validation.ValidateAll();
				AssertNoMessageErrors(Bill.JPB_Calc_TransportModeInfo);
				AssertNoErrors(Bill.JPB_Calc_TransportModeInfo);
				AssertNoWarnings(Bill.JPB_Calc_TransportModeInfo);

				Header.JPH_RL_NKDischarge = "AUSYD";
				Bill.Validation.ValidateAll();
				AssertHasMessageErrorContaining(Bill.JPB_Calc_TransportModeInfo, ValidationConstants.InbondDetails.EntryInvalidAsNotDischargeInJapan(Bill.JPB_Calc_TransportModeInfo.HumanReadableName));
				AssertNoErrors(Bill.JPB_Calc_TransportModeInfo);
				AssertNoWarnings(Bill.JPB_Calc_TransportModeInfo);
			}
		}

		public void TestCheckJPB_Calc_ArrivalBondedAreaCode()
		{
			ResetAFRHeaderBill();
			using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
			{
				Header.JPH_RL_NKDischarge = "JPABA";
				Bill.InBondDetailInitiator = inbondInitiator;
				Header.InBondDetailInitiator = inbondInitiator;

				Bill.JPB_Calc_ArrivalBondedAreaCode = ZString.Empty;
				Bill.Validation.ValidateAll();
				AssertNoWarnings(Bill.JPB_Calc_ArrivalBondedAreaCodeInfo);
				AssertNoErrors(Bill.JPB_Calc_ArrivalBondedAreaCodeInfo);
				AssertNoMessageErrors(Bill.JPB_Calc_ArrivalBondedAreaCodeInfo);

				Bill.JPB_Calc_ArrivalBondedAreaCode = "SDFS";
				Bill.Validation.ValidateAll();
				AssertNoWarnings(Bill.JPB_Calc_ArrivalBondedAreaCodeInfo);
				AssertNoErrors(Bill.JPB_Calc_ArrivalBondedAreaCodeInfo);
				AssertHasMessageErrorContaining(Bill.JPB_Calc_ArrivalBondedAreaCodeInfo, ValidationConstants.InbondDetails.InvalidBondedAreaCode);

				Bill.JPB_Calc_ArrivalBondedAreaCode = "1RWF8";
				Bill.Validation.ValidateAll();
				AssertNoWarnings(Bill.JPB_Calc_ArrivalBondedAreaCodeInfo);
				AssertNoErrors(Bill.JPB_Calc_ArrivalBondedAreaCodeInfo);
				AssertNoMessageErrors(Bill.JPB_Calc_ArrivalBondedAreaCodeInfo);

				Header.JPH_RL_NKDischarge = "AUSYD";
				Bill.Validation.ValidateAll();
				AssertNoWarnings(Bill.JPB_Calc_ArrivalBondedAreaCodeInfo);
				AssertNoErrors(Bill.JPB_Calc_ArrivalBondedAreaCodeInfo);
				AssertHasMessageErrorContaining(Bill.JPB_Calc_ArrivalBondedAreaCodeInfo, ValidationConstants.InbondDetails.EntryInvalidAsNotDischargeInJapan(Bill.JPB_Calc_ArrivalBondedAreaCodeInfo.HumanReadableName));
			}
		}

		public void TestCheckJPB_Calc_GeneralCustomsTransitApprovalNumber()
		{
			CombineAssertions("Test for NVOCC", () =>
			{
				ResetAFRHeaderBill();
				using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
				{
					Header.JPH_IsShippingLineEntry = false;
					Header.JPH_RL_NKDischarge = "JPABA";
					Bill.InBondDetailInitiator = inbondInitiator;
					Header.InBondDetailInitiator = inbondInitiator;
					AssertExceptionThrown(typeof(MaxLengthExceededException), () => { Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "TESTTESTTEST"; });
					ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
					AssertEquals(string.Empty, Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber);

					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);

					Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);

					Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "TESTTESTGT]";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);

					Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "TESTTESTGTN";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);

					Header.JPH_RL_NKDischarge = "AUABA";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);

					Header.JPH_RL_NKDischarge = "JPABA";
					Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = string.Empty;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);

					Bill.JPB_Calc_TransportMode = "XX";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);

					Bill.JPB_Calc_TemporaryLandingReason = "XX";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
				}
			});

			CombineAssertions("Test for VOCC", () =>
			{
				ResetAFRHeaderBill();
				using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
				{
					Header.JPH_IsShippingLineEntry = true;
					Header.JPH_RL_NKDischarge = "JPABA";
					Bill.InBondDetailInitiator = inbondInitiator;
					Header.InBondDetailInitiator = inbondInitiator;
					Bill.Validation.ValidateAll();
					AssertExceptionThrown(typeof(MaxLengthExceededException), () => { Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "TESTTESTTEST"; });
					ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
					AssertEquals(string.Empty, Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber);

					Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);

					Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "TESTTESTGTN";
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo, ValidationConstants.Bill.EntryInvalidAsCOCCodeNotValid(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo.HumanReadableName));
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);

					Bill.JPB_ContainerOperatorCode = "1ASAA";
					Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "TESTTESTGT]";
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo, ValidationConstants.Shared.InvalidNACCSChar(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo.HumanReadableName));
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);

					Bill.JPB_ContainerOperatorCode = "1ASAA";
					Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "TESTTESTGTN";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);

					Bill.JPB_ContainerOperatorCode = "99999";
					Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "TESTTESTGTN";
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo, ValidationConstants.Bill.EntryInvalidAsCOCCodeNotValid(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo.HumanReadableName));
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);

					Bill.JPB_ContainerOperatorCode = "1ASAA";
					Header.JPH_RL_NKDischarge = "AUABA";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrorContaining(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo, ValidationConstants.Bill.EntryInvalidAsCOCCodeNotValid(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo.HumanReadableName));
					AssertHasMessageErrorContaining(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo, ValidationConstants.InbondDetails.EntryInvalidAsNotDischargeInJapan(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo.HumanReadableName));
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);

					Header.JPH_RL_NKDischarge = "JPABA";
					Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = string.Empty;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);

					Bill.JPB_Calc_TransportMode = "XX";
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo, ValidationConstants.InbondDetails.ValueMissingWhenGeneralCustomsTransitIntended);
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);

					Bill.JPB_Calc_TemporaryLandingReason = "XX";
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoErrors(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
					AssertNoWarnings(Bill.JPB_Calc_GeneralCustomsTransitApprovalNumberInfo);
				}
			});
		}

		public void TestCheckJPB_Calc_TemporaryLandingReason()
		{
			CombineAssertions("Test for NVOCC", () =>
			{
				ResetAFRHeaderBill();
				Header.JPH_IsShippingLineEntry = false;
				using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
				{
					Bill.InBondDetailInitiator = inbondInitiator;
					Header.InBondDetailInitiator = inbondInitiator;

					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingReasonInfo);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingReasonInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingReasonInfo);

					Bill.JPB_Calc_TemporaryLandingReason = string.Empty;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingReasonInfo);

					Bill.JPB_Calc_TemporaryLandingDuration = 1;
					Bill.JPB_Calc_TemporaryLandingReason = string.Empty;
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_Calc_TemporaryLandingReasonInfo, ValidationConstants.InbondDetails.ValueMissingWhenTemporaryLandingIntended(Bill.JPB_Calc_TemporaryLandingReasonInfo.HumanReadableName));

					Bill.JPB_Calc_TemporaryLandingDuration = 1;
					Bill.JPB_Calc_TemporaryLandingReason = "XXX";
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_Calc_TemporaryLandingReasonInfo, ListValidation.InvalidCodeMessageError);

					Bill.JPB_Calc_TemporaryLandingDuration = 0;
					Bill.JPB_Calc_TransportMode = TransportModeList.Codes.Ship;
					Bill.JPB_Calc_TemporaryLandingReason = "XXX";
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_Calc_TemporaryLandingReasonInfo, ListValidation.InvalidCodeMessageError);

					Bill.JPB_Calc_TemporaryLandingDuration = 1;
					Bill.JPB_Calc_TemporaryLandingReason = TemporaryLandingReasonCodeList.Codes.RepackingGoodsInOtherContainers;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingReasonInfo);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingReasonInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingReasonInfo);
				}
			});

			CombineAssertions("Test for VOCC", () =>
			{
				ResetAFRHeaderBill();
				Header.JPH_IsShippingLineEntry = true;
				using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
				{
					Bill.InBondDetailInitiator = inbondInitiator;
					Header.InBondDetailInitiator = inbondInitiator;

					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingReasonInfo);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingReasonInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingReasonInfo);

					Bill.JPB_Calc_TemporaryLandingReason = string.Empty;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingReasonInfo);

					Bill.JPB_Calc_GoodsValue = 1;
					Bill.JPB_Calc_TemporaryLandingReason = string.Empty;
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_Calc_TemporaryLandingReasonInfo, ValidationConstants.InbondDetails.ValueMissingWhenTemporaryLandingIntended(Bill.JPB_Calc_TemporaryLandingReasonInfo.HumanReadableName));

					Bill.JPB_Calc_GoodsValue = 1;
					Bill.JPB_Calc_TemporaryLandingReason = "XXX";
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_Calc_TemporaryLandingReasonInfo, ListValidation.InvalidCodeMessageError);

					Bill.JPB_Calc_GoodsValue = 0;
					Bill.JPB_Calc_TransportMode = TransportModeList.Codes.Ship;
					Bill.JPB_Calc_TemporaryLandingReason = "XXX";
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_Calc_TemporaryLandingReasonInfo, ListValidation.InvalidCodeMessageError);

					Bill.JPB_Calc_GoodsValue = 1;
					Bill.JPB_Calc_TemporaryLandingReason = TemporaryLandingReasonCodeList.Codes.RepackingGoodsInOtherContainers;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingReasonInfo);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingReasonInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingReasonInfo);

					Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "XXX";
					Bill.Validation.ValidateAll();
					AssertHasMessageError(Bill.JPB_Calc_TemporaryLandingReasonInfo, ValidationConstants.InbondDetails.NoValueAllowedWhenGeneralCustomsTransitIntended);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingReasonInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingReasonInfo);

					Bill.JPB_Calc_TemporaryLandingReason = string.Empty;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingReasonInfo);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingReasonInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingReasonInfo);
				}
			});
		}

		public void TestCheckJPB_Calc_TemporaryLandingDuration()
		{
			CombineAssertions("Test for NVOCC", () =>
			{
				ResetAFRHeaderBill();
				Header.JPH_IsShippingLineEntry = false;
				using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
				{
					Bill.InBondDetailInitiator = inbondInitiator;
					Header.InBondDetailInitiator = inbondInitiator;

					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingDurationInfo);

					Bill.JPB_Calc_TemporaryLandingDuration = -1;
					Bill.Validation.ValidateAll();
					AssertHasErrorContaining(Bill.JPB_Calc_TemporaryLandingDurationInfo, ValidationConstants.Bill.ValueCannotBeNegative);

					Bill.JPB_Calc_TemporaryLandingDuration = 0;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingDurationInfo);

					Bill.JPB_Calc_TemporaryLandingReason = "x";
					Bill.JPB_Calc_TransportMode = TransportModeList.Codes.Ship;
					Bill.JPB_Calc_TemporaryLandingDuration = 0;
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_Calc_TemporaryLandingDurationInfo, ValidationConstants.InbondDetails.TemporaryLandingDurationZeroIsNotAllowed);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingDurationInfo);

					Bill.JPB_Calc_TemporaryLandingReason = string.Empty;
					Bill.JPB_Calc_TemporaryLandingDuration = -1;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertHasErrorContaining(Bill.JPB_Calc_TemporaryLandingDurationInfo, ValidationConstants.Bill.ValueCannotBeNegative);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingDurationInfo);

					Bill.JPB_Calc_ESDT = ZDateTime.Now.Date.AddDays(1);
					Bill.JPB_Calc_EFDT = ZDateTime.Now.Date.AddDays(1);
					Bill.JPB_Calc_TemporaryLandingDuration = 2;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);

					Bill.JPB_Calc_ESDT = ZDateTime.Now.Date.AddDays(1);
					Bill.JPB_Calc_EFDT = ZDateTime.Now.Date.AddDays(2);
					Bill.JPB_Calc_TemporaryLandingDuration = 2;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingDurationInfo);
				}
			});

			CombineAssertions("Test for VOCC", () =>
			{
				ResetAFRHeaderBill();
				Header.JPH_IsShippingLineEntry = true;
				using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
				{
					Bill.InBondDetailInitiator = inbondInitiator;
					Header.InBondDetailInitiator = inbondInitiator;

					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingDurationInfo);

					Bill.JPB_Calc_TemporaryLandingDuration = -1;
					Bill.Validation.ValidateAll();
					AssertHasErrorContaining(Bill.JPB_Calc_TemporaryLandingDurationInfo, ValidationConstants.Bill.ValueCannotBeNegative);

					Bill.JPB_Calc_TemporaryLandingDuration = 0;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingDurationInfo);

					Bill.JPB_Calc_TemporaryLandingReason = "x";
					Bill.JPB_Calc_TransportMode = TransportModeList.Codes.Ship;
					Bill.JPB_Calc_TemporaryLandingDuration = 0;
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_Calc_TemporaryLandingDurationInfo, ValidationConstants.InbondDetails.TemporaryLandingDurationZeroIsNotAllowed);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingDurationInfo);

					Bill.JPB_Calc_TemporaryLandingReason = string.Empty;
					Bill.JPB_Calc_TemporaryLandingDuration = -1;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertHasErrorContaining(Bill.JPB_Calc_TemporaryLandingDurationInfo, ValidationConstants.Bill.ValueCannotBeNegative);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingDurationInfo);

					Bill.JPB_Calc_ESDT = ZDateTime.Now.Date.AddDays(1);
					Bill.JPB_Calc_EFDT = ZDateTime.Now.Date.AddDays(1);
					Bill.JPB_Calc_TemporaryLandingDuration = 2;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);

					Bill.JPB_Calc_ESDT = ZDateTime.Now.Date.AddDays(1);
					Bill.JPB_Calc_EFDT = ZDateTime.Now.Date.AddDays(2);
					Bill.JPB_Calc_TemporaryLandingDuration = 2;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingDurationInfo);

					Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "XXX";
					Bill.Validation.ValidateAll();
					AssertHasMessageError(Bill.JPB_Calc_TemporaryLandingDurationInfo, ValidationConstants.InbondDetails.NoValueAllowedWhenGeneralCustomsTransitIntended);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingDurationInfo);

					Bill.JPB_Calc_TemporaryLandingDuration = 0;
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoErrors(Bill.JPB_Calc_TemporaryLandingDurationInfo);
					AssertNoWarnings(Bill.JPB_Calc_TemporaryLandingReasonInfo);
				}
			});
		}

		public void TestCheckJPB_Calc_ESDT()
		{
			CombineAssertions("Test for NVOCC", () =>
			{
				ResetAFRHeaderBill();
				Header.JPH_IsShippingLineEntry = false;
				using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
				{
					Bill.InBondDetailInitiator = inbondInitiator;
					Header.InBondDetailInitiator = inbondInitiator;

					AssertNoMessageErrors(Bill.JPB_Calc_ESDTInfo);
					AssertNoErrors(Bill.JPB_Calc_ESDTInfo);
					AssertNoWarnings(Bill.JPB_Calc_ESDTInfo);

					Bill.JPB_Calc_ESDT = ZDateTime.Now.Date.AddDays(-10);
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_Calc_ESDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);

					Factory.Save();
					Bill.Validation.ValidateAll();
					AssertNoMessageErrorContaining(Bill.JPB_Calc_ESDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);

					Bill.JPB_Calc_ESDT = ZDateTime.Now.Date.AddDays(1);
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_ESDTInfo);
					AssertNoErrors(Bill.JPB_Calc_ESDTInfo);
					AssertNoWarnings(Bill.JPB_Calc_ESDTInfo);
				}
			});

			CombineAssertions("Test for VOCC", () =>
			{
				ResetAFRHeaderBill();
				Header.JPH_IsShippingLineEntry = true;
				using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
				{
					Bill.InBondDetailInitiator = inbondInitiator;
					Header.InBondDetailInitiator = inbondInitiator;

					AssertNoMessageErrors("TST: test 1-1", Bill.JPB_Calc_ESDTInfo);
					AssertNoErrors("TST: test 1-2", Bill.JPB_Calc_ESDTInfo);
					AssertNoWarnings("TST: test 1-3", Bill.JPB_Calc_ESDTInfo);

					Bill.JPB_Calc_ESDT = ZDateTime.Now.Date.AddDays(-10);
					Bill.Validation.ValidateAll();
					AssertHasMessageError("TST: test 2-1", Bill.JPB_Calc_ESDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);

					Factory.Save();
					Bill.Validation.ValidateAll();
					AssertNoMessageError("TST: test 3-1", Bill.JPB_Calc_ESDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);

					Bill.JPB_Calc_ESDT = ZDateTime.Now.Date.AddDays(1);
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors("TST: test 4-1", Bill.JPB_Calc_ESDTInfo);
					AssertNoErrors("TST: test 4-2", Bill.JPB_Calc_ESDTInfo);
					AssertNoWarnings("TST: test 4-3", Bill.JPB_Calc_ESDTInfo);

					Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "XXX";
					Bill.Validation.ValidateAll();
					AssertHasMessageError("TST: test 5-1", Bill.JPB_Calc_ESDTInfo, ValidationConstants.InbondDetails.NoValueAllowedWhenGeneralCustomsTransitIntended);
					AssertNoErrors("TST: test 5-2", Bill.JPB_Calc_ESDTInfo);
					AssertNoWarnings("TST: test 5-3", Bill.JPB_Calc_ESDTInfo);

					Bill.JPB_Calc_ESDT = ZDateTime.Empty;
					AssertNoMessageErrors("TST: test 6-1", Bill.JPB_Calc_ESDTInfo);
					AssertNoErrors("TST: test 6-2", Bill.JPB_Calc_ESDTInfo);
					AssertNoWarnings("TST: test 6-3", Bill.JPB_Calc_ESDTInfo);
				}
			});
		}

		public void TestCheckJPB_Calc_EFDT()
		{
			CombineAssertions("Test for NVOCC", () =>
			{
				ResetAFRHeaderBill();
				Header.JPH_IsShippingLineEntry = false;
				using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
				{
					Bill.InBondDetailInitiator = inbondInitiator;
					Header.InBondDetailInitiator = inbondInitiator;

					AssertNoMessageErrors(Bill.JPB_Calc_EFDTInfo);
					AssertNoErrors(Bill.JPB_Calc_EFDTInfo);
					AssertNoWarnings(Bill.JPB_Calc_EFDTInfo);

					Bill.JPB_Calc_EFDT = ZDateTime.Now.Date.AddDays(-10);
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_Calc_EFDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);

					Bill.JPB_Calc_EFDT = ZDateTime.Now.Date.AddDays(-10);
					Factory.Save();
					Bill.Validation.ValidateAll();
					AssertNoMessageErrorContaining(Bill.JPB_Calc_EFDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);

					Bill.JPB_Calc_ESDT = ZDateTime.Now.Date.AddDays(10);
					Bill.JPB_Calc_EFDT = ZDateTime.Now.Date.AddDays(8);
					Bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining(Bill.JPB_Calc_EFDTInfo, ValidationConstants.InbondDetails.EFDTShouldBeAfterESDT);

					Bill.JPB_Calc_ESDT = ZDateTime.Now.Date.AddDays(8);
					Bill.JPB_Calc_EFDT = ZDateTime.Now.Date.AddDays(8);
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors(Bill.JPB_Calc_EFDTInfo);
					AssertNoErrors(Bill.JPB_Calc_EFDTInfo);
					AssertNoWarnings(Bill.JPB_Calc_EFDTInfo);
				}
			});

			CombineAssertions("Test for VOCC", () =>
			{
				ResetAFRHeaderBill();
				Header.JPH_IsShippingLineEntry = true;
				using (var inbondInitiator = new InBondDetailInitiatorTestHelper())
				{
					Bill.InBondDetailInitiator = inbondInitiator;
					Header.InBondDetailInitiator = inbondInitiator;
					//var emptyValue = Bill.JPB_Calc_EFDT;

					AssertNoMessageErrors("TST: Test 1-1", Bill.JPB_Calc_EFDTInfo);
					AssertNoErrors("TST: Test 1-1", Bill.JPB_Calc_EFDTInfo);
					AssertNoWarnings("TST: Test 1-1", Bill.JPB_Calc_EFDTInfo);

					Bill.JPB_Calc_EFDT = ZDateTime.Now.Date.AddDays(-10);
					Bill.Validation.ValidateAll();
					AssertHasMessageError("TST: Test 2-1", Bill.JPB_Calc_EFDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);

					Bill.JPB_Calc_EFDT = ZDateTime.Now.Date.AddDays(-10);
					Factory.Save();
					Bill.Validation.ValidateAll();
					AssertNoMessageError("TST: Test 3-1", Bill.JPB_Calc_EFDTInfo, ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);

					Bill.JPB_Calc_ESDT = ZDateTime.Now.Date.AddDays(10);
					Bill.JPB_Calc_EFDT = ZDateTime.Now.Date.AddDays(8);
					Bill.Validation.ValidateAll();
					AssertHasMessageError("TST: Test 4-1", Bill.JPB_Calc_EFDTInfo, ValidationConstants.InbondDetails.EFDTShouldBeAfterESDT);

					Bill.JPB_Calc_ESDT = ZDateTime.Now.Date.AddDays(8);
					Bill.JPB_Calc_EFDT = ZDateTime.Now.Date.AddDays(8);
					Bill.Validation.ValidateAll();
					AssertNoMessageErrors("TST: Test 5-1", Bill.JPB_Calc_EFDTInfo);
					AssertNoErrors("TST: Test 5-2", Bill.JPB_Calc_EFDTInfo);
					AssertNoWarnings("TST: Test 5-3", Bill.JPB_Calc_EFDTInfo);

					Bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = "XXX";
					Bill.Validation.ValidateAll();
					AssertHasMessageError("TST: Test 6-1", Bill.JPB_Calc_EFDTInfo, ValidationConstants.InbondDetails.NoValueAllowedWhenGeneralCustomsTransitIntended);
					AssertNoErrors("TST: Test 6-2", Bill.JPB_Calc_EFDTInfo);
					AssertNoWarnings("TST: Test 6-3", Bill.JPB_Calc_EFDTInfo);

					Bill.JPB_Calc_TemporaryLandingReason = string.Empty;
					Bill.JPB_Calc_EFDT = ZDateTime.Empty;
					AssertNoMessageErrors("TST: Test 7-1", Bill.JPB_Calc_EFDTInfo);
					AssertNoErrors("TST: Test 7-2", Bill.JPB_Calc_EFDTInfo);
					AssertNoWarnings("TST: Test 7-3", Bill.JPB_Calc_EFDTInfo);
				}
			});
		}

		#endregion

		public void TestCheckOtherRelevantLawCodes()
		{
			ResetAFRHeaderBill();
			Bill.OtherRelevantLaws.AddNew();
			Bill.OtherRelevantLaws.Last().CY_Data = OtherRelevantLawsAndOrdinancesCodeList.Codes.AD;
			Bill.OtherRelevantLaws.AddNew();
			Bill.OtherRelevantLaws.Last().CY_Data = OtherRelevantLawsAndOrdinancesCodeList.Codes.AM;
			Bill.OtherRelevantLaws.AddNew();
			Bill.OtherRelevantLaws.Last().CY_Data = OtherRelevantLawsAndOrdinancesCodeList.Codes.AN;
			Bill.OtherRelevantLaws.AddNew();
			Bill.OtherRelevantLaws.Last().CY_Data = OtherRelevantLawsAndOrdinancesCodeList.Codes.EI;
			Bill.OtherRelevantLaws.AddNew();
			Bill.OtherRelevantLaws.Last().CY_Data = OtherRelevantLawsAndOrdinancesCodeList.Codes.CA;
			Bill.OtherRelevantLaws.AddNew();
			Bill.OtherRelevantLaws.Last().CY_Data = OtherRelevantLawsAndOrdinancesCodeList.Codes.EX;

			AssertNoRowWarningContaining(Bill, ValidationConstants.Bill.MaximumOtherRelevantLawExceeded);

			Bill.Validation.ValidateAll();
			AssertHasRowWarning(Bill, ValidationConstants.Bill.MaximumOtherRelevantLawExceeded);

			Bill.OtherRelevantLaws.Last().CY_Data = string.Empty;
			Bill.Validation.ValidateAll();
			AssertNoRowWarningContaining(Bill, ValidationConstants.Bill.MaximumOtherRelevantLawExceeded);

			Bill.OtherRelevantLaws.Last().Delete();
			Bill.Validation.ValidateAll();
			AssertNoRowWarningContaining(Bill, ValidationConstants.Bill.MaximumOtherRelevantLawExceeded);
		}

		public void TestCheckContainersCount()
		{
			ResetAFRHeaderBill();

			Bill.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(Bill, ValidationConstants.Bill.AtLeastOneContainerIsRequired);

			Bill.Containers.AddNew();
			Bill.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(Bill, ValidationConstants.Bill.AtLeastOneContainerIsRequired);

			for (int i = 0; i < 100; i++)
			{
				Bill.Containers.AddNew();
			}
			Bill.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(Bill, ValidationConstants.Bill.MaximumContainersCountExceeded);
		}

		#region Preperation

		JPAFRBills Bill
		{
			get { return bill ?? (bill = Header.Bills.AddNew()); }
		}
		JPAFRBills bill;

		JPAFRHeader Header
		{
			get { return header ?? (header = Factory.New<JPAFRHeader>()); }
		}
		JPAFRHeader header;

		void ResetAFRHeaderBill()
		{
			this.header = null;
			this.bill = null;
		}

		#endregion
	}
}
