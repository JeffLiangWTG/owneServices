using System;
using System.Collections.Generic;

namespace Enterprise.DocumentWrappers.FormatTables
{
	internal sealed class FormatTableGroup : IFormatSectionComponent
	{
		public FormatTableGroup(params FormatTable[] tables)
		{
			if (tables == null)
			{
				throw new ArgumentNullException(nameof(tables));
			}

			this.tables = tables;
		}

		public bool IncludeHeadingInBody
		{
			get { return tables.Length > 0 && tables[0].IncludeHeadingInBody; }
		}

		public IEnumerable<string> Heading
		{
			get { return tables.Length > 0 ? tables[0].Heading : Array.Empty<string>(); }
		}

		public IEnumerable<string> Body
		{
			get
			{
				foreach (FormatTable table in tables)
				{
					foreach (string line in table.Body)
					{
						yield return line;
					}
				}
			}
		}

		readonly FormatTable[] tables;
	}
}
