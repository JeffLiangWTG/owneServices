using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static CargoWise.EntityFramework.Testing.TestCaseWithFactory;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	static class NctsAdditionalInfoValidationTestHelper
	{
		const string MessageInvalidFormat = "[TR0062] The MAWB number must consist of 11 digits.";
		const string MessageInvalidAirline = "[TR0062] Wrong Airline code in MAWB number.";
		const string MessageInvalidCheckdigit = "[TR0062] Wrong check-digit in MAWB number. Last digit should be '5'.";

		internal static void AssertWhenRuleActive(ZPropertyInfo csiReferenceInfo)
		{
			var allMessages = new[] { MessageInvalidFormat, MessageInvalidAirline, MessageInvalidCheckdigit };

			var additionalDocument = (CusSupportingInfo)csiReferenceInfo.BizObj;
			var factory = additionalDocument.Factory;
			var nctsHeader = additionalDocument.Parent as NctsHeader ?? (additionalDocument.Parent as NctsBill).Header;

			factory.New<RefAirline>().RM_EagleAddedAirlinePrefixOrAccountingCode = "222";

			AssertError("Too short", MessageInvalidFormat, "1234567890");
			AssertError("Too long", MessageInvalidFormat, "123456789012");
			AssertError("Non-digit", MessageInvalidFormat, "12345X78901");
			AssertError("Unknown airline", MessageInvalidAirline, "33312345675");
			AssertError("Invald check digit", MessageInvalidCheckdigit, "22212345679");
			AssertError("Valid MAWB number", null, "22212345675");

			void AssertError(string assertionMessage, string message, string csiReference)
			{
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalDocument.CSI_Code = AdditionalDocumentTypes.MAWB;
				additionalDocument.CSI_ReferenceNumber = csiReference;
				additionalDocument.Validation.ValidateCSI_ReferenceNumber();

				if (message != null)
				{
					AssertHasMessageError(AssertionMessage(), additionalDocument.CSI_ReferenceNumberInfo, message);
				}
				foreach (var unexpectedMessage in allMessages.Where(m => m != message))
				{
					AssertNoMessageError(AssertionMessage(), additionalDocument.CSI_ReferenceNumberInfo, unexpectedMessage);
				}

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalDocument.Validation.ValidateCSI_ReferenceNumber();
				foreach (var unexpectedMessage in allMessages)
				{
					AssertNoMessageError(AssertionMessage(), additionalDocument.CSI_ReferenceNumberInfo, unexpectedMessage);
				}

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalDocument.CSI_Code = AdditionalDocumentTypes.T0000;
				additionalDocument.Validation.ValidateCSI_ReferenceNumber();
				foreach (var unexpectedMessage in allMessages)
				{
					AssertNoError(AssertionMessage(), additionalDocument.CSI_ReferenceNumberInfo, unexpectedMessage);
				}

				string AssertionMessage() => $"{assertionMessage} - CSI_SubType={additionalDocument.CSI_SubType} CSI_Code={additionalDocument.CSI_Code} CSI_ReferenceNumber={additionalDocument.CSI_ReferenceNumber} BH_HeaderType={nctsHeader.BH_HeaderType}";
			}
		}

		internal static void AssertWhenRuleNotActive(ZPropertyInfo csiReferenceInfo)
		{
			var allMessages = new[] { MessageInvalidFormat, MessageInvalidAirline, MessageInvalidCheckdigit };

			var additionalDocument = (CusSupportingInfo)csiReferenceInfo.BizObj;
			var factory = additionalDocument.Factory;

			factory.New<RefAirline>().RM_EagleAddedAirlinePrefixOrAccountingCode = "222";

			AssertError("Too short", "1234567890");
			AssertError("Too long", "123456789012");
			AssertError("Non-digit", "12345X78901");
			AssertError("Unknown airline", "33312345675");
			AssertError("Invald check digit", "22212345679");
			AssertError("Valid MAWB number", "22212345675");

			void AssertError(string assertionMessage, string csiReference)
			{
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalDocument.CSI_Code = AdditionalDocumentTypes.MAWB;
				additionalDocument.CSI_ReferenceNumber = csiReference;
				foreach (var unexpectedMessage in allMessages)
				{
					AssertNoMessageError($"{assertionMessage} - TR0062 inactive", additionalDocument.CSI_ReferenceNumberInfo, unexpectedMessage);
				}
			}
		}
	}
}
