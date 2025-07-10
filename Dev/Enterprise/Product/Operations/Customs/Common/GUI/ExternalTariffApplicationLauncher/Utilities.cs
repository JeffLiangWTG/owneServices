using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Common.GUI
{
	public static class Utilities
	{
		public static bool IsBorderWiseMultiLineClassificationEnabled => ZArchitecture.Environment.DataRegistry.Instance.ExternalBorderComplianceTool == ExternalBorderComplianceToolList.Codes.BorderWiseWeb
																		&& ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableWebSocketClient
																		&& ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableMultilineTariffClassification;

		public static string GetCargoWiseClientId()
		{
			var productRegistrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var companyCode = EnvProxy.Instance.CurrentCompany.Code;

			return $"{productRegistrationKey.EnterpriseCode} - {companyCode} - {productRegistrationKey.ServerCode}";
		}

		public static string GetOrgCode()
		{
			var emailAddress = Env.CurrentUser?.EmailAddress;
			var loggedInOrganisationPK = Env.CurrentCompany?.OrganisationPK;

			if (!string.IsNullOrEmpty(emailAddress) && loggedInOrganisationPK != null)
			{
				var loggedInOrgProxy = new BusinessObjectFactory { NameForDebugging = nameof(BorderWiseWebLauncher) }.Load<OrgHeader>(loggedInOrganisationPK.Value);
				return loggedInOrgProxy?.OH_Code ?? null;
			}

			return null;
		}

		public static string GetCountryCode()
		{
			string[] countryCodeOverrideList = { Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes };

			var factory = new BusinessObjectFactory { NameForDebugging = nameof(BorderWiseAsyncBatchTariffProcessor) };

			var dataGrouping = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var possibleCountryOverride = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, dataGrouping);

			return possibleCountryOverride != null || countryCodeOverrideList.Contains(dataGrouping) ? dataGrouping : string.Empty;
		}

		public static int GetDatabaseNumber()
		{
			return ObjectFactory.Get<IProductRegistration>().Key.DatabaseNumber;
		}

		public static string ComputeSha256Hash(string rawData)
		{
			using var sha256Hash = SHA256.Create();
			return Convert.ToBase64String(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData)));
		}

		public static string SerializeToJson<T>(T obj)
		{
			if (obj == null)
			{
				return string.Empty;
			}

			return JsonSerializer.Serialize(obj, SerializerOptions);
		}

		public static T DeserializeFromJson<T>(string json)
		{
			return JsonSerializer.Deserialize<T>(json, SerializerOptions);
		}

		[ThreadSafe]
		static readonly JsonSerializerOptions SerializerOptions = new()
		{
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			// Web API properties are using camel case
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			// Deserializing get only properties like BorderWiseTariffExchangeModelV2.BorderWiseInvoiceLines
			PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
		};
	}
}

