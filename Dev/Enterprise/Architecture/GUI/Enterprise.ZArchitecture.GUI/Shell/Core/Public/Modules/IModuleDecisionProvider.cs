using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Modules
{
	public interface IModuleDecisionProvider : IEmbeddedModulePopupOKButtonStrategy, ISQLFilterOnlyModuleDecisionProvider
	{
		bool ShouldDisplayNotifications { get; }
		bool ShouldLoadFilterBizObj { get; }
		bool ShouldSaveFilterBizObj { get; }
		bool ShouldIgnoreAdditionalFilter { get; }
		bool AllowExcelExport { get; }
		bool EnablePreviousNextSupport { get; }
		IBusinessObjectCollection List { get; }
		void HandleDefaultAction(BusinessObject[] selectedBusinessObjects);
		void InitialiseFindBoxControllerLink(ZController controller);
		void SetFindBoxCodeDescription(BusinessObject bizo);
	}

	public interface IEmbeddedModulePopupOKButtonStrategy
	{
		void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObject);
		void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters);
	}
}
