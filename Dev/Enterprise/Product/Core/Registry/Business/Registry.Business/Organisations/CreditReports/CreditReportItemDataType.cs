using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.CreditReportsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class CreditReportItemDataType : NonPersistentBusinessObjectRegistryDataType<CreditReportItemCollection>
	{
		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, CreditReportItemCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var countriesList = new List<string>();
			foreach (CreditReportItem report in proposedValue)
			{
				if (report.CountryEnabledForOrganisation && !report.CommercialBureauEnquiryEnabled && !report.FailureRiskEnabled && !report.ComprehensiveReportEnabled && !report.LatePaymentRiskEnabled)
				{
					countriesList.Add(report.Country);
				}
			}

			if (countriesList.Any())
			{
				var countries = string.Join(", ", countriesList);
				throw new RegistryValidationException(ResString.GetMultilingualString(
					"E5FDDC32-6D56-4976-82D8-759FFA284D86",
					"Please select at least 1 report type when Credit Reports are enabled for {0}.", countries));
			}
		}
	}
}
