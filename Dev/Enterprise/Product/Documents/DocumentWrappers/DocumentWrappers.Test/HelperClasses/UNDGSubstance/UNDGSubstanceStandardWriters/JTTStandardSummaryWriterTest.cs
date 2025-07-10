using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static Enterprise.DocumentWrappers.AllRadioactiveComponents;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(JTTStandardSummaryWriter))]
	sealed class JTTStandardSummaryWriterTest : UNDGStandardSummaryWriterTest
	{
		public override IUNDGStandardSummaryWriter GetWriter() => new JTTStandardSummaryWriter();

		public override IList<IUNDGSummaryWriterComponent> GetExpectedComponents()
		{
			return new IUNDGSummaryWriterComponent[]
			{
				new UNNOComponent(),
				new JTTChinesePSNComponent(),
				new ClassComponent(),
				new PackingGroupComponent(),
				new FlashPointComponent(),
				new JTTExceptedQuantityComponent(),
				new MarinePollutantComponent(),
				new LimitedQuantityComponent(),
			}.Concat(GetRadioactiveComponents()).ToArray();
		}
	}
}
