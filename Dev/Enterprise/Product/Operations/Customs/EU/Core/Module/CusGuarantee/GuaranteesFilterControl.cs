using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module
{
	public partial class GuaranteesFilterControl : Customs.Module.GuaranteesFilterControl
	{
		public GuaranteesFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject) : base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			FilteredGrid.SetColumnVisible(true, nameof(CusPermitHeader.CPH_Calc_OpeningBalance));
		}

		protected override ZFilterStrip NewZFilterStrip() => new GuaranteesFilterStrip();
	}
}
