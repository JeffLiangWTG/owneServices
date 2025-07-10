using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Module
{
	class ARLStatementOfAccountFilterStripBusinessObject : StatementFilterStripBusinessObject
	{
		public new class Schema : StatementFilterStripBusinessObject.Schema
		{
			public const string SOABusinessNumber = "SOA Business Number";
			public const string SOAOrganization = "SOA Organization";

			public static MultilingualString SOABusinessNumberMultilingualDescription => ResString.GetMultilingualString("CA|ARLStatementOfAccountFilterStripBusinessObject|SOABusinessNumber", SOABusinessNumber);
			public static MultilingualString SOAOrganizationMultilingualDescription => ResString.GetMultilingualString("CA|ARLStatementOfAccountFilterStripBusinessObject|SOAOrganization", SOAOrganization);
		}

		protected override ZString ImporterFilterName => Schema.SOAOrganization;
		protected override MultilingualString ImporterFilterNameMultilingualDescription => Schema.SOAOrganizationMultilingualDescription;

		protected override ZString HeaderBusinessNubmerFilterName => Schema.SOABusinessNumber;
		protected override MultilingualString HeaderBusinessNubmerFilterNameMultilingualDescription => Schema.SOABusinessNumberMultilingualDescription;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			var paymentDueDateFilter = result.AddDateFilter(Schema.PaymentDueDate, CusStatementHeaderSchema.B2_DueDate);
			paymentDueDateFilter.Category = FilterCategories.Dates;
			paymentDueDateFilter.MultilingualDescription = Schema.PaymentDueDateMultilingualDescription;
			return result;
		}
	}
}
