using Enterprise.Customs.Business;
using Enterprise.Customs.MY.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MY.Module.Testing
{
	[TestedType(typeof(CusClassificationModule))]
	sealed class CusClassificationModuleTest : Customs.Module.Testing.SingleTariffClassificationModuleAbstractTest<CusClassificationModule>
	{
		public void TestGetNewFilterControl()
		{
			using (var module = new CusClassificationModule())
			using (var filterControlForGrid = module.GetNewFilterControlForGrid())
			{
				AssertType<CusClassificationFilterControl>(filterControlForGrid);
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new CusClassificationModule())
			{
				AssertType<BaseClassificationCollection<CusClassification>>(module.GridCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new CusClassificationModule())
			{
				AssertType<CusClassificationFilterBusinessObject>(module.FilterBusinessObject);
			}
		}
	}
}
