using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC;
using Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class DeltaGMessageBuilderManagerTest : TestCaseWithFactory
	{
		public void TestGetDeltaCImportMessage()
		{
			var deltaHelper = new DeltaMessageSenderHelper();

			var entry = deltaHelper.CreateEntryDeclarationForTest(true);
			entry.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			entry.Declaration.JE_CustomsProfile = "DG1";

			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			sendingObject.MessageType = EntryActionCodeList.Codes.ANT;
			var errCollector = new EU.Business.ErrorCollector();
			var messageBuilderManager = new DeltaGMessageBuilderManager(errCollector);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			var antMessage = messageBuilder.GetMessage();

			AssertNotContains("<MetaData>", antMessage);
			AssertContains("<Entete><codact>1</codact>", antMessage);
		}

		public void TestGetDeltaDImportMessage()
		{
			var deltaHelper = new DeltaMessageSenderHelper();

			var entry = deltaHelper.CreateEntryDeclarationForTest(false);
			entry.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			entry.Declaration.JE_CustomsProfile = "DG2";

			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.ANT;
			var errCollector = new EU.Business.ErrorCollector();
			var messageBuilderManager = new DeltaGMessageBuilderManager(errCollector);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			var antMessage = messageBuilder.GetMessage();

			AssertNotContains("<MetaData>", antMessage);
			AssertContains("<Entete><codact>3</codact>", antMessage);
		}

		public void TestGetDeltaCImportTypeofBuilder()
		{
			var deltaHelper = new DeltaMessageSenderHelper();

			var entry = deltaHelper.CreateEntryDeclarationForTest(true);
			entry.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			entry.Declaration.JE_CustomsProfile = "DG1";

			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.VAL;
			var errCollector = new EU.Business.ErrorCollector();
			var messageBuilderManager = new DeltaGMessageBuilderManager(errCollector);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCVALSendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.ANT;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCANTSendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.RPS;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCRPSSendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.VAA;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCVAASendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.REC;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCRECSendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.INV;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCINVSendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.MAP;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCMAPSendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.ANA;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCANASendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.EAV;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCEAVSendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.CMP;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCCMPSendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.ANR;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCANRSendImpMessageBuilder>(messageBuilder);
		}

		public void TestGetDeltaCExportTypeofBuilder()
		{
			var deltaHelper = new DeltaMessageSenderHelper();

			var entry = deltaHelper.CreateEntryDeclarationForTest(true);
			entry.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entry.Declaration.JE_CustomsProfile = "DG1";
			entry.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;

			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.VAL;
			var errCollector = new EU.Business.ErrorCollector();
			var messageBuilderManager = new DeltaGMessageBuilderManager(errCollector);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCVALSendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.ANT;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCANTSendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.RPS;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCRPSSendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.VAA;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCVAASendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.REC;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCRECSendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.INV;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCINVSendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.MAP;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCMAPSendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.ANA;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCANASendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.EAV;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCEAVSendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.CMP;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCCMPSendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.ANR;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DCANRSendExpMessageBuilder>(messageBuilder);
		}

		public void TestGetDeltaDImportTypeofBuilder()
		{
			var deltaHelper = new DeltaMessageSenderHelper();

			var entry = deltaHelper.CreateEntryDeclarationForTest(false);
			entry.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			entry.Declaration.JE_CustomsProfile = "DG2";

			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.VAL;
			var errCollector = new EU.Business.ErrorCollector();
			var messageBuilderManager = new DeltaGMessageBuilderManager(errCollector);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDVALSendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.D2M;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDD2MSendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.ANT;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDANTSendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.MDV;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDMDVSendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.RPS;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDRPSSendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.MDA;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDMDASendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.VAA;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDVAASendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.ANN;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDANNSendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.REC;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDRECSendImpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.INV;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDINVSendImpMessageBuilder>(messageBuilder);
		}

		public void TestGetDeltaDExportTypeofBuilder()
		{
			var deltaHelper = new DeltaMessageSenderHelper();

			var entry = deltaHelper.CreateEntryDeclarationForTest(false);
			entry.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entry.Declaration.JE_CustomsProfile = "DG2";
			entry.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;

			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.VAL;
			var errCollector = new EU.Business.ErrorCollector();
			var messageBuilderManager = new DeltaGMessageBuilderManager(errCollector);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDVALSendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.D2M;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDD2MSendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.ANT;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDANTSendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.MDV;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDMDVSendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.RPS;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDRPSSendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.MDA;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDMDASendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.VAA;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDVAASendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.ANN;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDANNSendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.REC;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDRECSendExpMessageBuilder>(messageBuilder);

			sendingObject.MessageType = FR.Business.EntryActionCodeList.Codes.INV;
			messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			AssertType<DDINVSendExpMessageBuilder>(messageBuilder);
		}
	}
}
