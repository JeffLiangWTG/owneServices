using System;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(EditableFieldBizObjectValidation))]
sealed class EditableFieldBizObjectValidationTest : TestCaseWithFactory
{
	public void TestCheckOverrideValue_GENS()
	{
		var fieldDefinition = JPMessageTestHelper.CreateField(1, "Container Count", "コンテナ本数", "COS", "an", 40);
		var sourceField = new EditableMessageField(fieldDefinition);
		var editableFieldBizObject1 = new EditableFieldBizObject("EDA", sourceField);
		var editableFieldBizObject2 = new EditableFieldBizObject("EDA", sourceField);

		var providerMock = new Mock<IMessageContentProvider>();
		providerMock.Setup(provider => provider.Factory).Returns(Factory);
		providerMock.Setup(provider => provider.GetMessageData()).Returns(Array.Empty<byte>());
		providerMock.Setup(provider => provider.ProcedureCode).Returns("TST");
		var messageVisualObject = new MessageVisualObject(providerMock.Object);
		messageVisualObject.Header.Add(editableFieldBizObject1);
		messageVisualObject.Header.Add(editableFieldBizObject2);

		var text = "is not supported by NACCS.";
		editableFieldBizObject2.OverrideValue = "______";
		AssertHasMessageErrorContaining(editableFieldBizObject2.OverrideValueInfo, text);
		editableFieldBizObject1.OverrideValue = ApprovalCertificateInfoCodes.GENS;
		editableFieldBizObject2.Validation.ValidateAll();
		AssertNoMessageError(editableFieldBizObject2.OverrideValueInfo, text);
	}

	public void TestCheckNOverrideValue()
	{
		var fieldDefinition = JPMessageTestHelper.CreateField(1, "Container Count", @"コンテナ本数", "COS", "n", 4);
		var sourceField = new EditableMessageField(fieldDefinition);
		var fieldBizObj = new EditableFieldBizObject(JPProcedureCodeList.Codes.EDA, sourceField);

		var errorMessage = "The override value is not valid.";

		fieldBizObj.OverrideValue = "TST";
		AssertHasError(fieldBizObj.OverrideValueInfo, errorMessage);

		fieldBizObj.OverrideValue = "15.1";
		AssertNoError(fieldBizObj.OverrideValueInfo, errorMessage);
	}

	public void TestCheckANOverrideValue()
	{
		var fieldDefinition = JPMessageTestHelper.CreateField(1, "Container Count", @"コンテナ本数", "COS", "an", 30);
		var sourceField = new EditableMessageField(fieldDefinition);
		var fieldBizObj = new EditableFieldBizObject(JPProcedureCodeList.Codes.EDA, sourceField);

		var warningMessage = "is not supported by NACCS.";

		fieldBizObj.OverrideValue = "a";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "$";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "¥";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "[";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "]";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "^";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "_";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "12A\r\n !\"%&'()*+,-./:;<=>?@#";
		AssertNoMessageError(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj = new EditableFieldBizObject(JPProcedureCodeList.Codes.IVA, sourceField);
		fieldBizObj.OverrideValue = "a";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "$";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "[";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "]";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "^";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "_";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "12A\r\n !\"%&'()*+,-./:;<=>?@#¥";
		AssertNoMessageError(fieldBizObj.OverrideValueInfo, warningMessage);
	}

	public void TestCheckSNOverrideValue()
	{
		var fieldDefinition = JPMessageTestHelper.CreateField(1, "Container Count", @"コンテナ本数", "COS", "sn", 40);
		var sourceField = new EditableMessageField(fieldDefinition);
		var fieldBizObj = new EditableFieldBizObject(JPProcedureCodeList.Codes.EDA, sourceField);

		var warningMessage = "is not supported by NACCS.";

		fieldBizObj.OverrideValue = "★";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "∋";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "ズ";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "嫁";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "12Aa\r\n !\"%&'()*+,-./:;<=>?@`{|}~$[]^_#";
		AssertNoMessageError(fieldBizObj.OverrideValueInfo, warningMessage);
	}

	public void TestCheckJOverrideValue()
	{
		var fieldDefinition = JPMessageTestHelper.CreateField(1, "Container Count", @"コンテナ本数", "COS", "j", 50);
		var sourceField = new EditableMessageField(fieldDefinition);
		var fieldBizObj = new EditableFieldBizObject(JPProcedureCodeList.Codes.EDA, sourceField);

		var warningMessage = "is not supported by NACCS.";
		fieldBizObj.OverrideValue = "俱";
		AssertHasMessageErrorContaining(fieldBizObj.OverrideValueInfo, warningMessage);

		fieldBizObj.OverrideValue = "12Aa\r\n !\"%&'()*+,-./:;<=>?@`{|}~$[]^_★∋じズ嫁";
		AssertNoMessageError(fieldBizObj.OverrideValueInfo, warningMessage);
	}
}
