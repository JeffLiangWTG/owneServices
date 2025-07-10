using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(RejectOrderReasonHolder))]
	internal class RejectOrderReasonHolderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHolderProperties()
		{
			RejectOrderReasonHolder holder = new RejectOrderReasonHolder();
			TestHolderProperties(holder);
		}

		void TestHolderProperties(RejectOrderReasonHolder holder)
		{
			AssertHolderProperties(holder, string.Empty, string.Empty, string.Empty);
			holder.Code = "INS";
			AssertHolderProperties(holder, "INS", "Insufficient Funds in Bank Account", "Insufficient Funds in Bank Account");
			holder.Code = "DIS";
			AssertHolderProperties(holder, "DIS", "Invoice charges disputed", "Invoice charges disputed");
			holder.Code = "TXT";
			holder.Reason = "This is my free text";
			AssertHolderProperties(holder, "TXT", "Free Text", "This is my free text");
			holder.Code = string.Empty;
			AssertHolderProperties(holder, string.Empty, string.Empty, string.Empty);
		}

		void AssertHolderProperties(RejectOrderReasonHolder holder, string code, string description, string reason)
		{
			AssertEquals("Code is incorrect", code, holder.Code);
			AssertEquals("Description is incorrect", description, holder.Description);
			AssertEquals("Reason is incorrect", reason, holder.Reason);
		}

		public void TestHolderDescription_ExceedMaxLength()
		{
			RejectOrderReasonHolder holder = new RejectOrderReasonHolder();
			holder.OrderRejectReasonCodes_List = SetupCodeDescriptionPairList();
			AssertEquals(2, holder.OrderRejectReasonCodes_List.Count);
			holder.Code = "IDE";
			AssertEquals("IDE's description", "Incorrect Data Entry", holder.Description);
			holder.Code = "XXX";
			AssertEquals("XXX's description", "000000000-111111111-222222222-333333333-444444444-", holder.Description);
		}

		ReadOnlyCodeDescriptionPairList SetupCodeDescriptionPairList()
		{
			CodeDescriptionPairList newlist1 = new CodeDescriptionPairList();
			newlist1.Add(new CodeDescriptionPair("IDE", "Incorrect Data Entry"));
			newlist1.Add(new CodeDescriptionPair("XXX", "000000000-111111111-222222222-333333333-444444444-5555555555"));
			return newlist1;
		}
	}
}
