using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Registry.Testing
{
	[TestedType(typeof(NctsDefaultConsignorConsigneeRegistryItemEditor))]
	class NctsDefaultConsignorConsigneeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new NctsDefaultConsignorConsigneeRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((NctsDefaultConsignorConsigneeRegistryItemUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(NctsDefaultConsignorConsigneeRegistryItemUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new NctsDefaultConsignorConsigneeRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, new NctsDefaultConsignorConsignee());

		protected override object[] GetValidRegistryValues()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var nctsDefaultConsignorConsignee = new NctsDefaultConsignorConsignee(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			return new object[] { nctsDefaultConsignorConsignee };
		}

		public override void TestEditorPaneLayout()
		{
			var editor = GetEditor();
			using (var control = editor.NewWinFormsEditorPane())
			{
				editor.SetEditorPaneLayout(control, 307, 100);
				CombineAssertions("Editor pane layout: ", () =>
				{
					AssertEquals("Should have the correct width.", 307, control.Width);
					AssertEquals("Should have the correct height.", 100, control.Height);
					AssertEquals("Should be anchored.", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, control.Anchor);
				});
			}
		}
	}
}
