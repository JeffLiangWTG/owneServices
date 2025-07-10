using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public interface IMultilingualRegistryItem
	{
		MultilingualString CaptionMultilingual { get; }
		MultilingualString[] CategoriesMultilingual { get; }
		MultilingualString CategoryMultilingual { get; }
		MultilingualString LocationMultilingual { get; }
	}
}
