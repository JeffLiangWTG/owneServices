using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	class GVMSInterchangeProviderTest : InterchangeProviderTestCase
	{
		[TestDate(2015, 10, 30, 09, 36, 0)]
		public override void TestMessagesPopulateNewInterchange()
		{
			GVMSMessageSenderTestHelper.TestProcess(null, manifest =>
			{
				var messages = new NonDependentEDIMessageCollection(manifest.Factory);
				messages.AddRange(manifest.Messages);
				return ((GVMSInterchangeProvider)GetInterchangeProvider(messages)).Interchanges;
			});
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new GVMSInterchangeProvider(new LoggingInformation(), collection);
		}
	}
}

