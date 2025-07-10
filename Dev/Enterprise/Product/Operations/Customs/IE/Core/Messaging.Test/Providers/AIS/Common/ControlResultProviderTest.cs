using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class ControlResultProviderTest : TestCase
	{
		ControlResultProvider provider;

		public void TestSequenceNumber()
		{
			provider = new ControlResultProvider(new MControlResultType02 { SequenceNumber = "1" });
			AssertEquals("1", provider.SequenceNumber);
		}

		public void TestRiskAreaCode()
		{
			provider = new ControlResultProvider(new MControlResultType02 { RiskAreaCode = "RAC001" });
			AssertEquals("RAC001", provider.RiskAreaCode);
		}

		public void TestControlType()
		{
			provider = new ControlResultProvider(new MControlResultType02 { ControlType = "CT1" });
			AssertEquals("CT1", provider.ControlType);
		}

		public void TestControlDate()
		{
			provider = GetProviderForControlDate(new DateTime(2023, 08, 11, 14, 30, 45));
			AssertEquals("Normal DateTime", new ZDate(2023, 08, 11), provider.ControlDate);

			provider = GetProviderForControlDate(DateTime.MinValue);
			AssertEquals("DateTime.MinValue", ZDate.Empty, provider.ControlDate);

			ControlResultProvider GetProviderForControlDate(DateTime date) => new ControlResultProvider(new MControlResultType02 { ControlDate = date });
		}

		public void TestRemarks()
		{
			provider = new ControlResultProvider(new MControlResultType02 { Remarks = "Remarks001" });
			AssertEquals("Remarks001", provider.Remarks);
		}

		public void TestControlDetails()
		{
			provider = new ControlResultProvider(new MControlResultType02
			{
				ControlDetails = new System.Collections.ObjectModel.Collection<MControlDetailsType>
				{
					new MControlDetailsType { SequenceNumber = "1" },
					new MControlDetailsType { SequenceNumber = "2" },
				},
			});

			CombineAssertions("When ControlDetails has items", () =>
			{
				var controlDetails = provider.ControlDetails.ToArray();
				AssertEquals("Count", 2, controlDetails.Length);
				AssertEquals("Item 1", "1", controlDetails[0].SequenceNumber);
				AssertEquals("Item 2", "2", controlDetails[1].SequenceNumber);
			});

			provider = new ControlResultProvider(new MControlResultType02());
			AssertEquals("When ControlDetails is null", 0, provider.ControlDetails.Count);
		}
	}
}
