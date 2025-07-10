using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitHeaderProcessTaskCollection))]
	sealed class CusExitHeaderProcessTaskCollectionTest : ProcessTaskCollectionTest<CusExitHeaderProcessTaskCollection>
	{
		protected override CusExitHeaderProcessTaskCollection GetCollectionToTestCore()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			return new CusExitHeaderProcessTaskCollection(exitHeader);
		}
	}
}
