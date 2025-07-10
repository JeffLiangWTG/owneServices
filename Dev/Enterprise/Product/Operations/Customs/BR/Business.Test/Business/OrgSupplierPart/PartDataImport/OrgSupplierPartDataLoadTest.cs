using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(GlobalOrgSupplierPartDataLoad))]
	class OrgSupplierPartDataLoadTest_ClassificationTypePivotMatching : Customs.Business.Testing.GlobalOrgSupplierPartDataLoadTest_ClassificationTypePivotMatching
	{
		protected override ZString ExpectedTariffNumber => "49011000";
	}

	[TestedType(typeof(GlobalOrgSupplierPartDataLoad))]
	class OrgSupplierPartDataLoadTest_ImportExportPivotMatching : Customs.Business.Testing.GlobalOrgSupplierPartDataLoadTest_ImportExportPivotMatching
	{
		protected override ZString ExpectedTariffNumber => "49011000";
	}

	[TestedType(typeof(OrgSupplierPartDataLoad))]
	class OrgSupplierPartDataLoadTest : DataLoadTestCase<OrgSupplierPartDataLoad>
	{
		protected override OrgSupplierPartDataLoad GetNewDataLoader()
		{
			return new OrgSupplierPartDataLoad();
		}
	}
}
