using System;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.Integration.ComplianceWise;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	class EnableComplianceRiskDataType : BooleanRegistryDataType, IEnableComplianceRiskDataType
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		const string FreightMessage = "- Freight -> Compliance -> Enable ComplianceWise";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		const string LinerAgencyMessage = "- Liner & Agency -> Compliance -> Enable ComplianceWise";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		const string CustomsMessage = "- ComplianceWise - Integrate ComplinaceWise to Customs Declaration module";

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;

		protected override void ValidateCore(IRegistryItem registryItem, bool proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (!proposedValue)
			{
				var isCustomsEnabledComplianceWise = ObjectFactory.Get<IComplianceWiseEnabledDetail>().IsCustomsEnabledComplianceWise;

				var shouldShowMessage = FreightDataRegistry.Instance.FreightEnableComplianceWise.Value.EnableComplianceWise
					|| LinerAgencyDataRegistry.Instance.LinerAgencyEnableComplianceWise.Value.EnableComplianceWise
					|| isCustomsEnabledComplianceWise;

				if (shouldShowMessage)
				{
					var freightMessage = FreightDataRegistry.Instance.FreightEnableComplianceWise.Value.EnableComplianceWise ? ("\r\n" + FreightMessage) : string.Empty;
					var linerAgencyMessage = LinerAgencyDataRegistry.Instance.LinerAgencyEnableComplianceWise.Value.EnableComplianceWise ? ("\r\n" + LinerAgencyMessage) : string.Empty;
					var customsMessage = isCustomsEnabledComplianceWise ? ("\r\n" + CustomsMessage) : string.Empty;
					var message = Res.GetString("1c9aec4c-6e07-4305-b7b7-cbd885e1e3f9", "The following module specific ComplianceWise registry settings, feature control need to be disabled in order to disable ComplianceWise on this system:{0}{1}{2}", freightMessage, linerAgencyMessage, customsMessage);

					throw new RegistryValidationException(message);
				}
			}

			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}
	}
}
