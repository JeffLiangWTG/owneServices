using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.ctypes;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC019CMessageInterpreter))]
	sealed class CC019CMessageInterpreterTest : MessageInterpreterTestCase<CC019CMessageInterpreter, ICC019CDataProvider>
	{
		[ExpectNoExceptions]
		public override void TestInterpret()
		{
			var guarantor = GuarantorXmlProvider.New(new GuarantorType06());
			var address = AddressXmlProvider.New(new AddressType07());
			guarantor.Id = "123";
			guarantor.Name = "Intris";
			address.StreetAndNumber = "Laan 2";
			address.Postcode = "2000";
			address.City = "Antwerp";
			address.Country = "BE";
			guarantor.Address = address;
			var mockCC019C = new Mock<ICC019CDataProvider>();
			mockCC019C.Setup(m => m.WriteOffDate).Returns(new DateTime(2022, 4, 1, 12, 34, 56));
			mockCC019C.Setup(m => m.NotificationText).Returns("TRALALALA really a massive amount of texxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxt");
			mockCC019C.Setup(m => m.Guarantor).Returns(guarantor);
			var result = Interpreter.Interpret(mockCC019C.Object, null);
			AssertEquals("Discrepancies for NCTS departure received at 01-Apr-22 12:34:56</br>TRALALALA really a massive amount of texxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxt</br>The guarantor for this declaration is 123 Intris</br>Laan 2</br>2000</br>Antwerp</br>BE", result);
			mockCC019C.VerifyAll();
		}
	}
}
