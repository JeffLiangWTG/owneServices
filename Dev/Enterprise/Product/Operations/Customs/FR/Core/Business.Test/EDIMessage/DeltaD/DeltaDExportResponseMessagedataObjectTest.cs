using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class DeltaDExportResponseArticleWrapperTest : TestCaseWithFactory
	{
		public void TestResponseArticleDAUs()
		{
			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDExportValidResponseMessage.xml");
			var message = Factory.New<DeltaDExportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.EXD;
			message.EM_MessageSubType = MessageSubTypeList.Codes.EXD;
			message.EM_MessageText = exportMessageText;
			var dataProvider = (DeltaDExportResponseMessageDataObject)message.MessageDataObject;
			var responseArticleDAUs = dataProvider.ResponseArticleDAUs.ToArray();
			AssertEquals(1, responseArticleDAUs.Length);
			var articleDAU = responseArticleDAUs.FirstOrDefault();
			AssertEquals(new ZShort(1), articleDAU.numart);
			AssertEquals(0m, articleDAU.Valstat);
			AssertEquals(5962m, articleDAU.Valdou);
			AssertEquals(0m, articleDAU.Asstva);
		}

		public void TestResponseArticleAndTaxWrappersIfLiquidationInTheMessage()
		{
			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDExportResponseMessageWithLiquidation.xml");
			var message = Factory.New<DeltaDExportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.EXD;
			message.EM_MessageSubType = MessageSubTypeList.Codes.EXD;
			message.EM_MessageText = exportMessageText;
			var dataProvider = (DeltaDExportResponseMessageDataObject)message.MessageDataObject;
			var responseArticles = dataProvider.ResponseLiquidations.ToArray();
			AssertEquals(dataProvider.Liquidation.Count, responseArticles.Length);
			for (var i = 0; i < dataProvider.Liquidation.Count; i++)
			{
				AssertEquals(dataProvider.Liquidation[i].Numart, responseArticles[i].numart);
				for (var j = 0; j < dataProvider.Liquidation[i].TTaxationDetails.Count; j++)
				{
					var messageTaxationDetail = dataProvider.Liquidation[i].TTaxationDetails[j];
					var wrapperTaxationDetail = responseArticles[i].TaxDetails.ElementAt(j);
					AssertEquals(messageTaxationDetail.Typtax, wrapperTaxationDetail.typtax);
					AssertEquals(messageTaxationDetail.Codtax, wrapperTaxationDetail.codtax);
					AssertEquals(messageTaxationDetail.Asstax ?? 0m, wrapperTaxationDetail.asstax);
					AssertEquals(messageTaxationDetail.Quotax ?? 0m, wrapperTaxationDetail.quotax);
					AssertEquals(messageTaxationDetail.Montanttax, wrapperTaxationDetail.montanttax);
					AssertEquals(messageTaxationDetail.Statutliquidation, wrapperTaxationDetail.statutLiquidation);
					AssertEquals(messageTaxationDetail.UniSpe != null, wrapperTaxationDetail.LiquidationItemHasSuppUnits);
					if (wrapperTaxationDetail.LiquidationItemHasSuppUnits)
					{
						AssertEquals(messageTaxationDetail.UniSpe.Unispe + messageTaxationDetail.UniSpe.Qualifunispe, wrapperTaxationDetail.SuppUnitsMethodOfCalculation);
					}
				}
			}
		}

		public void TestResponseArticleAndTaxWrappersIfNoLiquidationInTheMessage()
		{
			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDExportErrorResponseMessage.xml");
			var message = Factory.New<DeltaDExportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.EXD;
			message.EM_MessageSubType = MessageSubTypeList.Codes.EXD;
			message.EM_MessageText = exportMessageText;
			var dataProvider = (DeltaDExportResponseMessageDataObject)message.MessageDataObject;
			AssertEquals(false, dataProvider.ResponseLiquidations.Any());
		}

		public void TestIsGvmsAdvice()
		{
			AssertEquals(false, deltaDExportResponseMessageDataObject.IsGvmsAdvice);
		}

		public void TestEvenement()
		{
			AssertEquals(ZString.Empty, deltaDExportResponseMessageDataObject.Evenement);
		}

		public void TestIResponseDataProvider_ECS()
		{
			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDExportECSResponseMessage.xml");
			var message = Factory.New<DeltaDExportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.EXD;
			message.EM_MessageSubType = MessageSubTypeList.Codes.EXD;
			message.EM_MessageText = exportMessageText;
			var dataProvider = (DeltaDExportResponseMessageDataObject)message.MessageDataObject;
			AssertEquals("etatECS", "EEC", dataProvider.EtatECS);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var message = Factory.New<DeltaDExportFREDIMessage>();
			deltaDExportResponseMessageDataObject = new DeltaDExportResponseMessageDataObject(message);
		}

		DeltaDExportResponseMessageDataObject deltaDExportResponseMessageDataObject;

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
