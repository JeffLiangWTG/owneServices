using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRStatisticalClassificationPeriodMessageAdvice))]
	sealed class CMRStatisticalClassificationPeriodMessageAdviceTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CMRStatisticalClassificationPeriodMessageAdvice.New(Factory);
	}
}
