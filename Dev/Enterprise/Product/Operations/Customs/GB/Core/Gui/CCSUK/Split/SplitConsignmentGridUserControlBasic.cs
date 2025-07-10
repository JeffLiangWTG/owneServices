using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public class SplitConsignmentGridUserControlBasic : SplitConsignmentGridUserControl
	{
		protected override ModuleIdentifier ModuleId
		{
			get { return ModuleIDs.Customs.EU.GB.CcsukSplitBasic; }
		}
	}
}
