using System;
using System.Reflection;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.UserControls.Testing
{
	sealed class ComponentToolboxVisibilityTestCase : ComponentToolboxVisibilityBaseTestCase
	{
		protected override Type[] GetExpectedToolboxVisibleComponentTypes()
		{
			return new Type[]
			{
				typeof(ZAddressControl),
				typeof(ZTextBoxWithDetailsOnNote),
				typeof(ZTemplateTabControl),
				typeof(ZTextBoxWithDetailsOnNote),
				typeof(ColourLegend),
				typeof(ZStmNotePopupButton),
				typeof(CustomPropertiesControl),
				typeof(ProcessTemplateCustomFieldsControl),
				typeof(CustomPropertiesCollectionDetailsControl),
				typeof(ProcessTemplateCustomFieldsCollectionDetailsControl),
				typeof(ZSearchBox),
				typeof(ZSqlTextBox),
				typeof(RelatedJobsUserControl),
				typeof(ZStmALogUserControl),
				typeof(ZTreeViewAdv),
				typeof(ZTreeViewControl),
			};
		}

		protected override Assembly TargetAssembly
		{
			get
			{
				return Assembly.Load("Enterprise.ZArchitecture.GUI.UserControls");
			}
		}
	}
}
