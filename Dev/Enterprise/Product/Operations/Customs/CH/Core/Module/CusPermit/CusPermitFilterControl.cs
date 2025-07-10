using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.Module;

public partial class CusPermitFilterControl : Customs.Module.CusPermitFilterControl
{
	public CusPermitFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject) : base(gridCollection, filterStripBusinessObject)
	{
		InitializeComponent();
	}

	protected override ZFilterStrip NewZFilterStrip()
	{
		return new CusPermitModuleStrip();
	}
}
