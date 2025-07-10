using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Module.Testing
{
	[TestedType(typeof(TokenAuthenticationOnBoardingModule))]
	public class TokenAuthenticationOnBoardingModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.TokenAuthenticationOnBoarding;
		}

		public void TestAllowNew()
		{
			using (var module = (TokenAuthenticationOnBoardingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(true, module.AllowNew);
			}
		}

		public void TestAllowDelete()
		{
			using (var module = (TokenAuthenticationOnBoardingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(true, module.AllowDelete);
			}
		}

		public void TestAllowEdit()
		{
			using (var module = (TokenAuthenticationOnBoardingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(true, module.AllowEdit);
			}
		}

		public void TestAllowView()
		{
			using (var module = (TokenAuthenticationOnBoardingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(true, module.AllowView);
			}
		}
	}
}
