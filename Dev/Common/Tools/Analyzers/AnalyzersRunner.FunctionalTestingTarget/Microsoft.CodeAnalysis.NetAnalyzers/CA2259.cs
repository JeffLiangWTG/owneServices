using System;

namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.NetAnalyzers
{
	class CA2259
	{
		//CA2259:'ThreadStatic' only affects static fields
		[ThreadStatic]
		public string field;
	}
}
