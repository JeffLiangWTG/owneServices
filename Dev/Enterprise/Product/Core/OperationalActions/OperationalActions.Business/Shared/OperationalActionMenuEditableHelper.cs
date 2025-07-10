using CargoWise.Types;
using Enterprise.DocumentEngine;

namespace Enterprise.Services.OperationalActions.Business
{
	static class OperationalActionMenuEditableHelper
	{
		public static bool CanEdit(IOperationalActionMenuEditable menuEditable)
		{
			MenuEditingMode editingMode = menuEditable.EditingMode;
			return (editingMode == MenuEditingMode.AllowAll) || (editingMode == MenuEditingMode.AllowEditingOfSystemDefinedOnly);
		}

		public static bool ReadOnly(IOperationalActionMenuEditable menuEditable)
		{
			bool isSystemDefined =
				!menuEditable.SystemDefinedInfo.BizObj.IsDeleted &&
				(ZBool)menuEditable.SystemDefinedInfo.Value;

			return isSystemDefined && !CanEdit(menuEditable);
		}
	}
}
