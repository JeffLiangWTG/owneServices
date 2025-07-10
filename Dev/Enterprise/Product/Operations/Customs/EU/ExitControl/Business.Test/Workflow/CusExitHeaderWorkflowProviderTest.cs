using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitHeader))]
	sealed class CusExitHeaderWorkflowProviderTest : WorkflowProviderTest<CusExitHeader, CusExitHeaderProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CusExitHeaderWorkflowDescriptorCode;
	}
}
