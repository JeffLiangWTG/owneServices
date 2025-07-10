using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using CargoWise.Application;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZErrorPageLocalizationTest : ZPageTestCase
	{
		class ZTestErrorPage : ZTestPage
		{
			protected override Uri RequestUrl
			{
				get { return new Uri("http://www.test.com/ediWeb/Error.aspx?data=xyz"); }
			}
		}

		protected override ZPage GetNewZPage()
		{
			return new ZTestErrorPage();
		}

		public void TestNoLanguageOnErrorPage()
		{
			try
			{
				var allowedLanguages = WebDataRegistry.Instance.AllowedLanguages.Value;
				allowedLanguages.RemoveAll();
				var allowedItem = allowedLanguages.AddNew();
				allowedItem.Code = Enterprise.Core.SharedConstants.Languages.English;
				allowedItem = allowedLanguages.AddNew();
				allowedItem.Code = Enterprise.Core.SharedConstants.Languages.German;
				allowedItem = allowedLanguages.AddNew();
				allowedItem.Code = Enterprise.Core.SharedConstants.Languages.French;
				WebDataRegistry.Instance.AllowedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedLanguages);

				AssertNull("LanguageSelection", Page.Footer.Controls.Cast<Control>().FirstOrDefault(control => control.ID == "LanguageSelection"));

				Page.Request.Cookies.Add(new HttpCookie("Language", Enterprise.Core.SharedConstants.Languages.French));
				Page.InitializeCultureInternal();
				AssertEquals(Enterprise.Core.SharedConstants.Languages.English, Res.CurrentLanguage);
			}
			finally
			{
				ObjectFactory.Get<IResourceStrings>().CurrentLanguage = Res.DefaultLanguage;
			}
		}
	}
}
