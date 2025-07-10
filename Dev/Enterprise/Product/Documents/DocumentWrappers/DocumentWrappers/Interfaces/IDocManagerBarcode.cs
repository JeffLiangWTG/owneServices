using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	/// <summary>
	/// For DocumentWrappers that require a DocManager barcode.
	/// </summary>
	public interface IDocManagerBarcode
	{
		ZString BarcodeTextForFont { get; }
		ZString BarcodeText { get; }
	}
}
