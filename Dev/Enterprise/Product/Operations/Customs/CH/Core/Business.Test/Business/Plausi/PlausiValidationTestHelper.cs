using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

class PlausiValidationTestHelper : TestCase
{
	internal static void AssertNS30003(CusEntryInstruction entryInstruction, ZPropertyInfo propertyInfo, string expectedMessageError)
	{
		if (propertyInfo is ZWrappedPropertyInfo)
		{
			propertyInfo = ((ZWrappedPropertyInfo)propertyInfo).InnerInfo;
		}

		if (propertyInfo is ZPropertyInfoString)
		{
			AssertNS30003(entryInstruction, propertyInfo, ZString.Empty, "X", expectedMessageError);
		}
		else if (propertyInfo is ZPropertyInfoDecimal)
		{
			AssertNS30003(entryInstruction, propertyInfo, "0", "1", expectedMessageError);
		}
		else if (propertyInfo is ZPropertyInfoBool)
		{
			AssertNS30003(entryInstruction, propertyInfo, "N", "Y", expectedMessageError);
		}
	}

	static void AssertNS30003(CusEntryInstruction entryInstruction, ZPropertyInfo propertyInfo, string emptyValue, string value, string expectedMessageError)
	{
		AssertNS30003(entryInstruction, propertyInfo, () => propertyInfo.SetValueFromString(emptyValue), () => propertyInfo.SetValueFromString(value), expectedMessageError);
	}

	internal static void AssertNS30003(CusEntryInstruction entryInstruction, ZPropertyInfo propertyInfo, Action setEmptyValue, Action setValue, string expectedMessageError)
	{
		entryInstruction.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		entryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		setEmptyValue();
		TestCaseWithFactory.AssertNoMessageErrorContaining($"When Style is not Simplified and {propertyInfo.HumanReadableName} is {propertyInfo.Value}, no error", propertyInfo, expectedMessageError);

		setValue();
		TestCaseWithFactory.AssertNoMessageErrorContaining($"When Style is not Simplified and {propertyInfo.HumanReadableName} is {propertyInfo.Value}, no error", propertyInfo, expectedMessageError);

		entryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		setEmptyValue();
		TestCaseWithFactory.AssertNoMessageErrorContaining($"When Style is Simplified and {propertyInfo.HumanReadableName} is {propertyInfo.Value}, no error", propertyInfo, expectedMessageError);

		setValue();
		TestCaseWithFactory.AssertHasMessageErrorContaining($"When Style is Simplified and {propertyInfo.HumanReadableName} is {propertyInfo.Value}, error", propertyInfo, expectedMessageError);

		entryInstruction.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		setValue();
		TestCaseWithFactory.AssertNoMessageErrorContaining("Error is not shown for EDA", propertyInfo, expectedMessageError);
	}
}
