using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.Edifact.Auto;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders.Testing
{
	[TestsSubclassesOf(typeof(EDIFACTMessageBuilder<IEDIFACTMessageDataProvider, SegmentGroup>))]
	public abstract class NCTSEDIFACTMessageBuilderTest<TMessageBuilder, TProvider, TSegmentGroup> : EDIFACTMessageBuilderTest<TMessageBuilder, TProvider, TSegmentGroup>
		where TProvider : IEDIFACTMessageDataProvider
		where TSegmentGroup : SegmentGroup
		where TMessageBuilder : EDIFACTMessageBuilder<TProvider, TSegmentGroup>
	{
		protected sealed override TestFileReader TestFileReader => testFileReader ?? (testFileReader = new TestFileReader(typeof(NCTSEDIFACTMessageBuilderTest<,,>)));
		TestFileReader testFileReader;
	}
}
