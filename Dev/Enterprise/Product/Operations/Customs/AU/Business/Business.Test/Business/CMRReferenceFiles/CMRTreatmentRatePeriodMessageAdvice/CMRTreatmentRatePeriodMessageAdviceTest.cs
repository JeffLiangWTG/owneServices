using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRTreatmentRatePeriodMessageAdvice))]
	sealed class CMRTreatmentRatePeriodMessageAdviceTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CMRTreatmentRatePeriodMessageAdvice.New(Factory);
	}
}
