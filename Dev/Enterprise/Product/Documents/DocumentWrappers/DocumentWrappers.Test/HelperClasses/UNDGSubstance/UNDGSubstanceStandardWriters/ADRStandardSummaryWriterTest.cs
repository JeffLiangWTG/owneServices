using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static Enterprise.DocumentWrappers.AllRadioactiveComponents;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(ADRStandardSummaryWriter))]
	sealed class ADRStandardSummaryWriterTest : UNDGStandardSummaryWriterTest
	{
		public override IUNDGStandardSummaryWriter GetWriter() => new ADRStandardSummaryWriter();

		public override IList<IUNDGSummaryWriterComponent> GetExpectedComponents()
		{
			return new IUNDGSummaryWriterComponent[]
			{
				new UNNOComponent(),
				new PSNComponent(),
				new ClassComponent(),
				new PackingGroupComponent(),
				new ADRTunnelRestrictionComponent(),
				new FlashPointComponent(),
				new MarinePollutantComponent(),
				new LQAbbreviatedComponent(),
				new PSAGroupComponent(),
			}.Concat(GetRadioactiveComponents()).ToArray();
		}
	}
}
