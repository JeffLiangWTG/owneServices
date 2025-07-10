using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	[TestedType(typeof(ED810Provider))]
	class ED810ProviderTest : InboundDataProviderTestCase<IED810, ED810Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED810Provider(null));
		}

		public void TestMessageGroup()
		{
			AssertEquals("EME", dataProvider.MessageGroup);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals("0072260102", dataProvider.MessageIdentifier);
		}

		public void TestExciseMovementEad()
		{
			message.Body.CancellationOfEad.ExciseMovementEad = new ED810CBodyCancellationOfEadExciseMovementEad();
			AssertSame("Cached", dataProvider.ExciseMovementEad, dataProvider.ExciseMovementEad);
		}

		public void TestEventProviderConstructor()
		{
			message.Body.CancellationOfEad.ExciseMovementEad = null;
			AssertExceptionThrown<ArgumentException>(() => _ = dataProvider.ExciseMovementEad);
		}

		public void TestEventProviderValues()
		{
			message.Body.CancellationOfEad.ExciseMovementEad = new ED810CBodyCancellationOfEadExciseMovementEad() { AdministrativeReferenceCode = "20DE41000000001870745" };

			CombineAssertions(() =>
			{
				var exciseMovementEad = dataProvider.ExciseMovementEad;
				AssertEquals("Ead", "20DE41000000001870745", exciseMovementEad.AdministrativeReferenceCode);
				AssertEquals("Sequence", "1", exciseMovementEad.SequenceNumber);
			});
		}

		public void TestCancellationReasonCode()
		{
			message.Body.CancellationOfEad.Cancellation = new ED810CBodyCancellationOfEadCancellation() { CancellationReasonCode = "1" };
			AssertEquals("1", dataProvider.CancellationReasonCode);
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED810C()
			{
				Header = new ED810CHeader()
				{
					MessageGroup = ED810CHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102"
				},
				Body = new ED810CBody()
				{
					CancellationOfEad = new ED810CBodyCancellationOfEad()
				}
			};
			dataProvider = new ED810Provider(message);
		}
		ED810C message;
		IED810 dataProvider;

		protected override ED810Provider GetProvider() => (ED810Provider)dataProvider;
	}
}
