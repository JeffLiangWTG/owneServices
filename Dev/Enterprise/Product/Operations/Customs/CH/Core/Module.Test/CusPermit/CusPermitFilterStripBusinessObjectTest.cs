using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(Customs.Module.CusPermitFilterStripBusinessObject))]
class CusPermitFilterStripBusinessObjectTest : Customs.Module.Testing.CusPermitFilterStripBusinessObjectTest
{
	public void TestGetPermitTypeModuleFilter_ReturnType()
	{
		var filter = new CusPermitFilterStripBusinessObject();
		var permitTypeSubTypeFilter = filter[CusPermitFilterStripBusinessObject.Schema.PermitTypeSubType];
		AssertType<PermitTypeModuleFilter>(permitTypeSubTypeFilter);
	}

	public void TestGetPermitTypeModuleFilter_Description()
	{
		var filter = new CusPermitFilterStripBusinessObject();
		var permitTypeSubTypeFilter = filter[CusPermitFilterStripBusinessObject.Schema.PermitTypeSubType];
		AssertEquals("Permit Type", permitTypeSubTypeFilter.MultilingualDescription.ToString());
	}
}
