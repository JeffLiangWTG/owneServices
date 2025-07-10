using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;
using static Enterprise.Customs.AU.Declaration.Business.JobDeclaration;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUDeclarationDocManagerInfo))]
	sealed class AUDeclarationDocManagerInfoTest : Customs.Business.Testing.DeclarationDocManagerInfoTest
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<JobDeclaration>();
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			return Factory.NewWithValidTestData<JobDeclaration>();
		}
	}
}
