using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartDataLoad))]
	class OrgSupplierPartDataLoadTest : DataLoadTestCase<OrgSupplierPartDataLoad>
	{
		protected override OrgSupplierPartDataLoad GetNewDataLoader() => new OrgSupplierPartDataLoad();
	}

	[TestedType(typeof(GlobalOrgSupplierPartDataLoad))]
	class OrgSupplierPartDataLoadTest_ClassificationTypePivotMatching : Customs.Business.Testing.GlobalOrgSupplierPartDataLoadTest_ClassificationTypePivotMatching
	{
	}

	[TestedType(typeof(GlobalOrgSupplierPartDataLoad))]
	class OrgSupplierPartDataLoadTest_ImportExportPivotMatching : Customs.Business.Testing.GlobalOrgSupplierPartDataLoadTest_ImportExportPivotMatching
	{
	}
}
