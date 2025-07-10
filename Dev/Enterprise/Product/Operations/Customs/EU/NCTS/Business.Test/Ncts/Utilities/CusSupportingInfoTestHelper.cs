using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using NctsRefCusCodeListAttributeValues = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListAttributeValues;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public static class CusSupportingInfoTestHelper
	{
		public static (RefCusCodeList, RefCusCodeList, RefCusCodeList) CreateRefCusCodeListsForTest(ZString codeType, ZString levelType, BusinessObjectFactory factory, params string[] attributeNames)
		{
			return CreateRefCusCodeListsForTest(Core.Constants.CountryCodes.Latvia, codeType, levelType, factory, attributeNames);
		}

		public static (RefCusCodeList, RefCusCodeList, RefCusCodeList) CreateRefCusCodeListsForTest(ZString dataGrouping, ZString codeType, ZString levelType, BusinessObjectFactory factory, params string[] attributeNames)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping, dataGrouping, eun);
			helper.CreateNewOrGetExistingCusCodeType(codeType, "CusCodeType");

			var refCusCodeList1 = helper.CreateCusCodeList(dataGrouping, codeType, "SD1", "SD1 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, RefCusCodeListAttributeTypes.Codes.Level, levelType);
			foreach (var attribute in attributeNames)
			{
				helper.CreateCusCodeListAttribute(refCusCodeList1.PK, attribute, NctsRefCusCodeListAttributeValues.Yes);
			}

			var refCusCodeList2 = helper.CreateCusCodeList(dataGrouping, codeType, "SD2", "SD2 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList2.PK, RefCusCodeListAttributeTypes.Codes.Level, levelType);
			foreach (var attribute in attributeNames)
			{
				helper.CreateCusCodeListAttribute(refCusCodeList2.PK, attribute, NctsRefCusCodeListAttributeValues.No);
			}

			var refCusCodeList3 = helper.CreateCusCodeList(dataGrouping, codeType, "SD3", "SD3 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList3.PK, RefCusCodeListAttributeTypes.Codes.Level, levelType);
			factory.Save();

			return (refCusCodeList1, refCusCodeList2, refCusCodeList3);
		}

		public static IEnumerable<RefCusCodeList> CreateRefCusCodeListsForTesting(BusinessObjectFactory factory, ZString dataGrouping, ZString codeType, ZString[] codes, params (string, string)[] attributes)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping, dataGrouping, eun);
			helper.CreateNewOrGetExistingCusCodeType(codeType, "CusCodeType");

			foreach (var code in codes)
			{
				var refCusCodeList = helper.CreateCusCodeList(dataGrouping, codeType, code, code + " DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				foreach (var (attributeName, attributeValue) in attributes)
				{
					helper.CreateCusCodeListAttribute(refCusCodeList.PK, attributeName, attributeValue);
				}

				yield return refCusCodeList;
			}
		}

		public static void AssertPropertyIsMandatoryWhenHasAttributeWithValueY(ZPropertyInfo propertyInfo, string codeListType, string level, string attributeName)
		{
			var (refCusCodeList1, refCusCodeList2, refCusCodeList3) = CreateRefCusCodeListsForTest(codeListType, level, propertyInfo.BizObj.Factory, attributeName);
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage(propertyInfo.HumanReadableName);
			Assertion.CombineAssertions(() =>
			{
				var cusSupportingInfo = (CusSupportingInfo)propertyInfo.BizObj;
				cusSupportingInfo.CSI_Code = refCusCodeList1.ZZD_Code.Left(CusSupportingInfo.Schema.CSI_CodeMaxLength);
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo, messageError, $"RefCusCode1 with '{attributeName}'-attribute and value 'yes'");

				cusSupportingInfo.CSI_Code = refCusCodeList2.ZZD_Code.Left(CusSupportingInfo.Schema.CSI_CodeMaxLength);
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo, messageError, $"RefCusCode2 with '{attributeName}'-attribute and value 'no'");

				cusSupportingInfo.CSI_Code = refCusCodeList3.ZZD_Code.Left(CusSupportingInfo.Schema.CSI_CodeMaxLength);
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo, messageError, $"RefCusCode3 with '{attributeName}'-attribute not set");

				cusSupportingInfo.CSI_Code = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo, messageError, $"No RefCusCode");
			});
			refCusCodeList1.Delete();
			refCusCodeList2.Delete();
			refCusCodeList3.Delete();
		}

		public static void AssertPropertyIsMandatoryWhenHasAttributeWithValueYAndRule(ZPropertyInfo propertyInfo, string codeListType, string level, string attributeName, Action enableG0321Rule, Action disableG0321Rule)
		{
			var factory = propertyInfo.BizObj.Factory;
			var (refCusCodeList1, refCusCodeList2, refCusCodeList3) = CreateRefCusCodeListsForTest(codeListType, level, factory, attributeName);
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage(propertyInfo.HumanReadableName);
			Assertion.CombineAssertions(() =>
			{
				var cusSupportingInfo = (CusSupportingInfo)propertyInfo.BizObj;

				enableG0321Rule();
				cusSupportingInfo.CSI_Code = refCusCodeList1.ZZD_Code.Left(CusSupportingInfo.Schema.CSI_CodeMaxLength);
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo, messageError, $"RefCusCode1 with '{attributeName}'-attribute and value 'no'");

				disableG0321Rule();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo, messageError, $"RefCusCode1 with '{attributeName}'-attribute and value 'yes'");

				cusSupportingInfo.CSI_Code = refCusCodeList2.ZZD_Code.Left(CusSupportingInfo.Schema.CSI_CodeMaxLength);
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo, messageError, $"RefCusCode2 with '{attributeName}'-attribute and value 'no'");

				cusSupportingInfo.CSI_Code = refCusCodeList3.ZZD_Code.Left(CusSupportingInfo.Schema.CSI_CodeMaxLength);
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo, messageError, $"RefCusCode3 with '{attributeName}'-attribute not set");

				cusSupportingInfo.CSI_Code = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo, messageError, $"No RefCusCode");
			});
			refCusCodeList1.Delete();
			refCusCodeList2.Delete();
			refCusCodeList3.Delete();
		}

		public static void AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(ZPropertyInfo propertyInfo, string codeListType, string level, string attributeName)
		{
			var (refCusCodeList1, refCusCodeList2, refCusCodeList3) = CreateRefCusCodeListsForTest(codeListType, level, propertyInfo.BizObj.Factory);
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage(propertyInfo.HumanReadableName);

			var cusSupportingInfo = (CusSupportingInfo)propertyInfo.BizObj;
			cusSupportingInfo.CSI_Code = refCusCodeList1.ZZD_Code.Left(CusSupportingInfo.Schema.CSI_CodeMaxLength);
			ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo, messageError, $"RefCusCode1 with not any '{attributeName}'-attribute");

			refCusCodeList1.Delete();
			refCusCodeList2.Delete();
			refCusCodeList3.Delete();
		}
	}
}
