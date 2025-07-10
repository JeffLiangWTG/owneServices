using System.Collections.Generic;

namespace Enterprise.DocumentWrappers.FormatTables
{
	interface IFormatSectionComponent
	{
		bool IncludeHeadingInBody { get; }

		IEnumerable<string> Heading { get; }
		IEnumerable<string> Body { get; }
	}
}
