using Enterprise.Customs.NZ.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.NZ.Testing
{
	[TestedType(typeof(DocContainerAndPackageInfo))]
	sealed class DocContainerAndPackageInfoTest : DocumentWrapperTestCase
	{
		public void TestInstantiation()
		{
			DocContainerAndPackageInfo packInfo = new DocContainerAndPackageInfo(Factory, "ONE", "TWO", "THREE", "FOUR", "4 Barrel");
			AssertEquals("ONE", packInfo.BillNumber);
			AssertEquals("TWO", packInfo.BillType);
			AssertEquals("THREE", packInfo.ContainerNumber);
			AssertEquals("FOUR", packInfo.ContainerStatus);
			AssertEquals("4 Barrel", packInfo.PackagesAndType);
		}

		public void TestPackagesAndType()
		{
			DocContainerAndPackageInfo packInfo = new DocContainerAndPackageInfo(Factory, "ONE", "TWO", "THREE", "FOUR", "7 Keg, 5 Bag");
			AssertEquals("7 Keg, 5 Bag", packInfo.PackagesAndType);
		}

		public void TestPackageTypeDescription()
		{
			AssertEquals("Precondition", false, UniversalReferenceHelper.GetUNEPackageTypeList(Factory).ContainsCode("PCS"));
			DocContainerAndPackageInfo packInfo = new DocContainerAndPackageInfo(Factory, "", "", "", "", "4 PCS");
			AssertEquals("4 PCS", packInfo.PackagesAndType);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { new DocContainerAndPackageInfo(Factory, "ONE", "TWO", "THREE", "FOUR", "4 FIVE") };
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return new DocContainerAndPackageInfo(Factory, "ONE", "TWO", "THREE", "FOUR", "4 FIVE");
		}
	}
}
