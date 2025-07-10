using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(JPAFRHeaderDocManagerInfo))]
	class JPAFRHeaderDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject() => Factory.New<JPAFRHeader>();

		public override BusinessObject GetPopulatedParentBusinessObject() => Factory.NewWithValidTestData<JPAFRHeader>();
	}
}
