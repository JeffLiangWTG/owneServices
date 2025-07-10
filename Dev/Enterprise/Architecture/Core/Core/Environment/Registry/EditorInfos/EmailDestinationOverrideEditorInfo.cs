using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class EmailDestinationOverrideEditorInfo : TextRegistryEditorInfo
	{
		public EmailDestinationOverrideEditorInfo(TextEditorType editorType)
			: base(editorType)
		{
		}

		public override Type BaseDataTypeToBeEdited => typeof(EmailDestinationOverrideDataType);
	}
}
