using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces.DOA;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DOA.Testing
{
	public class DOASendMessageBuilderTest : TestCaseWithFactory
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
			<Request action=""CREATE"" id=""#INTERCHANGE_ID#"" type=""DOA"">
				<document-accompagnement>
					<reference-doc rca=""CNI0002552586"" num=""21FR230000440272A7"" ref=""POI-42803"" type=""STI"" circuit=""BAE"" statut=""VA"" anticip=""Y"" num-doss=""NCT004502"" />
					<tiers-doc sic=""APLEH"" />
					<lieux-doc md=""TDF"" bdd=""FR002300"" bds=""FR003800"" />
					<lmarchandise-doc nb=""27"" poids=""16149"" poidsnet=""12006"" scelle=""Y"" />
					<droits-doc montant=""86"" unite=""EUR"" />
					<description-doc ndp=""940360900"">
						<description>COFFEE</description>
					</description-doc>
					<description-doc ndp=""4301100000"">
						<description>MILK</description>
					</description-doc>
				</document-accompagnement>
			</Request>
		</Messages>
	</MessageSet>
</Interchanges>".Replace("	", "").Replace(System.Environment.NewLine, "");
			AssertXMLEquals(expectedMessage, message);

			doaMock.VerifyAll();
		}

		public void TestSenderUser()
		{
			var errorMessage = "User code of the message sender should not be empty.";
			doaMock.Setup(m => m.SenderUser).Returns("");
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate tag Emetteur when sender user has no value.", "Emetteur", message);
			AssertContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			doaMock.Setup(m => m.SenderUser).Returns("SON_001");
			message = messageBuilder.GetMessage();
			AssertContains(@"user=""SON_001""", message);
			AssertContains("Emetteur", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());

			doaMock.VerifyAll();
		}

		public void TestSenderTiersProf()
		{
			var errorMessage = "Profession ID of the message sender should not be empty.";
			doaMock.Setup(m => m.SenderTiersProf).Returns("");
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate tag Emetteur when sender Profession ID has no value.", "Emetteur", message);
			AssertContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			doaMock.Setup(m => m.SenderTiersProf).Returns("EASYLOG");
			message = messageBuilder.GetMessage();
			AssertContains(@"tiersProf=""EASYLOG""", message);
			AssertContains("Emetteur", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());

			doaMock.VerifyAll();
		}

		public void TestRecipientUser()
		{
			var errorMessage = "User code of the message recipient should not be empty.";
			doaMock.Setup(m => m.RecipientUser).Returns("");
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate tag Destinataire when recipient user has no value.", "Destinataire", message);
			AssertContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			doaMock.Setup(m => m.RecipientUser).Returns("CI5_001");
			message = messageBuilder.GetMessage();
			AssertContains(@"user=""CI5_001""", message);
			AssertContains("Destinataire", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());

			doaMock.VerifyAll();
		}

		public void TestRecipientTiersProf()
		{
			var errorMessage = "Profession ID of the message recipient should not be empty.";
			doaMock.Setup(m => m.RecipientTiersProf).Returns("");
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate tag Destinataire when recipient Profession ID has no value.", "Destinataire", message);
			AssertContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			doaMock.Setup(m => m.RecipientTiersProf).Returns("APPLUS");
			message = messageBuilder.GetMessage();
			AssertContains(@"tiersProf=""APPLUS""", message);
			AssertContains("Destinataire", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());

			doaMock.VerifyAll();
		}

		public void TestDeclarationNumber()
		{
			var errorMessage = "Declaration number should not be empty.";
			doaMock.Setup(m => m.DeclarationNumber).Returns("");
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate tag reference-doc when declaration number has no value.", "reference-doc", message);
			AssertContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			doaMock.Setup(m => m.DeclarationNumber).Returns("123");
			message = messageBuilder.GetMessage();
			AssertContains(@"num=""123""", message);
			AssertContains("reference-doc", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());

			doaMock.VerifyAll();
		}

		public void TestDeclarationType()
		{
			var errorMessage = "Declaration type should not be empty.";
			doaMock.Setup(m => m.DeclarationType).Returns("");
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate tag reference-doc when declaration type has no value.", "reference-doc", message);
			AssertContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			doaMock.Setup(m => m.DeclarationType).Returns("T1");
			message = messageBuilder.GetMessage();
			AssertContains(@"type=""T1""", message);
			AssertContains("reference-doc", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());

			doaMock.VerifyAll();
		}

		public void TestJobReference()
		{
			var errorMessage = "Job reference should not be empty.";
			doaMock.Setup(m => m.JobReference).Returns("");
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate tag num-doss when job reference has no value.", "reference-doc", message);
			AssertContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			doaMock.Setup(m => m.JobReference).Returns("NCT123");
			message = messageBuilder.GetMessage();
			AssertContains(@"num-doss=""NCT123""", message);
			AssertContains("reference-doc", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());

			doaMock.VerifyAll();
		}

		public void TestTotalNumberOfPackages()
		{
			var errorMessage = "Total number of packages should be greater than 0.";
			doaMock.Setup(m => m.TotalNumberOfPackages).Returns(0);
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate lmarchandise-doc when packages number is 0.", "lmarchandise-doc", message);
			AssertContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			doaMock.Setup(m => m.TotalNumberOfPackages).Returns(20);
			message = messageBuilder.GetMessage();
			AssertContains(@"nb=""20""", message);
			AssertContains("lmarchandise-doc", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());

			doaMock.VerifyAll();
		}

		public void TestTotalGrossWeightInKilograms()
		{
			var errorMessage = "Total gross weight should be greater than 0.";
			doaMock.Setup(m => m.TotalGrossWeightInKilograms).Returns(0);
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not populate lmarchandise-doc when gross weight is 0.", "lmarchandise-doc", message);
			AssertContains(errorMessage, errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			doaMock.Setup(m => m.TotalGrossWeightInKilograms).Returns(20);
			message = messageBuilder.GetMessage();
			AssertContains(@"poids=""20""", message);
			AssertContains("lmarchandise-doc", message);
			AssertNotContains(errorMessage, errorCollector.GetErrorsAsString());

			doaMock.VerifyAll();
		}

		public void TestContainers()
		{
			doaMock.Setup(m => m.EquipmentReference).Returns("CNT001");

			var message = messageBuilder.GetMessage();
			AssertContains("EquipmentReference should be displayed in eqd attribute", @"eqd=""CNT001""", message);

			doaMock.Setup(m => m.Containers).Returns(new List<CargoWise.Types.ZString>
			{
				"CNT001",
				"CNT002"
			});

			message = messageBuilder.GetMessage();
			AssertContains("Containers should be listed in conteneur-doc tags", @"conteneur-doc eqd=""CNT001""", message);
			AssertContains("Containers should be listed in conteneur-doc tags", @"conteneur-doc eqd=""CNT002""", message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			doaMock = new Mock<IDOA>();
			doaMock.Setup(m => m.PortSystem).Returns("MGI");
			doaMock.Setup(m => m.SenderUser).Returns("EASYLOG");
			doaMock.Setup(m => m.SenderTiersProf).Returns("APLEH");
			doaMock.Setup(m => m.RecipientUser).Returns("INTFEASY");
			doaMock.Setup(m => m.RecipientTiersProf).Returns("2302145");
			doaMock.Setup(m => m.DeclarationNumber).Returns("21FR230000440272A7");
			doaMock.Setup(m => m.DeclarationType).Returns("STI");
			doaMock.Setup(m => m.CommonAccessReference).Returns("CNI0002552586");
			doaMock.Setup(m => m.DeclarationReference).Returns("POI-42803");
			doaMock.Setup(m => m.DeclarationStatus).Returns("BAE");
			doaMock.Setup(m => m.JobReference).Returns("NCT004502");
			doaMock.Setup(m => m.Prelodged).Returns(true);
			doaMock.Setup(m => m.CustomsDepartureOffice).Returns("FR002300");
			doaMock.Setup(m => m.CustomsDestinationOffice).Returns("FR003800");
			doaMock.Setup(m => m.CTOUser).Returns("TDF");
			doaMock.Setup(m => m.TotalNumberOfPackages).Returns(27);
			doaMock.Setup(m => m.TotalGrossWeightInKilograms).Returns(16149);
			doaMock.Setup(m => m.TotalNetWeightInKilograms).Returns(12006);
			doaMock.Setup(m => m.HasSeal).Returns(true);
			doaMock.Setup(m => m.HarborDuesAmount).Returns(86);
			doaMock.Setup(m => m.HarborDuesCurrency).Returns("EUR");
			doaMock.Setup(m => m.Tariffs).Returns(new List<CodeDescriptionPair>
			{
				new CodeDescriptionPair("940360900", "COFFEE"),
				new CodeDescriptionPair("4301100000", "MILK")
			});
			errorCollector = new ErrorCollector();
			messageBuilder = new DOASendMessageBuilder(doaMock.Object, errorCollector, TransactionTypes.Original);
		}

		Mock<IDOA> doaMock;
		ErrorCollector errorCollector;
		DOASendMessageBuilder messageBuilder;
	}
}
