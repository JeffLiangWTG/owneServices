using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(GBOrgSupplierPartDataLoad))]
	class GlobalOrgSupplierPartDataLoadTest : OrgSupplierPartDataLoadTest
	{
		protected override OrgSupplierPartDataLoad GetNewDataLoader() => new GBOrgSupplierPartDataLoad();
	}
}
