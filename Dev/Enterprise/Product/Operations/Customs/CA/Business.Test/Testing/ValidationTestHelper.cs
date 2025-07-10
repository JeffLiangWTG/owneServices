using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	public class ValidationTestHelper : Customs.Business.Testing.ValidationTestHelper
	{
		public static void AddCarrierCodeToCurrentCompany(BusinessObjectFactory factory, ZString carrierCode)
		{
			var currentCompany = GlbCompany.GetCurrentCompany(factory);
			currentCompany.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, carrierCode, Core.Constants.CountryCodes.Canada);
			factory.Save();
		}

		public static void RemoveCarrierCodeFromCurrentCompany(BusinessObjectFactory factory)
		{
			var currentCompany = GlbCompany.GetCurrentCompany(factory);
			var orgCusCode = currentCompany.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Canada);
			if (orgCusCode != null)
			{
				currentCompany.OrgProxy.CustomsCodes.RemoveAndDelete(orgCusCode);
			}
			factory.Save();
		}

		#region List Validaion for Validation Type

		public static void AssertInvalidCodeOrEmptyMessageErrorForValidationType(ZPropertyInfo info, ZString invalidCode, ZString validCode, ValidateForMessageType validationMessagType, JobDeclaration declaration)
		{
			SetForValidationMessageType(validationMessagType, declaration);
			AssertInvalidCodeOrEmptyMessageError(info, invalidCode, validCode, ListValidation.InvalidCodeMessageError.ToString());
			ReSetForValidationMessageType(validationMessagType, declaration);
			AssertNoInvalidCodeMessageError(info, invalidCode, validCode, ListValidation.InvalidCodeMessageError.ToString());
		}

		public static void AssertInvalidCodeMessageErrorForValidationType(ZPropertyInfo info, ZString invalidCode, ZString validCode, ValidateForMessageType validationMessagType, JobDeclaration declaration)
		{
			SetForValidationMessageType(validationMessagType, declaration);
			AssertInvalidCodeMessageError(info, invalidCode, validCode, ListValidation.InvalidCodeMessageError.ToString());
			ReSetForValidationMessageType(validationMessagType, declaration);
			AssertNoInvalidCodeMessageError(info, invalidCode, validCode, ListValidation.InvalidCodeMessageError.ToString());
		}

		static void AssertNoInvalidCodeMessageError(ZPropertyInfo info, ZString invalidCode, ZString validCode, string invalidNotificationText)
		{
			if (info.PropertyType == typeof(ZString))
			{
				info.Value = invalidCode;
				TestCaseWithFactory.AssertNoMessageErrorContaining(info, invalidNotificationText);
				info.Value = validCode;
				TestCaseWithFactory.AssertNoMessageErrorContaining(info, invalidNotificationText);
			}
			else
			{
				ThrowNotSupportedTypeException(info);
			}
		}

		#endregion

		#region Mandatory Validation for Validation Type

		public static void AssertYouHaveNotEnteredMessageErrorForValidationType(ZPropertyInfo info, ValidateForMessageType validationMessagType, JobDeclaration declaration)
		{
			AssertYouHaveNotEnteredMessageErrorForValidationType(info, MandatoryValidation.YouHaveNotEntered, validationMessagType, declaration);
		}

		public static void AssertYouHaveNotEnteredMessageErrorForValidationType(ZPropertyInfo info, string notificationText, ValidateForMessageType validationMessagType, JobDeclaration declaration)
		{
			SetForValidationMessageType(validationMessagType, declaration);
			AssertYouHaveNotEnteredMessageError(info, notificationText);
			ReSetForValidationMessageType(validationMessagType, declaration);
			SetNotEmptyValue(info);
			SetEmptyValue(info);
			TestCaseWithFactory.AssertNoMessageErrorContaining(info, notificationText);
			SetNotEmptyValue(info);
			TestCaseWithFactory.AssertNoMessageErrorContaining(info, notificationText);
		}

		public static void AssertIfIsEnteredMessageErrorForValidationType(ZPropertyInfo info, ValidateForMessageType validationMessagType, JobDeclaration declaration)
		{
			AssertIfIsEnteredMessageErrorForValidationType(info, MandatoryValidation.DoNotEntered, validationMessagType, declaration);
		}

		public static void AssertIfIsEnteredMessageErrorForValidationType(ZPropertyInfo info, string notificationText, ValidateForMessageType validationMessagType, JobDeclaration declaration)
		{
			SetForValidationMessageType(validationMessagType, declaration);
			AssertIfIsEnteredMessageError(info, notificationText);
			ReSetForValidationMessageType(validationMessagType, declaration);
			SetEmptyValue(info);
			SetNotEmptyValue(info);
			TestCaseWithFactory.AssertNoMessageErrorContaining(info, notificationText);
			SetEmptyValue(info);
			TestCaseWithFactory.AssertNoMessageErrorContaining(info, notificationText);
		}
		#endregion

		#region Doc Address Validation

		public static void AssertMainAddressUsesCAAddressValidationIfOrgSpecified(ZPropertyInfo orgPkInfo, string addressCaption)
		{
			CAAddressValidatorTest.AssertMainAddressUsesCAAddressValidationIfOrgSpecified(orgPkInfo, addressCaption);
		}

		public static void AssertAddressUsesCAAddressValidationIfOrgSpecified(JobDocAddress address, string addressCaption)
		{
			CAAddressValidatorTest.AssertAddressUsesCAAddressValidationIfOrgSpecified(address, addressCaption);
		}

		#endregion

		#region Implementation

		public static void SetForValidationMessageType(ValidateForMessageType validationMessagType, JobDeclaration declaration)
		{
			declaration.JE_EntryAuthorisationDate = validationMessagType == ValidateForMessageType.B3CUSDEC ? ZDateTime.Now : ZDateTime.Empty;
		}

		public static void ReSetForValidationMessageType(ValidateForMessageType validationMessagType, JobDeclaration declaration)
		{
			declaration.JE_EntryAuthorisationDate = validationMessagType == ValidateForMessageType.B3CUSDEC ? ZDateTime.Empty : ZDateTime.Now;
		}

		public static void SetTotalValueForDuty(ZDecimal vFD, JobDeclaration declaration)
		{
			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			if (entryHeader == null)
			{
				entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			}
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = vFD;

			declaration.Factory.Save();
		}

		public static void SetUpHasUSPlaceOfExportInvoice(JobDeclaration declaration)
		{
			SetTotalValueForDuty(JobComInvoiceHeaderTest.VFDOverLimit, declaration);
			var invoice = declaration.Invoices.AddNew();
			invoice.CA_TradeZone = "101";
		}

		#endregion
	}
}
