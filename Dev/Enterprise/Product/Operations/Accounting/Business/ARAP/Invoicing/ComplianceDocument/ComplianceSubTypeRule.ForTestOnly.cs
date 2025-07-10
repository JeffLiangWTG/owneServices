#if DEBUG

using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class ComplianceSubTypeRule
	{
		public bool ContainsAtLeastOneTaxIDAndAmountOfTax_ForTestOnly => ContainsAtLeastOneTaxIDAndAmountOfTax;

		public bool ContainsAtLeastOneTaxIDAndNoAmountOfTax_ForTestOnly => ContainsAtLeastOneTaxIDAndNoAmountOfTax;

		public bool ContainsAnAmountOfTax_ForTestOnly => ContainsAnAmountOfTax;

		public bool IsContainsAtLeastOneTaxID_ForTestOnly => IsContainsAtLeastOneTaxID;

		public bool ContainsAtLeastOneTaxIDExcludingNOT_ForTestOnly => ContainsAtLeastOneTaxIDExcludingNOT;

		public bool ContainsAtLeastOneTaxIDExcludingNOTAndEXL_ForTestOnly => ContainsAtLeastOneTaxIDExcludingNOTAndEXL;

		public bool AllWithTaxIDAndZeroTaxAmount_ForTestOnly => AllWithTaxIDAndZeroTaxAmount;

		public bool AllContainedInSpecifiedTaxIDCodes_TestOnly(IReadOnlyCollection<ZString> splitTaxIDCodes) => AllContainedInSpecifiedTaxIDCodes(splitTaxIDCodes);

		public bool ContainsAtLeastOneSuspendedTaxID_ForTestOnly => ContainsAtLeastOneSuspendedTaxID;

		public bool ContainsAtLeastOneTaxIDExcludingSuspended_ForTestOnly => ContainsAtLeastOneTaxIDExcludingSuspended;

		public bool AllWithExemptTaxIDs_ForTestOnly => AllWithExemptTaxIDs;

		public bool AllWithNoReportTaxIDs_ForTestOnly => AllWithNoReportTaxIDs;

		public bool MeetVNOrganizationLocationRule_ForTestOnly(ZString organizationLocation) => MeetVNOrganizationLocationRule(organizationLocation);

		public bool EvaluateOrganisationCategoryRule_ForTestOnly(IAccComplianceRule rule) => EvaluateOrganisationCategoryRule(rule);
	}
}

#endif
