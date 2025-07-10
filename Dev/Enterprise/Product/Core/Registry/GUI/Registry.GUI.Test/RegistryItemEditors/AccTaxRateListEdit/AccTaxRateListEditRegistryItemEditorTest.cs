using System;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AccTaxRateListEditRegistryItemEditor))]
	sealed class AccTaxRateListEditRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestEditorPane()
		{
			using (AccTaxRateListEditContainer editorPane = (AccTaxRateListEditContainer)Editor.NewWinFormsEditorPane())
			{
				AssertEquals("EditorPane.Factory", Factory, editorPane.Factory);
				AssertEquals("EditorPane.CompanyPK", Env.CurrentCompany.PK, editorPane.CompanyPK);
			}
		}

		public void TestFilter()
		{
			FallbackLevel fallback = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);

			AccTaxRateListEditRegistryItemEditor editor = new AccTaxRateListEditRegistryItemEditor(new AccTaxRateListRegistryEditorInfo(RegistryFindBoxFilter.None), fallback, null);
			using (AccTaxRateListEditContainer editorPane = (AccTaxRateListEditContainer)editor.NewWinFormsEditorPane())
			{
				AssertEquals("EditorPane.Filter", RegistryFindBoxFilter.None, editorPane.Filter);
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new AccTaxRateListEditRegistryItemEditor(new AccTaxRateListRegistryEditorInfo(RegistryFindBoxFilter.None), Fallback, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AccTaxRateListEditContainer)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AccTaxRateListEditContainer);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			StringRegistryItem result = new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new AccTaxRateListRegistryEditorInfo(RegistryFindBoxFilter.None);
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new string[] { Guid.NewGuid().ToString(), Guid.Empty.ToString() };
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
