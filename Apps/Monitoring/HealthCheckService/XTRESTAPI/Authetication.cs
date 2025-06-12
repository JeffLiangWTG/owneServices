namespace XH.XT.Monitoring.HealthCheckService.XTRESTAPI
{
	public class AuthenticateRequestUserAccessToken
	{
		[Newtonsoft.Json.JsonProperty("useraccesstoken", Required = Newtonsoft.Json.Required.Always)]
		[System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
		public string UserAccessToken { get; set; }

		[Newtonsoft.Json.JsonProperty("timeout", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public long Timeout { get; set; } = 300L;
	}

	public class AuthenticateRequestUserPassword
	{
		[Newtonsoft.Json.JsonProperty("username", Required = Newtonsoft.Json.Required.Always)]
		[System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
		public string Username { get; set; }

		[Newtonsoft.Json.JsonProperty("password", Required = Newtonsoft.Json.Required.Always)]
		[System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
		public string Password { get; set; }

		[Newtonsoft.Json.JsonProperty("timeout", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public long Timeout { get; set; } = 300L;
	}

	public class AuthenticateResponse
	{
		[Newtonsoft.Json.JsonProperty("username", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public string Username { get; set; }

		[Newtonsoft.Json.JsonProperty("token", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
		public string Token { get; set; }
	}
}
