using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class GmailOAuth2JsonFileRegistryDataType : RegistryDataTypeWithJsonSerializer<GmailOAuth2JsonFile>
	{
		public GmailOAuth2JsonFileRegistryDataType()
			: base(RegistryDataTypes.Codes.Binary, new GmailOAuth2JsonFile())
		{
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new GmailOAuth2JsonFileRegistryEditorInfo();
		}

		protected override GmailOAuth2JsonFile CloneValue(GmailOAuth2JsonFile value)
		{
			GmailOAuth2JsonFile clonedJsonFile = null;
			if (value != null)
			{
				clonedJsonFile = new GmailOAuth2JsonFile
				{
					JsonText = value.JsonText,
					FileName = value.FileName
				};
			}

			return clonedJsonFile;
		}
	}
}
