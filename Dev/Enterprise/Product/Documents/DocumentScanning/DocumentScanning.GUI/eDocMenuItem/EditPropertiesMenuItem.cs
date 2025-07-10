using System;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.DocumentScanning.GUI
{
	public class EditPropertiesMenuItem : eDocMenuItem
	{
		public EditPropertiesMenuItem(EventHandler onClickHandler)
			: base(Constants.EditPropertiesMenuText, onClickHandler)
		{
		}

		public EditPropertiesMenuItem()
			: this(null)
		{
		}

		protected override bool IsDependentOnViewable
		{
			get { return true; }
		}

		public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
		{
			bool enabled = selectedElement != null && !selectedElement.SC_IsDeleted && !multipleSelected;
			if (selectedElement != null)
			{
				enabled = enabled && HasPermissionToModifySelectedDocument(selectedElement);
			}
			return enabled;
		}

		bool HasPermissionToModifySelectedDocument(StorageDocsBase selectedElement)
		{
			return (selectedElement.IsBelongingToCurrentLoginCompany || Environment.Env.Security.EditAllCompanySpecificDocuments.IsAllowed) &&
			 (selectedElement.IsBelongingToCurrentLoginBranch || Environment.Env.Security.EditAllBranchSpecificDocuments.IsAllowed) &&
			 (selectedElement.IsBelongingToCurrentLoginDepartment || Environment.Env.Security.EditAllDepartmentSpecificDocuments.IsAllowed);
		}
	}
}
