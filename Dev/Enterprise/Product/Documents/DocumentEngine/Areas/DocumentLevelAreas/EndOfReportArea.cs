using System.Collections.Generic;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.Areas
{
	sealed class EndOfReportArea : Area
	{
		public EndOfReportArea(int start, int end, Report report, string data)
			: base(start, start + 1, report, data)
		{
			ShouldDelete = true;
		}

		EndOfReportArea() { }

		public override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("#EndOfReport",
				ResString.GetMultilingualString("7d0e65ca-d140-4943-a1c0-2bfc9cc00fd1",
					@"This indicates the end of the template, and *MUST* be specified to indicate the end of the last Area in the Template.

You can only have one of these, and everything after this identifier will be ignored."));
		}

		public override Area Clone(int position)
		{
			throw new CloneAreaException("Cannot clone a #EndOfReport area.");
		}

		public override List<Area> Parents
		{
			get { return null; }
		}

		public override bool CanCloseAPage
		{
			get { return true; }
		}
	}
}
