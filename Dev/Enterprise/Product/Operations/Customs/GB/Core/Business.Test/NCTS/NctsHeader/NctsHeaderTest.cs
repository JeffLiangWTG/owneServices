using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.GB.Business.NCTS;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(NctsHeader))]
	public class NctsHeaderTest : NctsHeaderAbstractTest
	{
		public void TestGetIE29CusdecParser()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var message = Factory.New<NctsEdiMessage>();
			var result = header.GetIE29CusdecParser(message);
			AssertType<GBCtcIE29CusdecParser>(result);
			AssertSame(message, result.EdiMessage);
			AssertSame(message.Factory, result.Factory);
		}

		public void TestGuarantees_Phase5Departure()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			CombineAssertions(() =>
			{
				nctsHeader.GetEffectiveGuarantees().AddNew();
				var result = nctsHeader.GetEffectiveGuarantees();
				AssertEquals("Parent", nctsHeader.MovementHeader.PK, result[0].PW_ParentID);
				AssertType<NctsGuaranteeCollection<Guarantee>>("Type", result);
			});
		}

		public void TestGuarantees_Phase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			CombineAssertions(() =>
			{
				nctsHeader.GetEffectiveGuarantees().AddNew();
				var result = nctsHeader.GetEffectiveGuarantees();
				AssertEquals("Parent", nctsHeader.PK, result[0].PW_ParentID);
				AssertType<NctsGuaranteeCollection<Guarantee>>("Type", result);
			});
		}

		public void TestGetServiceReference()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

			CreateMessagesForServiceReferenceTest(header);

			AssertEquals("The most recent arrival message has a reference of 2", "2", header.GetServiceReference("007"));
			AssertEquals("The most recent arrival message has a reference of 2", "2", header.GetServiceReference("044"));

			AssertEquals("The most recent departure message has a reference of 5", "5", header.GetServiceReference("015"));
			AssertEquals("The most recent departure message has a reference of 5", "5", header.GetServiceReference("014"));
		}

		public void TestLinkedMessages()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.LinkedMessages.Add(Factory.New<NCTSOutboundEDIMessage>());
			AssertEquals("Pre-requisite", expected: true, nctsHeader.IsPhase5Departure);
			AssertMessageCollections(nctsHeader, 0, 1, 1);

			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			AssertMessageCollections(nctsHeader, 0, 1, 1);

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.LinkedMessages.Add(Factory.New<NCTSOutboundEDIMessage>());
			AssertMessageCollections(nctsHeader, 1, 0, 1);
		}

		void AssertMessageCollections(NctsHeader nctsHeader, int headerMessages, int movementHeaderMessages, int linkedMessages)
		{
			AssertEquals("Header Messages Count", headerMessages, nctsHeader.Messages.Count);
			AssertEquals("Movement Header Messages Count", movementHeaderMessages, nctsHeader.MovementHeader?.Messages.Count ?? 0);
			AssertEquals("Header Linked Messages Count", linkedMessages, nctsHeader.LinkedMessages.Count);
		}

		public static void CreateMessagesForServiceReferenceTest(NctsHeader header)
		{
			var dateTime = ZDateTime.Now;

			var arrivalMessage1 = header.Messages.AddNew();
			arrivalMessage1.EM_ApplicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.GbCommonTransitConvention;
			arrivalMessage1.EM_MessageSubType = "007";
			arrivalMessage1.EM_ReceiveTransmit = "TRX";
			arrivalMessage1.EM_ApplicationReference = "1";
			arrivalMessage1.EM_SystemCreateTimeUtc = dateTime.AddMinutes(1);

			var arrivalMessage2 = header.Messages.AddNew();
			arrivalMessage2.EM_ApplicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.GbCommonTransitConvention;
			arrivalMessage2.EM_MessageSubType = "007";
			arrivalMessage2.EM_ReceiveTransmit = "TRX";
			arrivalMessage2.EM_ApplicationReference = "2";
			arrivalMessage2.EM_SystemCreateTimeUtc = dateTime.AddMinutes(10); // The most recent 007 arrival message

			var arrivalMessage3 = header.Messages.AddNew();
			arrivalMessage3.EM_ApplicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.GbCommonTransitConvention;
			arrivalMessage3.EM_MessageSubType = "044";
			arrivalMessage3.EM_ReceiveTransmit = "TRX";
			arrivalMessage3.EM_ApplicationReference = "3";
			arrivalMessage3.EM_SystemCreateTimeUtc = dateTime.AddMinutes(5);

			var departureMessage1 = header.Messages.AddNew();
			departureMessage1.EM_ApplicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.GbCommonTransitConvention;
			departureMessage1.EM_MessageSubType = "015";
			departureMessage1.EM_ReceiveTransmit = "TRX";
			departureMessage1.EM_ApplicationReference = "4";
			departureMessage1.EM_SystemCreateTimeUtc = dateTime.AddMinutes(1);

			var departureMessage2 = header.Messages.AddNew();
			departureMessage2.EM_ApplicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.GbCommonTransitConvention;
			departureMessage2.EM_MessageSubType = "015";
			departureMessage2.EM_ReceiveTransmit = "TRX";
			departureMessage2.EM_ApplicationReference = "5";
			departureMessage2.EM_SystemCreateTimeUtc = dateTime.AddMinutes(10); // The most recent 015 departure message

			var departureMessage3 = header.Messages.AddNew();
			departureMessage3.EM_ApplicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.GbCommonTransitConvention;
			departureMessage3.EM_MessageSubType = "014";
			departureMessage3.EM_ReceiveTransmit = "TRX";
			departureMessage3.EM_ApplicationReference = "6";
			departureMessage3.EM_SystemCreateTimeUtc = dateTime.AddMinutes(5);

			var neitherArrivalOrDepartureMessage = header.Messages.AddNew();
			neitherArrivalOrDepartureMessage.EM_ApplicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.GbCommonTransitConvention;
			neitherArrivalOrDepartureMessage.EM_MessageSubType = "001";
			neitherArrivalOrDepartureMessage.EM_ReceiveTransmit = "TRX";
			neitherArrivalOrDepartureMessage.EM_ApplicationReference = "2";
			neitherArrivalOrDepartureMessage.EM_SystemCreateTimeUtc = dateTime.AddMinutes(60);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			return header;
		}
	}
}
