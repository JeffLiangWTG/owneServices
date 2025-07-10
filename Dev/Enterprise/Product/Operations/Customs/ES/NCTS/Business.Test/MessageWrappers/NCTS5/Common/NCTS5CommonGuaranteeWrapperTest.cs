using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonGuaranteeWrapperTest : WrapperHelperTest<NCTS5CommonGuaranteeWrapper>
	{
		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "2", wrapper.SequenceNumber);
		}

		public void TestGuaranteeType()
		{
			AssertEquals("Expected filled GuaranteeType", "3", wrapper.GuaranteeType);
		}

		public void TestGuaranteeReference()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty GuaranteeReference when no guarantees declared", 0, wrapper.GuaranteeReference.Count);

				var guarantee = (NctsGuarantee)nctsHeader.GetEffectiveGuarantees().AddNew();
				guarantee.PW_BondNumber = "GRN";
				var guarantee2 = (NctsGuarantee)nctsHeader.GetEffectiveGuarantees().AddNew();
				wrapper = new NCTS5CommonGuaranteeWrapper(new List<NctsGuarantee>() { guarantee, guarantee2 }, "3", 2);
				var guaranteeReference = wrapper.GuaranteeReference;
				AssertEquals("Expected filled GuaranteeReference with both guarantees in list even when one has no data", 2, guaranteeReference.Count);
				AssertSame("Cached GuaranteeReference", wrapper.GuaranteeReference, guaranteeReference);

				wrapper = new NCTS5CommonGuaranteeWrapper(new List<NctsGuarantee>() { guarantee, guarantee2 }, "B", 2);
				guaranteeReference = wrapper.GuaranteeReference;
				AssertEquals("Expected not filled GuaranteeReference if Guarantee Type is B", 0, guaranteeReference.Count);
			});
		}

		public void TestGetGuaranteeList()
		{
			var guarantee1 = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee1.PW_BondType = "3";
			guarantee1.PW_BondNumber = "GRN1";

			var guarantee2 = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee2.PW_BondType = "5";
			guarantee2.PW_BondNumber = "GRN2";

			var guarantee3 = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee3.PW_BondType = "9";
			guarantee3.PW_BondNumber = "GRN3";

			var guarantee4 = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee4.PW_BondType = "3";
			guarantee4.PW_BondNumber = "GRN4";

			var guarantees = NCTS5CommonGuaranteeWrapper.GetGuaranteeList(nctsHeader).ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled guarantees", 3, guarantees.Count);

				AssertEquals("For first guarantee expected filled GuaranteeType", "3", guarantees[0].GuaranteeType);
				AssertContainsExactElementsInAnyOrder("For first guarantee expected filled GuaranteeReference with correct data, GRN", new ZString[] { "GRN1", "GRN4" }, guarantees[0].GuaranteeReference.Select(x => x.GRN));
				AssertContainsExactElementsInExactOrder("For first guarantee expected filled GuaranteeReference with correct data, SequenceNumber", new ZString[] { "1", "2" }, guarantees[0].GuaranteeReference.Select(x => x.SequenceNumber));
				AssertEquals("For first guarantee expected filled SequenceNumber", "1", guarantees[0].SequenceNumber);

				AssertEquals("For second guarantee expected filled GuaranteeType", "5", guarantees[1].GuaranteeType);
				AssertContainsExactElementsInAnyOrder("For second guarantee expected filled GuaranteeReference with correct data, GRN", new ZString[] { "GRN2" }, guarantees[1].GuaranteeReference.Select(x => x.GRN));
				AssertContainsExactElementsInExactOrder("For first guarantee expected filled GuaranteeReference with correct data, SequenceNumber", new ZString[] { "1" }, guarantees[1].GuaranteeReference.Select(x => x.SequenceNumber));
				AssertEquals("For second guarantee expected filled SequenceNumber", "2", guarantees[1].SequenceNumber);

				AssertEquals("For third guarantee expected filled GuaranteeType", "9", guarantees[2].GuaranteeType);
				AssertContainsExactElementsInAnyOrder("For third guarantee expected filled GuaranteeReference with correct data, GRN", new ZString[] { "GRN3" }, guarantees[2].GuaranteeReference.Select(x => x.GRN));
				AssertContainsExactElementsInExactOrder("For first guarantee expected filled GuaranteeReference with correct data, SequenceNumber", new ZString[] { "1" }, guarantees[2].GuaranteeReference.Select(x => x.SequenceNumber));
				AssertEquals("For third guarantee expected filled SequenceNumber", "3", guarantees[2].SequenceNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			wrapper = new NCTS5CommonGuaranteeWrapper(new List<NctsGuarantee>(), "3", 2);
		}

		NctsHeader nctsHeader;
		NCTS5CommonGuaranteeWrapper wrapper;

		protected override NCTS5CommonGuaranteeWrapper GetProvider() => wrapper;
	}
}
