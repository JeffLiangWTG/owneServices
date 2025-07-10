using System;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.EU.EMCS.Registry;
using Enterprise.Customs.GB.EMCS.Messaging.Version4_1;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	sealed class EMCSMessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertNoExceptionThrown(() => new EMCSMessageProcessorForTest(null));
		}

		public void TestGetEmailGroupRegistryItem_Consignor()
		{
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			emcsMessageProcessor.SetLinkedEMCSDeclaration(declaration);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				AssertEquals(sendToRegistry.SendGroupPK, emcsMessageProcessor.GetEmailGroupPK(EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements, GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailGroupRegistryItem_Consignor_FallBack()
		{
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			emcsMessageProcessor.SetLinkedEMCSDeclaration(declaration);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistry))
			{
				AssertEquals(sendToRegistry.SendGroupPK, emcsMessageProcessor.GetEmailGroupPK(EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements, GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailGroupRegistryItem_Consignee()
		{
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			emcsMessageProcessor.SetLinkedEMCSDeclaration(declaration);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				AssertEquals(sendToRegistry.SendGroupPK, emcsMessageProcessor.GetEmailGroupPK(EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements, GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailGroupRegistryItem_Consignee_Fallback()
		{
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			emcsMessageProcessor.SetLinkedEMCSDeclaration(declaration);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistry))
			{
				AssertEquals(sendToRegistry.SendGroupPK, emcsMessageProcessor.GetEmailGroupPK(EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements, GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailSendMode_Consignor()
		{
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			emcsMessageProcessor.SetLinkedEMCSDeclaration(declaration);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				AssertEquals(Core.Constants.EmailTo.NominatedGroup, emcsMessageProcessor.GetEmailSendMode(GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailSendMode_Consignor_Fallback()
		{
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			emcsMessageProcessor.SetLinkedEMCSDeclaration(declaration);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistry))
			{
				AssertEquals(Core.Constants.EmailTo.NominatedGroup, emcsMessageProcessor.GetEmailSendMode(GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailSendMode_Consignee()
		{
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			emcsMessageProcessor.SetLinkedEMCSDeclaration(declaration);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				AssertEquals(Core.Constants.EmailTo.NominatedGroup, emcsMessageProcessor.GetEmailSendMode(GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailSendMode_Consignee_Fallback()
		{
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			emcsMessageProcessor.SetLinkedEMCSDeclaration(declaration);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistry))
			{
				AssertEquals(Core.Constants.EmailTo.NominatedGroup, emcsMessageProcessor.GetEmailSendMode(GlbBranch.CurrentBranch));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var emailGroupPK = Factory.NewWithValidTestData<GlbGroup>().PK;
			Factory.Save();
			emcsMessageProcessor = new EMCSMessageProcessorForTest(new LoggingInformation());
			declaration = Factory.New<EMCSJobDeclaration>();
			sendToRegistry = new EmcsGroupNotification(Core.Constants.EmailTo.NominatedGroup, emailGroupPK);
		}

		EMCSMessageProcessorForTest emcsMessageProcessor;
		EMCSJobDeclaration declaration;
		EmcsGroupNotification sendToRegistry;

		sealed class EMCSMessageProcessorForTest : EMCSMessageProcessor<IE801Provider>
		{
			internal EMCSMessageProcessorForTest(LoggingInformation logger) : base(logger, typeof(Ie801Type)) { }

			protected override string MessageFriendlyNameCore => ZString.Empty;

			internal void SetLinkedEMCSDeclaration(EMCSJobDeclaration emcsDeclaration)
			{
				linkedEMCSDeclaration = emcsDeclaration;
			}

			internal new BusinessObject GetLinkedObject(BusinessObjectFactory factory, EMCSInboundEDIMessage message) => null;

			internal new ZGuid GetBranchPk(BusinessObject linkedObject) => base.GetBranchPk(linkedObject);

			internal new IRegistryItem GetEmailGroupRegistryItem() => base.GetEmailGroupRegistryItem();

			internal new ZGuid GetEmailGroupPK(IRegistryItem registryItem, IGlbBranch branch) => base.GetEmailGroupPK(registryItem, branch);

			internal new ZString GetEmailSendMode(IGlbBranch branch) => base.GetEmailSendMode(branch);

			protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IE801Provider provider) { }
		}
	}
}
