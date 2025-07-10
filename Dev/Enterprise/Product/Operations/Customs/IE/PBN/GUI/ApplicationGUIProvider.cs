using System;
using Enterprise.Customs.IE.PBN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.PBN.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder((AsycudaManifestHeader)header, mainForm);

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new PBNManifestLayout();
	}
}
