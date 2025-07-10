using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing
{
	[TestedType(typeof(ChiefExportConsolIntegrationNPBO))]
	class ChiefExportConsolIntegrationNPBOTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var eacMessage = entry.Messages.AddNew();
			eacMessage.EM_ReceiveTransmit = "TRX";
			eacMessage.EM_MessageType = "EAC";
			var npbo = new ChiefExportConsolIntegrationNPBO(eacMessage);
			entry.CH_BGMReference = "BGM, yo";
			return npbo;
		}

		public void TestUCR()
		{
			var message = (ChiefExportConsolIntegrationNPBO)GetNewBusinessObject();
			AssertEquals("BGM, yo", message.UCR);
		}
	}

	[TestedType(typeof(ChiefExportDes242MessagesOnConsolsShipmentsDeclarationsCollection))]
	class ChiefExportDes242MessagesOnConsolsShipmentsDeclarationsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ChiefExportDes242MessagesOnConsolsShipmentsDeclarationsCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(ChiefExportDes242MessagesOnConsolsShipmentsDeclarationsCollection);
		}

		protected override ChiefExportDes242MessagesOnConsolsShipmentsDeclarationsCollection GetCollectionToTest()
		{
			var consol = Factory.New<ForwardingConsol>();
			return new ChiefExportDes242MessagesOnConsolsShipmentsDeclarationsCollection(consol);
		}

		public void TestIsReadOnly()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowNew);
			AssertEquals(false, collection.AllowRemove);
			AssertEquals(true, collection.ReadOnly);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var eacMessage = entry.Messages.AddNew();
			eacMessage.EM_ReceiveTransmit = "TRX";
			eacMessage.EM_MessageType = "EAC";
			var npbo = new ChiefExportConsolIntegrationNPBO(eacMessage);
			entry.CH_BGMReference = "BGM, yo";
			return npbo;
		}
	}

	class ConsolMessageSenderTests : TestCaseWithFactory
	{
		public void TestSendToCHIEF()
		{
			MawbTestHelper.MakeBadge("ZPE", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", false, false, BadgeDirectionList.Codes.EXP, "GBLHR");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = "12512345678";
			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, shutUp);
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "GBXXX";
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_JS = shipment1.PK;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var me = wrapper.MawbExportHelper;
			me.ME_Profile = "ZPE";
			wrapper.QueryMasterDEC();
			AssertEquals(1, consol.Messages.Count);
		}
	}
}
