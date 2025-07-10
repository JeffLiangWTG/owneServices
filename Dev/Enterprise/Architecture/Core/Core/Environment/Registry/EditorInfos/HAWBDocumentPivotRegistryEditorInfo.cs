using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class HAWBDocumentPivotRegistryEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(BinaryRegistryDataType); }
		}
	}
}
