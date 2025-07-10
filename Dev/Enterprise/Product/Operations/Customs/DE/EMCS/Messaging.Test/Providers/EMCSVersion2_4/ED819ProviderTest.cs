using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	[TestedType(typeof(ED819Provider))]
	class ED819ProviderTest : InboundDataProviderTestCase<IED819, ED819Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED819Provider(null));
		}

		public void TestMessageGroup()
		{
			AssertEquals(EmcsMessageSubTypeList.Codes.Eme, dataProvider.MessageGroup);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals("0072260102", dataProvider.MessageIdentifier);
		}

		public void TestExciseMovement()
		{
			CombineAssertions(() =>
			{
				var exciseMovement = dataProvider.ExciseMovement;
				AssertEquals("Ead", "20DE41000000001870745", exciseMovement.AdministrativeReferenceCode);
				AssertEquals("Sequence", "1", exciseMovement.SequenceNumber);
				AssertSame("Cached", exciseMovement, dataProvider.ExciseMovement);
			});
		}

		public void TestAlertOrRejectionReasons()
		{
			message.Body.AlertOrRejectionOfAnEad.AlertOrRejectionOfEadReason = new[]
			{
				new ED819CBodyAlertOrRejectionOfAnEadAlertOrRejectionOfEadReason(),
				new ED819CBodyAlertOrRejectionOfAnEadAlertOrRejectionOfEadReason()
			};

			CombineAssertions(() =>
			{
				var exciseMovementEads = dataProvider.AlertOrRejectionReasons;
				AssertEquals("Count", 2, exciseMovementEads.Count);
				AssertSame("Cached", exciseMovementEads, dataProvider.AlertOrRejectionReasons);
			});
		}

		public void TestNoAlertOrRejectionReasons()
		{
			AssertEquals(false, dataProvider.AlertOrRejectionReasons.Any());
		}

		public void TestReasonProviderConstructor()
		{
			message.Body.AlertOrRejectionOfAnEad.AlertOrRejectionOfEadReason = new ED819CBodyAlertOrRejectionOfAnEadAlertOrRejectionOfEadReason[]
			{
				null
			};
			AssertExceptionThrown<ArgumentException>(() => _ = dataProvider.AlertOrRejectionReasons);
		}

		public void TestReasonProviderValues()
		{
			message.Body.AlertOrRejectionOfAnEad.AlertOrRejectionOfEadReason = new ED819CBodyAlertOrRejectionOfAnEadAlertOrRejectionOfEadReason[]
			{
				new ED819CBodyAlertOrRejectionOfAnEadAlertOrRejectionOfEadReason() { AlertOrRejectionOfEadReasonCode = "1", ComplementaryInformation = "Additional Information" }
			};

			CombineAssertions(() =>
			{
				var eadReason = dataProvider.AlertOrRejectionReasons.Single();
				AssertEquals("Code", "1", eadReason.ReasonCode);
				AssertEquals("Information", "Additional Information", eadReason.ComplementaryInformation);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED819C
			{
				Header = new ED819CHeader
				{
					MessageGroup = ED819CHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102",
				},
				Body = new ED819CBody
				{
					AlertOrRejectionOfAnEad = new ED819CBodyAlertOrRejectionOfAnEad
					{
						ExciseMovementEad = new ED819CBodyAlertOrRejectionOfAnEadExciseMovementEad
						{
							AdministrativeReferenceCode = "20DE41000000001870745",
							SequenceNumber = "1"
						}
					}
				}
			};
			dataProvider = new ED819Provider(message);
		}
		ED819C message;
		IED819 dataProvider;

		protected override ED819Provider GetProvider() => (ED819Provider)dataProvider;
	}
}
