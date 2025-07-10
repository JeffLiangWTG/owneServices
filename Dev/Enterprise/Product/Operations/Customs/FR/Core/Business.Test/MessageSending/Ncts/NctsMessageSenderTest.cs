using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.EU.NCTS.DataTransfer.Phase4.Testing;
using Enterprise.Customs.FR.Business.NCTS;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class NctsMessageSenderTest : TestCaseWithFactory
	{
		[TestDate(2020, 5, 20, 10, 0, 0)]
		public void TestMessageIsDirectedToFRITask()
		{
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			NctsHeaderDataObjectWriterTest.SetupNctsHeaderForDeparture(Factory, nctsHeader);
			nctsHeader.Branch.Company.GC_CustomsRegistrationNo = "DANIEL";

			var sender = new NctsMessageSender();
			sender.CreateMessage(nctsHeader, new Customs.Business.SendsMessagesToCustomsShutterUpperer(), new NctsMessageFunctionSet.DeclarationDataMessage());
			AssertEquals(1, nctsHeader.Messages.Count);
			var message = nctsHeader.Messages[0];
			AssertEquals(ApplicationCodes.FRCustomsMessage, message.EM_ApplicationCode);
			AssertEquals(MessageSubTypeList.Codes.DT, message.EM_MessageSubType);
			AssertEquals("015", message.EM_MessageType);
			AssertEquals("1", message.EM_MessageNum);
			AssertEquals("DANIEL", message.EM_MessageOwner);
			AssertEquals(EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationSent, nctsHeader.EffectiveMessageStatus);
		}

		[TestDate(2020, 5, 20, 10, 0, 0)]
		public void TestMessageTypeForF15Message()
		{
			var nctsHeader = SetupNctsCC015ForTest(new BusinessObjectFactory());
			nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			nctsHeader.Branch.Company.GC_CustomsRegistrationNo = "DANIEL";

			var sender = new NctsMessageSender();
			sender.CreateMessage(nctsHeader, new Customs.Business.SendsMessagesToCustomsShutterUpperer(), new FRNctsMessageFunctionSet.PrelodgeValidationMessage());
			AssertEquals(1, nctsHeader.Messages.Count);
			var message = nctsHeader.Messages[0];
			AssertEquals(ApplicationCodes.FRCustomsMessage, message.EM_ApplicationCode);
			AssertEquals(MessageSubTypeList.Codes.DT, message.EM_MessageSubType);
			AssertEquals("F15", message.EM_MessageType);
			AssertEquals("1", message.EM_MessageNum);
			AssertEquals("DANIEL", message.EM_MessageOwner);
			AssertEquals(FrNctsMessageStatusList.Codes.PrelodgeValidationSent, nctsHeader.EffectiveMessageStatus);
		}

		NctsHeader SetupNctsCC015ForTest(BusinessObjectFactory factory)
		{
			var cc015 = factory.New<NctsHeader>();
			cc015.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			cc015.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			cc015.MovementHeader.BM_InBondEntryType = "T1";
			cc015.MovementHeader.BM_RL_NKDestinationPort = "IT";
			cc015.MovementHeader.BM_LocationOfGoods = "Pre-Lodged";
			cc015.MovementHeader.BM_LocationOfGoodsCode = "954131533-GB60DEP"; // TODO Port of presentation - GBLHRBAC
			cc015.MovementHeader.BM_RL_NKForeignDestPort = "GBDVR";
			cc015.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.France;
			cc015.MovementHeader.BM_TOLCarrierID = "NC15REG";
			cc015.MovementHeader.BM_EntryDate = ZDateTime.Today;

			NCTSTestHelper.SetupContainersAndSealsForTest(cc015);

			var departureOffice = cc015.CustomsOffices.AddNew();
			departureOffice.CY_Code = EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			departureOffice.CY_Data = "FR000060";
			var destinationOffice = cc015.CustomsOffices.AddNew();
			destinationOffice.CY_Code = EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
			destinationOffice.CY_Data = "IT021300";

			cc015.MovementHeader.BM_MethodOfPayment = "A";
			cc015.MovementHeader.BM_ExportTransportMode = EU.Business.ModeOfTransportList.Codes._3_RoadTransport;

			return cc015;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateNctsDeclarationTypeList(Core.Constants.CountryCodes.France);
			Factory.Save();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		}
		NctsHeader nctsHeader;
	}
}
