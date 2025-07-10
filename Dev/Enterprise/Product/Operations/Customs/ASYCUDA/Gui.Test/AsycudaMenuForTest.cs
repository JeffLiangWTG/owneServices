using System;
using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	public class AsycudaMenuForTest : AsycudaMenu
	{
		public AsycudaMenuForTest(AsycudaManifestHeader header)
			: base(header)
		{ }

		protected override DialogResult ShowSaveDialog(ZSaveFileDialog dialog)
		{
			return DialogResult.OK;
		}

		public new void OnPopup(EventArgs e) => base.OnPopup(e);
	}
}
