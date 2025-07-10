using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static Enterprise.DocumentWrappers.AllRadioactiveComponents;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(ADNStandardSummaryWriter))]
	sealed class ADNStandardSummaryWriterTest : UNDGStandardSummaryWriterTest
	{
		public override IUNDGStandardSummaryWriter GetWriter() => new ADNStandardSummaryWriter();

		public override IList<IUNDGSummaryWriterComponent> GetExpectedComponents()
		{
			return new IUNDGSummaryWriterComponent[]
			{
				new UNNOComponent(),
				new PSNComponent(),
				new ClassComponent(),
				new PackingGroupComponent(),
				new FlashPointComponent(),
				new LimitedQuantityComponent(false),
				new PSAGroupComponent(),
				new ExceptedQuantityComponent(),
				new MarinePollutantComponent(),
			}.Concat(GetRadioactiveComponents()).ToArray();
		}
	}
}
