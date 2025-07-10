using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSIncomingMessageProcessorTest : TestCase
	{
		// todo: this test is no longer needed
		public void TestOrderAndHint()
		{
			var processor = new CDSIncomingMessageProcessorForTest();
			var query = processor.GetMessageProcessorQueryExposed();
			var hint = query.TableIndexHints.Single();

			AssertEquals("EM_MessageNum, EM_SystemCreateTimeUtc", query.OrderBy);
			AssertEquals("NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc", hint.IndexName);
		}

		public void TestAvailableMessageProcessors()
		{
			CombineAssertions(() =>
			{
				var processors = new CDSIncomingMessageProcessorForTest().MessageProcessorsExposed;
				AssertEquals("Message processors", 9, processors.Count);
				AssertNotNull(nameof(CDSResponseMessageProcessor), processors.OfType<CDSResponseMessageProcessor>().SingleOrDefault());
				AssertNotNull(nameof(CDSInventoryLinkingControlResponseMessageProcessor), processors.OfType<CDSInventoryLinkingControlResponseMessageProcessor>().SingleOrDefault());
				AssertNotNull(nameof(CDSInventoryLinkingMovementResponseMessageProcessor), processors.OfType<CDSInventoryLinkingMovementResponseMessageProcessor>().SingleOrDefault());
				AssertNotNull(nameof(CDSInventoryLinkingMovementTotalsMessageProcessor), processors.OfType<CDSInventoryLinkingMovementTotalsMessageProcessor>().SingleOrDefault());
				AssertNotNull(nameof(CDSInventoryLinkingQueryResponseMessageProcessor), processors.OfType<CDSInventoryLinkingQueryResponseMessageProcessor>().SingleOrDefault());
				AssertNotNull(nameof(CDSErrorResponseMessageProcessor), processors.OfType<CDSErrorResponseMessageProcessor>().SingleOrDefault());
				AssertNotNull(nameof(CDSSynchronousResponseMessageProcessor), processors.OfType<CDSSynchronousResponseMessageProcessor>().SingleOrDefault());
				AssertNotNull(nameof(CDSDeclarationInfoResponseMessageProcessor), processors.OfType<CDSDeclarationInfoResponseMessageProcessor>().SingleOrDefault());
				AssertNotNull(nameof(CDSDocumentUploadConfirmationMessageProcessor), processors.OfType<CDSDocumentUploadConfirmationMessageProcessor>().SingleOrDefault());
			});
		}

		class CDSIncomingMessageProcessorForTest : CDSIncomingMessageProcessor
		{
			public CDSIncomingMessageProcessorForTest() : base(new LoggingInformation())
			{
			}

			public ZQuery GetMessageProcessorQueryExposed() => GetMessageProcessorQuery();

			public List<ApplicationTypeMessageProcessor> MessageProcessorsExposed => MessageProcessors;
		}
	}
}
