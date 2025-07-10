using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using static Enterprise.DocumentWrappers.AllRadioactiveComponents;

namespace Enterprise.DocumentWrappers
{
	class ADRStandardSummaryWriter : IUNDGStandardSummaryWriter
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
					new ADRTunnelRestrictionComponent(),
					new FlashPointComponent(),
					new MarinePollutantComponent(),
					new LQAbbreviatedComponent(),
					new PSAGroupComponent(),
				}.Concat(GetRadioactiveComponents()).ToArray();
			}
		}

		bool IUNDGStandardSummaryWriter.StandardConditionApplies(UNDGSubstanceWrapper wrapper)
		{
			return ADRTunnelRestrictionCodeIsApplicable(wrapper);
		}

		bool ADRTunnelRestrictionCodeIsApplicable(UNDGSubstanceWrapper wrapper)
		{
			var substance = wrapper?.DGData?.Subs;
			if (substance == null ||
				substance.DG_Standard != UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADR)
			{
				return false;
			}

			return ADRSubstanceHasTunnelRestrictions(substance.StandardSubstance as UNDGSubstanceADR);
		}

		bool ADRSubstanceHasTunnelRestrictions(UNDGSubstanceADR substanceADR)
		{
			var transportCategory = substanceADR?.ADR_TransportCategory ?? ZString.Empty;
			var regex = new Regex(@"^.*\((.*)\)$");
			var transportCategoryCode = regex.Match(transportCategory).Groups[1].Value;

			if (transportCategoryCode.IsNullOrEmpty()
				|| transportCategoryCode == "A")
			{
				return false;
			}

			return true;
		}
	}
}
