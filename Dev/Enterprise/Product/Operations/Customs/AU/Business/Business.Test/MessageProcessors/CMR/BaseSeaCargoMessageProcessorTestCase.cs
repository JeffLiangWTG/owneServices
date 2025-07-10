using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class BaseSeaCargoMessageProcessorTestCase : TestCaseWithFactory
	{
		public void TestEmailGroups()
		{
			var seaCargoAcknowledgementEmailGroup = Guid.NewGuid();
			var seaCargoImpedimentEmailGroup = Guid.NewGuid();
			var seaCargoErrorEmailGroup = Guid.NewGuid();
			var seaCargoSendAcknowledgementsMode = Core.Constants.EmailTo.NoEmails;
			var seaCargoSendImpedimentsMode = Core.Constants.EmailTo.NominatedGroup;
			var seaCargoSendErrorMode = Core.Constants.EmailTo.StaffMember;
			var processor = new SEAOUTRMessageProcessorForTest(new LoggingInformation());
			Env.Registry.AUCustoms.SeaCargoSendAcknowledgements = seaCargoSendAcknowledgementsMode;
			Env.Registry.AUCustoms.SeaCargoSendAcknowledgementsToGroup = seaCargoAcknowledgementEmailGroup;
			Env.Registry.AUCustoms.SeaCargoSendErrors = seaCargoSendErrorMode;
			Env.Registry.AUCustoms.SeaCargoSendErrorsToGroup = seaCargoErrorEmailGroup;
			Env.Registry.AUCustoms.SeaCargoSendImpediments = seaCargoSendImpedimentsMode;
			Env.Registry.AUCustoms.SeaCargoSendImpedimentsToGroup = seaCargoImpedimentEmailGroup;
			AssertEquals("Acknowledgement Mode", seaCargoSendAcknowledgementsMode, processor.AcknowledgementEmailMode);
			AssertEquals("Acknowledgement Email Group", seaCargoAcknowledgementEmailGroup, processor.AcknowledgementEmailGroup);
			AssertEquals("Impediments Mode", seaCargoSendImpedimentsMode, processor.ImpedimentEmailMode);
			AssertEquals("Impediments Email Group", seaCargoImpedimentEmailGroup, processor.ImpedimentEmailGroup);
			AssertEquals("Error Mode", seaCargoSendErrorMode, processor.ErrorEmailMode);
			AssertEquals("Error Email Group", seaCargoErrorEmailGroup, processor.ErrorEmailGroup);
		}
	}

	class SEAOUTRMessageProcessorForTest : SEAOUTRMessageProcessor
	{
		public SEAOUTRMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		new internal ZGuid AcknowledgementEmailGroup => base.AcknowledgementEmailGroup;
		new internal ZString AcknowledgementEmailMode => base.AcknowledgementEmailMode;
		new internal ZGuid ImpedimentEmailGroup => base.ImpedimentEmailGroup;
		new internal ZString ImpedimentEmailMode => base.ImpedimentEmailMode;
		new internal ZGuid ErrorEmailGroup => base.ErrorEmailGroup;
		new internal ZString ErrorEmailMode => base.ErrorEmailMode;
	}
}
