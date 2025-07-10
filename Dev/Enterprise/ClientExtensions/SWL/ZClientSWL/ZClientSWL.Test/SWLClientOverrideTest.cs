using System;
using Enterprise.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.SWL.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class SWLClientOverrideTest : ClientOverrideTest
	{
		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}

		public void TestJobDeclarationControllerOverride()
		{
			var overrides = ClientOverride.Instance;
			AssertNotNull(overrides.ControllerOverrides);
			var info = overrides.ControllerOverrides[ControllerIDs.Organisation, Constants.CountryCodes.Australia];
			var controllerOverrideType = typeof(SWLOrganisationControllerOverride);
			AssertNotNull(info);
			Assert(info.IsClientOverride);
			Assert(info.TypePath.IndexOf(controllerOverrideType.Assembly.FullName) > -1);
		}

		public void TestRegistry()
		{
			AssertEquals("AdditionalRegistryItemSet", SWLDataRegistry.Instance, ClientOverride.Instance.AdditionalRegistryItemSet);
		}
	}
}
