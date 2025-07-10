using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitHeaderProcessTask))]
	sealed class CusExitHeaderProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject() => WorkflowItems.AddNew();

		ProcessTaskCollection WorkflowItems => ExitHeader.WorkflowItems;

		CusExitHeader ExitHeader
		{
			get
			{
				if (exitHeader == null)
				{
					exitHeader = Factory.New<CusExitHeader>();
				}
				return exitHeader;
			}
		}
		CusExitHeader exitHeader;
	}
}
