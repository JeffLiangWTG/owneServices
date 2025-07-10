using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class ArrivalAuthorisationNumberCasingBehaviour : ControlBehaviour<ZCodeFindBox, NctsHeader>
	{
		protected override void UpdateBehaviourCore(ZCodeFindBox control, NctsHeader header)
		{
			if (header?.Configuration.AllowMixedCaseAuthorisationNumbers ?? false)
			{
				control.CodeBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			}
		}
	}
}
