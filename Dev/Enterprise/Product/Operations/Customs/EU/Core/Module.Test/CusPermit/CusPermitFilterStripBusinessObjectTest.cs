using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(Customs.Module.CusPermitFilterStripBusinessObject))]
	class CusPermitFilterStripBusinessObjectTest : Customs.Module.Testing.CusPermitFilterStripBusinessObjectTest
	{
		public void TestGetPermitTypeModuleFilter_ReturnType()
		{
			var filter = new CusPermitFilterStripBusinessObject();
			var permitTypeSubTypeFilter = filter[CusPermitFilterStripBusinessObject.Schema.PermitTypeSubType];
			AssertType<PermitTypeModuleFilter>(permitTypeSubTypeFilter);
		}
	}
}
