using Enterprise.Customs.EU.H7.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Module.Testing
{
	[TestedType(typeof(GBH7BillModule))]
	public class GBH7BillModuleTest : EUH7BillModuleTest
	{
		public void TestGetNewController()
		{
			AssertType<GBH7BillController>("Controller must be of type GBH7BillController", module.GetNewController());
		}

		public override void TestGetNewFilterBusinessObject()
		{
			AssertType<GBH7BillFilterBusinessObject>("Filter Business Object must be of type GBH7BillFilterBusinessObject", module.GetNewFilterBusinessObject_Exposed());
		}

		protected override void SetUp()
		{
			base.SetUp();
			module = new GBH7BillModuleForTest();
		}

		protected override void TearDown()
		{
			module.Dispose();
			base.TearDown();
		}

		GBH7BillModuleForTest module;
		class GBH7BillModuleForTest : GBH7BillModule
		{
			public IFilterControl GetNewFilterControl_Exposed() => base.GetNewFilterControl();

			public FilterBusinessObject GetNewFilterBusinessObject_Exposed() => base.GetNewFilterBusinessObject();
		}
	}
}
