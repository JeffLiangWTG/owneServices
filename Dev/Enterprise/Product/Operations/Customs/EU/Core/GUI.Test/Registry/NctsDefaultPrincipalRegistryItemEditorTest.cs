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
	[TestedType(typeof(NctsDefaultPrincipalRegistryItemEditor))]
	class NctsDefaultPrincipalRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new NctsDefaultPrincipalRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((NctsDefaultPrincipalRegistryItemUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(NctsDefaultPrincipalRegistryItemUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new NctsDefaultPrincipalRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, new NctsDefaultPrincipal());

		protected override object[] GetValidRegistryValues()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var nctsDefaultPrincipal = new NctsDefaultPrincipal(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			nctsDefaultPrincipal.Principal = organisation.PK;
			return new object[] { nctsDefaultPrincipal };
		}

		public override void TestEditorPaneLayout()
		{
			var editor = GetEditor();
			using (var control = editor.NewWinFormsEditorPane())
			{
				editor.SetEditorPaneLayout(control, 307, 100);
				AssertEquals("should have the correct width", 307, control.Width);
				AssertEquals("should have the correct height", 100, control.Height);
				AssertEquals("should be anchored", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, control.Anchor);
			}
		}
	}
}
