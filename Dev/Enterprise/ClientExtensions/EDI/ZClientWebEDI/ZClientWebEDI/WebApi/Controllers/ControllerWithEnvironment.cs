using System.Globalization;
using System.Web.Http;
using CargoWise.Application;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public abstract class ControllerWithEnvironment : ApiController
	{
		protected ControllerWithEnvironment()
		{
			if (Env.CurrentCompany == null)
			{
				WebAppEnvironment.Setup();

				var language = "";
				try
				{
					language = Culture.GetLanguageForCulture(WebEnvShared.ClientCulture);
				}
				catch (CultureNotFoundException) { }
				ObjectFactory.Get<IResourceStrings>().CurrentLanguage = string.IsNullOrEmpty(language) ? Res.DefaultLanguage : language;
			}
		}
	}
}
