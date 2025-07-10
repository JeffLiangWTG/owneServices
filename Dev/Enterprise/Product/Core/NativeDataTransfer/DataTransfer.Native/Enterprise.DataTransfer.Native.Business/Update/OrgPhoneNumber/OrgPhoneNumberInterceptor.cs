using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Tools.Telephony;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgPhoneNumber
{
	class OrgPhoneNumberInterceptor : BaseInterceptor
	{
		readonly PhoneNumberFormatter phoneNumberFormatter;
		public OrgPhoneNumberInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
			phoneNumberFormatter = new PhoneNumberFormatter();
			factory = setting.Context.ObjectFactory;
		}
		readonly BusinessObjectFactory factory;

		public override void Invoke(IEntitySet entitySet)
		{
			var orgHeader = entitySet.Root;
			StandardizePhoneNumber(orgHeader);
			Function(entitySet);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		void StandardizePhoneNumber(IEntity entity)
		{
			var orgCountryCode = GetOrgHeaderDefaultCountryCode(entity);
			foreach (IEntity contactOrAddressEntity in entity.Children.Where(c => c.EntityName == "OrgContact" || c.EntityName == "OrgAddress"))
			{
				foreach (var phoneNumber in contactOrAddressEntity.Properties.Where(p => p.Name == "Phone"))
				{
					if (phoneNumber != null && phoneNumber.Value != null && !String.IsNullOrEmpty(phoneNumber.Value.ToString()))
					{
						var formattedNumber = "";
						if (contactOrAddressEntity.EntityName == "OrgContact")
						{
							var contactCountryCode = GetContactAddressDefaultCountryCode(entity, contactOrAddressEntity);
							if (string.IsNullOrEmpty(contactCountryCode))
							{
								contactCountryCode = orgCountryCode;
							}
							formattedNumber = phoneNumberFormatter.FormatE164(phoneNumber.Value.ToString(), contactCountryCode);
						}
						else
						{
							var addressCountryCode = GetDefaultCountryCodeFromAddress(contactOrAddressEntity);
							if (string.IsNullOrEmpty(addressCountryCode))
							{
								addressCountryCode = orgCountryCode;
							}
							formattedNumber = phoneNumberFormatter.FormatE164(phoneNumber.Value.ToString(), addressCountryCode);
						}
						if (!string.IsNullOrEmpty(formattedNumber))
						{
							contactOrAddressEntity["Phone"] = formattedNumber;
						}
					}
				}
			}
		}

		string GetOrgHeaderDefaultCountryCode(IEntity rootOrgEntity)
		{
			var addressEntities = rootOrgEntity.Children.Where(c => c.EntityName == "OrgAddress");
			List<string> countryCodeList = new List<string>();
			foreach (var address in addressEntities)
			{
				countryCodeList.Add(GetDefaultCountryCodeFromAddress(address));
			}
			var countryCodes = countryCodeList.Distinct().ToArray();
			return countryCodes.Length == 1 ? countryCodes[0] : "";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		string GetDefaultCountryCodeFromAddress(IEntity addressEntity)
		{
			var countryCode = GetParentPropertyValue(addressEntity, "CountryCode", "Code");
			if (string.IsNullOrEmpty(countryCode))
			{
				var relatedPortCode = GetParentPropertyValue(addressEntity, "RelatedPortCode", "Code");
				if (!string.IsNullOrEmpty(relatedPortCode))
				{
					countryCode = relatedPortCode;
				}
			}
			return countryCode;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		string GetContactAddressDefaultCountryCode(IEntity rootEntity, IEntity contactEntity)
		{
			var countryCode = "";
			var branchAddressEntities = contactEntity.Parents.Where(c => c.EntityName == "OrgAddress" && c.Properties.Any());
			var overrideAddressEntities = contactEntity.Parents.Where(c => c.EntityName == "AddressOverride" && c.Properties.Any());

			if (branchAddressEntities.Any())
			{
				var branchAddressEntity = branchAddressEntities.Last();
				var branchAddressPKValue = GetEntityPropertyValue(branchAddressEntity, "PK");
				var branchAddressCodeValue = GetEntityPropertyValue(branchAddressEntity, "Code");
				if (overrideAddressEntities.Any())
				{
					OrgAddress branchAddress = null;
					ZGuid branchAddressPK;
					if (ZGuid.TryParse(branchAddressPKValue, out branchAddressPK))
					{
						branchAddress = factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, branchAddressPK));
					}

					if (branchAddress == null)
					{
						if (!string.IsNullOrEmpty(branchAddressCodeValue))
						{
							branchAddress = factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_Code, branchAddressCodeValue));
						}
					}
					if (branchAddress != null)
					{
						countryCode = branchAddress.DefaultCountryCodeForPhoneNumbers;
					}
				}
				else if (!string.IsNullOrEmpty(branchAddressCodeValue))
				{
					var addressEntities = rootEntity.Children.Where(entity => entity.EntityName == "OrgAddress");
					foreach (var address in addressEntities)
					{
						if (GetEntityPropertyValue(address, "Code") == branchAddressCodeValue)
						{
							countryCode = GetDefaultCountryCodeFromAddress(address);
							break;
						}
					}
				}
			}

			if (string.IsNullOrEmpty(countryCode) && overrideAddressEntities.Any())
			{
				var overrideAddressEntity = overrideAddressEntities.Last();
				OrgHeader overrideOrg = null;
				ZGuid overrideOrgPK;
				if (ZGuid.TryParse(GetEntityPropertyValue(overrideAddressEntity, "PK"), out overrideOrgPK))
				{
					overrideOrg = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, overrideOrgPK));
				}

				if (overrideOrg == null)
				{
					var overrideOrgCode = GetEntityPropertyValue(overrideAddressEntity, "Code");
					if (!string.IsNullOrEmpty(overrideOrgCode))
					{
						overrideOrg = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, overrideOrgCode));
					}
				}
				if (overrideOrg != null)
				{
					countryCode = overrideOrg.DefaultCountryCodeForPhoneNumbers;
				}
			}

			return countryCode;
		}

		string GetEntityPropertyValue(IEntity entity, string propertyName)
		{
			var properties = entity.Properties.Where(prop => prop.Name == propertyName);
			if (properties.Any() && properties.FirstOrDefault().Value != null)
			{
				return properties.FirstOrDefault().Value.ToString();
			}
			return "";
		}

		string GetParentPropertyValue(IEntity entity, string rootEntity, string propertyName)
		{
			var parentProperty = GetParentProperty(entity, rootEntity, propertyName);
			return parentProperty == null || parentProperty.Value == null ? "" : parentProperty.Value.ToString();
		}

		Property GetParentProperty(IEntity entity, string rootEntity, string propertyName)
		{
			Property result = null;
			var parentEntities = entity.Parents.Where(c => c.EntityName == rootEntity);
			if (parentEntities.Any())
			{
				var properties = parentEntities.FirstOrDefault().Properties.Where(p => p.Name == propertyName);
				if (properties.Any())
				{
					result = properties.FirstOrDefault();
				}
			}
			return result;
		}
	}
}
