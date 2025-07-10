using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	class CustomsManifestStatusMessageProcessorTestCase : TestCaseWithFactory
	{
		public void TestGetEmailRegistryValues_eManifest()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			house.BW_MessageReference = "CAH0000004";

			var processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, master);
			AssertEquals("newGroupMM", newGroupMM.PK, processor.NotifyEmailGroup_Exposed);
			AssertEquals("email Mode", Core.Constants.EmailTo.NominatedGroup, processor.NotifyEmailMode_Exposed);

			processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, house);
			AssertEquals("newGroupMM", newGroupMM.PK, processor.NotifyEmailGroup_Exposed);
			AssertEquals("email Mode", Core.Constants.EmailTo.NominatedGroup, processor.NotifyEmailMode_Exposed);
		}

		public void TestGetEmailRegistryValues_Declaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "Job001";
			CusEntryNumber.New(declaration, CusEntryNumber.EntryType.CATransactionNumber, Core.Constants.CountryCodes.Canada);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "12345000000011";
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			var processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, declaration);
			AssertEquals("newGroupNN", newGroupNN.PK, processor.NotifyEmailGroup_Exposed);
			AssertEquals("email Mode", Core.Constants.EmailTo.StaffMember, processor.NotifyEmailMode_Exposed);

			processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, entryHeader);
			AssertEquals("newGroupNN", newGroupNN.PK, processor.NotifyEmailGroup_Exposed);
			AssertEquals("email Mode", Core.Constants.EmailTo.StaffMember, processor.NotifyEmailMode_Exposed);
		}

		public void TestGetAssociatedBusinessObjectDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "Job001";
			CusEntryNumber.New(declaration, CusEntryNumber.EntryType.CATransactionNumber, Core.Constants.CountryCodes.Canada);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "12345000000011";
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			var expectedAssociatedBusinessObjectDescription = "A status update message has been received from the CBSA for a(n) Test Declaration.";
			var house = Factory.New<CusCAeMHHouse>();
			var message = Factory.New<UniversalEventMessage>();
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new CustomsManifestStatusMessageProcessorForTesting(logger, universalEvent, message, declaration);
			AssertEquals("GetAssociatedBusinessObjectDescription", expectedAssociatedBusinessObjectDescription, processor.AssociatedBusinessObjectDescription_Exposed);
		}

		protected class CustomsManifestStatusMessageProcessorForTesting : CustomsManifestStatusMessageProcessor
		{
			public CustomsManifestStatusMessageProcessorForTesting(IXmlSessionTracker logger, UniversalEvent universalEvent, UniversalEventMessage message, BusinessObject businessObject)
				: base(logger, universalEvent, message, businessObject)
			{ }

			public ZGuid NotifyEmailGroup_Exposed => base.NotifyEmailGroup;

			public ZString NotifyEmailMode_Exposed => base.NotifyEmailMode;

			public ZString AssociatedBusinessObjectDescription_Exposed => base.GetAssociatedBusinessObjectDescription();
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			message = Factory.New<UniversalEventMessage>();
			universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();

			newGroupMM = Factory.New<GlbGroup>();
			newGroupMM.GG_Code = "MM1";

			newGroupNN = Factory.New<GlbGroup>();
			newGroupNN.GG_Code = "NN1";

			CACustomsDataRegistry.Instance.SendeManifestForwarderMessageAcknowledgementsAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);
			CACustomsDataRegistry.Instance.SendeManifestForwarderMessageAcknowledgementsToGroupAppliesAllCountries.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newGroupMM.PK.ToGuid());

			CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgements.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMember);
			CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newGroupNN.PK.ToGuid());
		}

		GlbGroup newGroupMM;
		GlbGroup newGroupNN;
		IXmlSessionTracker logger;
		UniversalEventMessage message;
		UniversalEvent universalEvent;
	}
}
