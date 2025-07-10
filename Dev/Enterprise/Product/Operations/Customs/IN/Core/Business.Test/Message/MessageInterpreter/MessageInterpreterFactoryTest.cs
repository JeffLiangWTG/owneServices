using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(MessageInterpreterFactory))]
sealed class MessageInterpreterFactoryTest : TestCaseWithFactory
{
	public void TestGetMessageInterpreter()
	{
		AssertGetMessageInterpreter(EDIMessage.Direction.Transmit, typeof(TransmitMessageInterpreter));
		AssertGetMessageInterpreter(EDIMessage.Direction.Receive, typeof(ReceiveMessageInterpreter));

		void AssertGetMessageInterpreter(ZString receiveTransmit, Type expectInterpreterType)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = receiveTransmit;
			AssertType(expectInterpreterType, MessageInterpreterFactory.GetINMessageInterpreter(message));
		}
	}

	public void TestGetResponseDescription()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, RefCusCodeListTypes.Codes.ErrorCodeDescription, "ERR001", "Error description for ERR001", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, RefCusCodeListTypes.Codes.ErrorCodeDescription, "ERR001", "Error description for ERR002", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ERR001", "Error description for ERR003", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Error description for ERR001", MessageInterpreterFactory.GetResponseDescription(new BusinessObjectFactory(), "ERR001"));
			AssertEquals("Response code description not found", MessageInterpreterFactory.GetResponseDescription(new BusinessObjectFactory(), "ERR002"));
		});
	}
}
