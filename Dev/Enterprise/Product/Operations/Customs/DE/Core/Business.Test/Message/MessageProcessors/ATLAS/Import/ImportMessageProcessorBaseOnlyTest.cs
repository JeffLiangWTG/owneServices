using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class ImportMessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestGetEmailGroupRegistryItem()
		{
			AssertSame(DECustomsDataRegistry.Instance.SendImportAcknowledgements, importMessageProcessor.GetEmailGroupRegistryItem());
		}

		public void TestGetEmailGroupPK()
		{
			using (DECustomsDataRegistry.Instance.SendImportAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				AssertEquals(sendToRegistry.SendGroupPK, importMessageProcessor.GetEmailGroupPK(DECustomsDataRegistry.Instance.SendImportAcknowledgements, GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailGroupPK_Fallback()
		{
			using (DECustomsDataRegistry.Instance.SendImportAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistry))
			{
				AssertEquals(sendToRegistry.SendGroupPK, importMessageProcessor.GetEmailGroupPK(DECustomsDataRegistry.Instance.SendImportAcknowledgements, GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailSendMode()
		{
			using (DECustomsDataRegistry.Instance.SendImportAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				AssertEquals(Core.Constants.EmailTo.NominatedGroup, importMessageProcessor.GetEmailSendMode(GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailSendMode_Consignor_Fallback()
		{
			using (DECustomsDataRegistry.Instance.SendImportAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistry))
			{
				AssertEquals(Core.Constants.EmailTo.NominatedGroup, importMessageProcessor.GetEmailSendMode(GlbBranch.CurrentBranch));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var emailGroupPK = Factory.NewWithValidTestData<GlbGroup>().PK;
			Factory.Save();
			importMessageProcessor = new ImportMessageProcessorForTest(new LoggingInformation());
			sendToRegistry = new ImportGroupNotification(Core.Constants.EmailTo.NominatedGroup, emailGroupPK);
		}
		ImportMessageProcessorForTest importMessageProcessor;
		ImportGroupNotification sendToRegistry;
	}

	class ImportMessageProcessorForTest : ImportMessageProcessor<AtlasInboundEDIMessage<IDataProvider>, IDataProvider>
	{
		public ImportMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => ZString.Empty;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IDataProvider> message)
		{
		}

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IDataProvider> message) => null;
		
		public new ZGuid GetEmailGroupPK(IRegistryItem registryItem, IGlbBranch branch) => base.GetEmailGroupPK(registryItem, branch);

		public new ZString GetEmailSendMode(IGlbBranch branch) => base.GetEmailSendMode(branch);

		public new IRegistryItem GetEmailGroupRegistryItem() => base.GetEmailGroupRegistryItem();
	}
}
