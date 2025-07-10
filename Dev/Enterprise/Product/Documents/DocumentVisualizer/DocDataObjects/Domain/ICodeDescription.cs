using CargoWise.ComponentModel;
using CargoWise.Macros;
using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	[BindTo(nameof(Code))]
	public interface ICodeDescription
	{
		[List(nameof(Codes))]
		ZString Code { get; set; }
		ZString Description { get; set; }

		[MacroIgnore]
		object Codes { get; }
	}
}