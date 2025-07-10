using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static Enterprise.DocumentWrappers.AllRadioactiveComponents;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(IATAStandardSummaryWriter))]
	sealed class IATAStandardSummaryWriterTest : UNDGStandardSummaryWriterTest
	{
		public override IUNDGStandardSummaryWriter GetWriter() => new IATAStandardSummaryWriter();

		public override IList<IUNDGSummaryWriterComponent> GetExpectedComponents()
		{
			return new IUNDGSummaryWriterComponent[]
			{
				new UNNOComponent(),
				new IATAPSNComponent(),
				new ClassComponent(),
				new PackingGroupComponent(),
				new FlashPointComponent(),
				new MarinePollutantComponent(),
				new LimitedQuantityComponent(),
				new PSAGroupComponent(),
			}.Concat(GetRadioactiveComponents()).ToArray();
		}
	}
}
