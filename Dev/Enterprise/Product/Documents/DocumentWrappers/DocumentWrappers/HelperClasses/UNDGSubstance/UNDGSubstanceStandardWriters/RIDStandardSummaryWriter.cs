using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentWrappers.GenericWrappers;
using static Enterprise.DocumentWrappers.AllRadioactiveComponents;

namespace Enterprise.DocumentWrappers
{
	class RIDStandardSummaryWriter : IUNDGStandardSummaryWriter
	{
		IReadOnlyCollection<IUNDGSummaryWriterComponent> IUNDGStandardSummaryWriter.Components
		{
			get
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

		bool IUNDGStandardSummaryWriter.StandardConditionApplies(UNDGSubstanceWrapper wrapper)
		{
			return true;
		}
	}
}
