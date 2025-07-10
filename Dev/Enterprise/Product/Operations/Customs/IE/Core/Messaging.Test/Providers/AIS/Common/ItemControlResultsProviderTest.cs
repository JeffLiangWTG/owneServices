using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class ItemControlResultsProviderTest : TestCase
	{
		ItemControlResultsProvider provider;

		public void TestSequenceNumber()
		{
			provider = new ItemControlResultsProvider(new MItemControlResultsType01 { SequenceNumber = "1" });
			AssertEquals("1", provider.SequenceNumber);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			provider = new ItemControlResultsProvider(new MItemControlResultsType01 { DeclarationGoodsItemNumber = "10001" });
			AssertEquals("10001", provider.DeclarationGoodsItemNumber);
		}

		public void TestControlResultCode()
		{
			provider = new ItemControlResultsProvider(new MItemControlResultsType01 { ControlResultCode = "A1" });
			AssertEquals("A1", provider.ControlResultCode);
		}

		public void TestResultsOfControl()
		{
			provider = new ItemControlResultsProvider(new MItemControlResultsType01
			{
				ResultsOfControl = new System.Collections.ObjectModel.Collection<MControlResultType02>
				{
					new MControlResultType02 { SequenceNumber = "1" },
					new MControlResultType02 { SequenceNumber = "2" },
				},
			});

			CombineAssertions("When ResultsOfControl has items", () =>
			{
				var resultsOfControl = provider.ResultsOfControl.ToArray();
				AssertEquals("Count", 2, resultsOfControl.Length);
				AssertEquals("Item 1", "1", resultsOfControl[0].SequenceNumber);
				AssertEquals("Item 2", "2", resultsOfControl[1].SequenceNumber);
			});

			provider = new ItemControlResultsProvider(new MItemControlResultsType01());
			AssertEquals("When ResultsOfControl is null", 0, provider.ResultsOfControl.Count);
		}
	}
}
