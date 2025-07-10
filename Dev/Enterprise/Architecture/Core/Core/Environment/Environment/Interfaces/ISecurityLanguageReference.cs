using CargoWise.Types;

namespace Enterprise.ZArchitecture.Core
{
	public interface ISecurityLanguageReference
	{
		ZString FullLanguageCode { get; }
		MultilingualString Description { get; }
	}
}
