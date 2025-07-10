using System;
using System.Data;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Xml.Serializers
{
	class AddInfoPropertyElementGenerator : PropertyElementGenerator
	{
		protected override XElement GenerateCore(Property property)
		{
			XElement node = null;
			if (!string.IsNullOrEmpty(property.Value.ToString()))
			{
				node = new XElement(XmlConstants.EntityNames.AddInfoCollection);
				AddKeyValuePairsAndOptionalAddress(property.Value.ToString(), node);
			}
			return node;
		}

		void AddKeyValuePairsAndOptionalAddress(string addInfoString, XElement parentNode)
		{
			var addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(addInfoString);
			foreach (var addInfo in addInfos)
			{
				XElement addressNodeMaybeNull = null;
				var key = addInfo.Key;
				var value = addInfo.Value;
				if (key.ToString().IsAddInfoAddress() && ZGuid.TryParse(addInfo.Value, out var fk))
				{
					addressNodeMaybeNull = PopulateAddress(fk);
				}
				parentNode.Add(new XElement(XmlConstants.EntityNames.AddInfo,
										new XElement(XmlConstants.PropertyNames.Key, key),
										new XElement(XmlConstants.PropertyNames.Value, value),
										addressNodeMaybeNull
									)
								);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "special element string")]
		XElement PopulateAddress(ZGuid oa)
		{
			XElement addressNode = null;
			var factory = new RowFactory();
			var address = factory.LoadFromPK(OrgAddressSchema.Constants.TableName, oa, reloadExistingRows: false);
			if (address != null)
			{
				var orgHeaderPK = new ZGuid(address[OrgAddressSchema.Constants.OA_OH]);
				var orgHeader = factory.LoadFromPK(OrgHeaderSchema.Constants.TableName, orgHeaderPK);
				addressNode = new XElement("OrganizationAddress");
				addressNode.Add(new XElement("OrganizationCode", orgHeader[OrgHeaderSchema.Constants.OH_Code]));
				addressNode.Add(new XElement("Address1", address[OrgAddressSchema.Constants.OA_Address1]));
				addressNode.Add(new XElement("Address2", address[OrgAddressSchema.Constants.OA_Address2]));
				addressNode.Add(new XElement("City", address[OrgAddressSchema.Constants.OA_City]));
				addressNode.Add(new XElement("CompanyName", orgHeader[OrgHeaderSchema.Constants.OH_FullName]));
				AddCountry(addressNode, address, factory);
				addressNode.Add(new XElement("Email", address[OrgAddressSchema.Constants.OA_Email]));
				addressNode.Add(new XElement("Fax", address[OrgAddressSchema.Constants.OA_Fax]));
				addressNode.Add(new XElement("Phone", address[OrgAddressSchema.Constants.OA_Phone]));
				AddPort(addressNode, address, factory);
				addressNode.Add(new XElement("Postcode", address[OrgAddressSchema.Constants.OA_PostCode]));
				addressNode.Add(new XElement("State", address[OrgAddressSchema.Constants.OA_State]));
				addressNode.Add(new XElement("AddressType", "OFC"));
				addressNode.Add(new XElement("AddressShortCode", address[OrgAddressSchema.Constants.OA_Code]));
				PopulateCusCodes(addressNode, orgHeaderPK, oa, factory);
			}
			return addressNode;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "special element string")]
		void PopulateCusCodes(XElement addressNode, ZGuid oh, ZGuid oa, RowFactory rowFactory)
		{
			var overallQuery = new ZQuery(OrgCusCodeSchema.OK_OH, oh);
			var oaSetQuery = new ZQuery(OrgCusCodeSchema.OK_OA_PremisesAddress, oa);
			var oaNotSetQuery = new ZQuery(OrgCusCodeSchema.OK_OA_PremisesAddress, DBNull.Value);
			var oaBoth = new ZQuery();
			oaBoth.AddToFilter(oaSetQuery);
			oaBoth.AddToFilter(oaNotSetQuery, JoinCondition.Or);
			overallQuery.AddToFilter(oaBoth);
			overallQuery.OrderBy = OrgCusCodeSchema.OK_CustomsRegNo.Name;
			var cusCodes = rowFactory.Load(OrgCusCodeSchema.Constants.TableName, overallQuery);
			if (cusCodes.Length > 0)
			{
				var regoCollection = new XElement("RegistrationNumberCollection");
				foreach (var orgCusCode in cusCodes)
				{
					var codeElement = new XElement("RegistrationNumber",
													new XElement("Type",
																	new XElement(XmlConstants.PropertyNames.Code, orgCusCode[OrgCusCodeSchema.Constants.OK_CodeType]),
																	new XElement(XmlConstants.PropertyNames.Description, GetCodeTypeDescriptionFromCode(orgCusCode))
																),
													new XElement("CountryOfIssue",
																	new XElement(XmlConstants.PropertyNames.Code, orgCusCode[OrgCusCodeSchema.Constants.OK_RN_NKCodeCountry]),
																	new XElement(XmlConstants.PropertyNames.Name, LoadRefCountry(rowFactory, orgCusCode[OrgCusCodeSchema.Constants.OK_RN_NKCodeCountry].ToString())[RefCountrySchema.Constants.RN_Desc])
																),
													new XElement("Value", orgCusCode[OrgCusCodeSchema.Constants.OK_CustomsRegNo])
													 );
					regoCollection.Add(codeElement);
				}
				addressNode.Add(regoCollection);
			}
		}

		ZString GetCodeTypeDescriptionFromCode(DataRow orgCusCode)
		{
			ZString desc = new OrgCodeLists().CustomsCodes_List(orgCusCode[OrgCusCodeSchema.Constants.OK_RN_NKCodeCountry].ToString()).GetDescriptionFromCode(orgCusCode[OrgCusCodeSchema.Constants.OK_CodeType].ToString());
			return desc.SubstringSafe(0, 35);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "special element string")]
		static void AddPort(XElement addressNode, DataRow orgAddress, RowFactory rowFactory)
		{
			var portCode = orgAddress[OrgAddressSchema.Constants.OA_RL_NKRelatedPortCode].ToString();
			var refUnLoco = rowFactory.LoadFromNaturalKey(RefUNLOCOSchema.Constants.TableName, RefUNLOCOSchema.RL_Code, (ZString)portCode, reloadExistingRows: false);
			addressNode.Add(new XElement("Port", new XElement(XmlConstants.PropertyNames.Code, portCode), refUnLoco != null ? new XElement(XmlConstants.PropertyNames.Name, ((ZString)refUnLoco[RefUNLOCOSchema.Constants.RL_PortName].ToString()).SubstringSafe(0, 35)) : null));
		}

		static void AddCountry(XElement addressNode, DataRow orgAddress, RowFactory rowFactory)
		{
			var countryCode = orgAddress[OrgAddressSchema.Constants.OA_RN_NKCountryCode].ToString();
			var refCountry = LoadRefCountry(rowFactory, countryCode);
			if (refCountry == null)
			{
				countryCode = ((ZString)orgAddress[OrgAddressSchema.Constants.OA_RL_NKRelatedPortCode].ToString()).SubstringSafe(0, 2);
				refCountry = LoadRefCountry(rowFactory, countryCode);
			}
			addressNode.Add(new XElement(XmlConstants.EntityNames.Country, new XElement(XmlConstants.PropertyNames.Code, countryCode), refCountry != null ? new XElement(XmlConstants.PropertyNames.Name, refCountry[RefCountrySchema.Constants.RN_Desc]) : null));
		}

		static DataRow LoadRefCountry(RowFactory rowFactory, string countryCode)
		{
			return rowFactory.LoadFromNaturalKey(RefCountrySchema.Constants.TableName, RefCountrySchema.RN_Code, (ZString)countryCode, reloadExistingRows: false);
		}
	}
}
