using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.InterchangeProviders;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	public class GbInterchangeSenderTest : BaseInterchangeSender_Test
	{
		[TestUtcOffset(1, 0, 0)]
		public void TestValidTransmitDateMessageFilter()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_HeldUntilDate = ZDateTime.Now.AddMinutes(-1);
			Factory.Save();
			var sender = new GbInterchangeSenderTestForTest(null);
			var result = Factory.LoadTop1<EDIMessage>(sender.ValidTransmitDateMessageFilter);
			AssertNull(result);

			message.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(-1);
			Factory.Save();
			result = Factory.LoadTop1<EDIMessage>(sender.ValidTransmitDateMessageFilter);
			AssertNotNull(result);
		}
	}

	public class GbInterchangeSenderTestForTest : GbInterchangeSender
	{
		public GbInterchangeSenderTestForTest(ILogger serviceLogger) : base(serviceLogger)
		{
		}

		public new ZQuery ValidTransmitDateMessageFilter => base.ValidTransmitDateMessageFilter;

		public override int NumberOfMessagesPerInterchange => 1;

		public override string ApplicationCode => ZString.Empty;

		public override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection messages)
		{
			return null;
		}

		protected override bool SendInt(EDIInterchange interchange)
		{
			return true;
		}
	}
}
