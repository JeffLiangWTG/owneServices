using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class DeltaCImportResponseMessageDataObjectTest : TestCaseWithFactory
	{
		public void TestResponseArticleDAUs()
		{
			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportResponseMessageWithLiquidation.xml");
			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = exportMessageText;
			var dataProvider = (DeltaCImportResponseMessageDataObject)message.MessageDataObject;
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
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportValidResponseMessage.xml");

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			AssertType<DeltaCImportResponseMessageDataObject>("Import DELTA G1 Droit Commun response message", message.MessageDataObject);

			var dataProvider = (IResponseDataProvider)message.MessageDataObject;

			CombineAssertions(() =>
			{
				Assert("IsImport", dataProvider.IsImport);
				Assert("HasErrors", !dataProvider.HasErrors);
				AssertEquals("Etat", "VALIDE", dataProvider.Etat);
				AssertEquals("EtatDate", "13/02/2019", dataProvider.EtatDate);
				AssertEquals("EtatHeure", "04:04", dataProvider.EtatHeure);
				AssertEquals("Refdec", "1901204207", dataProvider.Refdec);
				AssertEquals("TransactionID", "AAAAAAAAA+BBB+0000000001", dataProvider.TransactionID);
				AssertEquals("EntryHeaderReference", "9000-B00177613", dataProvider.Refdos);

				var deltaCImportResponseMessageDataObject = (DeltaCImportResponseMessageDataObject)message.MessageDataObject;
				AssertEquals("Evenement", "Demande de validation", deltaCImportResponseMessageDataObject.Evenement);
				AssertEquals("IsGvmsAdvice", false, deltaCImportResponseMessageDataObject.IsGvmsAdvice);

				importMessageText = importMessageText.Replace("Demande de validation", "Embarquement Transmanche");
				importMessageText = importMessageText.Replace("VALIDE", EntryActionCodeList.Codes.ANT);
				var message2 = Factory.New<DeltaCImportFREDIMessage>();
				message2.EM_MessageText = importMessageText;
				var deltaCImportResponseMessageDataObject2 = (DeltaCImportResponseMessageDataObject)message2.MessageDataObject;
				AssertEquals("IsGvmsAdvice", true, deltaCImportResponseMessageDataObject2.IsGvmsAdvice);
			});
		}

		public void TestIResponseDataProvider_Erreur()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			AssertType<DeltaCImportResponseMessageDataObject>("Import DELTA G1 Droit Commun response message", message.MessageDataObject);

			var dataProvider = (IResponseDataProvider)message.MessageDataObject;

			CombineAssertions(() =>
			{
				Assert("IsImport", dataProvider.IsImport);
				Assert("HasErrors", dataProvider.HasErrors);
				AssertEquals("Etat", string.Empty, dataProvider.Etat);
				AssertEquals("EtatDate", string.Empty, dataProvider.EtatDate);
				AssertEquals("EtatHeure", string.Empty, dataProvider.EtatHeure);
				AssertEquals("Refdec", string.Empty, dataProvider.Refdec);
				AssertEquals("TransactionID", "AAAAAAAAA+BBB+0000000001", dataProvider.TransactionID);
				AssertEquals("EntryHeaderReference", "9000-B00177613", dataProvider.Refdos);

				var deltaCImportResponseMessageDataObject = (DeltaCImportResponseMessageDataObject)message.MessageDataObject;
				AssertEquals("Evenement", string.Empty, deltaCImportResponseMessageDataObject.Evenement);
				AssertEquals("IsGvmsAdvice", false, deltaCImportResponseMessageDataObject.IsGvmsAdvice);
			});
		}

		public void TestPrettier()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");
			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;

			AssertType<DeltaCImportResponsePrettier>("DeltaCImportResponsePrettier", message.MessageDataObject.Prettier);
		}

		public void TestPrettier_MessageInterpretation()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportValidResponseMessage.xml");
			var message = Factory.New<DeltaDImportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IMD;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMD;
			message.EM_MessageText = importMessageText;
			var messageInterpretation = @"<H1>Delta response</H1><br /><H3>TransactionID: AAAAAAAAA+BBB+0000000001</H3><H3>Entry Status : VALIDE</H3><H3>Entry Status date: 13/02/2019</H3><H3>Entry Status hour: 04:04</H3><H3>Entry Evenement tag: Demande de validation</H3><H3>Entry Reference: 9000-B00177613</H3><H3>Delta Reference: 1901204207</H3><br />";
			AssertEquals(messageInterpretation, message.MessageDataObject.Prettier.GetMessageInterpretation());
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}

	sealed class DeltaCImportResponseArticleWrapperTest : TestCaseWithFactory
	{
		public void TestResponseArticleAndTaxWrappersIfLiquidationInTheMessage()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportResponseMessageWithLiquidation.xml");
			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			var dataProvider = (DeltaCImportResponseMessageDataObject)message.MessageDataObject;
			var responseArticles = dataProvider.ResponseLiquidations.ToArray();
			AssertEquals(dataProvider.Liquidation.Count, responseArticles.Length);
			for (var i = 0; i < dataProvider.Liquidation.Count; i++)
			{
				AssertEquals(dataProvider.Liquidation[i].Numart, responseArticles[i].numart);
				for (var j = 0; j < dataProvider.Liquidation[i].TTaxationDetails.Count; j++)
				{
					var messageTaxationDetail = dataProvider.Liquidation[i].TTaxationDetails[j];
					var warpperTaxationDetail = responseArticles[i].TaxDetails.ElementAt(j);
					AssertEquals(messageTaxationDetail.Typtax, warpperTaxationDetail.typtax);
					AssertEquals(messageTaxationDetail.Codtax, warpperTaxationDetail.codtax);
					AssertEquals(messageTaxationDetail.Asstax ?? 0m, warpperTaxationDetail.asstax);
					AssertEquals(messageTaxationDetail.Quotax ?? 0m, warpperTaxationDetail.quotax);
					AssertEquals(messageTaxationDetail.Montanttax, warpperTaxationDetail.montanttax);
					AssertEquals(messageTaxationDetail.StatutLiquidation, warpperTaxationDetail.statutLiquidation);
					AssertEquals(messageTaxationDetail.UniSpe != null, warpperTaxationDetail.LiquidationItemHasSuppUnits);
					if (warpperTaxationDetail.LiquidationItemHasSuppUnits)
					{
						AssertEquals(messageTaxationDetail.UniSpe.Unispe + messageTaxationDetail.UniSpe.Qualifunispe, warpperTaxationDetail.SuppUnitsMethodOfCalculation);
					}
					else
					{
						AssertEquals(string.Empty, warpperTaxationDetail.SuppUnitsMethodOfCalculation);
					}
				}
			}
		}

		public void TestResponseArticleAndTaxWrappersIfNoLiquidationInTheMessage()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");
			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_MessageText = importMessageText;
			var dataProvider = (DeltaCImportResponseMessageDataObject)message.MessageDataObject;
			AssertEquals(false, dataProvider.ResponseLiquidations.Any());
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
