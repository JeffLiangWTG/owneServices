using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.DE.GUI
{
	public partial class CalculateFreightForm : EU.GUI.CalculateFreightForm
	{
		public CalculateFreightForm(CalculateFreightBizObj bizObj)
			: base(bizObj)
		{
			Controls.Remove(zCheckBoxIsFreightIncludedInLines);
		}
	}
}
