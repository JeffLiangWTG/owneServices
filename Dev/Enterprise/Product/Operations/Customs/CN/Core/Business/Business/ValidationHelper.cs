using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public static class ValidationHelper
	{
		public static void CheckMaxLength(ZPropertyInfo propertyInfo, int maxLength, IValidationModeProvider provider)
		{
			var actualLength = ((ZString)propertyInfo.Value).Length;
			if (actualLength > maxLength)
			{
				propertyInfo.AddNotification(GetExceedsMaxLengthMessage(propertyInfo.HumanReadableName, actualLength, maxLength), provider);
			}
		}

		public static ZBool CheckConditionForInvoiceLine(this JobComInvoiceLine invLine, ZString conditionType, ZString valueType, ZString inputValue)
		{
			var result = false;
			switch (valueType)
			{
				case (Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.Prohibitation):
					result = true;
					break;

				case (Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.PresentationOfSupportingDoc):
					result = invLine.CusSupportingDocuments?.IsDocumentProvided(inputValue) ?? false;
					break;
			}
			return result;
		}

		public static void CheckOrganizationHasValidRegNumbers(OrgHeader org, bool ciqRequires, ZPropertyInfo propertyInfo, INotificationType type, bool isOrgProxy = false)
		{
			CheckRegNumbers(org, OrgCusCode.ChinaCodeTypes.USC, propertyInfo, type, isOrgProxy);
			CheckRegNumbers(org, OrgCusCode.CodeTypes.CustomsClientCode, propertyInfo, type, isOrgProxy);
			if (ciqRequires)
			{
				CheckRegNumbers(org, OrgCusCode.ChinaCodeTypes.CIQ, propertyInfo, type, isOrgProxy);
			}
		}

		public static void CheckRegNumbers(OrgHeader org, string regNoType, ZPropertyInfo propertyInfo, INotificationType notificationType, bool isOrgProxy = false)
		{
			if (org != null)
			{
				CheckRegNumbers(org.GetChinaCustomsRegNo(regNoType), regNoType, propertyInfo, notificationType, true, isOrgProxy);
			}
		}

		public static void CheckRegNumbers(ZString regNo, string regNoType, ZPropertyInfo propertyInfo, INotificationType notificationType, bool checkOnOrganization = true, bool isOrgProxy = false)
		{
			ZString message;
			if (regNo.IsEmpty)
			{
				if (checkOnOrganization)
				{
					message = isOrgProxy ? OrgProxyRegNumIsRequired(regNoType) : RegNumIsRequired(regNoType);
				}
				else
				{
					message = MandatoryValidation.YouHaveNotEnteredMessage(propertyInfo.HumanReadableName);
				}
			}
			else
			{
				message = OrgCusCodeValidation.CheckCustomsRegNoFormat(regNoType, regNo);
				if (!message.IsEmpty && checkOnOrganization)
				{
					message = isOrgProxy ? OrgProxyInvalidRegNoFormat(regNoType) : InvalidRegNoFormat(regNoType);
				}
			}

			if (!message.IsEmpty)
			{
				propertyInfo.AddNotification(notificationType, message);
			}
		}

		#region Messages

		internal static ZString GetExceedsMaxLengthMessage(ZString propertyName, int actualLength, int maxLength)
		{
			return Res.GetString("b0cb5ba2-a97d-41bc-8803-e3ca40a0a49c", "The length of {0} ({1}) exceeds the maximum allowed ({2}).", propertyName, actualLength, maxLength);
		}

		internal static ZString RegNumIsRequired(string cusCodeType)
		{
			return Res.GetString("0ae6d08d-ca64-4916-8ca9-25d282d1e67f", "{0} Registration Number for CN is required. Please press F3 on the Organization, go to Details -> Config -> Registration Numbers/Codes and enter the Registration Number.", cusCodeType);
		}

		internal static ZString OrgProxyRegNumIsRequired(string cusCodeType)
		{
			return Res.GetString("7ddbdf3b-8126-4ccb-b8b8-3cb9942fb0d1", "{0} Registration Number for CN is required for this branch/company's Organization Proxy. Please press F3, edit its or its Company's Organization Proxy, go to Details -> Config -> Registration Numbers/Codes and enter the Registration Number.", cusCodeType);
		}

		internal static ZString InvalidRegNoFormat(string cusCodeType)
		{
			return Res.GetString("e4d4651b-4097-415a-9a68-e7351cad93a0", "The selected organization does not seem to have a valid {0} Registration Number.", cusCodeType);
		}

		internal static ZString OrgProxyInvalidRegNoFormat(string cusCodeType)
		{
			return Res.GetString("02f7216f-aa3e-40c6-b519-0d4f2cabebba", "This branch/company's Organization Proxy does not seem to have a valid {0} Registration Number.", cusCodeType);
		}

		#endregion
	}
}

