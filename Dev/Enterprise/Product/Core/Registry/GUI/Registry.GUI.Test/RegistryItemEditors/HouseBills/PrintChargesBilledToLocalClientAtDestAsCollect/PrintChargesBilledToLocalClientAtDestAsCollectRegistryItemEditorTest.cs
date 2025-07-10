using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(PrintChargesBilledToLocalClientAtDestAsCollectRegistryItemEditor))]
	sealed class PrintChargesBilledToLocalClientAtDestAsCollectRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public override void TestEditorPaneLayout()
		{
			RegistryItemEditor editor = GetEditor();
			using (Control control = editor.NewWinFormsEditorPane())
			{
				editor.SetEditorPaneLayout(control, 666, 333);
				AssertEquals("should have the correct width", 666, control.Width);
				AssertEquals("should have the correct height", 333, control.Height);
				AssertEquals("should be anchored", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, control.Anchor);
			}
		}

		#region Implementation

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new PrintChargesBilledToLocalClientAtDestAsCollectCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new PrintChargesBilledToLocalClientAtDestAsCollectRegistryItemEditor(RegistryItem.DataType, NewFallbackLevel(), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PrintChargesBilledToLocalClientAtDestAsCollectControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			AddSetting(collection, Core.Constants.TransportModes.Sea, ZString.Empty, ZString.Empty);
			AddSetting(collection, Core.Constants.TransportModes.Sea, Core.Constants.CountryCodes.Belgium, Core.Constants.CountryCodes.Australia);
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((PrintChargesBilledToLocalClientAtDestAsCollectControl)editorPane).ReadOnly;
		}

		void AddSetting(PrintChargesBilledToLocalClientAtDestAsCollectCollection collection, string mode, ZString exportCountry, ZString importCountry)
		{
			var transportMode = collection.AddNew();
			transportMode.TransportMode = mode;
			transportMode.ExportCountry = exportCountry;
			transportMode.ImportCountry = importCountry;
		}

		FallbackLevel NewFallbackLevel()
		{
			return new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		#endregion
	}
}
