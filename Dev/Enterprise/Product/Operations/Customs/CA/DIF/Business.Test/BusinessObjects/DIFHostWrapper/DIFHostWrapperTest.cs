using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business.DIS;
using NUnit.Framework;

namespace Enterprise.Customs.CA.DIF.Business.Testing
{
	[TestedType(typeof(DIFHostWrapper))]
	sealed class DIFHostWrapperTest : DISHostWrapperBaseTest<DIFHostWrapper, DIFDocument>
	{
		protected override BusinessObject GetNewBusinessObject() => new DIFHostWrapper((ICADIFHost)new TestHelper(Factory).GetJobDeclaration());
	}
}
