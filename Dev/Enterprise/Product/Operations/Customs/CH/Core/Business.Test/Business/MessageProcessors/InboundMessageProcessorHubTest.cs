using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(InboundMessageProcessorHub))]
sealed class InboundMessageProcessorHubTest : TestCaseWithFactory
{
	public void TestGetMessageProcessors() => CombineAssertions(() =>
	{
		const int edecProcessorCount = 15;
		const int passarAndCharteraProcessorCount = 24;

		AssertMessageProcessor(false, edecProcessorCount);

		GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).TokenCredentialsEnabled = true;
		AssertMessageProcessor(true, edecProcessorCount + passarAndCharteraProcessorCount);

		void AssertMessageProcessor(bool isTokenCredentialsEnabled, int expectedProcessorCount)
		{
			var processorHub = new InboundMessageProcessorHubForTesting();
			var processors = processorHub.GetMessageProcessors();

			AssertContains<EdecRuleErrorMessageProcessor>();
			AssertContains<EdecAcceptanceMessageProcessor>();
			AssertContains<EdecXmlSchemaErrorMessageProcessor>();
			AssertContains<DocumentMessageProcessor>();
			AssertContains<EdecStatusMessageProcessor>();
			AssertContains<EdecCustomsRejectionMessageProcessor>();
			AssertContains<EbdDocumentImportResponseMessageProcessor>();
			AssertContains<EComResponseMessageProcessor>();
			AssertContains<EComRequestMessageProcessor>();
			AssertContains<EvvDocumentResponseMessageProcessor>();
			AssertContains<BordereauListResponseMessageProcessor>();
			AssertContains<BordereauResponseMessageProcessor>();
			AssertContains<XtErrorResponseMessageProcessor>();
			AssertContains<BordereauErrorResponseMessageProcessor>();

			if (isTokenCredentialsEnabled)
			{
				AssertContains<TokenRefreshMessageProcessor>();
				AssertContains<PassarMessageListAcceptanceMessageProcessor>();
				AssertContains<PassarMessageListRejectionMessageProcessor>();
				AssertContains<PassarGetMessageAcknowledgeMessageProcessor>();
				AssertContains<PassarGetMessageRejectionMessageProcessor>();
				AssertContains<CharteraOutputGetMessageRejectionMessageProcessor>();
				AssertContains<CharteraOutputMessageListAcceptanceMessageProcessor>();
				AssertContains<CharteraOutputMessageListRejectionMessageProcessor>();
				AssertContains<CharteraOutputDocumentSearchResultMessageProcessor>();
				AssertContains<CharteraOutputDocumentDeliveryResultMessageProcessor>();
				AssertContains<CharteraOutputDocumentRejectionMessageProcessor>();
				AssertContains<CharteraOutputErrorMessageProcessor>();
				AssertContains<CharteraOutputAcknowledgeMessageProcessor>();
				AssertContains<PassarExportMessageProcessor>();
				AssertContains<NC084ResponseMessageProcessor>();
				AssertContains<NE004ResponseMessageProcessor>();
				AssertContains<NE009ResponseMessageProcessor>();
				AssertContains<NE021ResponseMessageProcessor>();
				AssertContains<NE028ResponseMessageProcessor>();
				AssertContains<NE029ResponseMessageProcessor>();
				AssertContains<NE060ResponseMessageProcessor>();
				AssertContains<NE083ResponseMessageProcessor>();
				AssertContains<NE096ResponseMessageProcessor>();
				AssertContains<NE131ResponseMessageProcessor>();
				expectedProcessorCount += processorHub.NctsProcessorsCount;
			}

			AssertEquals($"isTokenCredentialsEnabled={isTokenCredentialsEnabled} - Processor count", expectedProcessorCount, processors.Count);

			void AssertContains<T>() => AssertEquals($"isTokenCredentialsEnabled={isTokenCredentialsEnabled} - Expected type: {typeof(T).Name}", 1, processors.Count(p => p is T));
		}
	});

	public void TestMessageProcessorsAreUnique()
	{
		GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).TokenCredentialsEnabled = true;
		var processorHub = new InboundMessageProcessorHubForTesting();
		var processors = processorHub.GetMessageProcessors().Cast<BaseMessageProcessor>().ToArray();
		for (var p1 = 0; p1 < processors.Length - 1; p1++)
		{
			for (var p2 = p1 + 1; p2 < processors.Length; p2++)
			{
				if (processors[p1].ApplicationCode == processors[p2].ApplicationCode
					 && processors[p1].MessageTypesToInclude.Intersect(processors[p2].MessageTypesToInclude).Any()
					 && (processors[p1].MessageSubTypesToInclude.IsNullOrEmpty() || processors[p2].MessageSubTypesToInclude.IsNullOrEmpty()
						|| processors[p1].MessageSubTypesToInclude.Intersect(processors[p2].MessageSubTypesToInclude).Any()))
				{
					Fail($"Ambigous message processors: {processors[p1].GetType().Name} {processors[p2].GetType().Name}");
				}
			}
		}
		Assert("Test is not empty", true);
	}

	public void TestGetMessageProcessors_ShouldReturnProcessorsFromNctsMessageProcessorsProvider()
	{
		GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).TokenCredentialsEnabled = true;

		var messageProcessor = new Mock<BranchCustomsApplicationTypeMessageProcessor>(new LoggingInformation());
		var nctsMessageProcessorsProvider = Mock.Of<ICHNctsMessageProcessorsProvider>(m => m.GetMessageProcessors(It.IsAny<LoggingInformation>()) == new[] { messageProcessor.Object });
		using (ObjectFactory.Substitute(nctsMessageProcessorsProvider))
		{
			var inboundMessageProcessorHub = new InboundMessageProcessorHubForTesting();
			var messageProcessors = inboundMessageProcessorHub.GetMessageProcessors();
			AssertCollectionContains(messageProcessor.Object, messageProcessors);
		}
	}
}

class InboundMessageProcessorHubForTesting : InboundMessageProcessorHub
{
	public InboundMessageProcessorHubForTesting() : base(Enumerable.Empty<ZString>(), Enumerable.Empty<ZString>())
	{
	}

	public new List<ApplicationTypeMessageProcessor> GetMessageProcessors() => base.GetMessageProcessors();

	public int NctsProcessorsCount => ObjectFactory.Get<ICHNctsMessageProcessorsProvider>().GetMessageProcessors(new LoggingInformation()).Count();
}
