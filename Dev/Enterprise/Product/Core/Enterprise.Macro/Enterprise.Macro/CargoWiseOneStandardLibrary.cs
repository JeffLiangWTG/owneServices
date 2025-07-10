using System.Collections.Generic;
using CargoWise.Macros;

namespace Enterprise.Macro
{
	public sealed class CargoWiseOneStandardLibrary : MergedMacroLibrary
	{
		protected override IEnumerable<IMacroLibrary> GetLibraries()
		{
			yield return new StandardLibrary();
			yield return new GenericLibrary();
		}
	}
}
