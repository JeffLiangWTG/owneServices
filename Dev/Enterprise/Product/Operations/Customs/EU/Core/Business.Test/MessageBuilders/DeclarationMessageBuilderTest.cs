using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;

namespace Enterprise.Customs.EU.Business.MessageBuilders.Testing
{
	sealed class DeclarationMessageBuilderTest : TestCaseWithFactory
	{
		public void TestEDIMessageIsDeletedWhenSaveFailed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Dec must have a CusEntryHeader", 1, declaration.CustomsEntryHeaders.Count);

			var mockMessageGenerator = new Mock<IMessageGenerator<CusEntryHeader>>(MockBehavior.Strict);

			var expectedReturn = new BuilderResult(declaration.CustomsEntryHeaders[0], new List<string>(), null);
			expectedReturn.Message = Factory.New<EDIMessage>();
			expectedReturn.Message.EM_MessageText = "Hello World";
			mockMessageGenerator.Setup(m => m.Generate(It.IsAny<CusEntryHeader>())).Returns(expectedReturn);

			var messageBuilder = new DeclarationMessageBuilder(declaration, mockMessageGenerator.Object);
			IMessageBuilderResult messageBuilderResult = messageBuilder.PopulateMessages();

			BuilderResult builderResult = null;
			foreach (IBuilderResult bR in messageBuilderResult.GetBuilderResults())
			{
				builderResult = bR as BuilderResult;
			}
			AssertNotNull(builderResult.Message);
			mockMessageGenerator.VerifyAll();

			var message = builderResult.Message;
			AssertEquals(false, message.IsInDatabase);
			AssertEquals(false, message.IsDeleted);
			message.EM_GB = ZGuid.Empty;
			AssertExceptionThrown(typeof(System.ApplicationException), () => Factory.Save());
			ErrorReporter.Clear();
			AssertEquals(false, message.IsInDatabase);
			AssertEquals(true, message.IsDeleted);
			mockMessageGenerator.VerifyAll();
		}

		public void TestPopulateMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.RunPreSaveValidation();
			AssertEquals("PreCondition", true, declaration.HasMessageErrors);
			AssertEquals("Dec must have a CusEntryHeader", 1, declaration.CustomsEntryHeaders.Count);
			var mockMessageGenerator = new Mock<IMessageGenerator<CusEntryHeader>>(MockBehavior.Strict);
			var expectedReturn = new BuilderResult(declaration.CustomsEntryHeaders[0], new List<string>(), null);
			expectedReturn.Message = Factory.New<EDIMessage>();
			expectedReturn.Message.EM_MessageText = "Hello World" + CusEntryHeader.UCRReferencePlaceHolder + "How Are You?" + JobDeclaration.DeclarationReferencePlaceHolder + "Bye World" + EDIMessage.MessageNumberPlaceHolder + "Again" + CusEntryHeader.UCRPartPlaceHolder;
			mockMessageGenerator.Setup(m => m.Generate(It.IsAny<CusEntryHeader>())).Returns(expectedReturn);
			var messageBuilder = new DeclarationMessageBuilder(declaration, mockMessageGenerator.Object);
			var messageBuilderResult = messageBuilder.PopulateMessages();
			BuilderResult builderResult = null;
			foreach (IBuilderResult bR in messageBuilderResult.GetBuilderResults())
			{
				builderResult = bR as BuilderResult;
			}
			AssertNotNull(builderResult);
			AssertNotNull(builderResult.Message);
			var message = builderResult.Message;
			AssertEquals("Hello World" + CusEntryHeader.UCRReferencePlaceHolder + "How Are You?" + JobDeclaration.DeclarationReferencePlaceHolder + "Bye World" + EDIMessage.MessageNumberPlaceHolder + "Again" + CusEntryHeader.UCRPartPlaceHolder, message.EM_MessageText);
			mockMessageGenerator.VerifyAll();
		}

		public void TestReplacePlaceHolders()
		{
			var declaration = Factory.NewMoq<JobDeclaration>().Object;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "AR1/999";
			var message = Factory.NewMoq<EDIMessage>().Object;
			message.EM_MessageText = $"UCRReference > {CusEntryHeader.UCRReferencePlaceHolder} | UCRPart > {CusEntryHeader.UCRPartPlaceHolder} | UCRReferenceXmlFriendly > {CusEntryHeader.UCRReferencePlaceHolderXmlFriendly} " +
				$"| UCRPartXmlFriendly > {CusEntryHeader.UCRPartPlaceHolderXmlFriendly} | BGMReferenceXmlFriendly > {CusEntryHeader.BGMReferencePlaceHolderXmlFriendly}";
			var messageBuilder = new DeclarationMessageBuilder(declaration, null);
			AssertEquals("UCRReference > AR1 | UCRPart > 999 | UCRReferenceXmlFriendly > AR1 | UCRPartXmlFriendly > 999 | BGMReferenceXmlFriendly > AR1/999", messageBuilder.ReplacePlaceHolders(entryHeader, message.EM_MessageText));
		}

		public void TestCanSaveWithErrorXml()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();

			var generator = new MessageGeneratorForTest(Factory);

			var messageBuilder = new DeclarationMessageBuilder(declaration, generator);
			var result = messageBuilder.PopulateMessages();
			AssertNoExceptionThrown(() => Factory.Save());

			var message = result.GetBuilderResults().Single().Message;
			Assert("Message should have been saved", message.IsInDatabase);
			AssertEquals("EM_MessageInterpretation", "An error occurred when trying to interpret the message", message.EM_MessageInterpretation);

			AssertEquals("Should have reported the error", 1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestMakeXmlSafe()
		{
			var safeString = DeclarationMessageBuilder.MakeXmlSafe("\tTest string\b\r\nWith ampersands & 3 < 2 > 1 && 4 > 3 < 5");
			AssertEquals("\tTest string\r\nWith ampersands &amp; 3 &lt; 2 &gt; 1 &amp;&amp; 4 &gt; 3 &lt; 5", safeString);
		}

		class MessageGeneratorForTest : IMessageGenerator<CusEntryHeader>
		{
			public MessageGeneratorForTest(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			public IBuilderResult Generate(CusEntryHeader bizO)
			{
				var mockStrategy = new Mock<IMessageNumberStrategy>();
				mockStrategy.Setup(x => x.GetMessageReferenceNumber()).Returns("00000001");

				var message = factory.NewWithValidTestData<EDIMessage>();
				message.IsTransmitMessage = true;
				message.EM_LinkedObject = bizO;
				message.MessageNumberStrategy = mockStrategy.Object;

				var result = new BuilderResult(bizO, Enumerable.Empty<string>(), x => { });
				result.Message = message;

				return result;
			}

			public ZString MakePrettyForInterpretation(EDIMessage message)
			{
				throw new System.NotImplementedException();
			}

			public void PutReferenceNumberIntoMessageFromPlaceholder(EDIMessage message, ZString messageText, CusEntryHeader bizO)
			{
			}

			readonly BusinessObjectFactory factory;
		}
	}
}
