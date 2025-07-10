using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Testing
{
	public class ICSInterchangeProviderTest : InterchangeProviderTestCase
	{
		[TestDate(2015, 10, 30, 09, 36, 0)]
		public override void TestMessagesPopulateNewInterchange()
		{
			ICSMessageSenderTestHelper.TestProcess(null, manifest =>
			{
				var messages = new NonDependentEDIMessageCollection(manifest.Factory);
				messages.AddRange(manifest.Messages);
				return ((ICSInterchangeProvider)GetInterchangeProvider(messages)).Interchanges;
			});
		}
		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new ICSInterchangeProvider(new LoggingInformation(), collection);
		}
	}
}
