using System;
using System.Globalization;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.IE;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Messaging
{
	public static class Extensions
	{
		public static ZBool IsTrueOrFalse(this string value) => value == "1";

		public static bool TryParseToDate(this string input, out ZDateTime result) => new ZString(input).TryParseToDate(out result);

		public static bool TryParseToDate(this ZString input, out ZDateTime result)
		{
			input = input.Trim();
			if (input.Length == 8)
			{
				const string shortDateFormat = "yyyyMMdd";
				return ZDateTime.TryParseExact(input, out result, shortDateFormat);
			}
			else if (input.Length == 12 || input.Length == 15)
			{
				if (input.Length == 15)
				{
					input = input.Left(12);
				}
				return ZDateTime.TryParseExact(input, out result, "yyyyMMddHHmm");
			}
			else
			{
				return ZDateTime.TryParseISO8601Date(input, out result)
					|| ZDateTime.TryParseIgnoreTimezone(input, CultureInfo.InvariantCulture, out result);
			}
		}

		public static ZGuid GetCredentialPK(this GlbCompany company)
		{
			var result = ZGuid.Empty;
			if (company != null)
			{
				var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
				if (wrapper is IIEGlbCompanyWrapper ieWrapper)
				{
					var credential = ieWrapper.GetGlbExternalPassword();
					if (credential != null)
					{
						result = credential.PK;
					}
				}
			}
			return result;
		}

		public static bool HasInvalidCredentialOrSetInvalidIfCredentialHasExpired(this GlbCompany company, bool shouldSave)
		{
			var result = true;
			if (company != null
				&& new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbCompany>(company.PK) is GlbCompany companyInNewFactory
				&& GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(companyInNewFactory) is IIEGlbCompanyWrapper companyWrapper)
			{
				result = companyWrapper.GetGlbExternalPassword().HasInvalidCredentialOrSetInvalidIfCredentialHasExpired(shouldSave);
			}
			return result;
		}

		public static bool HasInvalidCredentialOrSetInvalidIfCredentialHasExpired(this IGlbExternalPasswordWithCertificate credential, bool shouldSave)
		{
			var result = true;
			if (credential != null
				&& credential.GP_PasswordStatus.EqualsIgnoringCase(PasswordStatusList.Codes.Valid))
			{
				if (credential.IsCertificateValid)
				{
					result = false;
				}
				else
				{
					credential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
					if (shouldSave)
					{
						try
						{
							credential.Factory.Save();
						}
						catch (ZSaveException ex)
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
					}
				}
			}
			return result;
		}

		public static string ValidateROSCredential(this GlbCompany company)
		{
			var validationMessage = string.Empty;
			if (company != null && company.HasInvalidCredentialOrSetInvalidIfCredentialHasExpired(true))
			{
				return Res.GetString("{E0D1B130-CA5A-48A9-BE8C-5E180763697F}",
					"Cannot send message as Company ({0}) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.",
					company.GC_Code);
			}

			return validationMessage;
		}

		public static bool CheckValidROSCredentialAndSetInvalidIfCredentialHasExpired(this GlbCompany company)
		{
			var credentialError = company.ValidateROSCredential();
			if (!string.IsNullOrEmpty(credentialError))
			{
				Globals.Message.ShowError(credentialError, Res.GetString("2fe7ec66-2651-46d1-98ac-2a8507263bb2", "Send to Customs Error"));
				return false;
			}
			return true;
		}

		public static EDIInterchange GetOutgoingIntechangeMatchingSessionGUID(this EDIInterchange incomingInterchange)
		{
			return incomingInterchange.Factory.GetCachedValue(incomingInterchange.EI_SessionGUID, () => incomingInterchange.Factory.LoadTop1<EDIInterchange>(incomingInterchange.BuildOutgoingInterchangeQueryMatchingSessionGUID()));
		}

		public static ZQuery BuildOutgoingInterchangeQueryMatchingSessionGUID(this EDIInterchange incomingInterchange)
		{
			var interchangeQuery = new ZQuery();
			interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, incomingInterchange.EI_ApplicationCode);
			interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, incomingInterchange.EI_SessionGUID);
			interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, incomingInterchange.EI_SystemCreateTimeUtc);
			interchangeQuery.OrderBy = EDIInterchange.Schema.EI_SystemCreateTimeUtc + OrderByClause.Descending;
			return interchangeQuery;
		}

		public static string GetXmlEnumAttributeValue(this Enum value)
		{
			var field = value.GetType().GetField(value.ToString());
			var xmlEnumAttribute = Attribute.GetCustomAttribute(field, typeof(XmlEnumAttribute)) as XmlEnumAttribute;
			if (xmlEnumAttribute == null)
			{
				return value.ToString();
			}

			return xmlEnumAttribute.Name;
		}
	}
}
