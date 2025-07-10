using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CA.Messaging.Testing
{
	sealed class CCSEntryResponseMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2018, 10, 19, 13, 10, 55)]
		[ExpectNoExceptions]
		public void TestMessageText()
		{
			var messageBuilder = new CCSEntryResponseMessageBuilder(GetCCSEntryResponseData(), MessageSubTypes.Create);
			var expectedMessage = ExpectedMessage.Replace("'", "\r\n");
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message.EM_MessageText.Replace("'", "\r\n");
			NUnit.Framework.Assert.That(message, CustomConstraints.MultilineASCIIEquals(expectedMessage), "Fully Populated Message");

			messageBuilder = new CCSEntryResponseMessageBuilder(GetEmptyCCSEntryResponseData(), MessageSubTypes.Create);
			expectedMessage = ExpectedEmptyMessage.Replace("'", "\r\n");
			message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message.EM_MessageText.Replace("'", "\r\n");
			NUnit.Framework.Assert.That(message, CustomConstraints.MultilineASCIIEquals(expectedMessage), "Empty Populated Message");
		}

		ICCSEntryResponseTypeX GetCCSEntryResponseData()
		{
			var messages = new EDIMessageCollection(Factory.New<OrgHeader>());
			var mock = new Mock<ICCSEntryResponseTypeX>();
			mock.Setup(m => m.BatchNumber).Returns("BT00001");
			mock.Setup(m => m.AccountSecurityNumber).Returns("102070000125487");
			mock.Setup(m => m.TotalNumberOfEntries).Returns(18);
			mock.Setup(m => m.TotalNumberOfValidEntries).Returns(20);
			mock.Setup(m => m.TotalNumberOfInvalidEntries).Returns(35);
			mock.Setup(m => m.ResponseCodes).Returns(new[] { GetResponseCode("IN00001", "AN00001", "EMN00001"), GetResponseCode("IN00002", "AN00002", "EMN00002") });
			mock.Setup(m => m.Messages).Returns(messages);
			return mock.Object;
		}

		ICCSEntryResponseTypeX GetEmptyCCSEntryResponseData()
		{
			var messages = new EDIMessageCollection(Factory.New<OrgHeader>());
			var mock = new Mock<ICCSEntryResponseTypeX>();
			mock.Setup(m => m.BatchNumber).Returns(ZString.Empty);
			mock.Setup(m => m.AccountSecurityNumber).Returns(ZString.Empty);
			mock.Setup(m => m.TotalNumberOfEntries).Returns(0);
			mock.Setup(m => m.TotalNumberOfValidEntries).Returns(0);
			mock.Setup(m => m.TotalNumberOfInvalidEntries).Returns(0);
			mock.Setup(m => m.ResponseCodes).Returns(new[] { GetResponseCode(ZString.Empty, ZString.Empty, ZString.Empty) });
			mock.Setup(m => m.Messages).Returns(messages);
			return mock.Object;
		}

		IResponseCodes GetResponseCode(string itemNumber, string appRefNumber, string errMsgNumber)
		{
			var mock = new Mock<IResponseCodes>();
			mock.Setup(m => m.MessageItemNumber).Returns(itemNumber);
			mock.Setup(m => m.ApplicableReferenceNumber).Returns(appRefNumber);
			mock.Setup(m => m.CSSErrorMessageNumber).Returns(errMsgNumber);
			return mock.Object;
		}

		string ExpectedMessage
		{
			get
			{
				var builder = new ZStringBuilder();
				/*Message Header*/
				builder.Append("UNH+<<MSGNO PLACEHOLDER>>+CUSRES:S:99B:UN");
				/*InboundB3TransactionNumber*/
				builder.Append("BGM+:::BT00001++9");
				/*DateTime.Now*/
				builder.Append("DTM+137:20181019:102");
				/*RFF*/
				builder.Append("RFF+ABP:102070000125487");
				/*ERP*/
				builder.Append("ERP+:IN00001");
				/*ERP*/
				builder.Append("ERP+:IN00002");
				/*RFF*/
				builder.Append("RFF+ABO:AN00001");
				/*RFF*/
				builder.Append("RFF+ABO:AN00002");
				/*ERC*/
				builder.Append("ERC+EMN00001");
				/*ERC*/
				builder.Append("ERC+EMN00002");
				/*DOC*/
				builder.Append("DOC+961");
				/*CST*/
				builder.Append("CST++18+20+35");
				return builder.ToStringWithDelimiterBetweenAppends("'");
			}
		}

		string ExpectedEmptyMessage
		{
			get
			{
				var builder = new ZStringBuilder();
				/*Message Header*/
				builder.Append("UNH+<<MSGNO PLACEHOLDER>>+CUSRES:S:99B:UN");
				/*InboundB3TransactionNumber*/
				builder.Append("BGM+++9");
				/*DateTime.Now*/
				builder.Append("DTM+137:20181019:102");
				/*RFF*/
				builder.Append("RFF+ABP");
				/*ERP*/
				builder.Append("ERP");
				/*RFF*/
				builder.Append("RFF+ABO");
				/*ERC*/
				builder.Append("ERC");
				/*DOC*/
				builder.Append("DOC+961");
				/*CST*/
				builder.Append("CST++0+0+0");
				return builder.ToStringWithDelimiterBetweenAppends("'");
			}
		}
	}
}
