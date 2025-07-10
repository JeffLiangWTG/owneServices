using System;
using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	sealed class MonthlyClosingMessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestApplicationCode()
		{
			AssertEquals(EDIMessage.ApplicationCodes.DECustomsAtlasSystem, messageProcessor.ApplicationCode);
		}

		public void TestGetCorrectBranchPK()
		{
			var branch = Factory.New<GlbBranch>();
			var cusReconDeclaration = Factory.New<CusReconDeclaration>();
			cusReconDeclaration.CRD_GB_Branch = branch.PK;
			AssertEquals(branch.PK, messageProcessor.GetCorrectBranchPK(cusReconDeclaration));
		}

		public void TestGetCorrectBranchPK_LinkedObjectNull()
		{
			AssertEquals(ZGuid.Invalid, messageProcessor.GetCorrectBranchPK(null));
		}

		public void TestGetAttachedDocuments()
		{
			var message = Factory.New<AtlasInboundEDIMessage<IDataProvider>>();
			message.AttachedDocuments.Add(new AttachedDocument());
			message.AttachedDocuments.Add(new AttachedDocument());
			AssertEquals(2, messageProcessor.GetAttachedDocuments(message).Count);
		}

		public void TestGetMessageIdentifier()
		{
			var dataProviderMock = new Mock<IDataProvider>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("0000000009");

			var messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IDataProvider>>();
			messageMock.Setup(x => x.DataProvider).Returns(dataProviderMock.Object);
			AssertEquals("0000000009", messageProcessor.GetMessageIdentifier(messageMock.Object));
		}

		public void TestGetMessageIdentifier_DataProviderNull()
		{
			var message = Factory.New<AtlasInboundEDIMessage<IDataProvider>>();
			AssertEquals(ZString.Empty, messageProcessor.GetMessageIdentifier(message));
		}

		public void TestGetEmailGroupPK()
		{
			var emailGroupPK = Factory.NewWithValidTestData<GlbGroup>().PK;
			Factory.Save();
			var sendToRegistry = new ImportGroupNotification(Core.Constants.EmailTo.NominatedGroup, emailGroupPK);
			using (DECustomsDataRegistry.Instance.SendImportAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				AssertEquals(emailGroupPK, messageProcessor.GetEmailGroupPK(DECustomsDataRegistry.Instance.SendImportAcknowledgements, GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailGroupPK_Fallback()
		{
			var emailGroupPK = Factory.NewWithValidTestData<GlbGroup>().PK;
			Factory.Save();
			var sendToRegistry = new ImportGroupNotification(Core.Constants.EmailTo.NominatedGroup, emailGroupPK);
			using (DECustomsDataRegistry.Instance.SendImportAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistry))
			{
				AssertEquals(emailGroupPK, messageProcessor.GetEmailGroupPK(DECustomsDataRegistry.Instance.SendImportAcknowledgements, GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailGroupRegistryItem()
		{
			AssertSame(DECustomsDataRegistry.Instance.SendImportAcknowledgements, messageProcessor.GetEmailGroupRegistryItem());
		}

		protected override void SetUp()
		{
			base.SetUp();
			messageProcessor = new MonthlyClosingMessageProcessorForTest(new LoggingInformation());
		}
		MonthlyClosingMessageProcessorForTest messageProcessor;
	}

	class MonthlyClosingMessageProcessorForTest : MonthlyClosingMessageProcessor<AtlasInboundEDIMessage<IDataProvider>, IDataProvider>
	{
		public MonthlyClosingMessageProcessorForTest(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => string.Empty;

		public new ZGuid GetCorrectBranchPK(BusinessObject linkedObject) => base.GetCorrectBranchPK(linkedObject);

		public new List<AttachedDocument> GetAttachedDocuments(AtlasInboundEDIMessage<IDataProvider> message) => base.GetAttachedDocuments(message);

		public new ZString GetMessageIdentifier(AtlasInboundEDIMessage<IDataProvider> message) => base.GetMessageIdentifier(message);

		public new ZGuid GetEmailGroupPK(IRegistryItem registryItem, IGlbBranch branch) => base.GetEmailGroupPK(registryItem, branch);

		public new IRegistryItem GetEmailGroupRegistryItem() => base.GetEmailGroupRegistryItem();

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IDataProvider> message)
		{
		}

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IDataProvider> message) => null;
	}
}
