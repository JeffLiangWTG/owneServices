using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.ctypes;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC045CMessageInterpreter))]
	sealed class CC045CMessageInterpreterTest : MessageInterpreterTestCase<CC045CMessageInterpreter, ICC045CDataProvider>
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
			var mockCC045C = new Mock<ICC045CDataProvider>();
			mockCC045C.Setup(m => m.WriteOffDate).Returns(new DateTime(2022, 4, 1, 12, 34, 56));
			mockCC045C.Setup(m => m.Guarantor).Returns(guarantor);
			var result = Interpreter.Interpret(mockCC045C.Object, null);
			AssertEquals("Write-Off notification for NCTS departure received at 01-Apr-22 12:34:56</br>The guarantor for this declaration is 123 Intris</br>Laan 2</br>2000</br>Antwerp</br>BE", result);
		}
	}
}
