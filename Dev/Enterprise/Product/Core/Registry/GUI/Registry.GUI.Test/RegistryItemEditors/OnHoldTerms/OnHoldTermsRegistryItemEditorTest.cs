using System;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OnHoldTermsRegistryItemEditor))]
	sealed class OnHoldTermsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new OnHoldTermsRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((OnHoldTermsRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(OnHoldTermsRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new OnHoldTermsRegistryItem("", null, null, null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new OnHoldTerms() };
		}
	}
}
