using System;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.Manifest.GUI;

public sealed class IGMApplicationGUIProvider : ApplicationGUIProvider
{
	public override Type ApplicationBusinessProviderType => typeof(IGMApplicationBusinessProvider);

	public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);
}
