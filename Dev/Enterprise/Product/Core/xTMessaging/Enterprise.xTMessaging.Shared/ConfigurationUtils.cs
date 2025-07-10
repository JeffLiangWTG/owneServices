using Xware.Xt.Grpc.Config;

namespace Enterprise.xTMessaging.Shared
{
	public static class ConfigurationUtils
	{
		public static bool IsValid(this Configuration config)
		{
			return !string.IsNullOrEmpty(config.Connect) &&
					!string.IsNullOrEmpty(config.CA) &&
					!string.IsNullOrEmpty(config.Application?.URI) &&
					!string.IsNullOrEmpty(config.Application?.Password);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static void StandardizeConfig(Configuration config)
		{
			const string URIPrefix = "xt-application:";
			if (config != null && config.Application != null && !string.IsNullOrEmpty(config.Application?.URI) && !config.Application.URI.StartsWith(URIPrefix))
			{
				config.Application.URI = URIPrefix + config.Application.URI;
			}
		}
	}
}
