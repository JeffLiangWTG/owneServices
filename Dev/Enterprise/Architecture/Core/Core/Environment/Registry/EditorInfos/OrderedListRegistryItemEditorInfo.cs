using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class OrderedListRegistryItemEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(StringRegistryDataType); }
		}
	}
}
