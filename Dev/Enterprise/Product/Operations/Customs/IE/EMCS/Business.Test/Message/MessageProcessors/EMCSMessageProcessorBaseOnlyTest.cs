using System;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.EU.EMCS.Registry;
using Enterprise.Customs.IE.EMCS.Messaging.Phase4_1;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	sealed class EMCSMessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertNoExceptionThrown(() => new EMCSMessageProcessorForTest(null));
		}

		public void TestGetDeclarationFromEADNumber()
		{
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			var cusEntryNumber = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Ireland);
			cusEntryNumber.CE_EntryNum = "MRN98761234";
			cusEntryNumber.CE_EntryLineReference = "1";
			Factory.Save();

			var declaration2 = Factory.New<EMCSJobDeclaration>();
			declaration2.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			var cusEntryNumber2 = CusEntryNumber.New(declaration2, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Ireland);
			cusEntryNumber2.CE_EntryNum = "MRN98761234";
			cusEntryNumber2.CE_EntryLineReference = "1";
			Factory.Save();
			AssertEquals(declaration2, emcsMessageProcessor.GetDeclarationFromEADNumber(message, "MRN98761234", "1"));
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
			message = Factory.New<EMCSInboundEDIMessage>();
			sendToRegistry = new EmcsGroupNotification(Core.Constants.EmailTo.NominatedGroup, emailGroupPK);
		}

		EMCSMessageProcessorForTest emcsMessageProcessor;
		EMCSJobDeclaration declaration;
		EMCSInboundEDIMessage message;
		EmcsGroupNotification sendToRegistry;

		sealed class EMCSMessageProcessorForTest : EMCSMessageProcessor<IE801Provider>
		{
			internal EMCSMessageProcessorForTest(LoggingInformation logger)
				: base(logger, typeof(Ie801Type))
			{
			}

			protected override string MessageFriendlyNameCore => ZString.Empty;

			public void SetLinkedEMCSDeclaration(EMCSJobDeclaration emcsDeclaration)
			{
				base.linkedEMCSDeclaration = emcsDeclaration;
			}

			public new BusinessObject GetLinkedObject(BusinessObjectFactory factory, EMCSInboundEDIMessage message) => null;

			protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IE801Provider provider)
			{
			}

			public new EMCSJobDeclaration GetDeclarationFromEADNumber(EMCSInboundEDIMessage message, ZString eadNumber, ZString sequenceNumber) => base.GetDeclarationFromEADNumber(message, eadNumber, sequenceNumber);

			public new ZGuid GetBranchPk(BusinessObject linkedObject) => base.GetBranchPk(linkedObject);

			public new IRegistryItem GetEmailGroupRegistryItem() => base.GetEmailGroupRegistryItem();
			public new ZGuid GetEmailGroupPK(IRegistryItem registryItem, IGlbBranch branch) => base.GetEmailGroupPK(registryItem, branch);

			public new ZString GetEmailSendMode(IGlbBranch branch) => base.GetEmailSendMode(branch);
		}
	}
}
