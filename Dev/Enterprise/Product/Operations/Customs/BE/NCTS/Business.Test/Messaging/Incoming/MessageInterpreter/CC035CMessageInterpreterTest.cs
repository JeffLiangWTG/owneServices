using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.ctypes;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC035CMessageInterpreter))]
	sealed class CC035CMessageInterpreterTest : MessageInterpreterTestCase<CC035CMessageInterpreter, ICC035CDataProvider>
	{
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
			var mockCC035C = new Mock<ICC035CDataProvider>();
			mockCC035C.Setup(m => m.NotificationDate).Returns(new DateTime(2022, 4, 1, 12, 34, 56));
			mockCC035C.Setup(m => m.NotificationText).Returns("a lot of text");
			mockCC035C.Setup(m => m.Guarantor).Returns(guarantor);
			var result = Interpreter.Interpret(mockCC035C.Object, null);
			AssertEquals("Recovery notification for NCTS departure received at 01-Apr-22 12:34:56</br>a lot of text</br>Amount recovered: 0 </br>The guarantor for this declaration is 123 Intris</br>Laan 2</br>2000</br>Antwerp</br>BE", result);
		}
	}
}
