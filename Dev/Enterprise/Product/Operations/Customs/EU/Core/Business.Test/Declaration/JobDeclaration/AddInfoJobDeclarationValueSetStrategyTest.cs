using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class AddInfoJobDeclarationValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestVATDeferStrategyShouldBeCalledWhenZG_VATDeferTypeChanged()
		{
			var declaration = Factory.New<DummyJobDeclaration>();
			declaration.ZG_VATDeferNumber = "XXX";
			declaration.ZG_VATDeferType = "A";
			AssertEquals("It should call OnVATDeferTypeChanged when VATDeferNumber changes.", "100", declaration.ZG_VATDeferNumber);
		}

		class DummyJobDeclaration : JobDeclaration
		{
			public DummyJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override VATDeferStrategy GetVATDeferStrategyCore() => new DummyVATDeferStrategy(this);
		}

		class DummyVATDeferStrategy : VATDeferStrategy
		{
			public DummyVATDeferStrategy(JobDeclaration declaration) : base(declaration)
			{
			}

			public override void OnVATDeferTypeChanged()
			{
				Declaration.ZG_VATDeferNumber = "100";
			}
		}
	}
}
