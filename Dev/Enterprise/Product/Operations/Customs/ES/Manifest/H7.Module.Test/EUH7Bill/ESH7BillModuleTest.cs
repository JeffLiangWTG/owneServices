using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.Module;
using Enterprise.Customs.EU.H7.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Module.Testing
{
	[TestedType(typeof(ESH7BillModule))]
	sealed class ESH7BillModuleTest : EUH7BillModuleTest
	{
		public override void TestGetNewFilterBusinessObject()
		{
			using(var module = new ESH7BillModuleForTest())
			{
				AssertType<ESH7BillFilterBusinessObject>(module.GetNewFilterBusinessObject_Exposed());
			}
		}

		public override void TestGetNewFilterControl()
		{
			using (var module = new ESH7BillModuleForTest())
			using (var control = module.GetNewFilterControl_Exposed())
			{
				AssertType<ESH7BillFilterStripControl>(control);
			}
		}

		public override void TestGetNewGridCollection()
		{
			using (var module = new ESH7BillModuleForTest())
			{
				AssertType<EUH7BillModuleCollection<AsycudaBill>>(module.GridCollection);
			}
		}

		class ESH7BillModuleForTest : ESH7BillModule
		{
			public IFilterControl GetNewFilterControl_Exposed() => base.GetNewFilterControl();

			public FilterBusinessObject GetNewFilterBusinessObject_Exposed() => base.GetNewFilterBusinessObject();
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.ES.EUH7Bill;

		protected override string CountryCode => Core.Constants.CountryCodes.Spain;
	}
}
