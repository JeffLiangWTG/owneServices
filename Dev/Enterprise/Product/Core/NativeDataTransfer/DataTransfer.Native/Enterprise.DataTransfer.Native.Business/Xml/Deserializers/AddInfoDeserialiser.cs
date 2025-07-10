using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ZString = CargoWise.Types.ZString;

namespace Enterprise.DataTransfer.Native.Business.Xml.Deserializers
{
	class AddInfoDeserialiser
	{
		public AddInfoDeserialiser(XElement childElement, List<Property> properties, IEntityDefinition definition, ZString addInfoTypeCode)
		{
			this.childElement = childElement;
			this.properties = properties;
			this.definition = definition;
			this.AddInfoTypeCode = addInfoTypeCode;
		}

		internal void Deserialise()
		{
			string propertyName = XmlConstants.PropertyNames.AddInfo;

			if (!string.IsNullOrEmpty(AddInfoTypeCode))
			{
				addInfoSchema = new CusAddInfoColumnSchemaResolver().GetCusAddInfoSchemaSchema(AddInfoTypeCode);
			}

			if (!definition.PropertyDefinitions.HasDefinition(propertyName))
			{
				var propertyColumn = definition.Table.Columns.AddInfoColumn;
				if (propertyColumn != null)
				{
					propertyName = new ZString(propertyColumn.Name).SubstringSafe(3);
				}
			}

			if (definition.PropertyDefinitions.HasDefinition(propertyName))
			{
				var property = new Property(definition.PropertyDefinitions[propertyName]);
				property.Value = CondenseKeyValuePairsIntoOneString(childElement);
				properties.Add(property);
			}
		}

		string CondenseKeyValuePairsIntoOneString(XElement childElement)
		{
			var pairs = new Dictionary<ZString, ZString>();
			foreach (var kvp in childElement.Elements().Where(e => e.Name == XmlConstants.EntityNames.AddInfo))
			{
				var key = kvp.Elements().FirstOrDefault(e => e.Name == XmlConstants.PropertyNames.Key);
				var value = kvp.Elements().FirstOrDefault(e => e.Name == XmlConstants.PropertyNames.Value);
				var address = kvp.Elements().FirstOrDefault(e => e.Name == "OrganizationAddress");
				if (key != null)
				{
					if (value != null)
					{
						var addInfoValue = value.Value;
						var addInfoColumn = addInfoSchema?.All.FirstOrDefault(x => IsMatchPropertyName(x.Name, key.Value));
						if (addInfoColumn != null && addInfoColumn.HasMaxLength && addInfoValue.Length > addInfoColumn.MaxLength)
						{
							addInfoValue = addInfoValue.Substring(0, addInfoColumn.MaxLength);
						}

						AddInfoParser.AddValue(pairs, key.Value, addInfoValue);
					}
					else if (key.Value.IsAddInfoAddress() && address != null)
					{
						ZString usMid = ZString.Empty;
						var addressPk = GetPkOfAddressInDatabaseUsingOrgHeaderCodeAndAddressShortCode(address);
						if (addressPk.IsEmpty)
						{
							var result = GetPkOfAddressInDatabaseUsingRegistrationNumberOrUsMid(address);
							if (!ZGuid.TryParse(result, out addressPk))
							{
								usMid = new ZString(result);
							}
						}

						if (!addressPk.IsEmpty)
						{
							AddInfoParser.AddValue(pairs, key.Value, addressPk.ToString());
						}
						else if (!usMid.IsEmpty)
						{
							AddInfoParser.AddValue(pairs, key.Value, usMid);
						}
					}
				}
			}

			return pairs.CondenseKeyValuePairsIntoSortedOneString();
		}

		ZGuid GetPkOfAddressInDatabaseUsingOrgHeaderCodeAndAddressShortCode(XElement address)
		{
			var result = ZGuid.Empty;
			var orgCode = address.Elements().FirstOrDefault(e => e.Name == "OrganizationCode");
			var addressCode = address.Elements().FirstOrDefault(e => e.Name == "AddressShortCode");
			if (orgCode != null && addressCode != null)
			{
				// It's be good to use a subquery here to hit the DB only once, but crappy ZDBOnly(Sub)Query requires knowledge of bizO types.  Pfff.
				var zquery = new ZQuery(OrgHeaderSchema.OH_Code, orgCode.Value);
				var orgs = RowFactory.Load(OrgHeaderSchema.Constants.TableName, zquery);
				if (orgs.Length == 1)
				{
					var org = orgs.First();
					var addressesOnThisOrg = RowFactory.Load(OrgAddressSchema.Constants.TableName, new ZQuery(OrgAddressSchema.OA_OH, org[OrgHeaderSchema.Constants.PK]));
					var adds = (from DataRow a in addressesOnThisOrg where a[OrgAddressSchema.Constants.OA_Code].ToString() == addressCode.Value select a);
					if (adds.Count() == 1)
					{
						result = new ZGuid(adds.FirstOrDefault()[OrgAddressSchema.Constants.PK]);
					}
				}
			}
			return result;
		}

		bool IsMatchPropertyName(ZString fieldName, ZString keyName)
		{
			return fieldName.Contains(Separator, System.StringComparison.Ordinal) && fieldName.SubstringSafe(fieldName.IndexOf(Separator, System.StringComparison.Ordinal) + 1) == keyName;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		object GetPkOfAddressInDatabaseUsingRegistrationNumberOrUsMid(XElement address)
		{
			ZString usMid = ZString.Empty;
			var registrationCollection = address.Elements().Where(e => e.Name == "RegistrationNumberCollection");
			foreach (var registrationNode in registrationCollection.Elements().Where(e => e.Name == "RegistrationNumber"))
			{
				var codeType = registrationNode.XPathSelectElement("Type/Code");
				var codeCountry = registrationNode.XPathSelectElement("CountryOfIssue/Code");
				var codeValue = registrationNode.XPathSelectElement("Value");
				if (codeType != null && codeCountry != null && codeValue != null)
				{
					var zquery = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType.Value);
					zquery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, codeCountry.Value);
					zquery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, codeValue.Value);
					var orgCusCodes = RowFactory.Load(OrgCusCodeSchema.Constants.TableName, zquery);
					if (orgCusCodes.Length == 1)
					{
						var addressFK = orgCusCodes[0][OrgCusCodeSchema.OK_OA_PremisesAddress.Name];
						if (addressFK != null)
						{
							return addressFK;
						}
					}

					if (codeType.Value == OrgCusCode.USACodeTypes.ManufacturerID && codeCountry.Value == Core.Constants.CountryCodes.UnitedStates)
					{
						usMid = codeValue.Value;
					}
				}
			}

			return usMid;
		}

		RowFactory rowFactory;
		RowFactory RowFactory
		{
			get { return rowFactory ?? (rowFactory = new RowFactory()); }
		}

		readonly XElement childElement;
		readonly List<Property> properties;
		readonly IEntityDefinition definition;
		ITableSchema addInfoSchema;
		readonly ZString AddInfoTypeCode;
		const string Separator = "_";
	}
}
