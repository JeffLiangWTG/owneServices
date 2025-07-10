using System;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AccChargeCodeListEditRegistryItemEditor))]
	sealed class AccChargeCodeListEditRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestEditorPane()
		{
			using (AccChargeCodeListEditContainer editorPane = (AccChargeCodeListEditContainer)Editor.NewWinFormsEditorPane())
			{
				AssertEquals("EditorPane.Factory", Factory, editorPane.Factory);
				AssertEquals("EditorPane.CompanyPK", Env.CurrentCompany.PK, editorPane.CompanyPK);
			}
		}

		public void TestFilter()
		{
			FallbackLevel fallback = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);

			AccChargeCodeListEditRegistryItemEditor editor = new AccChargeCodeListEditRegistryItemEditor(new AccChargeCodeListRegistryEditorInfo(RegistryFindBoxFilter.None), fallback, null);
			using (AccChargeCodeListEditContainer editorPane = (AccChargeCodeListEditContainer)editor.NewWinFormsEditorPane())
			{
				AssertEquals("EditorPane.Filter", RegistryFindBoxFilter.None, editorPane.Filter);
			}

			editor = new AccChargeCodeListEditRegistryItemEditor(new AccChargeCodeListRegistryEditorInfo(RegistryFindBoxFilter.FreightChargeCode), fallback, null);
			using (AccChargeCodeListEditContainer editorPane = (AccChargeCodeListEditContainer)editor.NewWinFormsEditorPane())
			{
				AssertEquals("EditorPane.Filter", RegistryFindBoxFilter.FreightChargeCode, editorPane.Filter);
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new AccChargeCodeListEditRegistryItemEditor(new AccChargeCodeListRegistryEditorInfo(RegistryFindBoxFilter.None), Fallback, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AccChargeCodeListEditContainer)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AccChargeCodeListEditContainer);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			StringRegistryItem result = new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new AccChargeCodeListRegistryEditorInfo(RegistryFindBoxFilter.None);
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			var cc1 = Factory.NewWithValidTestData<AccChargeCode>();
			cc1.AC_Desc = "CC1";
			Factory.Save();
			return new string[] { cc1.PK.ToString(), Guid.Empty.ToString() };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			if (string.IsNullOrEmpty((string)getValue))
			{
				getValue = Guid.Empty.ToString();
			}

			base.AssertSetAndGetValuesEqual(setValue, getValue);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			Fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}
		FallbackLevel Fallback;
		#endregion
	}
}
