using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CustomsSummaryLineCharge))]
sealed class CustomsSummaryLineChargeTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateCustomsSummaryLineCharge(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateCustomsSummaryLineCharge(factory);

	CustomsSummaryLineCharge CreateCustomsSummaryLineCharge(BusinessObjectFactory factory)
	{
		return CustomsSummaryTestHelper.CreateSummaryLine(Factory).LineCharge;
	}
}
