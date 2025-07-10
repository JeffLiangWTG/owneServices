using System.Collections.ObjectModel;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.ctypes;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC055CMessageInterpreter))]
	sealed class CC055CMessageInterpreterTest : MessageInterpreterTestCase<CC055CMessageInterpreter, ICC055CDataProvider>
	{
		public override void TestInterpret()
		{
			var guarenteeReference = GuaranteeReferenceXmlProvider.New(new GuaranteeReferenceType08
			{
				SequenceNumber = "1",
				Grn = "GR11",
				InvalidGuaranteeReason = new Collection<InvalidGuaranteeReasonType01>
				{
					new InvalidGuaranteeReasonType01
					{
						SequenceNumber = "1",
						Code = "G11",
						Text = "Text01"
					}
				}
			});
			var mockCC055C = new Mock<ICC055CDataProvider>();
			var guaranteeReferences = new Collection<GuaranteeReferenceXmlProvider> { guarenteeReference };
			mockCC055C.Setup(m => m.GuaranteeReferences).Returns(guaranteeReferences);
			var result = Interpreter.Interpret(mockCC055C.Object, null);
			AssertEquals("Guarantee invalid.</br>GRN GR11</br>Reason : sequence: 1</br>Reason : code: G11 Customs Office of Departure and Customs Office of Destination do not correspond (Guarantee type ‘2’)</br>Reason : text: Text01", result);
		}
	}
}
