using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	[TestedType(typeof(ED839Provider))]
	class ED839ProviderTest : InboundDataProviderTestCase<IED839, ED839Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED839Provider(null));
		}
		public void TestMessageGroup()
		{
			AssertEquals("EME", dataProvider.MessageGroup);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals("0072260102", dataProvider.MessageIdentifier);
		}

		public void TestSendingCustomsOffice()
		{
			AssertEquals("DE003302", dataProvider.SendingCustomsOffice);
		}

		public void TestIssuanceDate()
		{
			AssertEquals(new ZDate(2020, 01, 09), dataProvider.IssuanceDate);
		}

		public void TestMRN()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Element", ZString.Empty, dataProvider.MRN);
				message.Body.CustomsRejectionOfEad.ExportCrossCheckingDiagnoses = new ED839CBodyCustomsRejectionOfEadExportCrossCheckingDiagnoses()
				{
					DocumentReferenceNumber = "20DE12365485421158E2"
				};
				AssertEquals("Exists", "20DE12365485421158E2", dataProvider.MRN);
			});
		}

		public void TestRejectionReasonCode()
		{
			AssertEquals("1", dataProvider.RejectionReasonCode);
		}

		public void TestRejectedEads()
		{
			message.Body.CustomsRejectionOfEad.CEadVal = new ED839CBodyCustomsRejectionOfEadCEadVal[]
			{
				new ED839CBodyCustomsRejectionOfEadCEadVal(),
				new ED839CBodyCustomsRejectionOfEadCEadVal()
			};

			CombineAssertions(() =>
			{
				var exciseMovementEads = dataProvider.RejectedEads;
				AssertEquals("Count", 2, exciseMovementEads.Count);
				AssertSame("Cached", exciseMovementEads, dataProvider.RejectedEads);
			});
		}

		public void TestEventProviderConstructor()
		{
			message.Body.CustomsRejectionOfEad.CEadVal = new ED839CBodyCustomsRejectionOfEadCEadVal[]
			{
				null
			};
			AssertExceptionThrown<ArgumentException>(() => _ = dataProvider.RejectedEads);
		}

		public void TestEventProviderValues()
		{
			message.Body.CustomsRejectionOfEad.CEadVal = new ED839CBodyCustomsRejectionOfEadCEadVal[]
			{
				new ED839CBodyCustomsRejectionOfEadCEadVal() { AdministrativeReferenceCode = "20DE41000000001870745", SequenceNumber = "1" }
			};

			CombineAssertions(() =>
			{
				var rejectedEad = dataProvider.RejectedEads.Single();
				AssertEquals("Ead", "20DE41000000001870745", rejectedEad.AdministrativeReferenceCode);
				AssertEquals("Sequence", "1", rejectedEad.SequenceNumber);
			});
		}

		public void TestLocalReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", ZString.Empty, dataProvider.LocalReferenceNumber);
				message.Body.CustomsRejectionOfEad.ExportCrossCheckingDiagnoses = new ED839CBodyCustomsRejectionOfEadExportCrossCheckingDiagnoses()
				{
					LocalReferenceNumber = "20DE12365485421158E2"
				};
				AssertEquals("LocalReferenceNumber", "20DE12365485421158E2", dataProvider.LocalReferenceNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED839C()
			{
				Header = new ED839CHeader()
				{
					MessageGroup = ED839CHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102",
					MessageSender = "DE003302"
				},
				Body = new ED839CBody()
				{
					CustomsRejectionOfEad = new ED839CBodyCustomsRejectionOfEad()
					{
						Attributes = new ED839CBodyCustomsRejectionOfEadAttributes()
						{
							DateAndTimeOfIssuance = new DateTime(2020, 01, 09)
						},
						Rejection = new ED839CBodyCustomsRejectionOfEadRejection()
						{
							RejectionReasonCode = ED839CBodyCustomsRejectionOfEadRejectionRejectionReasonCode.Item1
						}
					}
				}
			};
			dataProvider = new ED839Provider(message);
		}
		ED839C message;
		IED839 dataProvider;

		protected override ED839Provider GetProvider()
		{
			message.Body.CustomsRejectionOfEad.CEadVal = new ED839CBodyCustomsRejectionOfEadCEadVal[]
			{
				new ED839CBodyCustomsRejectionOfEadCEadVal(),
				new ED839CBodyCustomsRejectionOfEadCEadVal()
			};
			return (ED839Provider)dataProvider;
		}
	}
}
