using System.Drawing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public static class DefinedColorThemes
	{
		public static ColorTheme CargoWiseColorTheme
		{
			get
			{
				if (CWNextFeatureHelper.IsCWNextEnabled())
				{
					return new ColorTheme(defaultThemeName, false)
					{
						FormBackgroundColor = Color.FromArgb(220, 225, 228),
						TabBackgroundColor = Color.FromArgb(244, 246, 247),
						MainFormBackgroundColor = Color.FromArgb(244, 246, 247),
						GridBackgroundColor = Color.FromArgb(255, 255, 255),
						FilterBackgroundColor = Color.FromArgb(244, 246, 247),
						GridReadOnlyColor = Color.FromArgb(196, 199, 200),
						GridAlternatingRowColor = Color.FromArgb(244, 246, 247),
						ToolbarColor = Color.FromArgb(244, 246, 247),
						NavBarBackgroundColor = Color.FromArgb(255, 255, 255),
						NavBarTextColor = Color.FromArgb(255, 255, 255),
						NavBarButtonColor1 = Color.FromArgb(0x37, 0x1e, 0xe1),
						NavBarGroupBackground1 = Color.FromArgb(255, 255, 255),
						NavBarGroupSelected1 = Color.FromArgb(0x1b, 0x1b, 0x1a),
						ButtonColor = Color.FromArgb(196, 199, 200),
						NavBarGroupHeaderBackground = Color.FromArgb(244, 246, 247),
						NavBarRecentPanelBackground = Color.FromArgb(247, 247, 247),
						TitleBarBackground = Color.FromArgb(0x37, 0x1e, 0xe1),
						TitleBarText = Color.FromArgb(255, 255, 255)
					};
				}
				else
				{
					return new ColorTheme(defaultThemeName, false)
					{
						FormBackgroundColor = Color.FromArgb(220, 225, 228),
						TabBackgroundColor = Color.FromArgb(244, 246, 247),
						MainFormBackgroundColor = Color.FromArgb(244, 246, 247),
						GridBackgroundColor = Color.FromArgb(255, 255, 255),
						FilterBackgroundColor = Color.FromArgb(244, 246, 247),
						GridReadOnlyColor = Color.FromArgb(196, 199, 200),
						GridAlternatingRowColor = Color.FromArgb(244, 246, 247),
						ToolbarColor = Color.FromArgb(244, 246, 247),
						NavBarBackgroundColor = Color.FromArgb(255, 255, 255),
						NavBarTextColor = Color.FromArgb(255, 255, 255),
						NavBarButtonColor1 = Color.FromArgb(0, 168, 225),
						NavBarGroupBackground1 = Color.FromArgb(255, 255, 255),
						NavBarGroupSelected1 = Color.FromArgb(143, 212, 0),
						ButtonColor = Color.FromArgb(196, 199, 200),
						NavBarGroupHeaderBackground = Color.FromArgb(244, 246, 247),
						NavBarRecentPanelBackground = Color.FromArgb(247, 247, 247),
						TitleBarBackground = Color.FromArgb(46, 203, 255),
						TitleBarText = Color.FromArgb(255, 255, 255)
					};
				}
			}
		}

		public static ColorTheme ClassicColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("7030bd25-5116-4315-b8a0-c9747a8f315b", "Classic"), false)
			{
				FormBackgroundColor = SystemColors.Control,
				TabBackgroundColor = SystemColors.Control,
				MainFormBackgroundColor = SystemColors.ControlDark,
				GridBackgroundColor = SystemColors.ControlDark,
				FilterBackgroundColor = SystemColors.Control,
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.FromArgb(248, 248, 255),
				ToolbarColor = SystemColors.Control,
				NavBarBackgroundColor = SystemColors.ControlDark,
				NavBarTextColor = SystemColors.ControlDarkDark,
				NavBarButtonColor1 = Color.White,
				NavBarGroupBackground1 = Color.White,
				NavBarGroupSelected1 = Color.White,
				ButtonColor = SystemColors.Control,
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = SystemColors.Control,
				TitleBarBackground = Color.White,
				TitleBarText = SystemColors.ControlDarkDark
			};

		public static ColorTheme ChamoisColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("a31984a0-a856-4789-b338-2b1ede7a728a", "Chamois"), false)
			{
				FormBackgroundColor = Color.FromArgb(253, 246, 230),
				TabBackgroundColor = Color.FromArgb(236, 233, 216),
				MainFormBackgroundColor = Color.FromArgb(253, 246, 230),
				GridBackgroundColor = Color.FromArgb(253, 246, 230),
				FilterBackgroundColor = Color.FromArgb(236, 233, 216),
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.FromArgb(253, 246, 230),
				ToolbarColor = Color.FromArgb(253, 246, 230),
				NavBarBackgroundColor = Color.FromArgb(225, 216, 149),
				NavBarTextColor = Color.White,
				NavBarButtonColor1 = Color.Black,
				NavBarGroupBackground1 = Color.FromArgb(244, 246, 247),
				NavBarGroupSelected1 = Color.FromArgb(254, 147, 9),
				ButtonColor = Color.FromArgb(250, 230, 184),
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = Color.FromArgb(244, 246, 247),
				TitleBarBackground = Color.Black,
				TitleBarText = Color.White
			};

		public static ColorTheme AmethystColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("d7aa705d-ab9a-49eb-9b73-4bee98609b10", "Amethyst"), false)
			{
				FormBackgroundColor = Color.FromArgb(172, 178, 200),
				TabBackgroundColor = Color.FromArgb(208, 203, 218),
				MainFormBackgroundColor = Color.FromArgb(241, 237, 243),
				GridBackgroundColor = Color.FromArgb(241, 237, 243),
				FilterBackgroundColor = Color.FromArgb(222, 227, 233),
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.FromArgb(231, 223, 236),
				ToolbarColor = Color.FromArgb(241, 237, 243),
				NavBarBackgroundColor = Color.FromArgb(182, 172, 199),
				NavBarTextColor = Color.White,
				NavBarButtonColor1 = Color.FromArgb(42, 42, 94),
				NavBarGroupBackground1 = Color.FromArgb(241, 237, 243),
				NavBarGroupSelected1 = Color.FromArgb(117, 89, 174),
				ButtonColor = Color.FromArgb(147, 155, 183),
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = Color.FromArgb(241, 237, 243),
				TitleBarBackground = Color.FromArgb(42, 42, 94),
				TitleBarText = Color.White
			};

		public static ColorTheme ArcticColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("0b1c8d5c-b549-4f5f-8fa7-19c51534e307", "Arctic"), false)
			{
				FormBackgroundColor = Color.FromArgb(172, 178, 200),
				TabBackgroundColor = Color.FromArgb(209, 220, 218),
				MainFormBackgroundColor = Color.FromArgb(231, 241, 240),
				GridBackgroundColor = Color.FromArgb(231, 241, 240),
				FilterBackgroundColor = Color.FromArgb(209, 220, 218),
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.FromArgb(219, 248, 243),
				ToolbarColor = Color.FromArgb(219, 248, 243),
				NavBarBackgroundColor = Color.FromArgb(190, 233, 223),
				NavBarTextColor = Color.White,
				NavBarButtonColor1 = Color.FromArgb(0, 86, 103),
				NavBarGroupBackground1 = Color.FromArgb(231, 241, 240),
				NavBarGroupSelected1 = Color.FromArgb(0, 144, 144),
				ButtonColor = Color.FromArgb(147, 155, 183),
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = Color.FromArgb(231, 241, 240),
				TitleBarBackground = Color.FromArgb(0, 86, 103),
				TitleBarText = Color.White
			};

		public static ColorTheme BerryColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("0dc8644a-007f-4f7e-8b66-6ae7a25e5900", "Berry"), false)
			{
				FormBackgroundColor = Color.FromArgb(172, 178, 200),
				TabBackgroundColor = Color.FromArgb(219, 207, 222),
				MainFormBackgroundColor = Color.FromArgb(234, 221, 228),
				GridBackgroundColor = Color.FromArgb(234, 221, 228),
				FilterBackgroundColor = Color.FromArgb(222, 227, 233),
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.FromArgb(234, 221, 228),
				ToolbarColor = Color.FromArgb(234, 221, 228),
				NavBarBackgroundColor = Color.FromArgb(188, 168, 195),
				NavBarTextColor = Color.White,
				NavBarButtonColor1 = Color.FromArgb(178, 12, 88),
				NavBarGroupBackground1 = Color.FromArgb(234, 221, 228),
				NavBarGroupSelected1 = Color.FromArgb(105, 92, 171),
				ButtonColor = Color.FromArgb(147, 155, 183),
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = Color.FromArgb(234, 221, 228),
				TitleBarBackground = Color.FromArgb(178, 12, 88),
				TitleBarText = Color.White
			};

		public static ColorTheme CoffeeColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("caa1fb5c-bf1a-4fb2-9c3a-5d1ed3eb2771", "Coffee"), false)
			{
				FormBackgroundColor = Color.FromArgb(248, 233, 209),
				TabBackgroundColor = Color.FromArgb(243, 235, 205),
				MainFormBackgroundColor = Color.FromArgb(236, 231, 216),
				GridBackgroundColor = Color.FromArgb(236, 231, 216),
				FilterBackgroundColor = Color.FromArgb(248, 233, 209),
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.FromArgb(236, 231, 216),
				ToolbarColor = Color.FromArgb(236, 231, 216),
				NavBarBackgroundColor = Color.FromArgb(184, 143, 77),
				NavBarTextColor = Color.White,
				NavBarButtonColor1 = Color.Black,
				NavBarGroupBackground1 = Color.Silver,
				NavBarGroupSelected1 = Color.FromArgb(254, 147, 9),
				ButtonColor = Color.FromArgb(239, 204, 146),
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = Color.FromArgb(236, 231, 216),
				TitleBarBackground = Color.Black,
				TitleBarText = Color.White
			};

		public static ColorTheme MochaColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("5f6e0af8-1042-481a-ba9a-9e6e94464a45", "Mocha"), false)
			{
				FormBackgroundColor = Color.FromArgb(253, 246, 230),
				TabBackgroundColor = Color.FromArgb(236, 233, 216),
				MainFormBackgroundColor = Color.FromArgb(236, 231, 216),
				GridBackgroundColor = Color.FromArgb(227, 220, 198),
				FilterBackgroundColor = Color.FromArgb(239, 214, 172),
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.FromArgb(236, 231, 216),
				ToolbarColor = Color.FromArgb(236, 231, 216),
				NavBarBackgroundColor = Color.FromArgb(184, 143, 77),
				NavBarTextColor = Color.White,
				NavBarButtonColor1 = Color.FromArgb(58, 36, 0),
				NavBarGroupBackground1 = Color.FromArgb(236, 231, 216),
				NavBarGroupSelected1 = Color.FromArgb(254, 147, 9),
				ButtonColor = Color.FromArgb(239, 204, 146),
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = Color.FromArgb(236, 231, 216),
				TitleBarBackground = Color.FromArgb(58, 36, 0),
				TitleBarText = Color.White
			};

		public static ColorTheme GrapeColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("8205ba59-dbcf-45b1-bef1-a6396b3e7add", "Grape"), false)
			{
				FormBackgroundColor = Color.FromArgb(172, 178, 200),
				TabBackgroundColor = Color.FromArgb(206, 210, 223),
				MainFormBackgroundColor = Color.FromArgb(223, 226, 235),
				GridBackgroundColor = Color.FromArgb(223, 226, 235),
				FilterBackgroundColor = Color.FromArgb(222, 227, 233),
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.FromArgb(223, 226, 235),
				ToolbarColor = Color.FromArgb(223, 226, 235),
				NavBarBackgroundColor = Color.FromArgb(172, 178, 200),
				NavBarTextColor = Color.White,
				NavBarButtonColor1 = Color.FromArgb(40, 41, 89),
				NavBarGroupBackground1 = Color.FromArgb(223, 226, 235),
				NavBarGroupSelected1 = Color.FromArgb(129, 100, 184),
				ButtonColor = Color.FromArgb(147, 155, 183),
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = Color.FromArgb(223, 226, 235),
				TitleBarBackground = Color.FromArgb(40, 41, 89),
				TitleBarText = Color.White
			};

		public static ColorTheme MarineColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("74e58abe-2c3a-44df-9b24-abb4afd0f9ba", "Marine"), false)
			{
				FormBackgroundColor = Color.FromArgb(172, 178, 200),
				TabBackgroundColor = Color.FromArgb(215, 230, 234),
				MainFormBackgroundColor = Color.FromArgb(221, 232, 232),
				GridBackgroundColor = Color.FromArgb(221, 232, 232),
				FilterBackgroundColor = Color.FromArgb(222, 227, 233),
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.FromArgb(221, 232, 232),
				ToolbarColor = Color.FromArgb(213, 244, 253),
				NavBarBackgroundColor = Color.FromArgb(213, 244, 253),
				NavBarTextColor = Color.White,
				NavBarButtonColor1 = Color.FromArgb(0, 87, 104),
				NavBarGroupBackground1 = Color.FromArgb(221, 232, 232),
				NavBarGroupSelected1 = Color.FromArgb(0, 145, 144),
				ButtonColor = Color.FromArgb(147, 155, 183),
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = Color.FromArgb(221, 232, 232),
				TitleBarBackground = Color.FromArgb(0, 87, 104),
				TitleBarText = Color.White
			};

		public static ColorTheme BeachColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("18110380-8bb8-4820-92af-acc121067f55", "Beach"), false)
			{
				FormBackgroundColor = Color.FromArgb(201, 218, 228),
				TabBackgroundColor = Color.FromArgb(228, 237, 241),
				MainFormBackgroundColor = Color.FromArgb(201, 218, 228),
				GridBackgroundColor = Color.FromArgb(201, 218, 228),
				FilterBackgroundColor = Color.FromArgb(228, 237, 241),
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.OldLace,
				ToolbarColor = Color.FromArgb(201, 218, 228),
				NavBarBackgroundColor = Color.OldLace,
				NavBarTextColor = Color.White,
				NavBarButtonColor1 = Color.Black,
				NavBarGroupBackground1 = Color.FromArgb(245, 250, 255),
				NavBarGroupSelected1 = Color.DarkOrange,
				ButtonColor = Color.FromArgb(153, 185, 204),
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = Color.FromArgb(245, 250, 255),
				TitleBarBackground = Color.Black,
				TitleBarText = Color.White
			};

		public static ColorTheme ClassicBlueColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("67f48379-acb7-49ef-b09a-d97a249148f4", "Classic Blue"), false)
			{
				FormBackgroundColor = Color.FromArgb(160, 191, 245),
				TabBackgroundColor = Color.FromArgb(220, 235, 254),
				MainFormBackgroundColor = Color.FromArgb(220, 235, 254),
				GridBackgroundColor = Color.FromArgb(195, 217, 249),
				FilterBackgroundColor = Color.FromArgb(160, 191, 245),
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.FromArgb(195, 217, 249),
				ToolbarColor = Color.FromArgb(220, 235, 254),
				NavBarBackgroundColor = Color.FromArgb(251, 230, 148),
				NavBarTextColor = Color.White,
				NavBarButtonColor1 = Color.FromArgb(15, 67, 157),
				NavBarGroupBackground1 = Color.FromArgb(220, 235, 254),
				NavBarGroupSelected1 = Color.FromArgb(239, 155, 29),
				ButtonColor = Color.FromArgb(113, 160, 240),
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = Color.FromArgb(220, 235, 254),
				TitleBarBackground = Color.FromArgb(15, 67, 157),
				TitleBarText = Color.White
			};

		public static ColorTheme OliveColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("8da105b4-74cb-4d1a-b1fd-0044f443e997", "Olive (256 color)"), false)
			{
				FormBackgroundColor = SystemColors.Control,
				TabBackgroundColor = SystemColors.Control,
				MainFormBackgroundColor = SystemColors.Control,
				GridBackgroundColor = Color.FromArgb(204, 204, 102),
				FilterBackgroundColor = SystemColors.Control,
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.FromArgb(255, 255, 204),
				ToolbarColor = Color.FromArgb(255, 255, 204),
				NavBarBackgroundColor = Color.FromArgb(153, 153, 51),
				NavBarTextColor = Color.White,
				NavBarButtonColor1 = Color.FromArgb(102, 102, 0),
				NavBarGroupBackground1 = Color.FromArgb(255, 255, 204),
				NavBarGroupSelected1 = Color.FromArgb(204, 204, 51),
				ButtonColor = Color.FromArgb(187, 187, 187),
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = Color.FromArgb(255, 255, 204),
				TitleBarBackground = Color.FromArgb(102, 102, 0),
				TitleBarText = Color.White
			};

		public static ColorTheme WedgewoodColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("e6f3be90-13cf-4d44-b264-51cf4bddcf74", "Wedgewood (256 color)"), false)
			{
				FormBackgroundColor = SystemColors.Control,
				TabBackgroundColor = SystemColors.Control,
				MainFormBackgroundColor = SystemColors.Control,
				GridBackgroundColor = Color.FromArgb(153, 153, 204),
				FilterBackgroundColor = SystemColors.Control,
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.FromArgb(204, 204, 255),
				ToolbarColor = Color.FromArgb(204, 204, 255),
				NavBarBackgroundColor = Color.FromArgb(153, 153, 255),
				NavBarTextColor = Color.White,
				NavBarButtonColor1 = Color.FromArgb(51, 0, 153),
				NavBarGroupBackground1 = Color.FromArgb(204, 204, 255),
				NavBarGroupSelected1 = Color.FromArgb(102, 0, 255),
				ButtonColor = Color.FromArgb(187, 187, 187),
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = Color.FromArgb(204, 204, 255),
				TitleBarBackground = Color.FromArgb(51, 0, 153),
				TitleBarText = Color.White
			};

		public static ColorTheme ButterscotchColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("4fa9dfca-4de3-454d-b48a-ef7b220ccc39", "Butterscotch (256 color)"), false)
			{
				FormBackgroundColor = SystemColors.Control,
				TabBackgroundColor = SystemColors.Control,
				MainFormBackgroundColor = SystemColors.Control,
				GridBackgroundColor = Color.FromArgb(255, 204, 102),
				FilterBackgroundColor = SystemColors.Control,
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.FromArgb(255, 255, 204),
				ToolbarColor = Color.FromArgb(255, 255, 204),
				NavBarBackgroundColor = Color.FromArgb(255, 153, 51),
				NavBarTextColor = Color.White,
				NavBarButtonColor1 = Color.FromArgb(102, 51, 0),
				NavBarGroupBackground1 = Color.FromArgb(255, 204, 102),
				NavBarGroupSelected1 = Color.FromArgb(204, 102, 0),
				ButtonColor = Color.FromArgb(187, 187, 187),
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = Color.FromArgb(255, 204, 102),
				TitleBarBackground = Color.FromArgb(102, 51, 0),
				TitleBarText = Color.White
			};

		public static ColorTheme CremeDeMentheColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("a8557203-cef0-484a-b1b8-e2add5c347f8", "Creme-de-Menthe (256 color)"), false)
			{
				FormBackgroundColor = SystemColors.Control,
				TabBackgroundColor = SystemColors.Control,
				MainFormBackgroundColor = SystemColors.Control,
				GridBackgroundColor = Color.FromArgb(153, 204, 204),
				FilterBackgroundColor = SystemColors.Control,
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.FromArgb(204, 255, 255),
				ToolbarColor = Color.FromArgb(204, 255, 255),
				NavBarBackgroundColor = Color.FromArgb(102, 204, 204),
				NavBarTextColor = Color.White,
				NavBarButtonColor1 = Color.FromArgb(0, 102, 102),
				NavBarGroupBackground1 = Color.FromArgb(153, 255, 255),
				NavBarGroupSelected1 = Color.FromArgb(0, 153, 153),
				ButtonColor = Color.FromArgb(187, 187, 187),
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = Color.FromArgb(153, 255, 255),
				TitleBarBackground = Color.FromArgb(0, 102, 102),
				TitleBarText = Color.White
			};

		public static ColorTheme SilverColorTheme =>
			new ColorTheme(ResString.GetMultilingualString("59b54606-3c50-48b9-9d30-4302aaa61e06", "Silver Bullet"), false)
			{
				FormBackgroundColor = Color.FromArgb(201, 218, 228),
				TabBackgroundColor = Color.FromArgb(228, 237, 241),
				MainFormBackgroundColor = Color.FromArgb(201, 218, 228),
				GridBackgroundColor = Color.FromArgb(201, 218, 228),
				FilterBackgroundColor = Color.FromArgb(228, 237, 241),
				GridReadOnlyColor = Color.FromArgb(196, 199, 200),
				GridAlternatingRowColor = Color.FromArgb(230, 248, 255),
				ToolbarColor = Color.FromArgb(201, 218, 228),
				NavBarBackgroundColor = Color.FromArgb(215, 215, 215),
				NavBarTextColor = Color.White,
				NavBarButtonColor1 = Color.FromArgb(147, 147, 147),
				NavBarGroupBackground1 = Color.FromArgb(201, 218, 228),
				NavBarGroupSelected1 = Color.FromArgb(0, 164, 228),
				ButtonColor = Color.FromArgb(153, 185, 204),
				NavBarGroupHeaderBackground = Color.Transparent,
				NavBarRecentPanelBackground = Color.FromArgb(201, 218, 228),
				TitleBarBackground = Color.FromArgb(147, 147, 147),
				TitleBarText = Color.White
			};

		static readonly ResourceString defaultThemeName = ResString.GetMultilingualString("2CCECB61-7DC1-4D0C-9B34-B0B5D874335F", "Default");
	}
}
