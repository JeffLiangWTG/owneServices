using System;
using System.IO;
using System.Web.UI;
using CargoWise.Data;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class LanguageSelectionControlTest : WebControlTest
	{
		public void TestLanguageSelectionControl()
		{
			var allowedLanguages = WebDataRegistry.Instance.AllowedLanguages.Value;
			allowedLanguages.RemoveAll();
			var allowedItem = allowedLanguages.AddNew();
			allowedItem.Code = Enterprise.Core.SharedConstants.Languages.English;
			WebDataRegistry.Instance.AllowedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedLanguages);

			var control = new LanguageSelectionControlForTest();
			control.Page = new ZPage();
			var s = new StringWriter();
			using (var writer = new HtmlTextWriter(s))
			{
				control.RenderForTesting(writer);
			}
			AssertEquals("", s.ToString());

			allowedItem = allowedLanguages.AddNew();
			allowedItem.Code = Enterprise.Core.SharedConstants.Languages.German;
			allowedItem = allowedLanguages.AddNew();
			allowedItem.Code = Enterprise.Core.SharedConstants.Languages.French;
			WebDataRegistry.Instance.AllowedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedLanguages);
			s = new StringWriter();
			using (var writer = new HtmlTextWriter(s))
			{
				control.RenderForTesting(writer);
			}
			AssertContains("Language: <select ", s.ToString());
			AssertContains("id=\"LanguageList\"", s.ToString());
			AssertContains("<option selected=\"selected\" value=\"EN\">English</option>", s.ToString());
			AssertContains("<option value=\"DE-DE\">Deutsch</option>", s.ToString());
			AssertContains("<option value=\"FR-FR\">Fran&#231;ais</option>", s.ToString());
			AssertNotContains("<option value=\"ES-ES\">", s.ToString());

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.German))
			{
				s = new StringWriter();
				using (var writer = new HtmlTextWriter(s))
				{
					control.RenderForTesting(writer);
				}
				AssertContains("Sprache: <select ", s.ToString());
				AssertContains("id=\"LanguageList\"", s.ToString());
				AssertContains("<option value=\"EN\">English</option>", s.ToString());
				AssertContains("<option selected=\"selected\" value=\"DE-DE\">Deutsch</option>", s.ToString());
				AssertContains("<option value=\"FR-FR\">Fran&#231;ais</option>", s.ToString());
				AssertNotContains("<option value=\"ES-ES\">", s.ToString());
			}

			using (Res.TemporarilySwitchLanguage(Constants.Languages.EnglishBritish))
			{
				s = new StringWriter();
				using (var writer = new HtmlTextWriter(s))
				{
					control.RenderForTesting(writer);
				}
				AssertContains("Language: <select ", s.ToString());
				AssertContains("id=\"LanguageList\"", s.ToString());
				AssertContains("<option selected=\"selected\" value=\"EN\">English</option>", s.ToString());
				AssertContains("<option value=\"DE-DE\">Deutsch</option>", s.ToString());
				AssertContains("<option value=\"FR-FR\">Fran&#231;ais</option>", s.ToString());
				AssertNotContains("<option value=\"ES-ES\">", s.ToString());
			}
		}

		public void TestDisconnectedDB()
		{
			using (ZPage testPage = new ZPage())
			{
				try
				{
					Db.Connection.CloseConnection();

					var control = new LanguageSelectionControlForTest();
					testPage.Controls.Add(control);
					var s = new StringWriter();

					using (var writer = new HtmlTextWriter(s))
					{
						AssertNoExceptionThrown(() => control.RenderForTesting(writer));
					}
				}
				finally
				{
					testPage.Controls.Remove(Control);
				}
			}
		}

		protected override Control GetNewControl()
		{
			return new LanguageSelectionControlForTest();
		}

		class LanguageSelectionControlForTest : LanguageSelectionControl
		{
			#region Test Properties

			public void RenderForTesting(HtmlTextWriter writer) => Render(writer);

			#endregion
		}
	}
}
