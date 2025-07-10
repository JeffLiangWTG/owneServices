using System;
using Enterprise.Customs.IN.Manifest.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(IGMApplicationGUIProvider))]
sealed class IGMApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<IGMApplicationGUIProvider, IGMAsycudaManifestHeader>
{
	protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);
}
