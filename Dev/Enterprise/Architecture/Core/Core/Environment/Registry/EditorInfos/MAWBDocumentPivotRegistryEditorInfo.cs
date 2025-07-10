using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class MAWBDocumentPivotRegistryEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(BinaryRegistryDataType); }
		}
	}
}
