using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusContainer))]
	public class CusContainerTest : BaseCusContainerWithCustomLabelsTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			jobDeclaration.JE_OH_Importer = base.Factory.New<OrgHeader>().PK;
			jobDeclaration.JE_MessageType = "IMP";
			return jobDeclaration.CusContainers.AddNew();
		}
	}
}
