using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class RatesServiceUrlRegistryEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited
		{
			get
			{
				return typeof(RatesServiceUrlRegistryDataType);
			}
		}
	}
}
