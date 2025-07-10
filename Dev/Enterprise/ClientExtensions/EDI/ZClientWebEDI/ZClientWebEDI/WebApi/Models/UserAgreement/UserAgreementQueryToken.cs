using CargoWise.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class UserAgreementQueryToken
	{
		public string Source { get; set; }

		public string Type { get; set; }

		public ZGuid FromContact { get; set; } = ZGuid.Empty;

		public ZGuid Database { get; set; } = ZGuid.Empty;

		public ZGuid Enterprise { get; set; } = ZGuid.Empty;

		public string AgreementType { get; set; }

		public string RecipientName { get; set; }

		public string RecipientJobTitle { get; set; }

		public string RecipientEmail { get; set; }

		public bool SendAgreementCopy { get; set; }

		public static UserAgreementQueryToken FromJson(string json)
		{
			JObject jObject;
			try
			{
				jObject = JObject.Parse(json);
			}
			catch (JsonReaderException)
			{
				return null;
			}

			var token = new UserAgreementQueryToken
			{
				Source = jObject[nameof(Source)]?.ToString(),
				Type = jObject[nameof(Type)]?.ToString(),
				AgreementType = jObject[nameof(AgreementType)]?.ToString(),
				RecipientName = jObject[nameof(RecipientName)]?.ToString(),
				RecipientJobTitle = jObject[nameof(RecipientJobTitle)]?.ToString(),
				RecipientEmail = jObject[nameof(RecipientEmail)]?.ToString(),
				SendAgreementCopy = jObject[nameof(SendAgreementCopy)]?.ToObject<bool>() ?? false
			};

			if (ZGuid.TryParse(jObject[nameof(FromContact)]?.ToString(), out var contactResult))
			{
				token.FromContact = contactResult;
			}

			if (ZGuid.TryParse(jObject[nameof(Enterprise)]?.ToString(), out var enterpriseResult))
			{
				token.Enterprise = enterpriseResult;
			}

			if (ZGuid.TryParse(jObject[nameof(Database)]?.ToString(), out var databaseResult))
			{
				token.Database = databaseResult;
			}

			return token;
		}

		public string ToJson()
		{
			var jObject = new JObject
			{
				[nameof(Source)] = Source,
				[nameof(Type)] = Type,
				[nameof(FromContact)] = FromContact.ToString(),
				[nameof(Enterprise)] = Enterprise.ToString(),
				[nameof(Database)] = Database.ToString(),
				[nameof(AgreementType)] = AgreementType,
				[nameof(RecipientName)] = RecipientName,
				[nameof(RecipientJobTitle)] = RecipientJobTitle,
				[nameof(RecipientEmail)] = RecipientEmail,
				[nameof(SendAgreementCopy)] = SendAgreementCopy
			};

			return jObject.ToString();
		}
	}
}
