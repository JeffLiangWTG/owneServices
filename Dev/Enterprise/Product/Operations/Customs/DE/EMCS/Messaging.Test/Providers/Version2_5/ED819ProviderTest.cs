using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
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
			message.Body.AlertOrRejectionOfAnEad.AlertOrRejectionOfEadEsadReason = new[]
			{
				new ED819DBodyAlertOrRejectionOfAnEadAlertOrRejectionOfEadEsadReason(),
				new ED819DBodyAlertOrRejectionOfAnEadAlertOrRejectionOfEadEsadReason()
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
			message.Body.AlertOrRejectionOfAnEad.AlertOrRejectionOfEadEsadReason = new ED819DBodyAlertOrRejectionOfAnEadAlertOrRejectionOfEadEsadReason[]
			{
				null
			};
			AssertExceptionThrown<ArgumentException>(() => _ = dataProvider.AlertOrRejectionReasons);
		}

		public void TestReasonProviderValues()
		{
			message.Body.AlertOrRejectionOfAnEad.AlertOrRejectionOfEadEsadReason = new ED819DBodyAlertOrRejectionOfAnEadAlertOrRejectionOfEadEsadReason[]
			{
				new ED819DBodyAlertOrRejectionOfAnEadAlertOrRejectionOfEadEsadReason() { AlertOrRejectionOfMovementReasonCode = "1", ComplementaryInformation = "Additional Information" }
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

			message = new ED819D
			{
				Header = new ED819DHeader
				{
					MessageGroup = ED819DHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102",
				},
				Body = new ED819DBody
				{
					AlertOrRejectionOfAnEad = new ED819DBodyAlertOrRejectionOfAnEad
					{
						ExciseMovement = new ED819DBodyAlertOrRejectionOfAnEadExciseMovement
						{
							AdministrativeReferenceCode = "20DE41000000001870745",
							SequenceNumber = "1"
						}
					}
				}
			};
			dataProvider = new ED819Provider(message);
		}
		ED819D message;
		IED819 dataProvider;

		protected override ED819Provider GetProvider() => (ED819Provider)dataProvider;
	}
}
