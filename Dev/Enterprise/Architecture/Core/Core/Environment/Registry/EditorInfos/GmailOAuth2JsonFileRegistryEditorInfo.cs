using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class GmailOAuth2JsonFileRegistryEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited
		{
			get => typeof(StringRegistryDataType);
		}
	}

	[Serializable]
	public class GmailOAuth2JsonFile
	{
		public string JsonText { get; set; }
		public string FileName { get; set; }
	}
}
