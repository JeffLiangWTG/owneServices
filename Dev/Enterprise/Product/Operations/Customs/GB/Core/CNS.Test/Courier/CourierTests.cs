using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CNS.CnsAirCourier;
using Enterprise.Customs.GB.CNS.ServiceTasks.AirCourier;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CNS.Testing
{
	class CourierTests : TestCaseWithFactory
	{
		[TestDate(2019, 12, 1)]
		public void TestMessageCreation_Add()
		{
			var dec = SetupDeclaration();
			var generator = new AirCourierFromDeclarationForTestDontReallyConnect(dec, AirCourierFromDeclaration.AirCourierMessageTypes.Add, AirCourierFromDeclarationForTestDontReallyConnect.ResponseTypesForTest.Success0000);
			var message = generator.DoEverything();
			var messages = AssertCommonResults(dec);
			using (Stream inStream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.GB.CNS.Testing.Courier.ExpectedGoodResultAdd.xml"))
			{
				AssertEquals(new StreamReader(inStream).ReadToEnd(), messages.requestMessage.EM_MessageText);
			}
			AssertEquals(EDIMessage.Status.Acknowledged, messages.requestMessage.EM_Status);
			AssertContains("0000", messages.responseMessage.EM_MessageText);
			AssertEquals("MUCR was recalculated upon success", "SITE1C123LONGREFERENCEMORETHANSIXTE", dec.JE_MasterUCR);
		}

		public void TestMessageCreation_Add_EA10()
		{
			var dec = SetupDeclaration();
			var generator = new AirCourierFromDeclarationForTestDontReallyConnect(dec, AirCourierFromDeclaration.AirCourierMessageTypes.Add, AirCourierFromDeclarationForTestDontReallyConnect.ResponseTypesForTest.SuccessEA10);
			var message = generator.DoEverything();
			var messages = AssertCommonResults(dec);
			AssertContains("EA10", messages.responseMessage.EM_MessageText);
			AssertEquals(EDIMessage.Status.Acknowledged, messages.requestMessage.EM_Status);
			AssertEquals("MUCR was recalculated upon success", "SITE1C123LONGREFERENCEMORETHANSIXTE", dec.JE_MasterUCR);
		}

		public void TestMessageCreation_AddWithErrorResponse()
		{
			var dec = SetupDeclaration();
			var generator = new AirCourierFromDeclarationForTestDontReallyConnect(dec, AirCourierFromDeclaration.AirCourierMessageTypes.Add, AirCourierFromDeclarationForTestDontReallyConnect.ResponseTypesForTest.Fail);
			var message = generator.DoEverything();
			var messages = AssertCommonResults(dec);
			AssertContains("1234", messages.responseMessage.EM_MessageText);
			AssertEquals(EDIMessage.Status.Rejected, messages.requestMessage.EM_Status);
			AssertEquals("MUCR was not touched upon failure", "ORIGINAL", dec.JE_MasterUCR);
		}

		[TestDate(2019, 12, 1)]
		public void TestMessageCreation_Delete()
		{
			var dec = SetupDeclaration();
			var generator = new AirCourierFromDeclarationForTestDontReallyConnect(dec, AirCourierFromDeclaration.AirCourierMessageTypes.Delete, AirCourierFromDeclarationForTestDontReallyConnect.ResponseTypesForTest.Success0000);
			var message = generator.DoEverything();
			var messages = AssertCommonResults(dec);
			using (Stream inStream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.GB.CNS.Testing.Courier.ExpectedGoodResultDelete.xml"))
			{
				AssertEquals(new StreamReader(inStream).ReadToEnd(), messages.requestMessage.EM_MessageText);
			}
			AssertContains("0000", messages.responseMessage.EM_MessageText);
			AssertEquals(EDIMessage.Status.Acknowledged, messages.requestMessage.EM_Status);
			AssertEquals("MUCR was wiped upon success", "", dec.JE_MasterUCR);
		}

		static (EDIMessage requestMessage, EDIMessage responseMessage) AssertCommonResults(JobDeclaration dec)
		{
			var requestMessage = dec.Messages.LastOutgoingMessage;
			var responseMessage = dec.Messages.LastIncomingMessage;
			AssertEquals(EDIMessage.ApplicationCodes.GbCnsCompass, requestMessage.EM_ApplicationCode);
			AssertEquals(EDIMessage.ApplicationCodes.GbCnsCompass, responseMessage.EM_ApplicationCode);
			AssertEquals("COU", requestMessage.EM_MessageType);
			AssertEquals("COU", responseMessage.EM_MessageType);
			AssertEquals(EDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
			AssertEquals("CNS/DJC", requestMessage.EM_ApplicationReference);
			AssertEquals("DJC", requestMessage.EM_MessageOwner);
			return (requestMessage, responseMessage);
		}

		JobDeclaration SetupDeclaration()
		{
			MakeCnsCredentials();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Test Facility Code");
			var shed = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "XXXABC", "Some Description1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			shed.Attributes.Add(helper.CreateNewOrGetExistingCusCodeListAttribute(shed.PK, EU.Business.UniversalReferenceConstants.ShedAttributes.SITECODE, "SITE1"));
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			dec.JE_RL_NKPortOfArrival = "GBMNC"; // MAN
			dec.JE_RL_NKOrigin = "USORL"; // IATA is MCO
			dec.JE_TransportMode = "AIR";
			dec.JE_CustomsProfile = "DJC";
			dec.ZG_Gateway = GatewayList.Codes.CNS_CUSDECOnly;
			var b = dec.AdditionalReferenceNumbers.AddNew();
			b.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BagReference;
			b.CE_EntryNum = "BAG1";
			var c = dec.AdditionalReferenceNumbers.AddNew();
			c.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CourierConsignmentReference;
			c.CE_EntryNum = "LONGREFERENCEMORETHANSIXTEENCHARACTERS";
			dec.CourierConsignmentType = "D";
			dec.JE_DateOfArrival = ZDateTime.UtcNow;
			dec.JE_VoyageFlightNo = "BA1234";
			dec.JE_AgentsReference = "AGENT123";
			dec.JE_TotalNoOfPacks = 6;
			dec.JE_TotalWeight = 35;
			dec.JE_GoodsDescription = "BOOKS AND MAGAZINES";
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "C123", Core.Constants.CountryCodes.UnitedKingdom);
			dec.JE_OH_ShippingLine = carrier.PK;
			dec.JE_LocationOfGoods = shed.ZZD_Code;
			dec.JE_MasterUCR = "ORIGINAL";
			return dec;
		}

		void MakeCnsCredentials()
		{
			var agentBadge = new BadgeCodeSetting();
			agentBadge.Direction = "IMP";
			agentBadge.BadgeCode = "DJC";
			agentBadge.CSPCode = GatewayList.Codes.CNS_CUSDECOnly;
			agentBadge.MasterUcrCalculationMode = MucrGenerationStyles.Codes.Courier;
			var badges = new BadgeCodeSettingCollection();
			badges.Add(agentBadge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, badges);
			var agentBadgeCredential = new CredentialsSetting();
			agentBadgeCredential.BadgeCode = "DJC";
			agentBadgeCredential.Company = "DJC";
			agentBadgeCredential.Printer = "MLBX";
			agentBadgeCredential.Username = "1";
			agentBadgeCredential.Password = "2";
			var credentials = new CredentialsSettingCollection();
			credentials.Add(agentBadgeCredential);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, credentials);
		}
	}

	public class AirCourierFromDeclarationForTestDontReallyConnect : AirCourierFromDeclaration
	{
		public AirCourierFromDeclarationForTestDontReallyConnect(JobDeclaration dec, AirCourierMessageTypes how, ResponseTypesForTest responseWanted) : base(dec, how)
		{
			this.responseWanted = responseWanted;
		}

		public enum ResponseTypesForTest
		{
			Unknown, Success0000, SuccessEA10, Fail
		}

		protected override T_AirImportManifestResponse GetResult(T_AirImportManifest manifest, AirImportManifestBindingQSService1 soapService)
		{
			// Don't actually upload anything
			var result = new T_AirImportManifestResponse();
			if (responseWanted == ResponseTypesForTest.Fail)
			{
				result.AcknowledgementCode = "1234";
				result.ErrorText = "Some Error";
			}
			else if (responseWanted == ResponseTypesForTest.SuccessEA10)
			{
				result.AcknowledgementCode = "EA10";
				result.ErrorText = "Success, believe it or not";
			}
			else
			{
				result.AcknowledgementCode = "0000";
				result.ErrorText = "";
			}
			return result;
		}

		readonly ResponseTypesForTest responseWanted;
	}
}
