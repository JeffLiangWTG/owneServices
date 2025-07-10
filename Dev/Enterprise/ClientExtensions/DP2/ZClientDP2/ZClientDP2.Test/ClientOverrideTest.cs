using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.ZClientDP2.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestModuleOverrides()
		{
			ModuleOverrides moduleOverrides = ClientOverride.Instance.ModuleOverrides;
			AssertNotNull("ModuleOverrides", moduleOverrides);
			AssertNotNull("ModuleOverrides should contain ARTransaction", moduleOverrides[ModuleIDs.ARTransaction, GlbCompany.CurrentCompany.GC_RN_NKCountryCode]);
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
	}
}
