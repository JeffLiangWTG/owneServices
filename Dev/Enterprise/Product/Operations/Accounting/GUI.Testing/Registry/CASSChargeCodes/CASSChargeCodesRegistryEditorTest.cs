namespace Enterprise.Accounting.GUI.Testing
{
	using System;
	using System.Windows.Forms;
	using Enterprise.Accounting.Registry.Business;
	using Enterprise.Accounting.Registry.GUI;
	using Enterprise.Environment;
	using Enterprise.Integration;
	using Enterprise.Registry.GUI;
	using Enterprise.Registry.GUI.Testing;
	using Enterprise.ZArchitecture.Environment;
	using NUnit.Framework;

	[TestedType(typeof(CASSChargeCodesRegistryEditor))]
	public class CASSChargeCodesRegistryEditorTest : RegistryItemEditorTestCase
	{
		public override void TestRegistryItemAcceptsEditorValue()
		{
			foreach (object validRegistryValue in GetValidRegistryValues())
			{
				RegistryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, validRegistryValue);
			}
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new CASSChargeCodesRegistryEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CASSChargeCodesControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CASSChargeCodesControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CASSChargeCodesRegistryItem("", null, null, null);
		}

		protected override object[] GetValidRegistryValues()
		{
			var copy = GetRegistryItemWithSystemStorageLevel().DefaultValue;
			return new object[] { copy };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForCASSChargeCodeTest(registryItem);
		}

		public class RegistryFormForCASSChargeCodeTest : RegistryFormForTest
		{
			public RegistryFormForCASSChargeCodeTest(IRegistryItem registryItem) : base(registryItem)
			{
			}

			public override void DisplayRegistryItem()
			{
				base.DisplayRegistryItem();
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0].Nodes[0];
			}
		}
	}
}


