using System.Collections.Generic;
using CargoWise.Macros;
using CargoWise.Macros.Testing;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	abstract class DocumentMacroTest : TestCaseWithMacros
	{
		protected override IEnumerable<IMacroLibrary> Libraries
		{
			get
			{
				yield return new StandardLibrary();
				yield return new DocumentLibrary();
			}
		}
	}
}