using System;
using System.Drawing;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class GuidedDecisionMakingTabTest : TestCase
	{
		public void TestFullCaption()
		{
			new StatusBasedPropertyTestCase<ZString>(tab => tab.FullCaption)
				.AssertTrue("GuidedDecisionMakingTab.FullCaption should be {0} when Status is {1} when caption is 'Caption'.",
				"  Caption\r\n(Incomplete)",
				"Caption",
				"  Caption\r\n(Completed)",
				"Caption\r\n (N/A)",
				"  Caption\r\n(Completed)");

			new StatusBasedPropertyTestCase<ZString>(tab => tab.FullCaption, "AdditionalCode")
				.AssertTrue("GuidedDecisionMakingTab.FullCaption should be {0} when Status is {1} when caption is 'AdditionalCode'.",
				"AdditionalCode\r\n (Incomplete)",
				"AdditionalCode",
				"AdditionalCode\r\n  (Completed)",
				"AdditionalCode\r\n     (N/A)",
				"AdditionalCode\r\n  (Completed)");
		}

		public void TestCaptionBackground()
		{
			new StatusBasedPropertyTestCase<Color>(tab => tab.CaptionBackground)
				.AssertTrue("GuidedDecisionMakingTab.CaptionBackground should be {0} when Status is {1}.",
				Color.FromArgb(176, 216, 255),
				Color.White,
				Color.FromArgb(198, 236, 198),
				Color.FromArgb(196, 199, 200),
				Color.FromArgb(240, 155, 89));
		}

#if !WINZOR
		public void TestIcon()
		{
			new StatusBasedPropertyTestCase<Icon>(tab => tab.Icon)
				.AssertTrue("GuidedDecisionMakingTab.CaptionBackground should be {0} when Status is {1}.",
				null,
				null,
				Icons.GetIcon(IconTypes.Tick),
				null,
				null);
		}
#else
		public void TestIconIndex()
		{
			var tab = new GuidedDecisionMakingTab();

			tab.Status = GuidedDecisionMakingTabStatus.Completed;
			AssertGreaterThan("GuidedDecisionMakingTab.IconIndex should be greater than -1 when Status is Completed.", tab.IconIndex, -1);

			var otherStatuses = new GuidedDecisionMakingTabStatus[] {
				GuidedDecisionMakingTabStatus.Incomplete,
				GuidedDecisionMakingTabStatus.Current,
				GuidedDecisionMakingTabStatus.NotApplicable,
				GuidedDecisionMakingTabStatus.CompletedWithWarning
			};

			foreach (var status in otherStatuses)
			{
				tab.Status = status;
				AssertEquals($"GuidedDecisionMakingTab.IconIndex should be -1 when Status is {status}.", -1, tab.IconIndex);
			}
		}
#endif

		class StatusBasedPropertyTestCase<T>
		{
			internal StatusBasedPropertyTestCase(Func<GuidedDecisionMakingTab, T> getInterestingProperty, string caption = "Caption")
			{
				this.getInterestingProperty = getInterestingProperty;
				this.tab = new GuidedDecisionMakingTab();
				tab.Caption = caption;
			}

			readonly Func<GuidedDecisionMakingTab, T> getInterestingProperty;

			readonly GuidedDecisionMakingTab tab;

			public void AssertTrue(string message, params T[] expectedValues)
			{
				foreach (var (status, expectedValue) in Enum.GetValues(typeof(GuidedDecisionMakingTabStatus)).Cast<GuidedDecisionMakingTabStatus>().Zip(expectedValues, Tuple.Create))
				{
					tab.Status = status;
					AssertEquals(string.Format(message, expectedValue, status), expectedValue, getInterestingProperty(tab));
				}
			}
		}
	}
}
