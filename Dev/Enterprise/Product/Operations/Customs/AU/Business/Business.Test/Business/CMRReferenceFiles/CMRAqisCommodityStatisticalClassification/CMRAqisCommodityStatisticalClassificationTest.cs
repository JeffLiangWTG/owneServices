using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRAqisCommodityStatisticalClassification))]
	sealed class CMRAqisCommodityStatisticalClassificationTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CMRAqisCommodityStatisticalClassification.New(Factory);
	}
}
