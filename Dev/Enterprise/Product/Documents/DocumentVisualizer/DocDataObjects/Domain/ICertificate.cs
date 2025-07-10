using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface ICertificate
	{
		ICodeDescription Type { get; }
		ZDateTime ExpiryDate { get; }
	}
}
