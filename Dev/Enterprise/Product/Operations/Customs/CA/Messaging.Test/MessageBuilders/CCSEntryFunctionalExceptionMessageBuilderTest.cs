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
	sealed class CCSEntryFunctionalExceptionMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2018, 10, 19, 13, 10, 55)]
		[ExpectNoExceptions]
		public void TestMessageText()
		{
			var messageBuilder = new CCSEntryFunctionalExceptionMessageBuilder(GetCCSEntryFunctionalExceptionData(), MessageSubTypes.Create);
			var expectedMessage = ExpectedMessage.Replace("'", "\r\n");
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message.EM_MessageText.Replace("'", "\r\n");
			NUnit.Framework.Assert.That(message, CustomConstraints.MultilineASCIIEquals(expectedMessage), "Fully Populated Message");

			messageBuilder = new CCSEntryFunctionalExceptionMessageBuilder(GetEmptyCCSEntryFunctionalExceptionData(), MessageSubTypes.Create);
			expectedMessage = ExpectedEmptyMessage.Replace("'", "\r\n");
			message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message.EM_MessageText.Replace("'", "\r\n");
			NUnit.Framework.Assert.That(message, CustomConstraints.MultilineASCIIEquals(expectedMessage), "Empty Populated Message");
		}

		ICCSEntryFunctionalExceptionTypeX GetCCSEntryFunctionalExceptionData()
		{
			var messages = new EDIMessageCollection(Factory.New<OrgHeader>());
			var mock = new Mock<ICCSEntryFunctionalExceptionTypeX>();
			mock.Setup(m => m.InboundB3TransactionNumber).Returns("B9999999");
			mock.Setup(m => m.ErrorFtxs).Returns(new[] { GetErrorFtxs("Free Text 11", "Free Text 12"), GetErrorFtxs("Free Text 21", "Free Text 22") });
			mock.Setup(m => m.Messages).Returns(messages);
			return mock.Object;
		}

		ICCSEntryFunctionalExceptionTypeX GetEmptyCCSEntryFunctionalExceptionData()
		{
			var messages = new EDIMessageCollection(Factory.New<OrgHeader>());
			var mock = new Mock<ICCSEntryFunctionalExceptionTypeX>();
			mock.Setup(m => m.InboundB3TransactionNumber).Returns(ZString.Empty);
			var errorExt = new Mock<IErrorFtx>();
			errorExt.Setup(m => m.ErrorText1).Returns(ZString.Empty);
			errorExt.Setup(m => m.ErrorText2).Returns(ZString.Empty);
			mock.Setup(m => m.ErrorFtxs).Returns(new[] { errorExt.Object });
			mock.Setup(m => m.Messages).Returns(messages);
			return mock.Object;
		}

		IErrorFtx GetErrorFtxs(string text1, string text2)
		{
			var mock = new Mock<IErrorFtx>();
			mock.Setup(m => m.ErrorText1).Returns(text1);
			mock.Setup(m => m.ErrorText2).Returns(text2);
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
				builder.Append("BGM+:::B9999999++11");
				/*DateTime.Now*/
				builder.Append("DTM+137:201810191310:203");
				/*GIS*/
				builder.Append("GIS+14");
				/*ERP*/
				builder.Append("ERP+2:<<MSGNO PLACEHOLDER>>:29");
				/*FTX1*/
				builder.Append("FTX+AAO+++FREE TEXT 11:FREE TEXT 12");
				/*FTX2*/
				builder.Append("FTX+AAO+++FREE TEXT 21:FREE TEXT 22");
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
				builder.Append("BGM+++11");
				/*DateTime.Now*/
				builder.Append("DTM+137:201810191310:203");
				/*GIS*/
				builder.Append("GIS+14");
				/*ERP*/
				builder.Append("ERP+2:<<MSGNO PLACEHOLDER>>:29");
				/*FTX1*/
				builder.Append("FTX+AAO");
				return builder.ToStringWithDelimiterBetweenAppends("'");
			}
		}
	}
}
