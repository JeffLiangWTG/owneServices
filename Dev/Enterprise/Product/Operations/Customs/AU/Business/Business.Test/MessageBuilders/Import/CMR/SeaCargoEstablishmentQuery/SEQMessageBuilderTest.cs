using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SEQMessageBuilderTest : TestCaseWithFactory
	{
		public void TestSEQMessageBuilder()
		{
			EDIMessageCollection messages = new EDIMessageCollection(Factory.New<JobDeclaration>());

			var mock = GetMock();
			var queryInfo = mock.Object;
			var builder = new SEQMessageBuilder(queryInfo);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			builder.Messages = messages;
			builder.PopulateMessages();
			const string expectedResult = "UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'BGM+45:::SEQ+<<SENDERS REFERENCE PLACE HOLDER>>/DAT1:1+9'RFF+AAQ:OCLU1234567'RFF+BH:HOUSEBILL'" +
"RFF+MB:OCEANBILL'NAD+VW+RESPPTYID::95'TDT+20+VOYNUM++11++++VESSELID::11'LOC+202+ESTID::95'UNT+9+<<MSGNO PLACEHOLDER>>'";
			AssertMultilineASCIIEquals("Message text", expectedResult, builder.MessageText);
		}

		[ExpectNoExceptions()]
		public void TestSEQMessageBuilderWithNulls()
		{
			var messages = new EDIMessageCollection(Factory.New<JobDeclaration>());

			var mock = GetNullMock();
			var queryInfo = mock.Object;
			var builder = new SEQMessageBuilder(queryInfo);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			builder.Messages = messages;
			builder.PopulateMessages();
		}

		static Mock<ISeaCargoEstablishmentQueryInformation> GetMock()
		{
			var mock = new Mock<ISeaCargoEstablishmentQueryInformation>();
			mock.Setup(m => m.ContainerNumber).Returns("OCLU1234567");
			mock.Setup(m => m.HouseBill).Returns("HOUSEBILL");
			mock.Setup(m => m.OceanBill).Returns("OCEANBILL");
			mock.Setup(m => m.ResponsiblePartyID).Returns("RESPPTYID");
			mock.Setup(m => m.VesselID).Returns("VESSELID");
			mock.Setup(m => m.VoyageNumber).Returns("VOYNUM");
			mock.Setup(m => m.EstablishmentID).Returns("ESTID");
			return mock;
		}

		static Mock<ISeaCargoEstablishmentQueryInformation> GetNullMock()
		{
			var mock = new Mock<ISeaCargoEstablishmentQueryInformation>();
			mock.Setup(m => m.ContainerNumber).Returns("");
			mock.Setup(m => m.HouseBill).Returns("");
			mock.Setup(m => m.OceanBill).Returns("");
			mock.Setup(m => m.ResponsiblePartyID).Returns("");
			mock.Setup(m => m.VesselID).Returns("");
			mock.Setup(m => m.VoyageNumber).Returns("");
			mock.Setup(m => m.EstablishmentID).Returns("");
			return mock;
		}
	}
}
