using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public class SWProductionDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, SWProductionDetailsControlBag> where T : SWProduction
{
	public override SWProductionDetailsControlBag CommonBag => SWProductionDetailsControlBag.Instance;

	protected override int MaxColumns => 2;
}
