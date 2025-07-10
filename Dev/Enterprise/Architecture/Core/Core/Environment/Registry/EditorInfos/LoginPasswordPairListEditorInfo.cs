using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	public class LoginPasswordPairListEditorInfo : CodeDescriptionPairListEditorInfo
	{
		public LoginPasswordPairListEditorInfo()
			: base(true, true, CharacterCasing.Normal, CharacterCasing.Normal,
				ResString.GetMultilingualString("2d1f883e-7fb6-4cd5-a16c-8ca937f9f7dc", "User Login"),
				ResString.GetMultilingualString("46a98958-57f5-411e-9f10-87bc730b99ae", "User Password"))
		{
		}
	}
}
