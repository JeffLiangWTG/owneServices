using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CalcEditRegistryItemEditor))]
	sealed class DependentIntCalcEditRegistryItemEditorTest : CalcEditRegistryItemEditorTest
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CalcEditRegistryItemEditor(new RegistryItemDependentIntRegistryDataType(null, null), new NumericRegistryEditorInfo(0));
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var result = new IntRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.DataType = new RegistryItemDependentIntRegistryDataType(null, null);
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { 1, 2, 3 };
		}

		protected override int MaxLength
		{
			get { return 9; }
		}

		protected override string MaxLengthString
		{
			get { return "000000000"; }
		}

		#endregion
	}
}
