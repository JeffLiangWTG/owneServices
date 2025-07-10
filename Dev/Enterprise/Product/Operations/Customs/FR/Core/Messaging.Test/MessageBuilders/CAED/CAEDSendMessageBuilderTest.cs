using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces.CAED;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.CAED.Testing
{
	public class CAEDSendMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2022, 02, 13, 08, 24, 00)]
		public void TestGenerateFullMessage()
		{
			var message = messageBuilder.GetMessage();
			var expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Interchanges xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" id=""#INTERCHANGE_ID#"" from=""#EDI_FROM#"" to=""2302145"">
	<MessageSet id=""#INTERCHANGE_ID#"" icid=""#INTERCHANGE_ID#"" date=""02/13/2022 08:24:00"">
		<Destinataire xsi:type=""Destinataire"" user=""INTFEASY"" tiersProf=""2302145"" />
		<Emetteur xsi:type=""Emetteur"" user=""EASYLOG"" tiersProf=""APLEH"" />
		<Messages>
			<Request action=""CREATE"" id=""#INTERCHANGE_ID#"" type=""CAED"">
				<controle-prealable>
					<references-ctrl rca=""CNI0002552586"" globale=""Y"" type=""STI"" dos=""NCT004502"" />
					<tiers-ctrl code=""FR1234567"" />
					<lieux-ctrl md=""TDF"" bdd=""FR002300"" />
					<lmarchandise-ctrl nb=""27"" />
					<equipement-ctrl id=""B940360900"" />
					<equipement-ctrl id=""B940360901"" />
					<droits-ctrl montant=""86"" unite=""EUR"" port=""123456"" />	
				</controle-prealable>
			</Request>
		</Messages>
	</MessageSet>
</Interchanges>".Replace("	", "").Replace(System.Environment.NewLine, "");
			AssertXMLEquals(expectedMessage, message);
			caedMock.VerifyAll();
		}

		public void TestSenderUser()
		{
			var errorMessage = "User code of the message sender should not be empty.";
			caedMock.Setup(m => m.SenderUser).Returns("");
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate tag Emetteur when sender user has no value.", "Emetteur");
			AssertContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			caedMock.Setup(m => m.SenderUser).Returns("SON_001");
			message = messageBuilder.GetMessage();
			AssertContains(@"user=""SON_001""", message);
			AssertContains("Emetteur", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());
			caedMock.VerifyAll();
		}

		public void TestSenderTiersProf()
		{
			var errorMessage = "Profession ID of the message sender should not be empty.";
			caedMock.Setup(m => m.SenderTiersProf).Returns("");
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate tag Emetteur when sender Profession ID has no value.", "Emetteur");
			AssertContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			caedMock.Setup(m => m.SenderTiersProf).Returns("EASYLOG");
			message = messageBuilder.GetMessage();
			AssertContains(@"tiersProf=""EASYLOG""", message);
			AssertContains("Emetteur", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());
			caedMock.VerifyAll();
		}

		public void TestRecipientUser()
		{
			var errorMessage = "User code of the message recipient should not be empty.";
			caedMock.Setup(m => m.RecipientUser).Returns("");
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate tag Destinataire when recipient user has no value.", "Destinataire");
			AssertContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			caedMock.Setup(m => m.RecipientUser).Returns("CI5_001");
			message = messageBuilder.GetMessage();
			AssertContains(@"user=""CI5_001""", message);
			AssertContains("Destinataire", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());
			caedMock.VerifyAll();
		}

		public void TestRecipientTiersProf()
		{
			var errorMessage = "Profession ID of the message recipient should not be empty.";
			caedMock.Setup(m => m.RecipientTiersProf).Returns("");
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate tag Destinataire when recipient Profession ID has no value.", "Destinataire");
			AssertContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			caedMock.Setup(m => m.RecipientTiersProf).Returns("APPLUS");
			message = messageBuilder.GetMessage();
			AssertContains(@"tiersProf=""APPLUS""", message);
			AssertContains("Destinataire", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());
			caedMock.VerifyAll();
		}

		public void TestDeclarationType()
		{
			var errorMessage = "Declaration type should not be empty.";
			caedMock.Setup(m => m.DeclarationType).Returns("");
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate tag  references-ctrl when declaration type has no value.", "reference-doc", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			caedMock.Setup(m => m.DeclarationType).Returns("T1");
			message = messageBuilder.GetMessage();
			AssertContains(@"type=""T1""", message);
			AssertContains("references-ctrl", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());

			caedMock.VerifyAll();
		}

		public void TestJobReference()
		{
			var errorMessage = "Job reference should not be empty.";
			caedMock.Setup(m => m.JobReference).Returns("");
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate tag references-ctrl when job reference has no value.", "reference-doc", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			caedMock.Setup(m => m.JobReference).Returns("NCT123");
			message = messageBuilder.GetMessage();
			AssertContains(@"dos=""NCT123""", message);
			AssertContains("references-ctrl", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());

			caedMock.VerifyAll();
		}

		public void TestTotalNumberOfPackages()
		{
			var errorMessage = "Total number of packages should be greater than 0.";
			caedMock.Setup(m => m.TotalNumberOfPackages).Returns(0);
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate lmarchandise-doc when packages number is 0.", "lmarchandise-ctrl", message);
			AssertContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			caedMock.Setup(m => m.TotalNumberOfPackages).Returns(20);
			message = messageBuilder.GetMessage();
			AssertContains(@"nb=""20""", message);
			AssertContains("lmarchandise-ctrl", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());

			caedMock.VerifyAll();
		}

		public void TestSiretNumber()
		{
			var errorMessage = "Declarant SIRET Number should not be empty.";
			caedMock.Setup(m => m.DeclarantsSIRETNumber).Returns("");
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate tag tiers-ctrl  when SiretNumber has no value.", "tiers-ctrl", message);
			AssertContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			caedMock.Setup(m => m.DeclarantsSIRETNumber).Returns("FR1234567");
			message = messageBuilder.GetMessage();
			AssertContains(@"code=""FR1234567""", message);
			AssertContains("tiers-ctrl", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());

			caedMock.VerifyAll();
		}

		public void TestPort()
		{
			caedMock.Setup(m => m.Port).Returns("");
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate port if has no value.", "port", message);
			AssertContains("Should populate group droits-ctrl even when port has no value.", "droits-ctrl", message);
			AssertEquals(0, errorCollector.ErrorCount);

			errorCollector.WipeErrors();
			caedMock.Setup(m => m.Port).Returns("port");
			message = messageBuilder.GetMessage();
			AssertContains(@"port=""port""", message);
			AssertContains("droits-ctrl", message);
			AssertEquals(0, errorCollector.ErrorCount);

			caedMock.VerifyAll();
		}

		public void TestHarborDuesAmount()
		{
			caedMock.Setup(m => m.HarborDuesAmount).Returns(0);
			var message = messageBuilder.GetMessage();
			AssertNotContains("montant", message);
			AssertNotContains("unite", message);
			AssertContains("Should populate group droits-ctrl even when harbor dues amount is zero.", "droits-ctrl", message);
			AssertEquals(0, errorCollector.ErrorCount);

			errorCollector.WipeErrors();
			caedMock.Setup(m => m.HarborDuesAmount).Returns(10);
			message = messageBuilder.GetMessage();
			AssertContains(@"montant=""10""", message);
			AssertContains(@"unite=""EUR""", message);
			AssertContains("droits-ctrl", message);
			AssertEquals(0, errorCollector.ErrorCount);

			caedMock.VerifyAll();
		}

		protected override void SetUp()
		{
			base.SetUp();
			caedMock = new Mock<ICAED>();
			caedMock.Setup(m => m.PortSystem).Returns("MGI");
			caedMock.Setup(m => m.SenderUser).Returns("EASYLOG");
			caedMock.Setup(m => m.SenderTiersProf).Returns("APLEH");
			caedMock.Setup(m => m.RecipientUser).Returns("INTFEASY");
			caedMock.Setup(m => m.RecipientTiersProf).Returns("2302145");
			caedMock.Setup(m => m.DeclarationType).Returns("STI");
			caedMock.Setup(m => m.CommonAccessReference).Returns("CNI0002552586");
			caedMock.Setup(m => m.JobReference).Returns("NCT004502");
			caedMock.Setup(m => m.CustomsDepartureOffice).Returns("FR002300");
			caedMock.Setup(m => m.CTOUser).Returns("TDF");
			caedMock.Setup(m => m.TotalNumberOfPackages).Returns(27);
			caedMock.Setup(m => m.HarborDuesAmount).Returns(86);
			caedMock.Setup(m => m.HarborDuesCurrency).Returns("EUR");
			caedMock.Setup(m => m.Containers).Returns(new List<ZString> {
				new ZString("B940360900"),
				new ZString("B940360901"),
			});
			caedMock.Setup(m => m.AppliesToAllPacks).Returns(true);
			caedMock.Setup(m => m.DeclarantsSIRETNumber).Returns("FR1234567");
			caedMock.Setup(m => m.Port).Returns("123456");
			errorCollector = new ErrorCollector();
			messageBuilder = new CAEDSendMessageBuilder(caedMock.Object, errorCollector, TransactionTypes.Original);
		}

		Mock<ICAED> caedMock;
		ErrorCollector errorCollector;
		CAEDSendMessageBuilder messageBuilder;
	}
}
