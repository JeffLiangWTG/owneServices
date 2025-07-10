using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageHeaderProcessTask))]
	public class TemporaryStorageHeaderProcessTaskTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<TemporaryStorageHeader>().WorkflowItems.AddNew();
	}
}
