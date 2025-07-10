using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(CashFlowCategoryBasedOnCreditorGroupRegistryItemEditor))]
	public class CashFlowCategoryBasedOnCreditorGroupRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CashFlowCategoryBasedOnCreditorGroupRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CashFlowCategoryBasedOnCreditorGroupControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CashFlowCategoryBasedOnCreditorGroupControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CashFlowCategoryBasedOnCreditorGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}

		protected override object[] GetValidRegistryValues()
		{
			OrgCreditorGroup creditorGroup = Factory.LoadTop1<OrgCreditorGroup>(new ZQuery());
			CashFlowCategoryBasedOnCreditorGroupCollection collection = new CashFlowCategoryBasedOnCreditorGroupCollection();
			CashFlowCategoryBasedOnCreditorGroup configuration = collection.AddNew();
			configuration.OrgGroupPK = creditorGroup.PK;
			configuration.CashFlowCategory = "O01";

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
