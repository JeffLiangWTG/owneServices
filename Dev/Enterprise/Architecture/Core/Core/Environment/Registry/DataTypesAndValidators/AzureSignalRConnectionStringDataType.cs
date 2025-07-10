using System;
using System.Text.RegularExpressions;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class AzureSignalRConnectionStringDataType : StringRegistryDataType
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "config url property - no translation required")]
		const string EndpointProperty = "Endpoint";
		const string AccessKeyProperty = "AccessKey"; // config url property - no translation required
		readonly string ConnectionStringExpression = $"{EndpointProperty}=(?<{EndpointProperty}>.+?);{AccessKeyProperty}=(?<{AccessKeyProperty}>.+?);";  // regular expression - no translation required

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (string.IsNullOrEmpty(proposedValue))
			{
				return;
			}

			var match = Regex.Match(proposedValue, ConnectionStringExpression, RegexOptions.IgnoreCase);
			if (!match.Success)
			{
				throw new RegistryValidationException(Res.GetString("C72A20E0-ECB2-42D8-9B41-F1DEF5E20967", "Please enter a valid connection string provided in the Azure portal."));
			}

			var endpoint = match.Groups[EndpointProperty];
			if (endpoint == null)
			{
				MissingMandatoryProperty(EndpointProperty);
			}

			var accessKey = match.Groups[AccessKeyProperty];
			if (accessKey == null || string.IsNullOrWhiteSpace(accessKey.Value))
			{
				MissingMandatoryProperty(AccessKeyProperty);
			}

			UriRegistryTypeValidator.ValidateUri(endpoint.Value.Trim(), Uri.UriSchemeHttps, false);
		}

		static void MissingMandatoryProperty(string property)
		{
			throw new RegistryValidationException(Res.GetString("8C3A277A-B4A8-4356-BCD4-C8ACD6CC6440", "Connection string missing mandatory property {0}.", property));
		}
	}
}
