using System.Windows.Forms;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Grid.Internal;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.UniversalCopy.GUI
{
	public class UniversalCopyMultiCombinationControl : ZMultiCombinationControl
	{
		protected override void EditCore(CurrencyManager source, string mappingName)
		{
			base.EditCore(source, mappingName);

			if (ControlType == FieldType.Guid)
			{
				var listUserControl = (ZPopupFindBox)CurrentEditor;
				listUserControl.ModuleID = GetModuleID(source.GetCurrent());
				SetFindBoxEnable(listUserControl);
			}
		}

		public void SetFindBoxEnable(ZPopupFindBox control)
		{
			control.Enabled = (control.ModuleID != ModuleIDs.NotAssigned);
		}

		public ModuleIdentifier GetModuleID(object currentItem)
		{
			var moduleId = ModuleIDs.NotAssigned;

			if (currentItem is PropertyCopyTemplateBizo bizo)
			{
				if (bizo.ModuleId == null)
				{
					if (GetPropertyListModuleIdMethod != null)
					{
						moduleId = GetPropertyListModuleIdMethod(bizo);
					}
					bizo.ModuleId = moduleId;
				}
				moduleId = bizo.ModuleId;
			}

			return moduleId;
		}

		public GetPropertyListModuleIdDelegate GetPropertyListModuleIdMethod { get; set; }
	}

	public delegate ModuleIdentifier GetPropertyListModuleIdDelegate(PropertyCopyTemplateBizo propertyCopyTemplateBizo);
}
