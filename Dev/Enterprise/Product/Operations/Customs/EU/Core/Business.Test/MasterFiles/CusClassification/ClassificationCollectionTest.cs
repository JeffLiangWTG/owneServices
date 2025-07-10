
using NUnit.Framework;
namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
{
	[TestedType(typeof(ClassificationCollection<CusClassification>))]
	public class ClassificationCollectionTest : Customs.Business.Testing.ClassificationCollectionTest<ClassificationCollection<CusClassification>, CusClassification>
	{
		protected override ClassificationCollection<CusClassification> GetCollectionToTest()
		{
			return new ClassificationCollection<CusClassification>(Factory.New<OrgSupplierPart>(), Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}
	}
}
