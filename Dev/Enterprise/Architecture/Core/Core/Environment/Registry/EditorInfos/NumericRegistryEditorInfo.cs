using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class NumericRegistryEditorInfo : RegistryEditorInfo
	{
		public NumericRegistryEditorInfo(int decimalPlaces)
		{
			DecimalPlaces = decimalPlaces;
		}

		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(NumericRegistryDataType<>); }
		}

		public int DecimalPlaces
		{
			get { return fDecimalPlaces; }
			set { fDecimalPlaces = value; }
		}
		int fDecimalPlaces;
	}
}
