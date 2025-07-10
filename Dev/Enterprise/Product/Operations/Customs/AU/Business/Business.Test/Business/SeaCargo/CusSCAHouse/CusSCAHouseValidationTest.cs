using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAHouseValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCA_ConsigneeBusinessNumber()
		{
			var house = Factory.New<CusSCAHouse>();
			house.CA_ConsigneeIdentifier = "123";
			house.CA_ConsigneeBusinessNumber = "12345678901234";
			AssertHasMessageError(house.CA_ConsigneeBusinessNumberInfo, "Only one of ABN/CAC combination OR Importer Identifier should be entered.");

			house.CA_ConsigneeIdentifier = "";
			house.CA_ConsigneeBusinessNumber = "12345678901234";
			AssertNoMessageError(house.CA_ConsigneeBusinessNumberInfo, "Only one of ABN/CAC combination OR Importer Identifier should be entered.");
		}

		public void TestValidateCA_ConsigneeIdentifier()
		{
			var house = Factory.New<CusSCAHouse>();
			house.CA_ConsigneeBusinessNumber = "12345678901234";
			house.CA_ConsigneeIdentifier = "123";
			AssertHasMessageError(house.CA_ConsigneeIdentifierInfo, "Only one of ABN/CAC combination OR Importer Identifier should be entered.");

			house.CA_ConsigneeBusinessNumber = "";
			house.CA_ConsigneeIdentifier = "123";
			AssertNoMessageError(house.CA_ConsigneeIdentifierInfo, "Only one of ABN/CAC combination OR Importer Identifier should be entered.");
		}

		public void TestValidateEstablishmentCode()
		{
			RefUNLOCO testUNLOCO = Factory.New<RefUNLOCO>();
			testUNLOCO.RL_Code = "CHIP1";
			testUNLOCO.RL_PortName = "Test Port Code Name";
			testUNLOCO.RL_HasAirport = true;
			testUNLOCO.RL_HasSeaport = true;

			CusSCAHouse bizO = Factory.New<CusSCAHouse>();
			bizO.CA_RL_NK_PortOfDestination = testUNLOCO.RL_Code;

			AssertNoMessageErrors("Pre-Condition: No Message Errors", bizO.CA_RL_NK_PortOfDestinationInfo);

			bizO.CA_RL_NK_PortOfDestination = "AUSY1";
			bizO.CA_RL_NK_PortOfDestination = testUNLOCO.RL_Code;
			AssertNoMessageErrors("Pre-Condition: No Message Errors", bizO.CA_RL_NK_PortOfDestinationInfo);

			bizO.CA_RL_NK_PortOfDestination = "AUSY1";
			bizO.CA_RL_NK_PortOfDestination = testUNLOCO.RL_Code;
			AssertNoMessageErrors("Pre-Condition: No Message Errors", bizO.CA_RL_NK_PortOfDestinationInfo);

			RefLocoMap airLocoMap = testUNLOCO.RefLocoMaps.AddNew();
			airLocoMap.RY_LocalPortCode = "4R";
			airLocoMap.RY_RN = Core.CountryGuids.Instance.Australia;
			airLocoMap.RY_SystemUsage = AirSeaMailSystemUsageList.Codes.Air;

			bizO.CA_RL_NK_PortOfDestination = "AUSYD";
			bizO.CA_RL_NK_PortOfDestination = testUNLOCO.RL_Code;
			AssertNoMessageErrors("Pre-Condition: No Message Errors", bizO.CA_RL_NK_PortOfDestinationInfo);

			bizO.CA_RL_NK_PortOfDestination = "AUSYD";
			bizO.CA_RL_NK_PortOfDestination = testUNLOCO.RL_Code;
			AssertNoMessageErrors("Pre-Condition: No Message Errors", bizO.CA_RL_NK_PortOfDestinationInfo);

			RefLocoMap seaLocoMap = testUNLOCO.RefLocoMaps.AddNew();
			seaLocoMap.RY_LocalPortCode = "3S";
			seaLocoMap.RY_RN = Core.CountryGuids.Instance.Australia;
			seaLocoMap.RY_SystemUsage = AirSeaMailSystemUsageList.Codes.Sea;

			bizO.CA_RL_NK_PortOfDestination = "AUSYD";
			bizO.CA_RL_NK_PortOfDestination = testUNLOCO.RL_Code;
			AssertNoMessageErrors("Pre-Condition: No Message Errors", bizO.CA_RL_NK_PortOfDestinationInfo);

			RefLocoMap mailLocoMap = testUNLOCO.RefLocoMaps.AddNew();
			mailLocoMap.RY_LocalPortCode = "1Z";
			mailLocoMap.RY_RN = Core.CountryGuids.Instance.Australia;
			mailLocoMap.RY_SystemUsage = AirSeaMailSystemUsageList.Codes.Mail;

			bizO.CA_RL_NK_PortOfDestination = "AUSYD";
			bizO.CA_RL_NK_PortOfDestination = testUNLOCO.RL_Code;
			AssertNoMessageErrors("Pre-Condition: No Message Errors", bizO.CA_RL_NK_PortOfDestinationInfo);
		}

		public void TestHouseBillValidation()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();
			container.Pivots.Add(houseBill.Pivot.AddNew());
			houseBill.Validation.ValidateCA_HouseBill();
			AssertHasMessageErrors("Pre-Condition, HouseBill Number initially has Messages Errors", houseBill.CA_HouseBillInfo);
			houseBill.CA_HouseBill = "TEST123";
			AssertNoMessageErrors("HouseBill Number should have no Message Errors", houseBill.CA_HouseBillInfo);
			CusSCAHouse houseBill2 = oceanBill.HouseBills.AddNew();
			houseBill2.CA_HouseBill = "TEST123";
			AssertHasMessageErrors("HouseBill Number should have Message Errors as it is duplicated", houseBill2.CA_HouseBillInfo);
		}

		public void TestPaymentTypeValidation()
		{
			houseBill.Validation.ValidateCA_PrepaidCollectOther();
			AssertHasMessageErrors("by default", houseBill.CA_PrepaidCollectOtherInfo);
			houseBill.CA_PrepaidCollectOther = "FOO";
			AssertHasMessageErrors("when invalid", houseBill.CA_PrepaidCollectOtherInfo);
			houseBill.CA_PrepaidCollectOther = CMRMethodsOfPayment.Codes.Collect;
			AssertNoNotifications("when valid", houseBill.CA_PrepaidCollectOtherInfo);
		}

		public void TestPortOfOriginValidation()
		{
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			houseBill.Validation.ValidateCA_RL_NK_PortOfOrigin();
			Assert("Pre-Condition, Port of Loading initially has Messages Errors", houseBill.CA_RL_NK_PortOfOriginInfo.HasMessageErrors() && !houseBill.CA_RL_NK_PortOfOriginInfo.HasErrors());
			houseBill.CA_RL_NK_PortOfOrigin = "TEST1";
			Assert("Port of Origin should have errors as TEST1 is not a valid UNLOCO", houseBill.CA_RL_NK_PortOfOriginInfo.HasMessageErrors());
			houseBill.CA_RL_NK_PortOfOrigin = "USLAX";
			Assert("Port of Origin should have no message errors", !houseBill.CA_RL_NK_PortOfOriginInfo.HasMessageErrors() && !houseBill.CA_RL_NK_PortOfOriginInfo.HasErrors());
			houseBill.CA_HouseBill = HouseBillNumber;
			houseBill.CA_RL_NK_PortOfOrigin = "";
			AssertHasMessageError(houseBill.CA_RL_NK_PortOfOriginInfo, "Port of origin is required on house bill: " + HouseBillNumber);
		}

		public void TestPortOfDestinationValidation()
		{
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			houseBill.Validation.ValidateCA_RL_NK_PortOfDestination();
			Assert("Pre-Condition, Port of Destination initially has Messages Errors", houseBill.CA_RL_NK_PortOfDestinationInfo.HasMessageErrors() && !houseBill.CA_RL_NK_PortOfDestinationInfo.HasErrors());
			houseBill.CA_RL_NK_PortOfDestination = "TEST1";
			Assert("Port of Destination should have errors as TEST1 is not a valid UNLOCO", houseBill.CA_RL_NK_PortOfDestinationInfo.HasMessageErrors());
			houseBill.CA_RL_NK_PortOfDestination = "USLAX";
			Assert("Port of Destination should not have message errors if the Port is not an Australian Port - Allow Transhipments", !houseBill.CA_RL_NK_PortOfDestinationInfo.HasMessageErrors());
			houseBill.CA_RL_NK_PortOfDestination = "AUMEL";
			Assert("Port of Destination should have no message errors", !houseBill.CA_RL_NK_PortOfDestinationInfo.HasMessageErrors() && !houseBill.CA_RL_NK_PortOfDestinationInfo.HasErrors());
			houseBill.CA_HouseBill = HouseBillNumber;
			houseBill.CA_RL_NK_PortOfDestination = "";
			AssertHasMessageError(houseBill.CA_RL_NK_PortOfDestinationInfo, "Port of destination is required on house bill: " + HouseBillNumber);
		}

		public void TestGoodsOriginValidation()
		{
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			houseBill.Validation.ValidateCA_RN_NKGoodsOrigin();
			Assert("Pre-Condition, Goods Origin initially has Messages Errors", houseBill.CA_RN_NKGoodsOriginInfo.HasMessageErrors() && !houseBill.CA_RN_NKGoodsOriginInfo.HasErrors());
			houseBill.CA_RN_NKGoodsOrigin = "ZY";
			Assert("Goods Origin should have errors as the origin is not real", houseBill.CA_RN_NKGoodsOriginInfo.HasMessageErrors());
			houseBill.CA_RN_NKGoodsOrigin = "NZ";
			Assert("Goods Origin should have no message errors", !houseBill.CA_RN_NKGoodsOriginInfo.HasMessageErrors() && !houseBill.CA_RN_NKGoodsOriginInfo.HasErrors());
			houseBill.CA_HouseBill = HouseBillNumber;
			houseBill.CA_RN_NKGoodsOrigin = "";
			AssertHasMessageError(houseBill.CA_RN_NKGoodsOriginInfo, "Goods origin is required on house bill: " + HouseBillNumber);
		}

		public void TestConsignorNameValidation()
		{
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			houseBill.Validation.ValidateCA_ConsignorName();
			Assert("Pre-Condition, Consignor Name initially has Messages Errors", houseBill.CA_ConsignorNameInfo.HasMessageErrors());
			houseBill.CA_ConsignorName = "Scott's Test Company";
			Assert("Consignor Name should have no message errors", !houseBill.CA_ConsignorNameInfo.HasMessageErrors());
			houseBill.CA_HouseBill = HouseBillNumber;
			houseBill.CA_ConsignorName = "";
			AssertHasMessageError(houseBill.CA_ConsignorNameInfo, "Consignor name is required on house bill: " + HouseBillNumber);
		}

		public void TestConsigneeNameValidation()
		{
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			houseBill.Validation.ValidateCA_ConsigneeName();
			Assert("Pre-Condition, Consignee Name initially has Messages Errors", houseBill.CA_ConsigneeNameInfo.HasMessageErrors());
			houseBill.CA_ConsigneeName = "Scott's Test Company";
			Assert("Consignee Name should have no message errors", !houseBill.CA_ConsigneeNameInfo.HasMessageErrors());

			houseBill.CA_HouseBill = HouseBillNumber;
			houseBill.CA_ConsigneeName = "";
			AssertHasMessageError(houseBill.CA_ConsigneeNameInfo, "Consignee name is required on house bill: " + HouseBillNumber);
		}

		public void TestValidateContainerCount()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_LloydsIMO = (Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, SQLComparisonOperator.NotEqual, ZString.Empty))).RV_LloydsNumber;
			oceanBill.CB_OceanBill = "HKGSYD4561268";
			oceanBill.CB_Voyage = "432";
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			oceanBill.CB_RL_NKPortOfLoading = "HKHKG";
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			houseBill.CA_HouseBill = HouseBillNumberForErrorMessage;
			houseBill.CA_RL_NK_PortOfDestination = "AUSYD";
			houseBill.CA_RL_NK_PortOfOrigin = "HKHKG";

			houseBill.RunPreSaveValidation();
			AssertHasMessageError("House bill should have message error about no container details", houseBill.CA_HouseBillInfo, CusSCAHouseValidation.ContainerCountMessageError + " " + HouseBillNumberForErrorMessage);
		}

		[TestDate(2024, 01, 22, 01, 15, 00)]
		public void TestIsDuplicateValidation()
		{
			var user1 = Factory.New<GlbStaff>();
			user1.GS_Code = "UR1";
			user1.GS_LoginName = "User1";

			var user2 = Factory.New<GlbStaff>();
			user2.GS_Code = "UR2";
			user2.GS_LoginName = "User2";

			var oceanBill = Factory.New<CusSCAOceanBill>();
			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			Factory.Save();

			AssertEquals("Timezone should be Brisbane UTC +10 (no daylight savings)", "BNE", Env.CurrentBranch.Code);
			var expectedValidationMessage = $"An existing House Bill (created by UR1 at 22-Jan-2024 11:15:00) is already linking the Shipment to this Sea Cargo.";

			using (Env.SetTemporaryUserContext(user1.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				CusSCAHouse houseBill1 = Factory.New<CusSCAHouse>();
				houseBill1.CA_JS = shipment.PK;
				houseBill1.CA_CB = oceanBill.PK;

				CusSCAHouse houseBill2 = null;
				using (Env.SetTemporaryUserContext(user2.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					var factory2 = new BusinessObjectFactory();
					houseBill2 = factory2.New<CusSCAHouse>();
					houseBill2.CA_JS = shipment.PK;
					houseBill2.CA_CB = oceanBill.PK;

					houseBill2.Validation.ValidateCA_JS();
					AssertNoErrorContaining(houseBill2.CA_JSInfo, expectedValidationMessage);
				}

				Factory.Save();

				using (Env.SetTemporaryUserContext(user2.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					houseBill2.Validation.ValidateCA_JS();
					AssertHasErrorContaining(houseBill2.CA_JSInfo, expectedValidationMessage);
				}
			}
		}

		public void TestResponsiblePartyIDIsKeyField()
		{
			houseBill.CA_MessageStatus = ZString.Empty;
			houseBill.CA_ResponsiblePartyID = "foo";
			Factory.Save();
			houseBill.CA_ResponsiblePartyID = "bar";
			AssertNoNotifications(houseBill.CA_ResponsiblePartyIDInfo);

			houseBill.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			houseBill.CA_ResponsiblePartyID = "foo";
			Factory.Save();
			houseBill.CA_ResponsiblePartyID = "bar";
			AssertHasErrorContaining(houseBill.CA_ResponsiblePartyIDInfo, "previous value (foo)");
		}

		public void TestValidateMasterHouseAgainstSelfAssessed()
		{
			houseBill.CA_IsMasterHouse = true;
			AssertEquals("This house can be a master", false, houseBill.CA_IsMasterHouseInfo.HasMessageErrors());

			CusSCAPivot pivot = houseBill.Pivot.AddNew();
			pivot.CV_IsSAC = true;

			houseBill.CA_IsMasterHouse = true;
			AssertEquals("Has Self-assessed packages", true, houseBill.Pivot.HasAnElementSelfAssessed);
			AssertEquals("This house cannot be a master as there is a self-assessed package", true, houseBill.CA_IsMasterHouseInfo.HasMessageErrors());
		}

		public void TestConsigneeAddress()
		{
			houseBill.Validation.ValidateCA_ConsigneeAddress1();
			AssertHasMessageErrors("by default", houseBill.CA_ConsigneeAddress1Info);

			houseBill.CA_ConsigneeAddress1 = "blah";
			AssertNoNotifications("when set", houseBill.CA_ConsigneeAddress1Info);

			houseBill.CA_HouseBill = HouseBillNumber;
			houseBill.CA_ConsigneeAddress1 = "";
			AssertHasMessageError(houseBill.CA_ConsigneeAddress1Info, "Consignee address line 1 required on house bill: " + HouseBillNumber);
		}
		public void TestKeyMessagingFields()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();

			string houseBill = "23423";
			string masterHouseBill = "623423";

			house.CA_HouseBill = houseBill;
			house.CA_MasterHouseBill = masterHouseBill;
			house.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			Factory.Save();

			house.CA_HouseBill = "234";
			Assert("Should have an error", house.CA_HouseBillInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", houseBill)));

			house.CA_HouseBill = houseBill;
			Assert("Should have no errors", !house.CA_HouseBillInfo.HasErrors());
		}

		public void TestConsignorAddress()
		{
			houseBill.Validation.ValidateCA_ConsignorAddress1();
			AssertHasMessageErrors("by default", houseBill.CA_ConsignorAddress1Info);

			houseBill.CA_ConsignorAddress1 = "blah";
			AssertNoNotifications("when set", houseBill.CA_ConsignorAddress1Info);

			houseBill.CA_HouseBill = HouseBillNumber;
			houseBill.CA_ConsignorAddress1 = "";
			AssertHasMessageError(houseBill.CA_ConsignorAddress1Info, "Consignor address line 1 required on house bill: " + HouseBillNumber);
		}

		public void TestNotifyAddress()
		{
			houseBill.Validation.ValidateCA_NotifyAddress1();
			AssertNoNotifications("by default", houseBill.CA_NotifyAddress1Info);

			houseBill.CA_NotifyName = "foo";
			houseBill.Validation.ValidateCA_NotifyAddress1();
			AssertHasMessageErrors("when name set and address blank", houseBill.CA_NotifyAddress1Info);

			houseBill.CA_NotifyAddress1 = "bar";
			AssertNoNotifications("when name and address set", houseBill.CA_NotifyAddress1Info);

			houseBill.CA_HouseBill = HouseBillNumber;
			houseBill.CA_NotifyAddress1 = "";
			AssertHasMessageError(houseBill.CA_NotifyAddress1Info, "Notify address is required when notify name is entered on house bill: " + HouseBillNumber);
		}

		public void TestMasterHouseValidatesSAC()
		{
			CusSCAPivot pivot = houseBill.Pivot.AddNew();
			houseBill.CA_IsMasterHouse = true;
			AssertNoNotifications("when masterhouse true, and sac false", houseBill.CA_IsMasterHouseInfo);

			pivot.CV_IsSAC = true;
			AssertHasMessageErrors("when sac changed to true", houseBill.CA_IsMasterHouseInfo);

			pivot.CV_IsSAC = false;
			AssertNoNotifications("when sac changed back to false", houseBill.CA_IsMasterHouseInfo);
		}

		public void TestConsigneeCountryCode()
		{
			houseBill.CA_HouseBill = HouseBillNumber;
			houseBill.Validation.ValidateCA_RN_NKConsigneeCountryCode();
			AssertHasMessageError(houseBill.CA_RN_NKConsigneeCountryCodeInfo, "You have not entered a Consignee country/region code required on house bill: " + HouseBillNumber + ".");
			houseBill.CA_RN_NKConsigneeCountryCode = "AU";
			AssertNoMessageErrors("Country/Region code is valid, no errors expected", houseBill.CA_RN_NKConsigneeCountryCodeInfo);

			houseBill.CA_RN_NKConsigneeCountryCode = "99";
			AssertListValidationInvalidCodeMessageError(houseBill.CA_RN_NKConsigneeCountryCodeInfo, true);
		}

		public void TestConsignorCountryCode()
		{
			houseBill.CA_HouseBill = HouseBillNumber;
			houseBill.Validation.ValidateCA_RN_NKConsignorCountryCode();
			AssertHasMessageError(houseBill.CA_RN_NKConsignorCountryCodeInfo, "You have not entered a Consignor country/region code required on house bill: " + HouseBillNumber + ".");
			houseBill.CA_RN_NKConsignorCountryCode = "AU";
			AssertNoMessageErrors("Country/Region code is valid, no errors expected", houseBill.CA_RN_NKConsignorCountryCodeInfo);

			houseBill.CA_RN_NKConsignorCountryCode = "XX";
			AssertListValidationInvalidCodeMessageError(houseBill.CA_RN_NKConsignorCountryCodeInfo, true);
		}

		public void TestNotifyPartyCountryCode()
		{
			houseBill.Validation.ValidateCA_RN_NKNotifyCountryCode();
			AssertNoNotifications("Pre-condition - no validation if Notify Party details are not entered", houseBill.CA_RN_NKNotifyCountryCodeInfo);

			houseBill.CA_NotifyName = "Bill Smith";
			houseBill.CA_HouseBill = HouseBillNumber;
			houseBill.Validation.ValidateCA_RN_NKNotifyCountryCode();
			AssertHasMessageError(houseBill.CA_RN_NKNotifyCountryCodeInfo, "You have not entered a Notify Party country/region code required on house bill: " + HouseBillNumber + ".");
			houseBill.CA_RN_NKNotifyCountryCode = "AU";
			AssertNoMessageErrors("Country/Region code is valid, no errors expected", houseBill.CA_RN_NKNotifyCountryCodeInfo);

			houseBill.CA_HouseBill = HouseBillNumber;
			houseBill.CA_RN_NKNotifyCountryCode = "ZZ";
			houseBill.Validation.ValidateCA_RN_NKNotifyCountryCode();
			AssertListValidationInvalidCodeMessageError(houseBill.CA_RN_NKNotifyCountryCodeInfo, true);
			houseBill.CA_RN_NKNotifyCountryCode = "AU";
			AssertListValidationInvalidCodeMessageError(houseBill.CA_RN_NKNotifyCountryCodeInfo, false);
			AssertNoMessageErrors("Country/Region code is valid, no errors expected", houseBill.CA_RN_NKNotifyCountryCodeInfo);
		}

		#region Implementation

		const string HouseBillNumberForErrorMessage = "HKGSYD3383829";
		const string HouseBillNumber = "HB0392TEST1";

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
			oceanBill = Factory.New<CusSCAOceanBill>();
			houseBill = oceanBill.HouseBills.AddNew();
		}

		CusSCAOceanBill oceanBill;
		CusSCAHouse houseBill;

		#endregion
	}
}
