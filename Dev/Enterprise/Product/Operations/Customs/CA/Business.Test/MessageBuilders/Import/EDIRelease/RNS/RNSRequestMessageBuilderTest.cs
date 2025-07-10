using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D96A.Elements;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class RNSRequestMessageBuilderTest : TestCaseWithFactory
	{
		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRNSRequestMessageBuilder()
		{
			var mock = GetMock();
			var rnsRequestor = mock.Object;

			var builder = new RNSRequestMessageBuilder(rnsRequestor, DocumentMessageNameCodedList.ForwardersWarehouseReceipt);
			builder.PopulateMessages();

			var query = new ZQuery();
			var msg = Factory.LoadTop1<EDIMessage>(query);
			AssertEquals("RNS", msg.EM_MessageType);
			AssertEquals("ACM", msg.EM_MessageSubType);
			ZString expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSREP:D:96A:UN
BGM+631
DTM+132:200801202200:203
RFF+ABT:2ITN12345678987654321
RFF+TN:1234000000019
LOC+14+9999:129::3252
UNT+7+<<MSGNO PLACEHOLDER>>";
			AssertMultilineASCIIEquals("Message text", expectedResult, msg.EM_FormattedMessageText);

			var expectedInterpretation = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\RNSRequestMessageInterpretation.html");
			expectedInterpretation = expectedInterpretation.Replace("<td>20/01/2008 10:00 PM</td>", string.Format("<td>{0}</td>", new ZDateTime(2008, 1, 20, 22, 0, 0).ToString("g")));
			AssertMultilineASCIIEquals("RNS Request Message Interpretation", expectedInterpretation, msg.EM_MessageInterpretation.Replace("<tr><td style=", "\r\n<tr><td style="));
		}

		public Mock<IRNSRequest> GetMock()
		{
			var mock = new Mock<IRNSRequest>();
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.Messages).Returns(new EDIMessageCollection(Factory.New<JobDeclaration>()));
			mock.Setup(m => m.DateOfArrival).Returns(new ZDateTime(2008, 1, 20, 22, 0, 0));
			mock.Setup(m => m.CargoControlNumber).Returns("2ITN12345678987654321");
			mock.Setup(m => m.TransactionNumber).Returns("1234000000019");
			mock.Setup(m => m.OfficeCode).Returns("9999");
			mock.Setup(m => m.SubLocationCode).Returns("3252");
			return mock;
		}
	}
}
