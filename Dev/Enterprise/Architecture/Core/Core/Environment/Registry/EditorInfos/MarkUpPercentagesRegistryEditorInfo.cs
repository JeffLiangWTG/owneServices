using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class MarkUpPercentagesRegistryEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(StringRegistryDataType); }
		}
	}
}
