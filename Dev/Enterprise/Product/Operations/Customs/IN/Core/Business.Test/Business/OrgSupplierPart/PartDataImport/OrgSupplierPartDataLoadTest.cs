using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(OrgSupplierPartDataLoad))]
sealed class OrgSupplierPartDataLoadTest : DataLoadTestCase<OrgSupplierPartDataLoad>
{
	protected override OrgSupplierPartDataLoad GetNewDataLoader() => new OrgSupplierPartDataLoad();
}
