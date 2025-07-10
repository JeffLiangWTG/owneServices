using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class EmailDestinationOverrideDataType : EmailStringRegistryDataType
	{
		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new EmailDestinationOverrideEditorInfo(TextEditorType.TextBox);
		}
	}
}
