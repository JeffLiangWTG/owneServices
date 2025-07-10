using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Modules
{
	class SQLFilterOnlyModuleDecisionProvider : IModuleDecisionProvider
	{
		public bool AllowExcelExport => false;
		public bool EnablePreviousNextSupport => false;
		public IBusinessObjectCollection List => null;
		public bool ShouldDisplayNotifications => false;
		public bool ShouldIgnoreAdditionalFilter => false;
		public bool ShouldLoadFilterBizObj => false;
		public bool ShouldSaveFilterBizObj => false;

		public void HandleDefaultAction(BusinessObject[] selectedBusinessObject)
		{
			throw new NotImplementedException();
		}

		public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
		{
			throw new NotImplementedException();
		}

		public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObject)
		{
			throw new NotImplementedException();
		}

		public void InitialiseFindBoxControllerLink(ZController controller)
		{
			throw new NotImplementedException();
		}

		public void SetFindBoxCodeDescription(BusinessObject bizo)
		{
			throw new NotImplementedException();
		}
	}
}
