using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing
{
	[TestedType(typeof(ChiefExportDes242MessagesOnConsolCollection))]
	sealed class ChiefExportDes242MessagesOnConsolCollectionTest : EDIMessageCollectionTest
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(ChiefExportDes242MessagesOnConsolCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, new SendsMessagesToCustomsShutterUpperer());
			return wrapper.MawbExportHelper.Messages;
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "AUSYD";
		}

		public void TestLoadMessages()
		{
			var badMessage1 = consol.Messages.AddNew();
			var goodMessage1 = consol.Messages.AddNew();
			var goodMessage2 = consol.Messages.AddNew();
			var goodMessage3 = consol.Messages.AddNew();
			var goodMessage4 = consol.Messages.AddNew();

			badMessage1.EM_ApplicationCode = "BAD";
			goodMessage1.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			goodMessage2.EM_ApplicationCode = ApplicationCodeList.Codes.GbEdifactShared;
			goodMessage3.EM_ApplicationCode = ApplicationCodeList.Codes.GbCustomsDeclarationServices;
			goodMessage4.EM_ApplicationCode = ApplicationCodeList.Codes.GbCDSViaCCSUK;

			var collection = (EDIMessageCollection)GetCollectionToTest();
			AssertContainsExactElementsInAnyOrder(new EDIMessage[] { goodMessage1, goodMessage2, goodMessage3, goodMessage4 }, collection);
		}

		ForwardingConsol consol;
	}
}
