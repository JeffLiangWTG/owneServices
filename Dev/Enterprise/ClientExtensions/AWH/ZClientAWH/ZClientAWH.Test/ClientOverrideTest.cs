using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.ZClientAWH.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : Enterprise.ZArchitecture.Modules.Testing.ClientOverrideTest
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
