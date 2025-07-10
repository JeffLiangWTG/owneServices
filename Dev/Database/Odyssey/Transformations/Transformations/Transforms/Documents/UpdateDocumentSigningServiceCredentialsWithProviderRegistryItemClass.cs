using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Documents
{
	public class UpdateDocumentSigningServiceCredentialsWithProviderRegistryItemClass : RegistryDataTransformation
	{
		public override string UserDescription => "Update SigningServiceProviderAPICredentials registry item by changing Class in use";

		const string RegistryItemName = "SigningServiceProviderAPICredentials";

		protected override void OfflinePostUpgradeTransform()
		{
			var table = GetDataTable(RegistryItemName);
			var companyCountry = GetCompanyCountryDictionary();

			foreach (DataRow row in table.Rows)
			{
				var dataRowPk = (Guid)row[StmDataSchema.Constants.PK];
				var binaryValue = row[StmDataSchema.Constants.SD_BinaryValue] as byte[];
				var ownerPk = (Guid)row[StmDataSchema.Constants.SD_Owner];
				var countryCode = string.Empty;

				if (companyCountry.ContainsKey(ownerPk))
				{
					countryCode = companyCountry[ownerPk];
				}

				if (binaryValue != null && binaryValue.Length > 0)
				{
					var xmlString = Encoding.Unicode.GetString(binaryValue);
					if (!xmlString.Contains("<DocumentSigningServiceCredentialsConfiguration"))
					{
						continue; // Nothing to transform
					}

					try
					{
						var defaultCountryProviderCode = GetProviderCodeForAlreadyOverriddenCountry(countryCode);
						if (defaultCountryProviderCode != null)
						{
							var xmlDoc = new XmlDocument();
							xmlDoc.LoadXml(xmlString);
							var newDoc = new XmlDocument();
							newDoc.LoadXml(defaultXml);

							var node = xmlDoc.SelectSingleNode("//DocumentSigningServiceCredentialsConfiguration");
							var newNode = newDoc.SelectSingleNode("//DocumentSigningServiceCredentialsWithProviderConfiguration");

							var providerCodeElement = newDoc.CreateElement("ProviderCode");
							providerCodeElement.InnerXml = defaultCountryProviderCode;
							newNode.AppendChild(providerCodeElement);

							CopyElementContents((XmlElement)node, (XmlElement)newNode);

							UpdateDatabaseValue(dataRowPk, Encoding.Unicode.GetBytes(newDoc.OuterXml));
						}
						else
						{
							ShowInfo($"No valid Provider Code found for the existing credentials with {countryCode} company. Registry Entry for '{RegistryItemName}' will be removed.");
							UpdateDatabaseValue(dataRowPk, null);
						}
					}
					catch (XmlException)
					{
						ShowInfo($"Registry Entry for '{RegistryItemName}' is in an invalid XML format and will be removed.");
						UpdateDatabaseValue(dataRowPk, null);
					}
				}
			}
		}

		const string defaultXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><DocumentSigningServiceCredentialsWithProviderConfiguration></DocumentSigningServiceCredentialsWithProviderConfiguration>";

		void CopyElementContents(XmlElement source, XmlElement destination)
		{
			foreach (XmlNode node in source.ChildNodes)
			{
				if (node is XmlElement)
				{
					var childElement = (XmlElement)node;
					var newChildElement = destination.OwnerDocument.CreateElement(childElement.Name);
					newChildElement.InnerXml = childElement.InnerXml;
					destination.AppendChild(newChildElement);
				}
			}
		}

		string GetProviderCodeForAlreadyOverriddenCountry(string countryCode)
		{
			switch (countryCode)
			{
				case "PT":
					return "DGS";
				case "IN":
					return "EMD";
			}
			return null;
		}

		Dictionary<Guid, string> GetCompanyCountryDictionary()
		{
			var result = new Dictionary<Guid, string>();

			var sql = @"
SELECT
	GC_PK, GC_RN_NKCountryCode
FROM
	dbo.GlbCompany
UNION ALL
SELECT
	GB_PK, GB_RN_NKCountryCode
FROM
	dbo.GlbBranch
;";
			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add(reader.GetGuid(0), reader.GetString(1));
				}
			}

			return result;
		}
	}
}
