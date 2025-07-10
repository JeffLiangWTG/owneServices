using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CCF02C;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CCF02CProcessor_NOTIFICATION_EMBARQUEMENTTest : TP5BaseProcessorTest<Ccf02CType, CCF02CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.StatusUpdateNotification;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CCF02CResponseMessage_NOTIFICATION_EMBARQUEMENT.xml");

		protected override ZString ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

		public void TestIE170MessageSending()
		{
			var northernIreland = CreateNewOrGetExistingRefCountryStates("HH", RefUNLOCO.Regions.NorthernIreland, "Northern Ireland", Core.Constants.CountryCodes.UnitedKingdom);
			CreateNewOrGetExistingRefUNLOCO("XI102", "GB").RL_RW = northernIreland.PK;
			CreateBorder(new Dictionary<string, string> { { "FR000040", "Le Havre" } }, true);
			CreateBorder(new Dictionary<string, string> { { "FR999999", "Bergerac" } }, false);
			Factory.Save();

			AssertIE170MessageSending("Movement Type is not convenient.", false, false, true, true, true, true,true, "TESTCASE1");
			AssertIE170MessageSending("Transport Mode is not convenient.", false, true, false, true, true, true, true, "TESTCASE2");
			AssertIE170MessageSending("Port of Dispatch is not convenient.", false, true, true, false, true, true, true, "TESTCASE3");
			AssertIE170MessageSending("No DEP or TRA Customs Office is related to intelligent border.", false, true, true, true, false, false, true, "TESTCASE4");
			AssertIE170MessageSending("Message statut is not NOTIFICATION_EMBARQUEMENT.", false, true, true, true, true, true, false, "TESTCASE5");
			AssertIE170MessageSending("All requirements are met.", true, true, true, true, true, true, true, "TESTCASE6");
			AssertIE170MessageSending("All requirements are met (Only TRA office has intelligentBorder).", true, true, true, true, true, false, true, "TESTCASE7");
			AssertIE170MessageSending("All requirements are met (Only DEP office has intelligentBorder).", true, true, true, true, false, true, true, "TESTCASE8");
		}

		void AssertIE170MessageSending(string comment, bool expectedResult, bool hasConvenientMovementType, bool hasConvenientTransport, bool hasConvenientPortOfDispatch, bool hasTransitOfficeWithIntelligentBorder, bool hasDepartureOfficeWithIntelligentBorder, bool messageHasConvenientEvent, ZString lrn)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.PortOfDispatch = hasConvenientPortOfDispatch ? "XI102" : "AUSYD";
			nctsHeader.SetMovementType(hasConvenientMovementType ? EU.NCTS.Business.NctsMovementType.Codes.Departure : EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			if (nctsHeader.MovementHeader != null)
			{
				nctsHeader.MovementHeader.BM_PaperlessInbondNum = lrn;
				nctsHeader.MovementHeader.BM_ExportTransportMode = hasConvenientTransport ? EU.Business.ModeOfTransportList.Codes._1_SeaTransport : EU.Business.ModeOfTransportList.Codes._9_OwnPropulsion;
				if (hasTransitOfficeWithIntelligentBorder)
				{
					NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader.MovementHeader, EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "FR000040", ZDateTime.Empty);
				}
				if (hasDepartureOfficeWithIntelligentBorder)
				{
					NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader.MovementHeader, EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "FR000040", ZDateTime.Empty);
				}
			}

			var message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = GetMessageSubType();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CCF02CResponseMessage_NOTIFICATION_EMBARQUEMENT.xml");
			message.EM_MessageText = message.EM_MessageText.Replace("LRN1", lrn);
			message.EM_LinkedObject = nctsHeader.MovementHeader;
			if (!messageHasConvenientEvent)
			{
				message.EM_MessageText = message.EM_MessageText.Replace("<statut>NOTIFICATION_EMBARQUEMENT", "<statut>ANTICIPEE");
			}

			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(message);

			var created170Message = nctsHeader.MovementHeader?.Messages.Cast<EDIMessage>().SingleOrDefault(x => x.EM_MessageSubType == TP5MessageTypeList.Codes.CC170C);
			if (expectedResult)
			{
				AssertNotNull("An IE170 message should be sent when " + comment, created170Message);
				Assert("Saving of factory should be delayed.", !created170Message.IsInDatabase);
			}
			else
			{
				AssertNull("No IE170 message should be sent when " + comment, created170Message);
			}
		}

		RefUNLOCO CreateNewOrGetExistingRefUNLOCO(ZString code, ZString countryCode)
		{
			var result = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
			if (result == null)
			{
				result = Factory.New<RefUNLOCO>();
				result.RL_Code = code;
				result.RL_RN_NKCountryCode = countryCode;
			}
			return result;
		}

		RefCountryStates CreateNewOrGetExistingRefCountryStates(ZString code, ZString regionName, ZString description, ZString countryCode)
		{
			var result = new RefCountryStates.Loader(Factory).LoadRefCountryStatesFromCode(code, countryCode);
			if (result == null)
			{
				result = Factory.NewWithValidTestData<RefCountryStates>();
				result.RW_Code = code;
				result.RW_RegionName = regionName;
				result.RW_Description = description;
				result.RW_RN_NKCountryCode = countryCode;
			}
			return result;
		}

		void CreateBorder(Dictionary<string, string> codeAndDescriptions, bool isIntelligent)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			foreach (var codeAndDescription in codeAndDescriptions)
			{
				var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, codeAndDescription.Key, codeAndDescription.Value, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, "IsIntelligentBorder", isIntelligent ? "Y" : "N");
			}
		}
	}
}
