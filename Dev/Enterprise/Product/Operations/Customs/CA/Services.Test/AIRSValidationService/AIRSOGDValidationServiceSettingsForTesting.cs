using CargoWise.Types;

namespace Enterprise.Customs.CA.Services.Testing
{
	public sealed class AIRSOGDValidationServiceSettingsForTesting : IAIRSValidationServiceSettings
	{
		public ZString Uri => "http://www.testuri.com";

		public string SchemaVersion => "1.0";

		public string Key => "Test";

		public bool FrenchPreferred => false;

		public string UserName => string.Empty;

		public string Password => string.Empty;

		public ZString WebProxyUri => ZString.Empty;
	}
}
