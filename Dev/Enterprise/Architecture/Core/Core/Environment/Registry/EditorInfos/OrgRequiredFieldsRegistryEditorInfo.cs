using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class OrgRequiredFieldsRegistryEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(BinaryRegistryDataType); }
		}
	}
}
