using Enterprise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CalcEditRegistryItemEditor))]
	sealed class DecimalCalcEditRegistryItemEditorTest : CalcEditRegistryItemEditorTest
	{
		public void TestDecimalPlaces()
		{
			using (var editorPane = Editor.NewWinFormsEditorPane())
			{
				var calcEditForThisEditorPane = (ZCalcEdit)editorPane.Controls[0];
				AssertEquals("EditorPane.Decimals", 3, calcEditForThisEditorPane.Decimals);
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CalcEditRegistryItemEditor(new DecimalRegistryDataType(), new NumericRegistryEditorInfo(3));
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DecimalRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { 1.234m, 2m, 3242222422.1m };
		}

		protected override int MaxLength
		{
			get
			{
				int result = 0;

				using (var defaultCalcEdit = new ZCalcEdit())
				{
					result = defaultCalcEdit.MaxLength;
				}

				return result;
			}
		}

		protected override string MaxLengthString
		{
			get { return "00000000000000000000000000000.000"; }
		}

		#endregion

	}
}
