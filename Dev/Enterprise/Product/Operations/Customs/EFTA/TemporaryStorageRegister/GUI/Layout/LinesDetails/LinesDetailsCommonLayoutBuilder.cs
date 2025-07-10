using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

public class LinesDetailsCommonLayoutBuilder<TBizObject> : ColumnLayoutBuilder<TBizObject, LinesDetailsControlBag> where TBizObject : CusTempStorageRegHeader
{
	public override LinesDetailsControlBag CommonBag => LinesDetailsControlBag.Instance;
}
