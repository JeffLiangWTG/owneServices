using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Moq;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class ACIForwarderCloseMessageBuilderTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOriginalMessageText()
		{
			var expectedMessage = ExpectedCreateMessage.Replace("\r\n", "");
			var expectedWithdrawMessage = ExpectedWithdrawMessage.Replace("\r\n", "");
			var expectedInterpretation = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\ACICloseMessageInterpretation.html");
			var expectedWithdrawInterpretation = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\ACIWithdrawCloseMessageInterpretation.html");

			using (CACustomsDataRegistry.Instance.IncludeAssociationAssignedCode.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertOriginalMessageText(expectedMessage, expectedInterpretation);
				AssertWithdrawMessageText(expectedWithdrawMessage, expectedWithdrawInterpretation);
			}
			using (CACustomsDataRegistry.Instance.IncludeAssociationAssignedCode.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				AssertOriginalMessageText(expectedMessage.Replace(":ACIHCM", ""), expectedInterpretation.Replace(":ACIHCM", ""));
				AssertWithdrawMessageText(expectedWithdrawMessage.Replace(":ACIHCM", ""), expectedWithdrawInterpretation.Replace(":ACIHCM", ""));
			}
		}

		public void TestMessageSubTypeForPostArrivalChange()
		{
			var messageBuilder = new ACIForwarderCloseMessageBuilder(GetMockData(), MessageSubTypes.Request);
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message;
			AssertEquals(MessageSubTypeCodes.Codes.Change, message.EM_MessageSubType);
		}

		void AssertOriginalMessageText(ZString expectedMessage, ZString expectedInterpretation)
		{
			var messageBuilder = new ACIForwarderCloseMessageBuilder(GetMockData(), MessageSubTypes.Create);
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message;
			AssertMultilineASCIIEquals("Fully Populated Create Message", expectedMessage, message.EM_MessageText);
			AssertEquals(MessageSubTypeCodes.Codes.Original, message.EM_MessageSubType);
			AssertMultilineASCIIEquals("Fully Populated Create Interpretation", expectedInterpretation, message.EM_MessageInterpretation.Replace("<tr><td style=", "\r\n<tr><td style="));
		}

		void AssertWithdrawMessageText(ZString expectedMessage, ZString expectedInterpretation)
		{
			var messageBuilder = new ACIForwarderCloseMessageBuilder(GetMockData(), MessageSubTypes.Withdraw);
			var message = messageBuilder.PopulateMessages().GetBuilderResults().First().Message;
			AssertMultilineASCIIEquals("Fully Populated Create Message", expectedMessage, message.EM_MessageText);
			AssertEquals(MessageSubTypeCodes.Codes.Cancellation, message.EM_MessageSubType);
			AssertMultilineASCIIEquals("Fully Populated Create Interpretation", expectedInterpretation, message.EM_MessageInterpretation.Replace("<tr><td style=", "\r\n<tr><td style="));
		}

		IACIForwarderCloseProvider GetMockData()
		{
			var mock = new Mock<IACIForwarderCloseProvider>();
			mock.Setup(m => m.Messages).Returns(Factory.New<CusCAeMHMaster>().Messages);
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.PreviousCCN).Returns("9999 12345678");
			mock.Setup(m => m.PreviousCCNForWithdraw).Returns("9999 XXXXXXXX");
			mock.Setup(m => m.JobIdentification).Returns("C12345678");
			mock.Setup(m => m.CarrierCode).Returns("8123");
			mock.Setup(m => m.CarrierCodeForWithdraw).Returns("8XXX");
			mock.Setup(m => m.RelatedCCNs).Returns(new ZString[] { "CCN 1", "CCN2 " });
			mock.Setup(m => m.AmendmentReason).Returns("35");
			return mock.Object;
		}

		const string ExpectedCreateMessage = @"UNH+<<MSGNO PLACEHOLDER>>+GOVCBR:D:11B:UN:ACIHCM'
BGM+87+999912345678+9'
RFF+ABO:C12345678'
NAD+FW+8123'
DOC+85+CCN1'
DOC+85+CCN2'
AJT+ZZZ+35'
UNS+D'
HYN+3'
UNS+S'
UNT+11+<<MSGNO PLACEHOLDER>>'";

		const string ExpectedWithdrawMessage = @"UNH+<<MSGNO PLACEHOLDER>>+GOVCBR:D:11B:UN:ACIHCM'
BGM+87+9999XXXXXXXX+1'
RFF+ABO:C12345678'
NAD+FW+8XXX'
AJT+ZZZ+35'
UNS+D'HYN+3'
UNS+S'
UNT+9+<<MSGNO PLACEHOLDER>>'";
	}
}
