using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.Testing
{
	[TestedType(typeof(CDSCashPaymentsModule))]
	class CDSCashPaymentsModuleTests : ZModuleBasherTest
	{
		public void TestAllowNew()
		{
			using (var module = GetModule())
			{
				AssertEquals(false, module.AllowNew);
			}
		}

		public void TestAllowEdit()
		{
			using (var module = GetModule())
			{
				AssertEquals(false, module.AllowEdit);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.EU.GB.CDSCashPayments;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedKingdom;
	}
}
