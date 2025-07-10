using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.GUI
{
	public class EntryCustomsBillsModuleDecisionProvider : IModuleDecisionProvider
	{
		public EntryCustomsBillsModuleDecisionProvider(IFindBox findBox)
		{
			this.findBox = findBox;
		}
		readonly IFindBox findBox;

		#region IModuleDecisionProvider Members

		public bool EnablePreviousNextSupport
		{
			get { return false; }
		}

		public bool ShouldLoadFilterBizObj
		{
			get { return false; }
		}

		public bool ShouldSaveFilterBizObj
		{
			get { return false; }
		}

		public bool ShouldDisplayNotifications
		{
			get { return false; }
		}

		public bool ShouldIgnoreAdditionalFilter
		{
			get { return false; }
		}

		public bool AllowExcelExport
		{
			get { return false; }
		}

		public IBusinessObjectCollection List
		{
			get { return null; }
		}

		public void HandleDefaultAction(BusinessObject[] selectedBusinessObjects)
		{
			HandleFindBoxOKButton(selectedBusinessObjects);
		}

		public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObjects)
		{
			new EntryCustomsBillsPopupOKButtonStrategy(findBox).HandleFindBoxOKButton(selectedBusinessObjects);
		}

		public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
		{
		}
		public void InitialiseFindBoxControllerLink(ZController controller)
		{
		}
		public void SetFindBoxCodeDescription(BusinessObject bizo)
		{
		}

		#endregion
	}
}
