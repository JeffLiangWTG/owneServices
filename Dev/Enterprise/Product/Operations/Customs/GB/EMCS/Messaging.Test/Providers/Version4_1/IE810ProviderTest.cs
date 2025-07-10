using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie810;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tms;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE810ProviderTest : Business.Testing.DataProviderTestCase<IE810Provider>
	{
		public void TestMrnNumber()
		{
			AssertEquals("22GB58500000004684557", Provider.MrnNumber);
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
				AssertEquals("Ead", "22GB58500000004684557", exciseMovementEad.AdministrativeReferenceCode);
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
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 07, 14),
					TimeOfPreparation = new DateTime(2022, 07, 14, 13, 29, 08, 000),
					MessageIdentifier = "04A219EB-4E21-4D21-AE10-71338047833E",
				},
				Body = new BodyType
				{
					CancellationOfEad = new CancellationOfEadType
					{
						Attributes = new AttributesType { DateAndTimeOfValidationOfCancellation = new DateTime(2022, 07, 14, 13, 29, 08, 000) },
						ExciseMovementEad = new ExciseMovementEadType { AdministrativeReferenceCode = "22GB58500000004684557" },
						Cancellation = new CancellationType { CancellationReasonCode = "0", ComplementaryInformation = new LsdComplementaryInformationType { Language = "en" } }
					}
				}
			};
			return new IE810Provider(message);
		}
		Ie810Type message;
	}
}
