using System.Collections.ObjectModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces.COD;
using Moq;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.COD.Testing
{
	public class CODSendMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGetMessageWhenActionCodeIs3()
		{
			genMock = new Mock<IGen>();
			genMock.Setup(m => m.EntryNumber).Returns("10000002");
			genMock.Setup(m => m.Direction).Returns("IMP");
			genMock.Setup(m => m.Numcod).Returns("3");
			genMock.Setup(m => m.Opecod).Returns("FR123456");
			var gens = new Collection<IGen>();
			gens.Add(genMock.Object);

			headerMock = new Mock<ICOD>();
			headerMock.Setup(m => m.ActionCode).Returns("3");
			headerMock.Setup(m => m.FileReference).Returns("CORRELATIONID");
			headerMock.Setup(m => m.Gens).Returns(gens);

			errorCollector = new ErrorCollector();
			messageBuilder = new CODSendMessageBuilder(headerMock.Object, errorCollector, TransactionTypes.Original);

			var message = messageBuilder.GetMessage();
			var expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Message xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<Declaration>
<Entete>
<codact>3</codact>
<refdos>CORRELATIONID</refdos>
</Entete>
<Gens>
<Gen>
<refdec>10000002</refdec>
<typflux>IMP</typflux>
<numcod>3</numcod>
<Operateur>
<opecod>FR123456</opecod>
</Operateur>
</Gen>
</Gens>
</Declaration>
</Message>";
			AssertXMLContains(expectedMessage.Replace(System.Environment.NewLine, ""), message);

			genMock.VerifyAll();
			headerMock.VerifyAll();
		}

		public void TestGetMessageWhenActionCodeIs1_IndicateurApurementIs1()
		{
			docAapurerMock = new Mock<IDocAapurer>();
			docAapurerMock.Setup(m => m.DocumentCode).Returns("code1");
			docAapurerMock.Setup(m => m.DocumentReference).Returns("reference1");
			var documents = new Collection<IDocAapurer>();
			documents.Add(docAapurerMock.Object);

			apurMock = new Mock<IApur>();
			apurMock.Setup(m => m.IndicateurApurement).Returns(true);
			apurMock.Setup(m => m.Mnt).Returns(5m);
			apurMock.Setup(m => m.Refdecapur).Returns("10000001");

			articleMock = new Mock<IArticle>();
			articleMock.Setup(m => m.EntryNumber).Returns("10000001");
			articleMock.Setup(m => m.Direction).Returns("IMP");
			articleMock.Setup(m => m.ItemNumber).Returns("2");
			articleMock.Setup(m => m.Documents).Returns(documents);
			articleMock.Setup(m => m.Apur).Returns(apurMock.Object);
			var articles = new Collection<IArticle>();
			articles.Add(articleMock.Object);

			headerMock = new Mock<ICOD>();
			headerMock.Setup(m => m.ActionCode).Returns("1");
			headerMock.Setup(m => m.FileReference).Returns("CORRELATIONID");
			headerMock.Setup(m => m.Items).Returns(articles);

			errorCollector = new ErrorCollector();
			messageBuilder = new CODSendMessageBuilder(headerMock.Object, errorCollector, TransactionTypes.Original);

			var message = messageBuilder.GetMessage();
			var expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Message xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<Declaration>
<Entete>
<codact>1</codact>
<refdos>CORRELATIONID</refdos>
</Entete>
<Articles>
<Article>
<refdec>10000001</refdec>
<typflux>IMP</typflux>
<numart>2</numart>
<Apur>
<apurementREC>
<indicateurApurement>1</indicateurApurement>
<mnt>5</mnt>
<refdecapur>10000001</refdecapur>
</apurementREC>
</Apur>
<Documents>
<DocumentAapurer>
<doc>code1</doc>
<refdoc>reference1</refdoc>
</DocumentAapurer>
</Documents>
</Article>
</Articles>
</Declaration>
</Message>";
			AssertXMLContains(expectedMessage.Replace(System.Environment.NewLine, ""), message);

			docAapurerMock.VerifyAll();
			apurMock.VerifyAll();
			articleMock.VerifyAll();
			headerMock.VerifyAll();
		}

		public void TestGetMessageWhenActionCodeIs1_IndicateurApurementIs0()
		{
			docAapurerMock = new Mock<IDocAapurer>();
			docAapurerMock.Setup(m => m.DocumentCode).Returns("code1");
			docAapurerMock.Setup(m => m.DocumentReference).Returns("reference1");
			var documents = new Collection<IDocAapurer>();
			documents.Add(docAapurerMock.Object);

			apurMock = new Mock<IApur>();
			apurMock.Setup(m => m.IndicateurApurement).Returns(false);
			apurMock.Setup(m => m.Mnt).Returns(0m);
			apurMock.Setup(m => m.Refdecapur).Returns("10000001");

			articleMock = new Mock<IArticle>();
			articleMock.Setup(m => m.EntryNumber).Returns("10000001");
			articleMock.Setup(m => m.Direction).Returns("IMP");
			articleMock.Setup(m => m.ItemNumber).Returns("2");
			articleMock.Setup(m => m.Documents).Returns(documents);
			articleMock.Setup(m => m.Apur).Returns(apurMock.Object);
			var articles = new Collection<IArticle>();
			articles.Add(articleMock.Object);

			headerMock = new Mock<ICOD>();
			headerMock.Setup(m => m.ActionCode).Returns("1");
			headerMock.Setup(m => m.FileReference).Returns("CORRELATIONID");
			headerMock.Setup(m => m.Items).Returns(articles);

			errorCollector = new ErrorCollector();
			messageBuilder = new CODSendMessageBuilder(headerMock.Object, errorCollector, TransactionTypes.Original);

			var message = messageBuilder.GetMessage();
			var expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Message xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<Declaration>
<Entete>
<codact>1</codact>
<refdos>CORRELATIONID</refdos>
</Entete>
<Articles>
<Article>
<refdec>10000001</refdec>
<typflux>IMP</typflux>
<numart>2</numart>
<Apur>
<apurementREC>
<indicateurApurement>0</indicateurApurement>
<refdecapur>10000001</refdecapur>
</apurementREC>
</Apur>
<Documents>
<DocumentAapurer>
<doc>code1</doc>
<refdoc>reference1</refdoc>
</DocumentAapurer>
</Documents>
</Article>
</Articles>
</Declaration>
</Message>";
			AssertXMLContains(expectedMessage.Replace(System.Environment.NewLine, ""), message);

			docAapurerMock.VerifyAll();
			apurMock.VerifyAll();
			articleMock.VerifyAll();
			headerMock.VerifyAll();
		}

		public void TestGetMessageWithoutArticles()
		{
			docAapurerMock = new Mock<IDocAapurer>();
			var documents = new Collection<IDocAapurer>();
			documents.Add(docAapurerMock.Object);

			articleMock = new Mock<IArticle>();
			var articles = new Collection<IArticle>();
			articles.Add(articleMock.Object);

			headerMock = new Mock<ICOD>();
			headerMock.Setup(m => m.ActionCode).Returns("1");
			headerMock.Setup(m => m.FileReference).Returns("CORRELATIONID");

			errorCollector = new ErrorCollector();
			messageBuilder = new CODSendMessageBuilder(headerMock.Object, errorCollector, TransactionTypes.Original);

			headerMock.Setup(m => m.ActionCode).Returns("2");
			var message = messageBuilder.GetMessage();
			var expectedMessage = @"<?xml version=""1.0"" encoding=""utf-8""?><Message xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<Declaration>
<Entete>
<codact>2</codact>
<refdos>CORRELATIONID</refdos>
</Entete>
</Declaration>
</Message>";
			AssertXMLContains(expectedMessage.Replace(System.Environment.NewLine, ""), message);

			docAapurerMock.VerifyAll();
			articleMock.VerifyAll();
			headerMock.VerifyAll();
		}

		Mock<IDocAapurer> docAapurerMock;
		Mock<IArticle> articleMock;
		Mock<IApur> apurMock;
		Mock<IGen> genMock;
		Mock<ICOD> headerMock;
		ErrorCollector errorCollector;
		CODSendMessageBuilder messageBuilder;
	}
}
