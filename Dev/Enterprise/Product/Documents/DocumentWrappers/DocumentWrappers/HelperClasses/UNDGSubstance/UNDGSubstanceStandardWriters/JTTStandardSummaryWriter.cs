using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentWrappers.GenericWrappers;
using static Enterprise.DocumentWrappers.AllRadioactiveComponents;

namespace Enterprise.DocumentWrappers
{
	class JTTStandardSummaryWriter : IUNDGStandardSummaryWriter
	{
		IReadOnlyCollection<IUNDGSummaryWriterComponent> IUNDGStandardSummaryWriter.Components
		{
			get
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

		bool IUNDGStandardSummaryWriter.StandardConditionApplies(UNDGSubstanceWrapper wrapper)
		{
			return true;
		}
	}
}
