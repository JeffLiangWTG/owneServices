using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageJobHeaderProcessTask))]
	public class CusTempStorageJobHeaderProcessTaskTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<CusTempStorageJobHeader>().WorkflowItems.AddNew();
	}
}
