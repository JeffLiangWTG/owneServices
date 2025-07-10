namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	public abstract class ZFilterStripConstants
	{
		public abstract class CssClasses
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "css classname should not be translated")]
			public const string Clear = "clear";
		}

		public abstract class Cells
		{
			public static int FilterDescriptionWidth
			{
				get { return Controls.FilterDescriptionDropListWidth + Spacing.LeftOrRightCellPadding; }
			}

			public static int FilterClauseWidth
			{
				get { return Controls.FilterClauseDropListWidth + Spacing.LeftOrRightCellPadding; }
			}

			public const int FilterControlsWidth = 355;
			public const int FilterAddButtonWidth = 85;
		}

		public abstract class Controls
		{
			public const int FilterDescriptionDropListWidth = 530;
			public const int FilterClauseDropListWidth = 90;
			public const int FilterLayoutsDropListWidth = 150;
			public const int ButtonHeight = 21;
			public const int AddButtonWidth = 25;
		}

		public abstract class Spacing
		{
			public const int Button = 10;
			public const int LeftOrRightCellPadding = 10;
			public const int AfterLabelInFilterClauseCell = LeftOrRightCellPadding - 2;
		}
	}
}
