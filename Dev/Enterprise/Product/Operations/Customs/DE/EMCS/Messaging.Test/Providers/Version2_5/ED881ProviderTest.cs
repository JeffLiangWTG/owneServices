using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	[TestedType(typeof(ED881Provider))]
	class ED881ProviderTest : InboundDataProviderTestCase<IED881, ED881Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED881Provider(null));
		}

		public void TestMessageGroup()
		{
			AssertEquals("EME", dataProvider.MessageGroup);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals("0072260102", dataProvider.MessageIdentifier);
		}

		public void TestResponseAttributes()
		{
			message.Body.ManualClosureResponse.Attributes = new ED881ABodyManualClosureResponseAttributes();
			AssertSame("Cached", dataProvider.ResponseAttributes, dataProvider.ResponseAttributes);
		}

		public void TestEventProviderConstructor()
		{
			message.Body.ManualClosureResponse.Attributes = null;
			AssertExceptionThrown<ArgumentException>(() => _ = dataProvider.ResponseAttributes);
		}

		public void TestEventProviderValues()
		{
			message.Body.ManualClosureResponse.Attributes = new ED881ABodyManualClosureResponseAttributes() { AdministrativeReferenceCode = "20DE41000000001870745", SequenceNumber = "1" };

			CombineAssertions(() =>
			{
				var responseAttributes = dataProvider.ResponseAttributes;
				AssertEquals("Ead", "20DE41000000001870745", responseAttributes.AdministrativeReferenceCode);
				AssertEquals("Sequence", "1", responseAttributes.SequenceNumber);
			});
		}

		public void TestMessageSender()
		{
			AssertEquals("DE007226", dataProvider.MessageSender);
		}

		public void TestRequestAccepted()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Accepted", ZBool.True, dataProvider.RequestAccepted);
				message.Body.ManualClosureResponse.Attributes.ManualClosureRequestAccepted = ED881ABodyManualClosureResponseAttributesManualClosureRequestAccepted.Item0;
				AssertEquals("Rejected", ZBool.False, dataProvider.RequestAccepted);
			});
		}

		public void TestRejectionReason()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Rejection Reason", ZString.Empty, dataProvider.RejectionReason);
				message.Body.ManualClosureResponse.Attributes.ManualClosureRejectionReasonCode = "3";
				AssertEquals("Reject Reason", "3", dataProvider.RejectionReason);
			});
		}

		public void TestRejectionComplement()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Rejection Complement", ZString.Empty, dataProvider.RejectionComplement);
				message.Body.ManualClosureResponse.Attributes.ManualClosureRejectionComplement = "Additional Information";
				AssertEquals("Rejection Complement", "Additional Information", dataProvider.RejectionComplement);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED881A()
			{
				Header = new ED881AHeader()
				{
					MessageGroup = ED881AHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102",
					MessageSender = "DE007226"
				},
				Body = new ED881ABody()
				{
					ManualClosureResponse = new ED881ABodyManualClosureResponse()
					{
						Attributes = new ED881ABodyManualClosureResponseAttributes()
						{
							ManualClosureRequestAccepted = ED881ABodyManualClosureResponseAttributesManualClosureRequestAccepted.Item1
						}
					}
				}
			};
			dataProvider = new ED881Provider(message);
		}
		ED881A message;
		IED881 dataProvider;

		protected override ED881Provider GetProvider() => (ED881Provider)dataProvider;
	}
}
