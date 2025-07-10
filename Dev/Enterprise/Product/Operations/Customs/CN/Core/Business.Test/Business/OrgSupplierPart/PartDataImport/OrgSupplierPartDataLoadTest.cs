using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartDataLoad))]
	class OrgSupplierPartDataLoadTest : DataLoadTestCase<OrgSupplierPartDataLoad>
	{
		protected override OrgSupplierPartDataLoad GetNewDataLoader()
		{
			return new OrgSupplierPartDataLoad();
		}
	}
}
