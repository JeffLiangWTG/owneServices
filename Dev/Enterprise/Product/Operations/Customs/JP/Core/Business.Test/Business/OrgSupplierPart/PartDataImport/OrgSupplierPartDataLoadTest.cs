using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(GlobalOrgSupplierPartDataLoad))]
	sealed class OrgSupplierPartDataLoadTest : DataLoadTestCase<OrgSupplierPartDataLoad>
	{
		protected override OrgSupplierPartDataLoad GetNewDataLoader() => new OrgSupplierPartDataLoad();
	}
}
