using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.EU.EMCS.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	sealed class EmcsMessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertNoExceptionThrown(() => new EmcsMessageProcessorForTest(null));
		}

		public void TestLoadFromEADNumberNoMessage()
		{
			AssertNull(emcsMessageProcessor.GetDeclarationFromEADNumber(null, "MRN98761234", EmcsMessageSubTypeList.Codes.Eme, "1"));
		}

		public void TestLoadFromEADNumberParentNotEMCSDeclaration()
		{
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			var cusEntryNumber = CusEntryNumber.New(declaration.InvoiceHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			cusEntryNumber.CE_EntryNum = "MRN98761234";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Entry Number persisted", true, cusEntryNumber.IsInDatabase);
				AssertNull("Incorrect Parent table", emcsMessageProcessor.GetDeclarationFromEADNumber(message, "MRN98761234", EmcsMessageSubTypeList.Codes.Eme, "1"));
			});
		}

		public void TestLoadFromEADNumber_Consignor()
		{
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			var cusEntryNumber = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			cusEntryNumber.CE_EntryNum = "MRN98761234";
			cusEntryNumber.CE_EntryLineReference = "1";
			Factory.Save();

			var declaration2 = Factory.New<EMCSJobDeclaration>();
			declaration2.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			var cusEntryNumber2 = CusEntryNumber.New(declaration2, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			cusEntryNumber2.CE_EntryNum = "MRN98761234";
			cusEntryNumber2.CE_EntryLineReference = "1";
			Factory.Save();
			AssertEquals(declaration2, emcsMessageProcessor.GetDeclarationFromEADNumber(message, "MRN98761234", EmcsMessageSubTypeList.Codes.Eme, "1"));
		}

		public void TestLoadFromEADNumber_Consignee()
		{
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			var cusEntryNumber = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			cusEntryNumber.CE_EntryNum = "MRN98761234";
			cusEntryNumber.CE_EntryLineReference = "1";

			var declaration2 = Factory.New<EMCSJobDeclaration>();
			declaration2.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			var cusEntryNumber2 = CusEntryNumber.New(declaration2, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			cusEntryNumber2.CE_EntryNum = "MRN98761234";
			cusEntryNumber2.CE_EntryLineReference = "1";
			Factory.Save();
			AssertEquals(declaration, emcsMessageProcessor.GetDeclarationFromEADNumber(message, "MRN98761234", EmcsMessageSubTypeList.Codes.Ema, "1"));
		}

		public void TestLoadFromLocalReferenceNoMessage()
		{
			AssertNull(emcsMessageProcessor.GetDeclarationFromLocalReference(null, "B000222547896254786321", EmcsMessageSubTypeList.Codes.Eme));
		}

		public void TestLoadFromLocalReferenceNoReferenceNumber()
		{
			AssertNull(emcsMessageProcessor.GetDeclarationFromLocalReference(message, ZString.Empty, EmcsMessageSubTypeList.Codes.Eme));
		}

		public void TestLoadFromLocalReference_Registry()
		{
			declaration.JE_OwnerRef = "B000222547896254786321";
			Factory.Save();

			AssertEquals(declaration, emcsMessageProcessor.GetDeclarationFromLocalReference(message, "B000222547896254786321", EmcsMessageSubTypeList.Codes.Eme));
		}

		public void TestGetEmailGroupRegistryItem_Consignor()
		{
			emcsMessageProcessor.SetMessageGroup(EmcsMessageSubTypeList.Codes.Eme);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				AssertEquals(sendToRegistry.SendGroupPK, emcsMessageProcessor.GetEmailGroupPK(EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements, GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailGroupRegistryItem_Consignor_FallBack()
		{
			emcsMessageProcessor.SetMessageGroup(EmcsMessageSubTypeList.Codes.Eme);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistry))
			{
				AssertEquals(sendToRegistry.SendGroupPK, emcsMessageProcessor.GetEmailGroupPK(EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements, GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailGroupRegistryItem_Consignee()
		{
			emcsMessageProcessor.SetMessageGroup(EmcsMessageSubTypeList.Codes.Emb);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				AssertEquals(sendToRegistry.SendGroupPK, emcsMessageProcessor.GetEmailGroupPK(EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements, GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailGroupRegistryItem_Consignee_Fallback()
		{
			emcsMessageProcessor.SetMessageGroup(EmcsMessageSubTypeList.Codes.Emb);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistry))
			{
				AssertEquals(sendToRegistry.SendGroupPK, emcsMessageProcessor.GetEmailGroupPK(EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements, GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailSendMode_Consignor()
		{
			emcsMessageProcessor.SetMessageGroup(EmcsMessageSubTypeList.Codes.Eme);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				AssertEquals(Core.Constants.EmailTo.NominatedGroup, emcsMessageProcessor.GetEmailSendMode(GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailSendMode_Consignor_Fallback()
		{
			emcsMessageProcessor.SetMessageGroup(EmcsMessageSubTypeList.Codes.Eme);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistry))
			{
				AssertEquals(Core.Constants.EmailTo.NominatedGroup, emcsMessageProcessor.GetEmailSendMode(GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailSendMode_Consignee()
		{
			emcsMessageProcessor.SetMessageGroup(EmcsMessageSubTypeList.Codes.Emb);
			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				AssertEquals(Core.Constants.EmailTo.NominatedGroup, emcsMessageProcessor.GetEmailSendMode(GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailSendMode_Consignee_Fallback()
		{
			emcsMessageProcessor.SetMessageGroup(EmcsMessageSubTypeList.Codes.Emb);
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
			emcsMessageProcessor = new EmcsMessageProcessorForTest(new LoggingInformation());
			declaration = Factory.New<EMCSJobDeclaration>();
			message = Factory.NewMoq<EmcsInboundEDIMessage<IEmcsDataProvider>>().Object;
			sendToRegistry = new EmcsGroupNotification(Core.Constants.EmailTo.NominatedGroup, emailGroupPK);
		}
		EmcsMessageProcessorForTest emcsMessageProcessor;
		EMCSJobDeclaration declaration;
		EmcsInboundEDIMessage<IEmcsDataProvider> message;
		EmcsGroupNotification sendToRegistry;

		class EmcsMessageProcessorForTest : EmcsMessageProcessor<EmcsInboundEDIMessage<IEmcsDataProvider>, IEmcsDataProvider>
		{
			internal EmcsMessageProcessorForTest(LoggingInformation logger)
				: base(logger)
			{
			}

			protected override string MessageFriendlyNameCore => ZString.Empty;

			public void SetMessageGroup(ZString messageGroup)
			{
				base.messageGroup = messageGroup;
			}

			protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IEmcsDataProvider> message) => null;

			protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IEmcsDataProvider> message)
			{
			}

			public new EMCSJobDeclaration GetDeclarationFromEADNumber(EmcsInboundEDIMessage<IEmcsDataProvider> message, ZString eadNumber, ZString messageGroup, ZString sequenceNumber) => base.GetDeclarationFromEADNumber(message, eadNumber, messageGroup, sequenceNumber);

			public new EMCSJobDeclaration GetDeclarationFromLocalReference(EmcsInboundEDIMessage<IEmcsDataProvider> message, ZString localReferenceNumber, ZString messageGroup) => base.GetDeclarationFromLocalReference(message, localReferenceNumber, messageGroup);

			public new IRegistryItem GetEmailGroupRegistryItem() => base.GetEmailGroupRegistryItem();

			public new ZGuid GetEmailGroupPK(IRegistryItem registryItem, IGlbBranch branch) => base.GetEmailGroupPK(registryItem, branch);

			public new ZString GetEmailSendMode(IGlbBranch branch) => base.GetEmailSendMode(branch);
		}
	}
}
