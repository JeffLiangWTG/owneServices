using System.Collections.Generic;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class GuaranteeProviderTest : DataProviderTestCase<GuaranteeProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals("Sequence number", "2", provider.SequenceNumber);
		}

		public void TestGuaranteeType()
		{
			AssertEquals("Guarantee type", "3", provider.GuaranteeType);
		}

		public void TestGuaranteeReference()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty GuaranteeReference when no guarantees declared", 0, provider.GuaranteeReference.Count);

				var guarantee = entryInstruction.Guarantees.AddNew();
				guarantee.PW_BondNumber = "GRN";
				var guarantee2 = entryInstruction.Guarantees.AddNew();
				provider = new GuaranteeProvider(new List<GuaranteeForEntryInstruction>() { guarantee, guarantee2 }, "3", 2);
				var guaranteeReference = provider.GuaranteeReference;
				AssertEquals("Expected filled GuaranteeReference with both guarantees in list even when one has no data", 2, guaranteeReference.Count);
				AssertSame("Cached GuaranteeReference", provider.GuaranteeReference, guaranteeReference);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			entryInstruction = Factory.New<CusEntryInstruction>();

			provider = new GuaranteeProvider(new List<GuaranteeForEntryInstruction>(), "3", 2);
		}

		CusEntryInstruction entryInstruction;
		GuaranteeProvider provider;

		protected override GuaranteeProvider GetProvider() => provider;
	}
}
