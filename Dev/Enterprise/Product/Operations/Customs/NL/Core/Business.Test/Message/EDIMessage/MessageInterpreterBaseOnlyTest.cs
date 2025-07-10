using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(MessageInterpreterForTest))]
sealed class MessageInterpreterBaseOnlyTest : MessageInterpreterTestCase<MessageInterpreterForTest, IDMSIncomingDataProvider>
{
	public void TestGetValidStatementTypeCode_Null()
	{
		AssertEquals(string.Empty, Interpreter.GetValidStatementTypeCodeExposed(string.Empty));
	}

	public void TestGetValidStatementTypeCode_Empty()
	{
		AssertEquals(null, Interpreter.GetValidStatementTypeCodeExposed(null));
	}

	public void TestGetValidStatementTypeCode_NotInResponseStatementTypes()
	{
		AssertEquals("ABC", Interpreter.GetValidStatementTypeCodeExposed("ABC"));
	}

	public void TestGetValidStatementTypeCode_AAZ()
	{
		AssertEquals("Enquiry information code", Interpreter.GetValidStatementTypeCodeExposed("AAZ"));
	}

	public void TestGetValidStatementTypeCode_BLF()
	{
		AssertEquals("Examination result comment", Interpreter.GetValidStatementTypeCodeExposed("BLF"));
	}

	public void TestGetValidStatementTypeCode_BAL()
	{
		AssertEquals("Non-acceptance information", Interpreter.GetValidStatementTypeCodeExposed("BAL"));
	}

	public void TestGetValidStatementTypeCode_SPH()
	{
		AssertEquals("Type alternative evidence", Interpreter.GetValidStatementTypeCodeExposed("SPH"));
	}

	public void TestGetValidStatementTypeCode_AES()
	{
		AssertEquals("Reason for an amendment", Interpreter.GetValidStatementTypeCodeExposed("AES"));
	}

	public void TestGetValidStatementTypeCode_CUS()
	{
		AssertEquals("Reason for an invalidation", Interpreter.GetValidStatementTypeCodeExposed("CUS"));
	}

	public void TestGetValidStatementTypeCode_AHN()
	{
		AssertEquals("Status details", Interpreter.GetValidStatementTypeCodeExposed("AHN"));
	}

	public void TestGetValidStatementTypeCode_ACR()
	{
		AssertEquals("Unexpected stops information", Interpreter.GetValidStatementTypeCodeExposed("ACR"));
	}

	public override string ExpectedMessageInterpretation => "Interpret";
}

sealed class MessageInterpreterForTest : BaseMessageInterpreter<IDMSIncomingDataProvider>
{
	public override string Interpret(IDMSIncomingDataProvider dataProvider, EDIMessage ediMessage) => "Interpret";

	public string GetValidStatementTypeCodeExposed(string statementTypeCode) => GetValidStatementTypeCode(statementTypeCode);
}
