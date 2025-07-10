using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE810;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tms;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	class IE810ProviderTest : Business.Testing.DataProviderTestCase<IE810Provider>
	{
		public void TestMrnNumber()
		{
			AssertEquals("22DE58500000004684557", Provider.MrnNumber);
		}

		public void TestMrnNumberSequenceNumber()
		{
			AssertEquals("1", Provider.MrnNumberSequenceNumber);
		}

		public void TestCancellationReasonCode()
		{
			AssertEquals("0", Provider.CancellationReasonCode);
		}

		public void TestExciseMovementEad()
		{
			CombineAssertions(() =>
			{
				var exciseMovementEad = Provider.ExciseMovementEad;
				AssertEquals("Ead", "22DE58500000004684557", exciseMovementEad.AdministrativeReferenceCode);
				AssertEquals("Sequence", "1", exciseMovementEad.SequenceNumber);
			});
		}

		protected override IEnumerable<Expression<Func<IE810Provider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ExciseMovementEad;
		}

		protected override IE810Provider GetProvider()
		{
			message = new Ie810Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 07, 14),
					TimeOfPreparation = "13:29:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new BodyType
				{
					CancellationOfEad = new CancellationOfEadType()
					{
						Attributes = new AttributesType { DateAndTimeOfValidationOfCancellation = new DateTime(2022, 07, 14, 13, 29, 08, 000) },
						ExciseMovementEad = new ExciseMovementEadType { AdministrativeReferenceCode = "22DE58500000004684557" },
						Cancellation = new CancellationType { CancellationReasonCode = "0", ComplementaryInformation = new LsdComplementaryInformationType { Language = "de" } }
					}
				}
			};
			return new IE810Provider(message);
		}
		Ie810Type message;
	}
}
