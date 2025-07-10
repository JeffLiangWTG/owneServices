
namespace Enterprise.ZArchitecture.Core
{
	public interface IMultilingual
	{
		object GetLocalizedValue(string language);
		object GetUnresolvedValue();
	}
}
