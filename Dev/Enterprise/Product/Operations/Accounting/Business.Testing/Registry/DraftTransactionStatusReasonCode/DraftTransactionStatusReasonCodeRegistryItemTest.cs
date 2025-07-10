using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(DraftTransactionStatusReasonCodeRegistryItem))]
	public class DraftTransactionStatusReasonCodeRegistryItemTest : StronglyTypedRegistryItemTestCase<DraftTransactionStatusReasonCodeCollection>
	{
		public void TestDefaultValue()
		{
			AssertEquals(4, Item.DefaultValue.Count);
			CombineAssertions(() =>
			{
				var defaultRow = Item.DefaultValue.Cast<DraftTransactionStatusReasonCode>().FirstOrDefault(x => x.Code == DraftTransactionStatusReasonCode.DefaultReasonCode);
				AssertNotNull(defaultRow);
				Assert(defaultRow.ANL);
				Assert(defaultRow.DFT);
				Assert(defaultRow.DSC);
				Assert(defaultRow.DIS);
				Assert(defaultRow.AFP);
				Assert(defaultRow.AWA);
				Assert(defaultRow.PRS);

				var chqRow = Item.DefaultValue.Cast<DraftTransactionStatusReasonCode>().FirstOrDefault(x => x.Code == "CHQ");
				AssertNotNull(chqRow);
				AssertEquals("Charge billed is higher than quoted. Querying with supplier.", chqRow.EnglishDescription);
				Assert(!chqRow.ANL);
				Assert(!chqRow.DFT);
				Assert(!chqRow.DSC);
				Assert(chqRow.DIS);
				Assert(!chqRow.AFP);
				Assert(!chqRow.AWA);
				Assert(!chqRow.PRS);

				var njrRow = Item.DefaultValue.Cast<DraftTransactionStatusReasonCode>().FirstOrDefault(x => x.Code == "NJR");
				AssertNotNull(njrRow);
				AssertEquals("No jobs or job references identified", njrRow.EnglishDescription);
				Assert(!njrRow.ANL);
				Assert(njrRow.DFT);
				Assert(!njrRow.DSC);
				Assert(njrRow.DIS);
				Assert(!njrRow.AFP);
				Assert(njrRow.AWA);
				Assert(!njrRow.PRS);

				var nafRow = Item.DefaultValue.Cast<DraftTransactionStatusReasonCode>().FirstOrDefault(x => x.Code == "NAF");
				AssertNotNull(nafRow);
				AssertEquals("No accruals found for listed jobs", nafRow.EnglishDescription);
				Assert(!nafRow.ANL);
				Assert(nafRow.DFT);
				Assert(!nafRow.DSC);
				Assert(nafRow.DIS);
				Assert(!nafRow.AFP);
				Assert(nafRow.AWA);
				Assert(!nafRow.PRS);
			});
		}

		protected override StronglyTypedRegistryItem<DraftTransactionStatusReasonCodeCollection, DraftTransactionStatusReasonCodeCollection> GetNewRegistryItem()
		{
			return new DraftTransactionStatusReasonCodeRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}
	}
}
