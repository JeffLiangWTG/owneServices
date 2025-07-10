using System.Collections.Generic;

namespace Enterprise.DocumentEngine.Areas
{
	sealed class BackPageArea : Area
	{
		public BackPageArea(int start, int end, Report report, string data)
			: base(start, end, report, data)
		{
			FirstPageOnly = ContainsParam(nameof(FirstPageOnly));
		}

		BackPageArea() { }

		public override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("#BackPage[:FirstPageOnly]",
				ResString.GetMultilingualString("d37b1a11-4d24-4b68-905e-7f89dae1b171",
					@"Contents of a {0} Area will be inserted after the first page only if the optional '{1}' parameter is specified and every end of page if the optional '{1}' parameter is not specified in the generated output. 

Is used to print a fixed back page when using a duplex printer. E.g: printing the terms and conditions on the back page of a Way Bill.

Please note that you will need to insert your own form feed control commands as back pages sometimes require unusual form feed coding sequences.",
					"#BackPage", ":FirstPageOnly"));
		}

		public override Area Clone(int position)
		{
			Area cloned = new BackPageArea(position, position + fEnd - fStart, ParentReport, "");
			CopyCommonMembers(cloned);
			return cloned;
		}

		public override List<Area> Parents
		{
			get { return null; }
		}

		public override bool CanCloseAPage
		{
			get { return true; }
		}

		public bool FirstPageOnly { get; private set; }
	}
}
