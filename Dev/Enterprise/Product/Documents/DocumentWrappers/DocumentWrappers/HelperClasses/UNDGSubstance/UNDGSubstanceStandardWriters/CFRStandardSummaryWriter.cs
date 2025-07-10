using System.Collections.Generic;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	class CFRStandardSummaryWriter : IUNDGStandardSummaryWriter
	{
		IReadOnlyCollection<IUNDGSummaryWriterComponent> IUNDGStandardSummaryWriter.Components =>
			new IUNDGSummaryWriterComponent[]
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

		bool IUNDGStandardSummaryWriter.StandardConditionApplies(UNDGSubstanceWrapper wrapper) => true;
	}
}
