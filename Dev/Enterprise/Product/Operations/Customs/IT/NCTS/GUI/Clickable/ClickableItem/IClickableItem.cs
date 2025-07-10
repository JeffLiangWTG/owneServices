using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.GUI;

public interface IClickableItem
{
	public IClickableContext ClickableContext { get; }

	bool IsFormPreSaved(ITopLevelBusinessObjectProvider topLevelBizObjProvider);
}
