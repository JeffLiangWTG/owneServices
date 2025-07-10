using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Environment.Registry.DataTypesAndValidators;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DeleteExpiredRatesRegistryItemEditor))]
	sealed class DeleteExpiredRatesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestWebPrintNudgeRegistryItemEditor()
		{
			try
			{
				_ = new DeleteExpiredRatesRegistryItemEditor(null, null);
				AssertEquals("DeleteExpiredRatesRegistryItemEditor", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DeleteExpiredRatesRegistryItemEditor(new DeleteExpiredRatesRegistryDataType(DeleteExpiredRatesForTest), new DeleteExpiredRatesRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, DeleteExpiredRatesForTest));
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DeleteExpiredRatesUserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DeleteExpiredRatesRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, DeleteExpiredRatesForTest);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { DeleteExpiredRatesForTest };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DeleteExpiredRatesUserControl)editorPane).ReadOnly;
		}

		protected override bool CanNotHaveReferenceEquality => false;

		DeleteExpiredRates DeleteExpiredRatesForTest => new DeleteExpiredRates();
	}
}
