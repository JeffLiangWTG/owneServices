using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class BaseAirCargoMessageProcessorTest : TestCaseWithFactory
	{
		public void TestEmailGroups()
		{
			BaseAirCargoMessageProcessorForTest processor = new BaseAirCargoMessageProcessorForTest(new LoggingInformation(), "BLAH", "CuckooSqueaker");
			AssertEquals(Env.Registry.AUCustoms.AirCargoSendAcknowledgementsToGroup, processor.AcknowledgementEmailGroup);
			AssertEquals(Env.Registry.AUCustoms.AirCargoSendAcknowledgements, processor.AcknowledgementEmailMode);
			AssertEquals(Env.Registry.AUCustoms.AirCargoSendImpedimentsToGroup, processor.ImpedimentEmailGroup);
			AssertEquals(Env.Registry.AUCustoms.AirCargoSendImpediments, processor.ImpedimentEmailMode);
			AssertEquals(Env.Registry.AUCustoms.AirCargoSendErrorsToGroup, processor.ErrorEmailGroup);
			AssertEquals(Env.Registry.AUCustoms.AirCargoSendErrors, processor.ErrorEmailMode);
		}
	}

	class BaseAirCargoMessageProcessorForTest : BaseAirCargoMessageProcessor
	{
		public BaseAirCargoMessageProcessorForTest(LoggingInformation logger, ZString messageCode, ZString messageName) : base(logger, messageCode, messageName)
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
