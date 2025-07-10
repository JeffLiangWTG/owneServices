using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class BaseUnderbondMessageProcessorTestCase : TestCaseWithFactory
	{
		public void TestEmailGroups()
		{
			var underbondAcknowledgementEmailGroup = Guid.NewGuid();
			var underbondImpedimentEmailGroup = Guid.NewGuid();
			var underbondErrorEmailGroup = Guid.NewGuid();
			var underbondSendAcknowledgementsMode = Enterprise.Core.Constants.EmailTo.NoEmails;
			var underbondSendImpedimentsMode = Enterprise.Core.Constants.EmailTo.NominatedGroup;
			var underbondSendErrorMode = Enterprise.Core.Constants.EmailTo.StaffMember;
			var processor = new UBMREQEMessageProcessorForTest(new LoggingInformation());
			Env.Registry.AUCustoms.UnderbondSendAcknowledgements = underbondSendAcknowledgementsMode;
			Env.Registry.AUCustoms.UnderbondSendAcknowledgementsToGroup = underbondAcknowledgementEmailGroup;
			Env.Registry.AUCustoms.UnderbondSendErrors = underbondSendErrorMode;
			Env.Registry.AUCustoms.UnderbondSendErrorsToGroup = underbondErrorEmailGroup;
			Env.Registry.AUCustoms.UnderbondSendImpediments = underbondSendImpedimentsMode;
			Env.Registry.AUCustoms.UnderbondSendImpedimentsToGroup = underbondImpedimentEmailGroup;
			AssertEquals("Acknowledgement Mode", underbondSendAcknowledgementsMode, processor.AcknowledgementEmailMode);
			AssertEquals("Acknowledgement Email Group", underbondAcknowledgementEmailGroup, processor.AcknowledgementEmailGroup);
			AssertEquals("Impediments Mode", underbondSendImpedimentsMode, processor.ImpedimentEmailMode);
			AssertEquals("Impediments Email Group", underbondImpedimentEmailGroup, processor.ImpedimentEmailGroup);
			AssertEquals("Error Mode", underbondSendErrorMode, processor.ErrorEmailMode);
			AssertEquals("Error Email Group", underbondErrorEmailGroup, processor.ErrorEmailGroup);
		}
	}

	class UBMREQEMessageProcessorForTest : UBMREQEMessageProcessor
	{
		public UBMREQEMessageProcessorForTest(LoggingInformation logger) : base(logger)
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
