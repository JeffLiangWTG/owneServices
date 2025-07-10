using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.GUI
{
	public class KREntryDetailsPopupOKButtonStrategy : IModuleDecisionProvider
	{
		public KREntryDetailsPopupOKButtonStrategy(EmbeddedModulePopup popup, IEmbeddedModulePopupCollection embeddedModulePopupCollection)
		{
			this.popup = popup;
			this.embeddedModulePopupCollection = embeddedModulePopupCollection;
		}

		readonly EmbeddedModulePopup popup;
		readonly IEmbeddedModulePopupCollection embeddedModulePopupCollection;

		public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObjects)
		{
			if (selectedBusinessObjects.Length == 0 && selectedBusinessObjects == null)
			{
				Globals.Message.ShowInformation(Res.GetString("F0031250-1351-4BEB-A0C0-764DB97B8EA8", "You must select at least one record."));
			}
			else
			{
				popup.Close();

				foreach (KREntryHeaderDetailsView selectedBusinessObject in selectedBusinessObjects)
				{
					embeddedModulePopupCollection.AddNewLine(selectedBusinessObject);
				}
			}
		}

		public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
		{
		}

		#region IModuleDecisionProvider Members

		public bool EnablePreviousNextSupport
		{
			get { return true; }
		}

		public bool ShouldLoadFilterBizObj
		{
			get { return true; }
		}

		public bool ShouldSaveFilterBizObj
		{
			get { return true; }
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

		public void InitialiseFindBoxControllerLink(ZController controller)
		{
		}
		public void SetFindBoxCodeDescription(BusinessObject bizo)
		{
		}

		#endregion
	}
}
