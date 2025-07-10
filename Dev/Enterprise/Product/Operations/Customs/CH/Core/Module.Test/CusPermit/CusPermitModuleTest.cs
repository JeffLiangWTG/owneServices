using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(Customs.Module.CusPermitModule))]
class CusPermitModuleTest : Customs.Module.Testing.CusPermitModuleTest
{
	public void TestGetNewFilterControl_ReturnType()
	{
		using (var module = new CusPermitModule())
		{
			using (var filterControl = module.GetNewFilterControlForGrid())
			{
				AssertType<CusPermitFilterControl>(filterControl);
			}
		}
	}

	public void TestGetFilterStripBusinessObject_ReturnType()
	{
		using (var module = new CusPermitModule())
		{
			var filterStipBizO = module.FilterBusinessObject;
			AssertType<CusPermitFilterStripBusinessObject>(filterStipBizO);
		}
	}
}
