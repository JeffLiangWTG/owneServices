using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public abstract class ClickableItem : IClickableItem
{
	public ClickableItem(IClickableContext clickableContext)
	{
		ClickableContext = Argument.NotNull(clickableContext, nameof(clickableContext));
	}

	#region IClickableItem

	public IClickableContext ClickableContext { get; }

	public bool IsFormPreSaved(ITopLevelBusinessObjectProvider topLevelBizObjProvider)
	{
		Argument.NotNull(topLevelBizObjProvider, nameof(topLevelBizObjProvider));
		var topLevelBizObj = Argument.NotNull(topLevelBizObjProvider.TopLevelBusinessObject, nameof(topLevelBizObjProvider.TopLevelBusinessObject));
		return CustomsPlugIn.FormPreSaved(topLevelBizObj, ParentForm);
	}

	#endregion

	#region Implementation

	protected abstract ZForm ParentForm { get; }

	#endregion
}
