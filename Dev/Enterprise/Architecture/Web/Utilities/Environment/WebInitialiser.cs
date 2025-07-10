using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Initialisation;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment
{
	public static class WebInitialiser
	{
		public static void Initialise(bool? enableErrorReport = null, EnvProvider envProvider = null)
		{
			if (!alreadyInitialisedForWebServices.Value)
			{
				Initialiser.InitialiseWeb(enableErrorReport, new WebExceptionReporter(), envProvider);
				alreadyInitialisedForWebServices.Value = true;
			}
		}

		static readonly Overridable<bool> alreadyInitialisedForWebServices = new Overridable<bool>(false);
	}
}
