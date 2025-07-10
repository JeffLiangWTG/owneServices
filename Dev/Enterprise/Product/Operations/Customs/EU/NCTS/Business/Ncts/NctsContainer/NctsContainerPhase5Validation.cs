using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsContainerPhase5Validation : NctsContainerValidation
	{
		public NctsContainerPhase5Validation(NctsContainer parent) : base(parent)
		{
		}

		protected override void CheckBC_ContainerNumCore_Incident(NctsContainer nctsContainer, EnRouteIncident incident)
		{
			if (incident.Header is NctsHeader header
				&& header.Configuration.NctsContainerConfiguration.GetHeaderValidationDecider(header) is INctsContainerPhase5ValidationDecider { IsRuleTR0044Active: true })
			{
				nctsContainer.CheckContainerNumberIsUnique(incident.IncidentContainers, ValidationRuleCodeConstants.TR0044.GetRuleCodeMessagePrefix(addSpaceAtEnd: true));
			}
		}

		protected override void CheckBC_ModeCore_Incident(NctsContainer nctsContainer, EnRouteIncident incident)
		{
			if (!nctsContainer.BC_ContainerNum.IsEmpty || !nctsContainer.BC_Seal1.IsEmpty || !nctsContainer.BC_Seal2.IsEmpty)
			{
				if (incident.Header is NctsHeader header
					&& header.Configuration.NctsContainerConfiguration.GetHeaderValidationDecider(header) is INctsContainerPhase5ValidationDecider { IsRuleTR0043Active: true })
				{
					MandatoryValidation.CheckEntered(nctsContainer.BC_ModeInfo, errorNotificationPrefix: ValidationRuleCodeConstants.TR0043.GetRuleCodeMessagePrefix(addSpaceAtEnd: true));
				}
			}
			if (ValidationRuleConfigurationOrNull?.IsRuleTR0046Active ?? false)
			{
				nctsContainer.CheckTR0046AllSameMode(incident.IncidentContainers);
			}
		}

		protected override void CheckBC_Seal1Core_Incident(NctsContainer nctsContainer, EnRouteIncident incident)
		{
			if (Parent.Header?.Configuration.NctsContainerConfiguration.GetHeaderValidationDecider(Parent.Header) is INctsContainerPhase5ValidationDecider { IsRuleTR0045Active: true }
				&& HasDuplicateSealNumbersOnIncident(nctsContainer.BC_Seal1, incident))
			{
				nctsContainer.BC_Seal1Info.AddWarning(ValidationRuleCodeConstants.TR0045.GetRuleCodeMessagePrefix(addSpaceAtEnd: true) + Res.GetString("4E95AD47-E61B-4AE9-9816-AB24FB0FD4ED", "Duplicate Seal 1 Number entered."));
			}
		}

		protected override void CheckBC_Seal2Core_Incident(NctsContainer nctsContainer, EnRouteIncident incident)
		{
			if (Parent.Header?.Configuration.NctsContainerConfiguration.GetHeaderValidationDecider(Parent.Header) is INctsContainerPhase5ValidationDecider { IsRuleTR0045Active: true }
				&& HasDuplicateSealNumbersOnIncident(nctsContainer.BC_Seal2, incident))
			{
				nctsContainer.BC_Seal2Info.AddWarning(ValidationRuleCodeConstants.TR0045.GetRuleCodeMessagePrefix(addSpaceAtEnd: true) + Res.GetString("6D040F56-EE0A-4437-883A-986713204CB1", "Duplicate Seal 2 Number entered."));
			}
		}

		ValidationRuleConfiguration ValidationRuleConfigurationOrNull => Parent.Header?.Configuration.ValidationRuleConfiguration;
	}
}
