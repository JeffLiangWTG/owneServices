using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EdecSpecialMentionDataProviderTest : TestCaseWithFactory
{
	public void TestNewCollection()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Null argument", 0, EdecSpecialMentionDataProvider.NewCollection(null).Count());

			specialMentions.SpecialMentions = "Test";
			AssertNotNull("Non-null argument", EdecSpecialMentionDataProvider.NewCollection(new[] { specialMentions }));
		});
	}

	public void TestEmpty()
	{
		specialMentions.SpecialMentions = ZString.Empty;
		AssertEquals("Count", 0, EdecSpecialMentionDataProvider.NewCollection(new[] { specialMentions }).Count());
	}

	public void TestNonEmpty()
	{
		specialMentions.SpecialMentions = "Line 1\r\nLine 2\r\nLine 3";
		var dataProviders = EdecSpecialMentionDataProvider.NewCollection(new[] { specialMentions });

		CombineAssertions(() =>
		{
			AssertEquals("", 3, dataProviders.Count());
			AssertEquals(1, dataProviders.ElementAt(0).SequenceNumber);
			AssertEquals(2, dataProviders.ElementAt(1).SequenceNumber);
			AssertEquals(3, dataProviders.ElementAt(2).SequenceNumber);
			AssertEquals("Line 1", dataProviders.ElementAt(0).Text);
			AssertEquals("Line 2", dataProviders.ElementAt(1).Text);
			AssertEquals("Line 3", dataProviders.ElementAt(2).Text);
		});
	}

	public void TestMaximumNumberOfLines()
	{
		var text = new ZStringBuilder();
		for (int n = 1; n <= 100; n++)
		{
			text.AppendLine($"Line {n}");
		}
		specialMentions.SpecialMentions = text.ToString();

		var dataProviders = EdecSpecialMentionDataProvider.NewCollection(new[] { specialMentions });
		CombineAssertions(() =>
		{
			AssertEquals("Count", 99, dataProviders.Count());
			AssertEquals("First line", "Line 1", dataProviders.First().Text);
			AssertEquals("Last line", "Line 99", dataProviders.Last().Text);
		});
	}

	public void TestMultipleInvoiceHeaders()
	{
		var specialMentionsList = new[]
		{
				new SpecialMentionsForTesting("Invoice 1 Line 1\r\nInvoice 1 Line 2\r\nInvoice 1 Line 3\r\n"),
				new SpecialMentionsForTesting("Invoice 2 Line 1\r\nInvoice 2 Line 2\r\n"),
			};

		var dataProviders = EdecSpecialMentionDataProvider.NewCollection(specialMentionsList);

		CombineAssertions(() =>
		{
			AssertEquals("Count", 5, dataProviders.Count());
			AssertEquals("Inv 1 Line 1", "Invoice 1 Line 1", dataProviders.ElementAt(0).Text);
			AssertEquals("Inv 1 Line 2", "Invoice 1 Line 2", dataProviders.ElementAt(1).Text);
			AssertEquals("Inv 1 Line 3", "Invoice 1 Line 3", dataProviders.ElementAt(2).Text);
			AssertEquals("Inv 2 Line 1", "Invoice 2 Line 1", dataProviders.ElementAt(3).Text);
			AssertEquals("Inv 2 Line 2", "Invoice 2 Line 2", dataProviders.ElementAt(4).Text);
			AssertEquals("Inv 1 Line 1", 1, dataProviders.ElementAt(0).SequenceNumber);
			AssertEquals("Inv 1 Line 2", 2, dataProviders.ElementAt(1).SequenceNumber);
			AssertEquals("Inv 1 Line 3", 3, dataProviders.ElementAt(2).SequenceNumber);
			AssertEquals("Inv 2 Line 1", 4, dataProviders.ElementAt(3).SequenceNumber);
			AssertEquals("Inv 2 Line 2", 5, dataProviders.ElementAt(4).SequenceNumber);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		specialMentions = new SpecialMentionsForTesting(ZString.Empty);
	}
	ISpecialMentions specialMentions;

	class SpecialMentionsForTesting : ISpecialMentions
	{
		internal SpecialMentionsForTesting(ZString text)
		{
			SpecialMentions = text;
		}

		public ZString SpecialMentions { get; set; }
	}
}
