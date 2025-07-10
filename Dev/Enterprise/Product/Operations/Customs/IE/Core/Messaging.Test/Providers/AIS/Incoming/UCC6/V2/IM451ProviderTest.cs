using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM451;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM451ProviderTest : TestCase
	{
		IM451Provider provider;

		public void TestMovementReferenceNumber()
		{
			provider = new IM451Provider(new Im451 { ImportOperation = new MCciOperationType48 { Mrn = "12MRN345ABCDE678R9" } });
			AssertEquals("12MRN345ABCDE678R9", provider.MovementReferenceNumber);
		}

		public void TestLocalReferenceNumber()
		{
			provider = new IM451Provider(new Im451 { ImportOperation = new MCciOperationType48 { Lrn = "LRN001" } });
			AssertEquals("LRN001", provider.LocalReferenceNumber);
		}

		public void TestDeclarationType()
		{
			provider = new IM451Provider(new Im451 { ImportOperation = new MCciOperationType48 { DeclarationType = "CO" } });
			AssertEquals("CO", provider.DeclarationType);
		}

		public void TestAdditionalDeclarationType()
		{
			provider = new IM451Provider(new Im451 { ImportOperation = new MCciOperationType48 { AdditionalDeclarationType = "A" } });
			AssertEquals("A", provider.AdditionalDeclarationType);
		}

		public void TestDecisionDate()
		{
			provider = GetProviderForDate(new DateTime(2023, 08, 11, 14, 30, 45));
			AssertEquals("Normal DateTime", new ZDate(2023, 08, 11), provider.DecisionDate);

			provider = GetProviderForDate(DateTime.MinValue);
			AssertEquals("DateTime.MinValue", ZDate.Empty, provider.DecisionDate);

			IM451Provider GetProviderForDate(DateTime date) => new IM451Provider(new Im451 { ImportOperation = new MCciOperationType48 { DecisionDate = date } });
		}

		public void TestDecisionReason()
		{
			provider = new IM451Provider(new Im451 { ImportOperation = new MCciOperationType48 { DecisionReason = "Decision Reason" } });
			AssertEquals("Decision Reason", provider.DecisionReason);
		}

		public void TestPreferredPaymentMethod()
		{
			provider = new IM451Provider(new Im451 { ImportOperation = new MCciOperationType48 { PreferredPaymentMethod = "A" } });
			AssertEquals("A", provider.PreferredPaymentMethod);
		}

		public void TestRemarks()
		{
			provider = new IM451Provider(new Im451 { ImportOperation = new MCciOperationType48 { Remarks = "Remarks001" } });
			AssertEquals("Remarks001", provider.Remarks);
		}

		public void TestControlResults()
		{
			provider = new IM451Provider(new Im451
			{
				ControlResults = new System.Collections.ObjectModel.Collection<MItemControlResultsType01>
				{
					new MItemControlResultsType01 { SequenceNumber = "1" },
					new MItemControlResultsType01 { SequenceNumber = "2" },
				},
			});

			CombineAssertions("When ControlResults has items", () =>
			{
				var controlResults = provider.ControlResults.ToArray();
				AssertEquals("Count", 2, controlResults.Length);
				AssertEquals("Item 1", "1", controlResults[0].SequenceNumber);
				AssertEquals("Item 2", "2", controlResults[1].SequenceNumber);
			});

			provider = new IM451Provider(new Im451());
			AssertEquals("When ControlResults is null", 0, provider.ControlResults.Count);
		}
	}
}
