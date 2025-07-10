using System.Text.Json.Serialization;

namespace Enterprise.Customs.IT.Business;

sealed class AutomaticSignatureMetadata
{
	[JsonIgnore]
	public SignatureOperation Operation { get; set; }

	[JsonPropertyName("custom.MessageSubType")]
	public string MessageSubType
	{
		get
		{
			if (Operation == SignatureOperation.Amendment)
			{
				return EDIMessageTypeList.Codes.Amendment;
			}
			if (Operation == SignatureOperation.Cancellation)
			{
				return EDIMessageTypeList.Codes.Cancellation;
			}

			return null;
		}
	}

	[JsonPropertyName("custom.IT.Signature")]
	public string Signature => "Y";

	[JsonPropertyName("custom.IT.SignatureDelegatedUser")]
	public string DelegatedUser { get; set; }

	[JsonPropertyName("custom.IT.SignatureUser")]
	public string SignatureUser { get; set; }
}

public enum SignatureOperation
{
	New = 0,
	Cancellation = 1,
	Amendment = 2,
}
