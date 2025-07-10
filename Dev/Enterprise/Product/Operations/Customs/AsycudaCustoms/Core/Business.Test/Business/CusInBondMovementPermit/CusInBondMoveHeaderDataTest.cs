using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusInBondMoveHeaderDataTest : AssemblyDataTest
	{
		public void TestOverrides()
		{
			var data = new CusInBondMoveHeaderData();
			CombineAssertions(() =>
			{
				AssertEquals("BusinessObjectType", typeof(CusInBondMoveHeader), data.BusinessObjectType);
				AssertType<CusInBondMoveHeaderCollection>("GetBusinessObjectCollection", data.GetBusinessObjectCollection(Factory));
				AssertEquals("ModuleID", ModuleIDs.Customs.EntryHeader, data.ModuleID);
				AssertEquals("ReferenceType", Core.Constants.ReferenceTypes.ClientSupplierRelationship, data.ReferenceType);
				AssertEquals("HumanReadableName", "Transit Permits", data.HumanReadableName);
				AssertEquals("IsAllowedForUnallocatedeDocs", true, data.IsAllowedForUnallocatedeDocs);
			});
		}
	}
}
