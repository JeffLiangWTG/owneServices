using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	/// <summary>
	/// Summary description for IDocManagerPlaceholderBarcode.
	/// </summary>
	public interface IDocManagerPlaceholderBarcode
	{
		ZString BarcodeTextForFontPlaceholder { get; }
		ZString BarcodeTextPlaceholder { get; }
	}
}
