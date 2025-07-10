using System.Windows.Forms;

namespace Enterprise.Customs.EU.Module
{
	public class CusPermitModuleStrip : Customs.Module.CusPermitModuleStrip
	{
		protected override Control GetPermitTypeFilterStrip()
		{
			return new PermitTypeFilterStrip();
		}
	}
}
