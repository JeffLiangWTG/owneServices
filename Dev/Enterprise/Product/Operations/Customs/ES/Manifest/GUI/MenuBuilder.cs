using System;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.GUI
{
	public sealed class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("7EBDD001-F6A8-416C-B127-4796117D8773", "ES Manifest");

		public override ZMenuItem[] BuildMenu() => Array.Empty<ZMenuItem>();
	}
}
