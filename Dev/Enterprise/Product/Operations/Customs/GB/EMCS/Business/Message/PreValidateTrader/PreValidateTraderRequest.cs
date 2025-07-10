using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Enterprise.Customs.GB.EMCS.Business.PreValidateTraderInfo;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class PreValidateTraderRequest
	{
		public ExciseTraderValidationRequest ExciseTraderValidationRequest { get; set; }

		public string SerializeAsJson() => JsonSerializer.Serialize(this, jsonOptions);

		static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		public static PreValidateTraderRequest CreateRequest(TraderData trader, string productCodes) => new PreValidateTraderRequest
		{
			ExciseTraderValidationRequest = new ExciseTraderValidationRequest
			{
				ExciseTraderRequest = new ExciseTraderRequest
				{
					ExciseRegistrationNumber = trader.TraderID,
					EntityGroup = trader.TraderType,
					ValidateProductAuthorisationRequest = productCodes.Split(',').Select(code => new ValidateProductAuthorisationRequest
					{
						Product = new ExciseProduct
						{
							ExciseProductCode = code
						}
					}).ToArray()
				}
			}
		};
	}

	public sealed class ExciseTraderValidationRequest
	{
		public ExciseTraderRequest ExciseTraderRequest { get; set; }
	}

	public sealed class ExciseTraderRequest
	{
		public string ExciseRegistrationNumber { get; set; }
		public string EntityGroup { get; set; }
		public ValidateProductAuthorisationRequest[] ValidateProductAuthorisationRequest { get; set; }
	}

	public sealed class ValidateProductAuthorisationRequest
	{
		public ExciseProduct Product { get; set; }
	}

	public sealed class ExciseProduct
	{
		public string ExciseProductCode { get; set; }
	}
}
