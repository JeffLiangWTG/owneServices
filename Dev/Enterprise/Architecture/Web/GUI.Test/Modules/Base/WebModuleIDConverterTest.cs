using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	sealed class WebModuleIDConverterTest : ModuleIDConverterTest
	{
		protected override ModuleIdentifier DummyModuleID
		{
			get { return WebModuleIDs.Dummy; }
		}

		protected override Type ModuleIDType
		{
			get { return typeof(WebModuleID); }
		}

		protected override ModuleIDConverter GetNewConverter()
		{
			return new WebModuleIDConverter();
		}

		protected override string DummyModuleName
		{
			get { return "DummyWeb"; }
		}
	}
}
