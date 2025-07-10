using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	class MessageValidationFailedMessageProcessorTestCase : TestCaseWithFactory
	{
		public void TestGetEmailRegistryValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var processor = new MessageValidationFailedMessageProcessorForTesting(logger, universalEvent, message, entry);
			AssertEquals("newGroupMM", newGroupNN.PK, processor.NotifyEmailGroup_Exposed);
			AssertEquals("email Mode", Core.Constants.EmailTo.StaffMemberAndNominatedGroup, processor.NotifyEmailMode_Exposed);
		}

		public void TestGetAssociatedBusinessObjectDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var processor = new MessageValidationFailedMessageProcessorForTesting(logger, universalEvent, message, entry);

			var expectedAssociatedBusinessObjectDescription = "A 'Message Syntax Rejection' response has been received from the CBSA for a(n) Test IID Declaration.";
			AssertEquals("GetAssociatedBusinessObjectDescription", expectedAssociatedBusinessObjectDescription, processor.AssociatedBusinessObjectDescription_Exposed);
		}

		protected class MessageValidationFailedMessageProcessorForTesting : MessageValidationFailedMessageProcessor
		{
			public MessageValidationFailedMessageProcessorForTesting(IXmlSessionTracker logger, UniversalEvent universalEvent, UniversalEventMessage message, CusEntryHeader entry)
				: base(logger, universalEvent, message, entry)
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

			newGroupNN = Factory.New<GlbGroup>();
			newGroupNN.GG_Code = "NN1";

			CACustomsDataRegistry.Instance.SendDeclarationMessageErrors.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			CACustomsDataRegistry.Instance.SendDeclarationMessageErrorsToGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newGroupNN.PK.ToGuid());
		}

		GlbGroup newGroupNN;
		IXmlSessionTracker logger;
		UniversalEventMessage message;
		UniversalEvent universalEvent;
	}
}
