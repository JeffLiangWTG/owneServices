using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Registry
{
	public class EORINumberStringRegistryDataType : StringRegistryDataType
	{
		public EORINumberStringRegistryDataType()
		{
			CharacterCase = CharacterCase.Upper;
			MinLength = 3;
			MaxLength = 17;
		}
	}
}
