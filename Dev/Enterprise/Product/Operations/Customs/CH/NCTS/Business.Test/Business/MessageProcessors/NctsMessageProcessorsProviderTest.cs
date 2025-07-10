using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsMessageProcessorsProvider))]
sealed class NctsMessageProcessorsProviderTest : TestCaseWithFactory
{
	public void TestGetMessageProcessors()
	{
		var loggingInformation = new LoggingInformation();
		var nctsMessageProcessorsProvider = new NctsMessageProcessorsProvider();

		var messageProcessors = nctsMessageProcessorsProvider.GetMessageProcessors(loggingInformation).ToArray();

		AssertEquals(21, messageProcessors.Length);
		AssertCollectionContainsType<NC124ResponseMessageProcessor>();
		AssertCollectionContainsType<NC909ResponseMessageProcessor>();
		AssertCollectionContainsType<NT008ResponseMessageProcessor>();
		AssertCollectionContainsType<NT009ResponseMessageProcessor>();
		AssertCollectionContainsType<NT019ResponseMessageProcessor>();
		AssertCollectionContainsType<NT021ResponseMessageProcessor>();
		AssertCollectionContainsType<NT025ResponseMessageProcessor>();
		AssertCollectionContainsType<NT029ResponseMessageProcessor>();
		AssertCollectionContainsType<NT035ResponseMessageProcessor>();
		AssertCollectionContainsType<NT043ResponseMessageProcessor>();
		AssertCollectionContainsType<NT045ResponseMessageProcessor>();
		AssertCollectionContainsType<NT055ResponseMessageProcessor>();
		AssertCollectionContainsType<NT057ResponseMessageProcessor>();
		AssertCollectionContainsType<NT060ResponseMessageProcessor>();
		AssertCollectionContainsType<NT061ResponseMessageProcessor>();
		AssertCollectionContainsType<NT140ResponseMessageProcessor>();
		AssertCollectionContainsType<NT146ResponseMessageProcessor>();
		AssertCollectionContainsType<NT182ResponseMessageProcessor>();
		AssertCollectionContainsType<NTx04ResponseMessageProcessor>();
		AssertCollectionContainsType<NTx28ResponseMessageProcessor>();
		AssertCollectionContainsType<PassarNctsMessageProcessor>();

		void AssertCollectionContainsType<T>() => AssertCollectionContains(typeof(T).Name, messageProcessors, p => p is T);
	}
}
