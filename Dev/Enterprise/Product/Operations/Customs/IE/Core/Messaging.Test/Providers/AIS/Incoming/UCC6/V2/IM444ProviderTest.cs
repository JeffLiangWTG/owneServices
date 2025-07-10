using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM444;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM444ProviderTest : TestCase
	{
		IM444Provider provider;

		public void TestLocalReferenceNumber()
		{
			provider = new IM444Provider(new Im444 { ImportOperation = new MCciOperationType01 { Lrn = "LRN001" } });
			AssertEquals("LRN001", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			provider = new IM444Provider(new Im444 { ImportOperation = new MCciOperationType01 { Mrn = "12MRN345ABCDE678R9" } });
			AssertEquals("12MRN345ABCDE678R9", provider.MovementReferenceNumber);
		}

		public void TestCode()
		{
			provider = new IM444Provider(new Im444 { ControlResult = new MControlResultType01 { Code = "A1" } });
			AssertEquals("A1", provider.Code);
		}

		public void TestDate()
		{
			provider = GetProviderForDate(new DateTime(2023, 08, 11, 14, 30, 45));
			AssertEquals("Normal DateTime", new ZDate(2023, 08, 11), provider.Date);

			provider = GetProviderForDate(DateTime.MinValue);
			AssertEquals("DateTime.MinValue", ZDate.Empty, provider.Date);

			IM444Provider GetProviderForDate(DateTime date) => new IM444Provider(new Im444 { ControlResult = new MControlResultType01 { Date = date } });
		}

		public void TestRemarks()
		{
			provider = new IM444Provider(new Im444 { ControlResult = new MControlResultType01 { Remarks = "Remarks001" } });
			AssertEquals("Remarks001", provider.Remarks);
		}

		public void TestPendingSamplingResults()
		{
			provider = new IM444Provider(new Im444 { ControlResult = new MControlResultType01 { PendingSamplingResults = "0" } });
			AssertEquals("0", provider.PendingSamplingResults);
		}

		public void TestControlResults()
		{
			provider = new IM444Provider(new Im444
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

			provider = new IM444Provider(new Im444());
			AssertEquals("When ControlResults is null", 0, provider.ControlResults.Count);
		}
	}
}
