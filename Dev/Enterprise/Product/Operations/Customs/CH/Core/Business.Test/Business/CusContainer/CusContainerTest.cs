using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusContainer))]
class CusContainerTest : Customs.Business.Testing.BaseCusContainerTest<CusContainer, JobDeclaration>
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
	protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bo) => new CusContainer.CustomLabelsProvider(((CusContainer)bo).Declaration);
}
