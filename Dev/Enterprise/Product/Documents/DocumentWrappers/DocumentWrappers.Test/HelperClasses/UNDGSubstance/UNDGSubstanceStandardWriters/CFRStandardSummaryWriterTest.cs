using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(CFRStandardSummaryWriter))]
	sealed class CFRStandardSummaryWriterTest : UNDGStandardSummaryWriterTest
	{
		public override IUNDGStandardSummaryWriter GetWriter() => new CFRStandardSummaryWriter();

		public override IList<IUNDGSummaryWriterComponent> GetExpectedComponents()
		{
			return new IUNDGSummaryWriterComponent[]
			{
				new UNNOComponent(),
				new CFRPSNComponent(),
				new CFRSpecialPermitNumberComponent(),
				new CFRWasteCodeComponent(),
				new MaterialFormDescriptionComponent(),
				new CFRClassComponent(),
				new RadionuclideComponent(),
				new RadioactiveLabelCategoryComponent(),
				new RadioactiveTransportIndexComponent(),
				new PackingGroupComponent(),
				new CFRReportableQuantityComponent(),
				new CFRPoisonInhalationHazardComponent(),
				new FlashPointComponent(),
				new MarinePollutantComponent(),
				new PSAGroupComponent(),
				new CFRLimitedQuantityComponent(),
				new CFRResidueLastContainedComponent(),
				new HRCQComponent(),
				new FissileExceptedComponent()
			};
		}
	}
}
