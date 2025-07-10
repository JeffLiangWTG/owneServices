using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public abstract class ZNodeModuleDecisionProvider<T> : IModuleDecisionProvider
		where T : class, IBusiness
	{
		protected ZNodeModuleDecisionProvider(ZNode<T> parentNode)
		{
			Argument.NotNull(parentNode, "parentNode");
			this.parentNode = parentNode;
		}

		protected readonly ZNode<T> parentNode;

		public EmbeddedModulePopup Popup { get; set; }

		public bool AllowExcelExport
		{
			get { return false; }
		}

		public bool EnablePreviousNextSupport
		{
			get { return false; }
		}

		public IBusinessObjectCollection List
		{
			get { return GetList(); }
		}

		protected virtual IBusinessObjectCollection GetList()
		{
			return null;
		}

		public virtual bool ShouldDisplayNotifications
		{
			get { return false; }
		}

		public virtual bool ShouldIgnoreAdditionalFilter
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

		public void HandleDefaultAction(BusinessObject[] selectedBusinessObjects)
		{
			var added = TryAddChildren(selectedBusinessObjects.OfType<T>());
			if (added)
			{
				Popup.Close();
			}
		}

		public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
		{
		}

		protected abstract bool TryAddChildren(IEnumerable<T> children);

		public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObject)
		{
			HandleDefaultAction(selectedBusinessObject);
		}

		public void InitialiseFindBoxControllerLink(ZController controller)
		{
		}

		public void SetFindBoxCodeDescription(BusinessObject bizo)
		{
		}
	}
}
