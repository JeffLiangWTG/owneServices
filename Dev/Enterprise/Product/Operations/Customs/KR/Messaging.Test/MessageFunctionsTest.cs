using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class MessageFunctionsTest : TestCaseWithFactory
	{
		public void TestDeclarationNumberFormat()
		{
			var result = "";
			result = MessageFunctions.DeclarationNumberFormat(null);
			AssertEquals("", result);
			result = MessageFunctions.DeclarationNumberFormat("");
			AssertEquals("", result);
			result = MessageFunctions.DeclarationNumberFormat("12345678901234");
			AssertEquals("12345-67-8901234", result);
			AssertEquals("12345-67-8901234", MessageFunctions.GetFormattedNumber("12345678901234", new int[] { 0, 5, 7 }));
			AssertEquals("1234567", MessageFunctions.GetFormattedNumber("1234567", new int[] { 0, 5, 7 }));

			result = MessageFunctions.DeclarationNumberFormat("12345678901234567");
			AssertEquals("12345-67-8901234567", result);
			result = MessageFunctions.DeclarationNumberFormat("1234567");
			AssertEquals("1234567", result);
		}
		public void TestGetCustomsOfficeAndDivision()
		{
			var result = "";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "100", "동해세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "10", "통관지원(1)과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			result = MessageFunctions.GetCustomsOfficeAndDivision(Factory, null);
			AssertEquals("", result);
			result = MessageFunctions.GetCustomsOfficeAndDivision(Factory, "");
			AssertEquals("", result);
			result = MessageFunctions.GetCustomsOfficeAndDivision(Factory, "1234567");
			AssertEquals("", result);
			result = MessageFunctions.GetCustomsOfficeAndDivision(Factory, "1");
			AssertEquals("", result);
			result = MessageFunctions.GetCustomsOfficeAndDivision(Factory, "12345");
			AssertEquals("", result);
			result = MessageFunctions.GetCustomsOfficeAndDivision(Factory, "10025");
			AssertEquals("동해세관", result);
			result = MessageFunctions.GetCustomsOfficeAndDivision(Factory, "55510");
			AssertEquals("통관지원(1)과", result);
			result = MessageFunctions.GetCustomsOfficeAndDivision(Factory, "10010");
			AssertEquals("동해세관 통관지원(1)과", result);
		}

		public void TestGetRefCusCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "100", "동해세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "10", "통관지원(1)과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var result = MessageFunctions.GetRefCusCodeList(Factory, "100", Constants.ZZ.NKCodeType.CustomsOffice);
			AssertNotNull(result);
			AssertEquals("동해세관", result.ZZD_Description);

			result = MessageFunctions.GetRefCusCodeList(Factory, "10", Constants.ZZ.NKCodeType.CustomsDepartment);
			AssertNotNull(result);
			AssertEquals("통관지원(1)과", result.ZZD_Description);
		}
		public void TestRequestDocumentNumber()
		{
			string result;
			result = MessageFunctions.RequestDocumentNumber(null);
			AssertEquals("", result);
			result = MessageFunctions.RequestDocumentNumber("");
			AssertEquals("", result);
			result = MessageFunctions.RequestDocumentNumber("1234567890123");
			AssertEquals("123-45-67-890123", result);
			result = MessageFunctions.GetFormattedNumber("1234567890123", new int[] { 0, 3, 5, 7 });
			AssertEquals("123-45-67-890123", result);

			result = MessageFunctions.RequestDocumentNumber("1234567");
			AssertEquals("1234567", result);
			AssertEquals("1234567", MessageFunctions.GetFormattedNumber("1234567", new int[] { 0, 3, 5, 7 }));
		}

		public void TestFormattedUnipassIDForOrganization()
		{
			ZString inputString = ZString.Empty;
			AssertEquals(ZString.Empty, MessageFunctions.GetFormattedUnipassIDForOrganization(inputString));

			inputString = "통관고유1234567";
			AssertEquals("통관고유-1-23-4-56-7", MessageFunctions.GetFormattedUnipassIDForOrganization(inputString));

			inputString = "통관고**1234567";
			AssertEquals("통관고**-1-23-4-56-7", MessageFunctions.GetFormattedUnipassIDForOrganization(inputString));

			inputString = "통관****1234567";
			AssertEquals("통관****-1-23-4-56-7", MessageFunctions.GetFormattedUnipassIDForOrganization(inputString));

			inputString = "통******1234567";
			AssertEquals("통******-1-23-4-56-7", MessageFunctions.GetFormattedUnipassIDForOrganization(inputString));

			inputString = "1234567";
			AssertEquals("1234567", MessageFunctions.GetFormattedUnipassIDForOrganization(inputString));
		}

		public void TestGetIndividualDigits()
		{
			ZString inputString = ZString.Empty;
			ZString[] individualDigit = MessageFunctions.GetIndividualDigits(inputString, 1);
			AssertEquals(1, individualDigit.Length);
			AssertEquals(ZString.Empty, individualDigit[0]);

			inputString = "0";
			individualDigit = MessageFunctions.GetIndividualDigits(inputString, 1);
			AssertEquals(1, individualDigit.Length);
			AssertEquals(ZString.Empty, individualDigit[0]);

			inputString = "123";
			individualDigit = MessageFunctions.GetIndividualDigits(inputString, 2);
			AssertEquals(5, individualDigit.Length);
			AssertEquals(ZString.Empty, individualDigit[0]);
			AssertEquals(ZString.Empty, individualDigit[1]);
			AssertEquals("1", individualDigit[2]);
			AssertEquals("2", individualDigit[3]);
			AssertEquals("3", individualDigit[4]);
		}

		public void TestRoundDecimalValueRoundedWithDecimalPlaces()
		{
			var roundDecimal1 = new NoAttributeRoundDecimalTest();

			roundDecimal1.noAttribute = 1.000m;
			roundDecimal1.RoundDecimalValueRoundedWithDecimalPlaces();
			AssertEquals("Decimal places attribute is missing for decimal property: noAttribute", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var roundDecimal2 = new AttributeRoundDecimalTest();
			roundDecimal2.attribute = 1.000m;
			roundDecimal2.RoundDecimalValueRoundedWithDecimalPlaces();
			AssertEquals(1m, roundDecimal2.attribute);

			roundDecimal2.attribute = 1.12345m;
			roundDecimal2.RoundDecimalValueRoundedWithDecimalPlaces();
			AssertEquals(1.1235m, roundDecimal2.attribute);
		}

		class NoAttributeRoundDecimalTest : INoAttributeSRoundDecimalTest
		{
			public decimal noAttribute { get; set; }

			ZDecimal INoAttributeSRoundDecimalTest.NoAttribute => noAttribute;
		}
		interface INoAttributeSRoundDecimalTest : IMessageDataProvider
		{
			ZDecimal NoAttribute { get; }
		}
		class AttributeRoundDecimalTest : IAttributeRoundDecimalTest
		{
			[DecimalPlaces(DecimalPlacesConstants.Amount)]
			public decimal attribute { get; set; }

			ZDecimal IAttributeRoundDecimalTest.Attribute => attribute;
		}
		interface IAttributeRoundDecimalTest : IMessageDataProvider
		{
			ZDecimal Attribute { get; }
		}
	}
}
