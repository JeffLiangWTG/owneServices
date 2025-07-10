using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

public class DetailsHeaderCommonLayoutBuilder<TBizObject> : ColumnLayoutBuilder<TBizObject, DetailsHeaderControlBag>
		where TBizObject : CusTempStorageRegHeader
{
	public override DetailsHeaderControlBag CommonBag => DetailsHeaderControlBag.Instance;

	protected override int MaxColumns => 3;
}
