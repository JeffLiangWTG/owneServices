using System;
using System.Drawing;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.GUI
{
	public class GuidedDecisionMakingTab
	{
		public GuidedDecisionMakingTab()
		{
			GetUnsatisfiedGroupDescriptions = () => ZString.Empty;
			CanNavigateToNext = () => true;
			IsApplicable = () => true;
		}

		readonly Color incompleteColor = Color.FromArgb(176, 216, 255);
		readonly Color currentColor = Color.White;
		readonly Color completedColor = Color.FromArgb(198, 236, 198);
		readonly Color completedWithWarningColor = Color.FromArgb(240, 155, 89);
		readonly Color notApplicableColor = Color.FromArgb(196, 199, 200);

		public ZString Caption { get; set; }
		public ZTabPage Tab { get; set; }
		public GuidedDecisionMakingTabStatus Status { get; set; }
		public Func<bool> CanNavigateToNext { get; set; }
		public Func<ZString> GetUnsatisfiedGroupDescriptions { get; set; }
		public Func<bool> IsApplicable { get; set; }

		public ZString FullCaption
		{
			get
			{
				switch (Status)
				{
					case GuidedDecisionMakingTabStatus.Incomplete:
						return $"{(Caption.Length > 12 ? Caption + "\r\n" + new String(' ', (Caption.Length - 11) / 2) : new String(' ', (12 - Caption.Length) / 2) + Caption + "\r\n")}" + Res.GetString("C2364784-6965-4ADC-BBCA-7B549D02BB7F", "(Incomplete)");
					case GuidedDecisionMakingTabStatus.CompletedWithWarning:
					case GuidedDecisionMakingTabStatus.Completed:
						return $"{(Caption.Length > 11 ? Caption + "\r\n" + new String(' ', (Caption.Length - 10) / 2) : new String(' ', (11 - Caption.Length) / 2) + Caption + "\r\n")}" + Res.GetString("59265C95-4BC5-4D3B-95A6-B4C8783296CD", "(Completed)");
					case GuidedDecisionMakingTabStatus.NotApplicable:
						return $"{(Caption.Length > 5 ? Caption + "\r\n" + new String(' ', (Caption.Length - 4) / 2) : new String(' ', (5 - Caption.Length) / 2) + Caption + "\r\n")}" + Res.GetString("EDD2D7B5-9462-467B-AF90-35E9DF143121", "(N/A)");
					default:
						return Caption;
				}
			}
		}
		public Color CaptionBackground
			=> Status switch
		{
			GuidedDecisionMakingTabStatus.Incomplete => incompleteColor,
			GuidedDecisionMakingTabStatus.Current => currentColor,
			GuidedDecisionMakingTabStatus.Completed => completedColor,
			GuidedDecisionMakingTabStatus.CompletedWithWarning => completedWithWarningColor,
			GuidedDecisionMakingTabStatus.NotApplicable => notApplicableColor,
			_ => notApplicableColor
		};

#if !WINZOR
		public Icon Icon
		{
			get
			{
				switch (Status)
				{
					case GuidedDecisionMakingTabStatus.Completed:
						return Icons.GetIcon(IconTypes.Tick);
					default:
						return null;
				}
			}
		}

#else
		public int IconIndex
			=> Status switch
		{
			GuidedDecisionMakingTabStatus.Completed => Icons.GetImageIndex(IconTypes.Tick),
			_ => -1
		};

#endif
	}
}
