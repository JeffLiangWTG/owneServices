using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(OrgSupplierPartDataLoad))]
class OrgSupplierPartDataLoadTest : DataLoadTestCase<OrgSupplierPartDataLoad>
{
	protected override OrgSupplierPartDataLoad GetNewDataLoader() => new OrgSupplierPartDataLoad();
}
