using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Winzor.Architecture.Test;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace ZClientEDI.Winzor.Test
{
	class TriageAssistSearchBarUserControlTest
	{
		[Test]
		public async Task TestControlOverlapAsync()
		{
			using var ctx = new EnterpriseTestContext();
			TriageAssistSearchBarUserControl control;
			await ctx.RenderControlOnFormAsync(() =>
			{
				control = new TriageAssistSearchBarUserControl();
				var showSearchOptionsCheckBox = control.Controls.Find("ShowSearchOptionsCheckBox", false).First() as ZCheckBox;
				var searchOptionsGroupBox = control.Controls.Find("SearchOptionsGroupBox", false).First() as ZGroupBox;
				Assert.That(showSearchOptionsCheckBox, Is.Not.Null);
				Assert.That(searchOptionsGroupBox, Is.Not.Null);
				Assert.That(showSearchOptionsCheckBox!.ZIndex, Is.GreaterThan(searchOptionsGroupBox!.ZIndex));

				var shouldSearchSuggestedListCheckBox = searchOptionsGroupBox.Controls.Find("ShouldSearchSuggestedListCheckBox", false).First() as ZCheckBox;
				var shouldSearchDescriptionCheckBox = searchOptionsGroupBox.Controls.Find("ShouldSearchDescriptionCheckBox", false).First() as ZCheckBox;
				Assert.That(shouldSearchSuggestedListCheckBox, Is.Not.Null);
				Assert.That(shouldSearchDescriptionCheckBox, Is.Not.Null);
				Assert.That(shouldSearchSuggestedListCheckBox!.ZIndex, Is.GreaterThan(shouldSearchDescriptionCheckBox!.ZIndex));
				return control;
			});
		}
	}
}
