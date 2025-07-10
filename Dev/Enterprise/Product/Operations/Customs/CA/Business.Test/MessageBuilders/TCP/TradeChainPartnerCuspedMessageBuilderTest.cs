using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Moq;
using IDocAddress = Enterprise.MasterFiles.Integration.IDocAddress;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TradeChainPartnerCuspedMessageBuilderTest : TestCaseWithFactory
	{
		public void TestShortNADMessageText()
		{
			TestMessageText(GetTCP(" Company A "), "Simple Message", ExpectedShortNADMessage);
		}

		public void TestLongNADText()
		{
			TestMessageText(GetTCP(" This is a really LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG name ",
				"SOME LONGLONGLONGLONGLONGLONGLONGLONGLONG ADDRESS"), "Complex Message", ExpectedLongNADMessage);
		}

		public void TestShortNameWithSuperLongAddress1MessageText()
		{
			TestMessageText(GetTCP(address1: "This is a really LONGLONGLONGLONGLOLONGLONGLONGLONGLONG address"), "Short name with a super long address1", ExpectedShortNameWithLongAddress1Message);
		}

		public void TestShortNameWithEmptyAddress1MessageText()
		{
			TestMessageText(GetTCP(address1: string.Empty), "Short name with a super long address1", ExpectedShortNameWithEmptyAddress1);
		}

		public void TestShortNameWithEmptyAddress2MessageText()
		{
			TestMessageText(GetTCP(address2: string.Empty), "Short name with a super long address1", ExpectedShortNameWithEmptyAddress2);
		}

		void TestMessageText(ITradeChainPartner iTCP, string errorMessage, string expectedMessage)
		{
			var messageBuilder = new TradeChainPartnerCuspedMessageBuilder(iTCP, MessageSubTypes.AddLines);
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message;
			AssertMultilineASCIIEquals(errorMessage, expectedMessage.Replace("'", "'\r\n"), message.EM_MessageText.Replace("'", "'\r\n"));
		}

		string ExpectedShortNADMessage => GetExpectedMessage("NAD+VN+++COMPANY A:::::1+SOME ADDRESS LINE1:SOME ADDRESS LINE2+SYDNEY+NSW:163:5+2000+AU");
		string ExpectedLongNADMessage => GetExpectedMessage("NAD+VN+++THIS IS A REALLY LONGLONGLONGLONGLO:NGLONGLONGLONGLONGLONGLONGLONGLONGL:ONGLONGLONGLONGLONGLONGLONGLONGLONG:LONGLONGLONGLONGLONGLONGLONGLONGLON:GLONGLONGLONGLONGLONGLONGLONG NAME:1+SOME LONGLONGLONGLONGLONGLONGL:ONGLONGLONG ADDRESS SOME ADDRE+SYDNEY+NSW:163:5+2000+AU");
		string ExpectedShortNameWithLongAddress1Message => GetExpectedMessage("NAD+VN+++COMPANY A:::::1+THIS IS A REALLY LONGLONGLONGL:ONGLOLONGLONGLONGLONGLONG ADDR+SYDNEY+NSW:163:5+2000+AU");
		string ExpectedShortNameWithEmptyAddress1 => GetExpectedMessage("NAD+VN+++COMPANY A:::::1+:SOME ADDRESS LINE2+SYDNEY+NSW:163:5+2000+AU");
		string ExpectedShortNameWithEmptyAddress2 => GetExpectedMessage("NAD+VN+++COMPANY A:::::1+SOME ADDRESS LINE1+SYDNEY+NSW:163:5+2000+AU");

		string GetExpectedMessage(string nameAndAddress)
		{
			var builder = new ZStringBuilder();
			/*Message Header*/
			builder.Append("UNH+<<MSGNO PLACEHOLDER>>+CUSPED:S:99B:UN");
			/*Begnning of Message*/
			builder.Append("BGM+101+M2135423145+2");
			/*Vendor or Consignee*/
			builder.Append("CST++570:105+102453396:58+102453396RM0002:58");
			/*Message Processing Date*/
			builder.Append("DTM+9:201909280000:203");
			/*Message Summary*/
			builder.Append("DMS++10");
			builder.Append("RFF+IA:RDFACXXCZASDF8");
			/*Message Details*/
			builder.Append("DOC+10");
			/*Name And Address*/
			builder.Append(nameAndAddress);
			/*Message Trailer*/
			builder.Append("UNT+9+<<MSGNO PLACEHOLDER>>'");
			return builder.ToStringWithDelimiterBetweenAppends("'");
		}

		ITradeChainPartner GetTCP(string companyName = "Company A", string address1 = "SOME ADDRESS LINE1", string address2 = "SOME ADDRESS LINE2")
		{
			var mockAddress = GenerateMockNAD(companyName, address1, address2);
			var mockTCP = GenerateMockTCP(mockAddress);
			return mockTCP.Object;
		}

		Mock<IDocAddress> GenerateMockNAD(string companyName, string address1, string address2)
		{
			var mockAddress = new Mock<IDocAddress>();
			mockAddress.Setup(m => m.E2_CompanyName).Returns(companyName);
			mockAddress.Setup(m => m.E2_Address1).Returns(address1);
			mockAddress.Setup(m => m.E2_Address2).Returns(address2);
			mockAddress.Setup(m => m.E2_City).Returns("Sydney");
			mockAddress.Setup(m => m.E2_State).Returns("NSW");
			mockAddress.Setup(m => m.E2_Postcode).Returns("2000");
			mockAddress.Setup(m => m.CountryCode).Returns("AU");
			return mockAddress;
		}

		Mock<ITradeChainPartner> GenerateMockTCP(Mock<IDocAddress> mockAddress)
		{
			var messages = new EDIMessageCollection(Factory.New<OrgHeader>());
			var mockTCPPartner = new Mock<ITradeChainPartner>();
			mockTCPPartner.Setup(m => m.MessageNumber).Returns("M2135423145");
			mockTCPPartner.Setup(m => m.IsVendor).Returns(true);
			mockTCPPartner.Setup(m => m.ApplicationImporterNumber).Returns("102453396");
			mockTCPPartner.Setup(m => m.DivisionImporterNumber).Returns("102453396RM0002");
			mockTCPPartner.Setup(m => m.TransactionDate).Returns(new ZDateTime(2019, 09, 28, 0, 0, 0));
			mockTCPPartner.Setup(m => m.OrgCodeType).Returns("CSA");
			mockTCPPartner.Setup(m => m.ReferenceIdentifier).Returns("Rdfacxxczasdf8");
			mockTCPPartner.Setup(m => m.VendorOrConsigneeAddress).Returns(mockAddress.Object);
			mockTCPPartner.Setup(m => m.Messages).Returns(messages);
			return mockTCPPartner;
		}
	}
}
