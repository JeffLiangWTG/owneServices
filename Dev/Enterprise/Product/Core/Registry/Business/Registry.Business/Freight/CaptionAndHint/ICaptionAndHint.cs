using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	public interface ICaptionAndHint
	{
		ZString Caption { get; set; }
		ZString Hint { get; }
		int CaptionMaxLength { get; set; }
	}
}
