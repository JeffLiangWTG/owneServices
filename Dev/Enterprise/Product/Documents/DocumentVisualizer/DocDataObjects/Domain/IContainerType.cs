using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IContainerType : ICodeDescription
	{
		ZString ISOCode { get; set; }
		ICodeDescription Type { get; }
	}
}