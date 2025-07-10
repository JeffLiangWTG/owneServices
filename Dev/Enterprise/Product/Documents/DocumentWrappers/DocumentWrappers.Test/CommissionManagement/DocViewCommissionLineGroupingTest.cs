using Enterprise.CommissionManagement.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocViewCommissionLineGroupingTest : DocBaseWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var grouping = new ViewCommissionLineGrouping(Factory);
			return DocViewCommissionLineGrouping.New(grouping, Factory);
		}
	}
}
