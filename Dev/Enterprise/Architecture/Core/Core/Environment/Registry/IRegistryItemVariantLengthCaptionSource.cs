
namespace Enterprise.ZArchitecture.Environment
{
	public interface IRegistryItemVariantLengthCaptionSource : IRegistryItemCaptionSource
	{
		int GetMaxLength(string caption);
	}
}
