using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IAddInfoPropertyData
	{
		string PropertyName { get; }
		IZType OriginalValue { get; set; }
		IZType Value { get; set; }
	}
}
