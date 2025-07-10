using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface ISignatureDetails
	{
		object Signature { get; }
		ZString Name { get; }
		ZDateTime DateTime { get; }
	}
}
