namespace Enterprise.ZArchitecture.Environment.Email.Html
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "adding HTML tag")]
	public static class EmailHtmlTags
	{
		public static string BR
		{
			get
			{
				return "<br/>";
			}
		}

		public static string B_Start
		{
			get
			{
				return "<b>";
			}
		}

		public static string B_End
		{
			get
			{
				return "</b>";
			}
		}

		public static string PRE_Start
		{
			get
			{
				return "<pre>";
			}
		}

		public static string PRE_End
		{
			get
			{
				return "</pre>";
			}
		}

		public static string UL_Start
		{
			get
			{
				return "<ul>";
			}
		}

		public static string UL_End
		{
			get
			{
				return "</ul>";
			}
		}

		public static string LI_Start
		{
			get
			{
				return "<li>";
			}
		}

		public static string LI_End
		{
			get
			{
				return "</li>";
			}
		}

		public static string BIG_Start
		{
			get
			{
				return "<big>";
			}
		}

		public static string BIG_End
		{
			get
			{
				return "</big>";
			}
		}

		public static string Paragraph_Start
		{
			get
			{
				return "<p>";
			}
		}

		public static string Paragraph_End
		{
			get
			{
				return "</p>";
			}
		}
	}
}
