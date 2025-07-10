using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class DeltaDImportResponseMessageDataObjectTest : TestCaseWithFactory
	{
		public void TestResponseArticleDAUs()
		{
			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDImportResponseMessageWithLiquidation.xml");
			var message = Factory.New<DeltaDImportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IMD;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMD;
			message.EM_MessageText = exportMessageText;
			var dataProvider = (DeltaDImportResponseMessageDataObject)message.MessageDataObject;
			var responseArticleDAUs = dataProvider.ResponseArticleDAUs.ToArray();
			AssertEquals(1, responseArticleDAUs.Length);
			var articleDAU = responseArticleDAUs.FirstOrDefault();
			AssertEquals(new ZShort(1), articleDAU.numart);
			AssertEquals(50m, articleDAU.Valstat);
			AssertEquals(61m, articleDAU.Valdou);
			AssertEquals(71m, articleDAU.Asstva);
		}

		public void TestIResponseDataProvider_Valid()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDImportValidResponseMessage.xml");

			var message = Factory.New<DeltaDImportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IMD;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMD;
			message.EM_MessageText = importMessageText;
			AssertType<DeltaDImportResponseMessageDataObject>("Import Delta D response message", message.MessageDataObject);

			var dataProvider = (IResponseDataProvider)message.MessageDataObject;

			CombineAssertions(() =>
			{
				Assert("IsImport", dataProvider.IsImport);
				Assert("HasErrors", !dataProvider.HasErrors);
				AssertEquals("Etat", "VALIDE", dataProvider.Etat);
				AssertEquals("EtatDate", "22/06/2020", dataProvider.EtatDate);
				AssertEquals("EtatHeure", "11:05", dataProvider.EtatHeure);
				AssertEquals("Refdec", "2001333222", dataProvider.Refdec);
				AssertEquals("TransactionID", "0000000001", dataProvider.TransactionID);
				AssertEquals("EntryHeaderReference", "0001-B00183250", dataProvider.Refdos);

				var deltaDImportResponseMessageDataObject = (DeltaDImportResponseMessageDataObject)message.MessageDataObject;
				AssertEquals("Evenement", "Demande de validation", deltaDImportResponseMessageDataObject.Evenement);
				AssertEquals("IsGvmsAdvice", false, deltaDImportResponseMessageDataObject.IsGvmsAdvice);

				importMessageText = importMessageText.Replace("Demande de validation", "Embarquement Transmanche");
				importMessageText = importMessageText.Replace("VALIDE", EntryActionCodeList.Codes.ANT);
				var message2 = Factory.New<DeltaDImportFREDIMessage>();
				message2.EM_MessageText = importMessageText;
				var deltaDImportResponseMessageDataObject2 = (DeltaDImportResponseMessageDataObject)message2.MessageDataObject;
				AssertEquals("IsGvmsAdvice", true, deltaDImportResponseMessageDataObject2.IsGvmsAdvice);
			});
		}

		public void TestIResponseDataProvider_Erreur()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDImportErrorResponseMessage.xml");

			var message = Factory.New<DeltaDImportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IMD;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMD;
			message.EM_MessageText = importMessageText;
			AssertType<DeltaDImportResponseMessageDataObject>("Import Delta D response message", message.MessageDataObject);

			var dataProvider = (IResponseDataProvider)message.MessageDataObject;

			CombineAssertions(() =>
			{
				Assert("IsImport", dataProvider.IsImport);
				Assert("HasErrors", dataProvider.HasErrors);
				AssertEquals("Etat", string.Empty, dataProvider.Etat);
				AssertEquals("EtatDate", string.Empty, dataProvider.EtatDate);
				AssertEquals("EtatHeure", string.Empty, dataProvider.EtatHeure);
				AssertEquals("Refdec", string.Empty, dataProvider.Refdec);
				AssertEquals("TransactionID", "0000000001", dataProvider.TransactionID);
				AssertEquals("EntryHeaderReference", "0001-B00183200", dataProvider.Refdos);

				var deltaDImportResponseMessageDataObject = (DeltaDImportResponseMessageDataObject)message.MessageDataObject;
				AssertEquals("Evenement", string.Empty, deltaDImportResponseMessageDataObject.Evenement);
				AssertEquals("IsGvmsAdvice", false, deltaDImportResponseMessageDataObject.IsGvmsAdvice);
			});
		}

		public void TestPrettier()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDImportErrorResponseMessage.xml");
			var message = Factory.New<DeltaDImportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IMD;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMD;
			message.EM_MessageText = importMessageText;

			AssertType<DeltaDImportResponsePrettier>("DeltaDImportResponsePrettier", message.MessageDataObject.Prettier);
		}

		public void TestPrettier_MessageInterpretation()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDImportValidResponseMessage.xml");
			var message = Factory.New<DeltaDImportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IMD;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMD;
			message.EM_MessageText = importMessageText;
			var messageInterpretation = @"<H1>Delta response</H1><br /><H3>TransactionID: 0000000001</H3><H3>Entry Status : VALIDE</H3><H3>Entry Status date: 22/06/2020</H3><H3>Entry Status hour: 11:05</H3><H3>Entry Evenement tag: Demande de validation</H3><H3>Entry Reference: 0001-B00183250</H3><H3>Delta Reference: 2001333222</H3><br />";
			AssertEquals(messageInterpretation, message.MessageDataObject.Prettier.GetMessageInterpretation());
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}

	sealed class DeltaDImportResponseArticleWrapperTest : TestCaseWithFactory
	{
		public void TestResponseArticleAndTaxWrappersIfLiquidationInTheMessage()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDImportResponseMessageWithLiquidation.xml");
			var message = Factory.New<DeltaDImportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IMD;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMD;
			message.EM_MessageText = importMessageText;
			var dataProvider = (DeltaDImportResponseMessageDataObject)message.MessageDataObject;
			var responseArticles = dataProvider.ResponseLiquidations.ToArray();
			AssertEquals(dataProvider.Liquidation.Count, responseArticles.Length);
			for (var i = 0; i < dataProvider.Liquidation.Count; i++)
			{
				AssertEquals(dataProvider.Liquidation[i].Numart, responseArticles[i].numart);
				for (var j = 0; j < dataProvider.Liquidation[i].TTaxationDetails.Count; j++)
				{
					var wrapperTaxationDetail = dataProvider.Liquidation[i].TTaxationDetails[j];
					var messageTaxationDetail = responseArticles[i].TaxDetails.ElementAt(j);
					AssertEquals(wrapperTaxationDetail.Typtax, messageTaxationDetail.typtax);
					AssertEquals(wrapperTaxationDetail.Codtax, messageTaxationDetail.codtax);
					AssertEquals(wrapperTaxationDetail.Asstax ?? 0m, messageTaxationDetail.asstax);
					AssertEquals(wrapperTaxationDetail.Quotax ?? 0m, messageTaxationDetail.quotax);
					AssertEquals(wrapperTaxationDetail.Montanttax, messageTaxationDetail.montanttax);
					AssertEquals(wrapperTaxationDetail.Statutliquidation, messageTaxationDetail.statutLiquidation);
					AssertEquals(wrapperTaxationDetail.UniSpe != null, messageTaxationDetail.LiquidationItemHasSuppUnits);
					if (messageTaxationDetail.LiquidationItemHasSuppUnits)
					{
						AssertEquals(wrapperTaxationDetail.UniSpe.Unispe + wrapperTaxationDetail.UniSpe.Qualifunispe, messageTaxationDetail.SuppUnitsMethodOfCalculation);
					}
				}
			}
		}

		public void TestResponseArticleAndTaxWrappersIfNoLiquidationInTheMessage()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDImportErrorResponseMessage.xml");
			var message = Factory.New<DeltaDImportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IMD;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMD;
			message.EM_MessageText = importMessageText;
			var dataProvider = (DeltaDImportResponseMessageDataObject)message.MessageDataObject;
			AssertEquals(false, dataProvider.ResponseLiquidations.Any());
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
