using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static Enterprise.DocumentWrappers.AllRadioactiveComponents;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(IMOStandardSummaryWriter))]
	sealed class IMOStandardSummaryWriterTest : UNDGStandardSummaryWriterTest
	{
		public override IUNDGStandardSummaryWriter GetWriter() => new IMOStandardSummaryWriter();

		public override IList<IUNDGSummaryWriterComponent> GetExpectedComponents()
		{
			var list = new List<IUNDGSummaryWriterComponent>
			{
				new UNNOComponent(),
				new PSNComponent(),
				new ClassComponent(),
				new PackingGroupComponent(),
				new FlashPointComponent(),
				new MarinePollutantComponent(),
				new LimitedQuantityComponent(),
				new PSAGroupComponent(),
				new EMSComponent()
			};
			return list.Concat(GetRadioactiveComponents()).ToArray();
		}
	}
}
