using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class CusHAWBValidationAbstractTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCS_ConsigneeBusinessNumber()
		{
			HAWB.CS_ConsigneeIdentifier = "123";
			HAWB.CS_ConsigneeBusinessNumber = "12345678901234";
			AssertHasMessageError(HAWB.CS_ConsigneeBusinessNumberInfo, "Only one of ABN/CAC combination OR Importer Identifier should be entered.");

			HAWB.CS_ConsigneeIdentifier = "";
			HAWB.CS_ConsigneeBusinessNumber = "12345678901234";
			AssertNoMessageError(HAWB.CS_ConsigneeBusinessNumberInfo, "Only one of ABN/CAC combination OR Importer Identifier should be entered.");
		}

		public void TestValidateCS_ConsigneeIdentifier()
		{
			HAWB.CS_ConsigneeBusinessNumber = "12345678901234";
			HAWB.CS_ConsigneeIdentifier = "123";
			AssertHasMessageError(HAWB.CS_ConsigneeIdentifierInfo, "Only one of ABN/CAC combination OR Importer Identifier should be entered.");

			HAWB.CS_ConsigneeBusinessNumber = "";
			HAWB.CS_ConsigneeIdentifier = "123";
			AssertNoMessageError(HAWB.CS_ConsigneeIdentifierInfo, "Only one of ABN/CAC combination OR Importer Identifier should be entered.");
		}

		public void TestValidateCS_fPartShipConsignmentReference()
		{
			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "08100000001";
			var hawb1 = mawb1.ChildBills.AddNew();
			hawb1.CS_HAWB = "TESTBILL";
			hawb1.CS_fPartShipConsignmentReference = "TESTCONREF";
			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = "08100000002";
			var hawb2 = mawb1.ChildBills.AddNew();
			hawb2.CS_HAWB = "TESTBILL";
			hawb2.CS_fPartShipConsignmentReference = "TESTCONREF";
			Factory.Save();

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "08100000003";
			var hawb = mawb1.ChildBills.AddNew();
			hawb.CS_HAWB = "TESTBILL";
			hawb.CS_fPartShipConsignmentReference = "TESTCONREF2";
			AssertEquals(2, hawb.CS_fPartShipConsignmentReferenceInfo.Notifications.Count());
			AssertHasMessageError(hawb.CS_fPartShipConsignmentReferenceInfo, "MAWB 08100000001 also contains this HAWB but has a different Coinsignment Reference specified (TESTCONREF)");
			hawb.CS_fPartShipConsignmentReference = "TESTCONREF";
			AssertNoMessageErrors(hawb.CS_fPartShipConsignmentReferenceInfo);
			AssertNoMessageError(hawb.CS_fPartShipConsignmentReferenceInfo, "MAWB 08100000001 also contains this HAWB but has a different Coinsignment Reference specified (TESTCONREF)");
			hawb.CS_fPartShipConsignmentReference = ZString.Empty;
			AssertEquals(2, hawb.CS_fPartShipConsignmentReferenceInfo.Notifications.Count());
			AssertHasMessageError(hawb.CS_fPartShipConsignmentReferenceInfo, "MAWB 08100000001 also contains this HAWB but has a different Coinsignment Reference specified (TESTCONREF)");

			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			hawb.Validation.ValidateCS_fPartShipConsignmentReference();
			AssertNoMessageErrors(hawb.CS_fPartShipConsignmentReferenceInfo);
		}

		public void TestValidateCS_ResponsiblePartyID()
		{
			HAWB.MAWB.CM_IsCTOMAWB = false;
			HAWB.MAWB.CM_ResponsiblePartyID = "456789";
			Assert("Pre-condition", !HAWB.MAWB.CM_ResponsiblePartyID.IsEmpty);
			HAWB.CS_ResponsiblePartyID = "123456";
			AssertNoMessageError(HAWB.CS_ResponsiblePartyIDInfo, "You have not entered a Responsible Party Id and the MAWB Responsible party is also blank.");
			HAWB.CS_ResponsiblePartyID = ZString.Empty;
			AssertNoMessageError(HAWB.CS_ResponsiblePartyIDInfo, "You have not entered a Responsible Party Id and the MAWB Responsible party is also blank.");
			HAWB.MAWB.CM_ResponsiblePartyID = ZString.Empty;
			Assert("Pre-condition", HAWB.MAWB.CM_ResponsiblePartyID.IsEmpty);
			HAWB.CS_ResponsiblePartyID = "123456";
			AssertNoMessageError(HAWB.CS_ResponsiblePartyIDInfo, "You have not entered a Responsible Party Id and the MAWB Responsible party is also blank.");
			HAWB.CS_ResponsiblePartyID = ZString.Empty;
			AssertHasMessageError(HAWB.CS_ResponsiblePartyIDInfo, "You have not entered a Responsible Party Id and the MAWB Responsible party is also blank.");
			HAWB.MAWB.CM_IsCTOMAWB = true;
			HAWB.Validation.ValidateCS_ResponsiblePartyID();
			AssertHasMessageError("CTO also need a responsible party id", HAWB.CS_ResponsiblePartyIDInfo, "You have not entered a Responsible Party Id and the MAWB Responsible party is also blank.");
		}

		public void TestConsigneeAddressIsNotPOBox()
		{
			HAWB.CS_ConsigneeStreet = "PO BOX 2";
			Assert("PO BOX is not acceptable", HAWB.CS_ConsigneeStreetInfo.HasWarnings());
		}

		public void TestValidateWeight()
		{
			HAWB.CS_Weight = 0m;
			Assert("Weight required for messaging", HAWB.CS_WeightInfo.HasMessageErrors());
		}

		public void TestValidateWeightUQ()
		{
			HAWB.CS_WeightUQ = ZString.Empty;
			Assert("Weight UQ required for messaging", HAWB.CS_WeightUQInfo.HasMessageErrors());

			HAWB.CS_WeightUQ = "T";
			Assert("Weight UQ required for messaging", HAWB.CS_WeightUQInfo.HasMessageErrors());

			HAWB.CS_WeightUQ = "KG";
			Assert("Weight UQ", !HAWB.CS_WeightUQInfo.HasMessageErrors());
		}

		public void TestValidatePiecesManifested()
		{
			HAWB.CS_PiecesManifested = 0;
			Assert("Pieces Manifested required", HAWB.CS_PiecesManifestedInfo.HasNotifications());
		}

		public void TestValidateOriginBeingOverseas()
		{
			HAWB.CS_RL_NKOrigin = "AUSYD";
			Assert("Origin from AU is not right", HAWB.CS_RL_NKOriginInfo.HasNotifications());
		}

		public void TestValidateGoodsValue()
		{
			HAWB.CS_GoodsValue = 0m;
			Assert("Goods value is required", HAWB.CS_GoodsValueInfo.HasNotifications());
		}

		public void TestZeroGoodsValueHasWarning()
		{
			HAWB.CS_GoodsValue = 0m;
			Assert("Zero goods value is a warning", HAWB.CS_GoodsValueInfo.HasWarnings());

			HAWB.CS_GoodsValue = 40m;
			Assert("Goods value > 0", !HAWB.CS_GoodsValueInfo.HasWarnings());
		}

		public void TestValidateConsigneeName()
		{
			HAWB.Validation.ValidateCS_ConsigneeName();
			AssertHasMessageErrors("by default", HAWB.CS_ConsigneeNameInfo);

			HAWB.CS_ConsigneeName = "1";
			AssertHasMessageErrors("with no alpha-chars", HAWB.CS_ConsigneeNameInfo);

			HAWB.CS_ConsigneeName = "blah1";
			AssertNoNotifications("when entered with alpha-chars", HAWB.CS_ConsigneeNameInfo);
		}

		public void TestValidateConsignorName()
		{
			HAWB.Validation.ValidateCS_ConsignorName();
			AssertHasMessageErrors("by default", HAWB.CS_ConsignorNameInfo);

			HAWB.CS_ConsignorName = "1";
			AssertHasMessageErrors("with no alpha-chars", HAWB.CS_ConsignorNameInfo);

			HAWB.CS_ConsignorName = "blah1";
			AssertNoNotifications("when entered with alpha-chars", HAWB.CS_ConsignorNameInfo);
		}

		public void TestValidateConsigneeCountry()
		{
			var mAWB = Factory.New<CusMAWB>();
			CusHAWB houseBill = mAWB.ChildBills.AddNew();
			houseBill.CS_RN_NKConsigneeCountry = "XX";
			AssertHasError(houseBill.CS_RN_NKConsigneeCountryInfo, "Please enter a valid consignee country/region.");

			houseBill.CS_RN_NKConsigneeCountry = "AU";
			Assert("Country/Region is valid", !houseBill.CS_RN_NKConsigneeCountryInfo.HasNotifications());
			houseBill.CS_RN_NKConsigneeCountry = "";
			AssertHasWarnings(houseBill.CS_RN_NKConsigneeCountryInfo);
		}

		public void TestValidateConsignorCountry()
		{
			var mAWB = Factory.New<CusMAWB>();
			CusHAWB houseBill = mAWB.ChildBills.AddNew();
			houseBill.CS_RN_NKConsignorCountry = "XX";
			AssertHasError(houseBill.CS_RN_NKConsignorCountryInfo, "Please enter a valid consignor country/region.");

			houseBill.CS_RN_NKConsignorCountry = "AU";
			AssertHasMessageError(houseBill.CS_RN_NKConsignorCountryInfo, "Consignor should be an overseas party.");
			houseBill.CS_RN_NKConsignorCountry = "US";
			Assert("Country/Region is valid", !houseBill.CS_RN_NKConsignorCountryInfo.HasNotifications());
			houseBill.CS_RN_NKConsignorCountry = "";
			AssertHasWarnings(houseBill.CS_RN_NKConsignorCountryInfo);
		}

		public void TestValidateGoodsDescription()
		{
			HAWB.Validation.ValidateCS_GoodsDescription();
			AssertHasMessageErrors("by default", HAWB.CS_GoodsDescriptionInfo);

			HAWB.CS_GoodsDescription = "1";
			AssertHasMessageErrors("with no alpha-chars", HAWB.CS_GoodsDescriptionInfo);

			HAWB.CS_GoodsDescription = "blah1";
			AssertNoNotifications("when entered with alpha-chars", HAWB.CS_GoodsDescriptionInfo);
		}

		public virtual void TestValidateHAWB()
		{
			HAWB.Validation.ValidateCS_HAWB();
			AssertHasMessageErrors("by default", HAWB.CS_HAWBInfo);

			HAWB.CS_HAWB = "AOEUAO";
			AssertNoNotifications("when entered", HAWB.CS_HAWBInfo);
		}

		public void TestValidateRL_NKOrigin()
		{
			RefUNLOCO port = GetNewPort();
			port.RL_Code = "SGSI1";

			HAWB.Validation.ValidateCS_RL_NKOrigin();
			AssertHasMessageErrors("by default", HAWB.CS_RL_NKOriginInfo);

			HAWB.CS_RL_NKOrigin = "~~~";
			AssertHasMessageErrors("when invalid", HAWB.CS_RL_NKOriginInfo);

			HAWB.CS_RL_NKOrigin = port.RL_Code;
			AssertNoNotifications("when entered", HAWB.CS_RL_NKOriginInfo);

			port.RL_HasAirport = false;
			HAWB.CS_RL_NKOrigin = port.RL_Code;
			AssertHasWarnings("when not air port", HAWB.CS_RL_NKOriginInfo);
		}

		public void TestValidateRL_NKDestination()
		{
			RefUNLOCO port = GetNewPort();
			port.RL_Code = "AUSY1";

			HAWB.CS_RL_NKDestination = ZString.Empty;
			AssertHasMessageErrors("when blank", HAWB.CS_RL_NKDestinationInfo);

			HAWB.CS_RL_NKDestination = "~~~";
			AssertHasMessageErrors("when invalid", HAWB.CS_RL_NKDestinationInfo);

			HAWB.CS_RL_NKDestination = port.RL_Code;
			AssertNoNotifications("when entered", HAWB.CS_RL_NKDestinationInfo);

			port.RL_HasAirport = false;
			HAWB.CS_RL_NKDestination = port.RL_Code;
			AssertHasWarnings("when not air port", HAWB.CS_RL_NKDestinationInfo);
		}

		public void TestValidateCS_CommercialStatus()
		{
			HAWB.CS_CommercialStatus = ">|<";
			AssertListValidationInvalidCodeError(HAWB.CS_CommercialStatusInfo, true);

			HAWB.CS_CommercialStatus = "";
			AssertListValidationInvalidCodeError(HAWB.CS_CommercialStatusInfo, false);

			HAWB.CS_CommercialStatus = Env.Registry.AUCustoms.AirCargoCommercialStatus[0].Code;
			AssertListValidationInvalidCodeError(HAWB.CS_CommercialStatusInfo, false);
		}

		public void TestIfDischargingOverseasThenDestinationMustBeOverseas()
		{
			HAWB.MAWB.CM_RL_NKDischargePort = "SGSIN";
			HAWB.CS_RL_NKDestination = "AUSYD";
			AssertHasMessageError(HAWB.CS_RL_NKDestinationInfo, "Destination Port must be overseas as the Discharge Port is overseas.");

			HAWB.CS_RL_NKDestination = "NZAKL";
			AssertNoNotifications(HAWB.CS_RL_NKDestinationInfo);

			HAWB.MAWB.CM_RL_NKDischargePort = "AUSYD";
			HAWB.CS_RL_NKDestination = "AUSYD";
			AssertNoNotifications(HAWB.CS_RL_NKDestinationInfo);
		}

		protected RefUNLOCO GetNewPort()
		{
			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_HasAirport = true;
			port.RL_IATA = "bla";
			return port;
		}

		CusHAWBBase hawb;
		protected CusHAWBBase HAWB => hawb ?? (hawb = GetNewHAWB());

		protected virtual CusHAWBBase GetNewHAWB() => Factory.New<CusHAWB>();
	}
}
