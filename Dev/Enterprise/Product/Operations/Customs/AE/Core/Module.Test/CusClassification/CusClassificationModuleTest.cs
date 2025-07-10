using Enterprise.Customs.AE.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Module.Testing;

[TestedType(typeof(CusClassificationModule))]
sealed class CusClassificationModuleTest : Customs.Module.Testing.SingleTariffClassificationModuleAbstractTest<CusClassificationModule>
{
	public void TestGetNewFilterControl()
	{
		using (var module = new CusClassificationModuleForTest())
		{
			IFilterControl filterControl = module.NewFilterControl;
			AssertType<CusClassificationFilterControl>(filterControl);
			filterControl.Dispose();
		}
	}

	public void TestGetNewGridCollection()
	{
		using (var module = new CusClassificationModuleForTest())
		{
			AssertType<BaseClassificationCollection<CusClassification>>(module.NewGridCollection);
		}
	}

	public void TestGetNewFilterBusinessObject()
	{
		using (var module = new CusClassificationModuleForTest())
		{
			AssertType<CusClassificationFilterBusinessObject>(module.NewFilterBusinessObject);
		}
	}
}
