using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.Customs.FR.Messaging.Interfaces.DCG;
using Moq;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DCG.Testing
{
	public class DCGSendMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGetMessage()
		{
			var message = messageBuilder.GetMessage();
			var expectedMessage = @"<EnveloppeMessage>
<schemaID>MessageDcg</schemaID>
<schemaVersion>18122012</schemaVersion>
<partyId>43000664300034</partyId>
<transactionId>MODSYS+DCG+2011</transactionId>
<numseq>3</numseq>
</EnveloppeMessage>
<Declaration>
<Entete>
<refdos>MODSYS+DCG+2011</refdos>
</Entete>
<Gen>
<numagr>00001930</numagr>
<numcre>AUPK</numcre>
<operep>FR40218856900048</operep>
<paiement>R</paiement>
<debutregul>01/08/2021</debutregul>
<perioderegul>M</perioderegul>
<typflux>IMP</typflux>
</Gen>
</Declaration>";
			AssertXMLContains(expectedMessage.Replace(System.Environment.NewLine, ""), message);

			headerMock.VerifyAll();
			envelopeMock.VerifyAll();
		}

		public void TestEmptyTransactionId()
		{
			envelopeMock.Setup(m => m.TransactionId).Returns(ZString.Empty);
			var message = messageBuilder.GetMessage();
			AssertXMLContains("<transactionId>~CORRELATIONID~</transactionId>", message);
			AssertXMLContains("<refdos>~CORRELATIONID~</refdos>", message);

			headerMock.VerifyAll();
			envelopeMock.VerifyAll();
		}

		public void TestEmptyPartnerId()
		{
			envelopeMock.Setup(m => m.PartnerId).Returns(ZString.Empty);
			var message = messageBuilder.GetMessage();
			AssertCollectionContains(MessageBuilderHelper.RepresentativeIdNotConfigured("G2", "DGI"), errorCollector.GetErrors());

			headerMock.VerifyAll();
			envelopeMock.VerifyAll();
		}

		public void TestDeltaAgreementNumber()
		{
			headerMock.Setup(m => m.DeltaAgreementNumber).Returns(ZString.Empty);
			var message = messageBuilder.GetMessage();
			AssertEquals(0, errorCollector.ErrorCount);

			headerMock.VerifyAll();
			envelopeMock.VerifyAll();
		}

		public void TestOperationalRepresentative()
		{
			headerMock.Setup(m => m.OperationalRepresentative).Returns(ZString.Empty);
			var message = messageBuilder.GetMessage();
			AssertCollectionContains(MessageBuilderHelper.CBRNotConfigured, errorCollector.GetErrors());

			headerMock.VerifyAll();
			envelopeMock.VerifyAll();
		}

		protected override void SetUp()
		{
			base.SetUp();
			envelopeMock = new Mock<IMessageEnvelope>();
			envelopeMock.Setup(m => m.SchemaID).Returns("MessageDcg");
			envelopeMock.Setup(m => m.SchemaVersion).Returns("18122012");
			envelopeMock.Setup(m => m.PartnerId).Returns("43000664300034");
			envelopeMock.Setup(m => m.TransactionId).Returns("MODSYS+DCG+2011");
			envelopeMock.Setup(m => m.NumSeq).Returns(new ZShort("3"));
			headerMock = new Mock<IDCG>();
			headerMock.Setup(m => m.MessageEnvelope).Returns(envelopeMock.Object);
			headerMock.Setup(m => m.DeltaAgreementNumber).Returns("00001930");
			headerMock.Setup(m => m.DefermentAccountNumber).Returns("AUPK");
			headerMock.Setup(m => m.OperationalRepresentative).Returns("FR40218856900048");
			headerMock.Setup(m => m.PaymentType).Returns("R");
			headerMock.Setup(m => m.PeriodStartDate).Returns(new ZDate("2021, 08, 01"));
			headerMock.Setup(m => m.Frequency).Returns("M");
			headerMock.Setup(m => m.Direction).Returns("IMP");
			errorCollector = new ErrorCollector();
			messageBuilder = new DCGSendMessageBuilder(headerMock.Object, errorCollector, TransactionTypes.Original);
		}

		Mock<IDCG> headerMock;
		Mock<IMessageEnvelope> envelopeMock;
		ErrorCollector errorCollector;
		DCGSendMessageBuilder messageBuilder;
	}
}
