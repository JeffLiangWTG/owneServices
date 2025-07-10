using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class DeltaCExportResponseArticleWrapperTest : TestCaseWithFactory
	{
		public void TestResponseArticleDAUs()
		{
			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DaltaCExportValidResponseMessage.xml");
			var message = Factory.New<DeltaCExportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.EXC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.EXC;
			message.EM_MessageText = exportMessageText;
			var dataProvider = (DeltaCExportResponseMessageDataObject)message.MessageDataObject;
			var responseArticleDAUs = dataProvider.ResponseArticleDAUs.ToArray();
			AssertEquals(1, responseArticleDAUs.Length);
			var articleDAU = responseArticleDAUs.FirstOrDefault();
			AssertEquals(new ZShort(1), articleDAU.numart);
			AssertEquals(5962m, articleDAU.Valstat);
			AssertEquals(5962m, articleDAU.Valdou);
			AssertEquals(0m, articleDAU.Asstva);
		}

		public void TestResponseArticleAndTaxWrappersIfLiquidationInTheMessage()
		{
			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportResponseMessageWithLiquidation.xml");
			var message = Factory.New<DeltaCExportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.EXC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.EXC;
			message.EM_MessageText = exportMessageText;
			var dataProvider = (DeltaCExportResponseMessageDataObject)message.MessageDataObject;
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
			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportErrorMessage.xml");
			var message = Factory.New<DeltaCExportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.EXC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.EXC;
			message.EM_MessageText = exportMessageText;
			var dataProvider = (DeltaCExportResponseMessageDataObject)message.MessageDataObject;
			AssertEquals(false, dataProvider.ResponseLiquidations.Any());
		}

		public void TestIsGvmsAdvice()
		{
			AssertEquals(false, deltaCExportResponseMessageDataObject.IsGvmsAdvice);
		}

		public void TestEvenement()
		{
			AssertEquals(ZString.Empty, deltaCExportResponseMessageDataObject.Evenement);
		}

		public void TestIResponseDataProvider_ECS()
		{
			var exportMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportECSResponseMessage.xml");
			var message = Factory.New<DeltaCExportFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.EXC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.EXC;
			message.EM_MessageText = exportMessageText;
			var dataProvider = (DeltaCExportResponseMessageDataObject)message.MessageDataObject;

			AssertEquals("etatECS", "EEC", dataProvider.EtatECS);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var message = Factory.New<DeltaCExportFREDIMessage>();
			deltaCExportResponseMessageDataObject = new DeltaCExportResponseMessageDataObject(message);
		}

		DeltaCExportResponseMessageDataObject deltaCExportResponseMessageDataObject;

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
