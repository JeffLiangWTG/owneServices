using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(CusClassificationModule))]
sealed class CusClassificationModuleTest : Customs.Module.Testing.SingleTariffClassificationModuleAbstractTest<CusClassificationModule>
{
	public void TestGetNewFilterBusinessObject()
	{
		using (var module = new CusClassificationModuleForTest())
		{
			AssertType<CusClassificationFilterBusinessObject>(module.NewFilterBusinessObject);
		}
	}

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

	sealed class CusClassificationModuleForTest : CusClassificationModule
	{
		public CusClassificationModuleForTest()
		{
		}

		public IFilterControl NewFilterControl => GetNewFilterControl();

		public IBusinessObjectCollection NewGridCollection => GetNewGridCollection();

		public FilterBusinessObject NewFilterBusinessObject => GetNewFilterBusinessObject();
	}
}
