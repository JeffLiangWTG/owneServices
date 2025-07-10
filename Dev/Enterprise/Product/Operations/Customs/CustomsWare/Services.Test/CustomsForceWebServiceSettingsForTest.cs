namespace Enterprise.Customs.CustomsWare.Services.Testing
{
	public class CustomsForceWebServiceSettingsForTest : ICustomsForceWebServiceSettings
	{
		public string Uri
		{
			get;
			set;
		}

		public string UserName => "";
		public string Password => "";
		public string Company => "";
		public string ApplicationID => "";
	}
}
