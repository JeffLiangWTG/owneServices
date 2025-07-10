using CargoWise.EntityFramework;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public sealed class DocAddressControlDisplayModeCompactWithOverrideBehaviour : ControlBehaviour<ZDocAddressControl, BusinessObject>
	{
		public DocAddressControlDisplayModeCompactWithOverrideBehaviour()
		{
		}

		protected override void UpdateBehaviourCore(ZDocAddressControl control, BusinessObject dataItem)
		{
			if (control == null || dataItem == null)
			{
				return;
			}

			control.DisplayMode = ZDocAddressControlDisplayMode.CompactWithOverride;
		}
	}
}
