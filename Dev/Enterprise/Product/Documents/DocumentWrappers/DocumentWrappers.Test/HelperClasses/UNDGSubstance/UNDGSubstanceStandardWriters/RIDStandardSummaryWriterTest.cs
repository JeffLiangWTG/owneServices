using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static Enterprise.DocumentWrappers.AllRadioactiveComponents;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(RIDStandardSummaryWriter))]
	sealed class RIDStandardSummaryWriterTest : UNDGStandardSummaryWriterTest
	{
		public override IUNDGStandardSummaryWriter GetWriter() => new RIDStandardSummaryWriter();

		public override IList<IUNDGSummaryWriterComponent> GetExpectedComponents()
		{
			return new IUNDGSummaryWriterComponent[]
			{
				new UNNOComponent(),
				new PSNComponent(),
				new ClassComponent(),
				new PackingGroupComponent(),
				new FlashPointComponent(),
				new MarinePollutantComponent(),
				new LimitedQuantityComponent(),
				new PSAGroupComponent(),
				new Class1WeightComponent(),
				new RIDMilitaryConsignmentComponent(),
				new ExceptedQuantityComponent()
			}.Concat(GetRadioactiveComponents()).ToArray();
		}
	}
}
