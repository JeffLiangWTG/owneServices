using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.GUI
{
	public class EntryCustomsBillsPopupOKButtonStrategy : IEmbeddedModulePopupOKButtonStrategy
	{
		public EntryCustomsBillsPopupOKButtonStrategy(IFindBox findBox)
		{
			this.findBox = findBox;
		}

		readonly IFindBox findBox;

		public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObjects)
		{
			if (selectedBusinessObjects.Length == 0 && selectedBusinessObjects == null)
			{
				Globals.Message.ShowInformation(Res.GetString("96967129-B9D6-43E6-A50B-6D7CC8F3DB2F", "You must select at least one record."));
			}
			else
			{
				var popupForm = (EmbeddedModulePopup)findBox.PopupForm;
				var declarationForm = (RefundDeclarationForm)ZFormModaliser.GetParentFormForModalForm(popupForm);
				var refundDeclaration = (CusReconDeclaration)declarationForm.BusinessEntity;
				popupForm.Close();

				if (refundDeclaration != null)
				{
					foreach (KREntryCustomsBillsView selectedBusinessObject in selectedBusinessObjects)
					{
						refundDeclaration.CusReconEntryLines.AddNewLine(selectedBusinessObject);
					}
				}
			}
		}

		public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
		{
		}
	}
}
