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
	[TestedType(typeof(CashFlowCategoryBasedOnDebtorGroupRegistryItemEditor))]
	public class CashFlowCategoryBasedOnDebtorGroupRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CashFlowCategoryBasedOnDebtorGroupRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CashFlowCategoryBasedOnDebtorGroupControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CashFlowCategoryBasedOnDebtorGroupControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CashFlowCategoryBasedOnDebtorGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}

		protected override object[] GetValidRegistryValues()
		{
			OrgDebtorGroup debtorGroup = Factory.LoadTop1<OrgDebtorGroup>(new ZQuery());
			CashFlowCategoryBasedOnDebtorGroupCollection collection = new CashFlowCategoryBasedOnDebtorGroupCollection();
			CashFlowCategoryBasedOnDebtorGroup configuration = collection.AddNew();
			configuration.OrgGroupPK = debtorGroup.PK;
			configuration.CashFlowCategory = "O01";

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
