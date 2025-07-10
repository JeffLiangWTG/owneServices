using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(TransactionReasonHolder))]
	internal class TransactionReasonHolderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.ReversingCode = "IDE";
			apInvoice.ReversingReason = "reversal reason goes here";

			TransactionReasonHolder holder = new TransactionReasonHolder(apInvoice);
			AssertHolderProperties(holder, TransactionReasonCategory.ReverseReason, "IDE", "Incorrect Data Entry", "reversal reason goes here");

			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice.ReversingCode = "IDE";
			arInvoice.ReversingReason = "amendment reason goes here";

			holder = new TransactionReasonHolder(arInvoice, TransactionReasonCategory.AmendmentReason);
			AssertHolderProperties(holder, TransactionReasonCategory.AmendmentReason, "IDE", "Incorrect Data Entry", "amendment reason goes here");
		}

		public void TestHolderProperties()
		{
			TransactionReasonHolder holder = new TransactionReasonHolder(null);
			TestHolderProperties(holder, TransactionReasonCategory.ReverseReason);

			holder = new TransactionReasonHolder(null, TransactionReasonCategory.AmendmentReason);
			TestHolderProperties(holder, TransactionReasonCategory.AmendmentReason);

			holder = new TransactionReasonHolder(null, TransactionReasonCategory.CreditNoteReversalReason);
			TestHolderProperties(holder, TransactionReasonCategory.CreditNoteReversalReason);

			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			holder = new TransactionReasonHolder(apInvoice, TransactionReasonCategory.ReverseReason);
			TestHolderProperties(holder, TransactionReasonCategory.ReverseReason);

			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			holder = new TransactionReasonHolder(arInvoice, TransactionReasonCategory.AmendmentReason);
			TestHolderProperties(holder, TransactionReasonCategory.AmendmentReason);

			Assert(!holder.IsAmendInFull);

			holder.IsAmendInFull = true;
			Assert(holder.IsAmendInFull);
		}

		public void TestHolderDescription_ExceedMaxLength()
		{
			TransactionReasonHolder holder = new TransactionReasonHolder(null, TransactionReasonCategory.AmendmentReason);

			holder.TransactionReasonCodes_List = SetupCodeDescriptionPairList();
			AssertEquals(2, holder.TransactionReasonCodes_List.Count);

			holder.Code = "IDE";
			AssertEquals("IDE's description", "Incorrect Data Entry", holder.Description);

			holder.Code = "XXX";
			AssertEquals("XXX's description", "000000000-111111111-222222222-333333333-444444444-", holder.Description);
		}

		void TestHolderProperties(TransactionReasonHolder holder, TransactionReasonCategory category)
		{
			AssertHolderProperties(holder, category, string.Empty, string.Empty, string.Empty);

			if (category == TransactionReasonCategory.CreditNoteReversalReason)
			{
				holder.Code = Enterprise.Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectOrganisationBilled;
				AssertHolderProperties(holder, category, Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectOrganisationBilled, Core.Constants.GenApprovalRequestReasonCode.Description.IncorrectOrganisationBilled, Core.Constants.GenApprovalRequestReasonCode.Description.IncorrectOrganisationBilled);

				holder.Code = Enterprise.Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectRating;
				AssertHolderProperties(holder, category, Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectRating, Core.Constants.GenApprovalRequestReasonCode.Description.IncorrectRating, Core.Constants.GenApprovalRequestReasonCode.Description.IncorrectRating);

				holder.Reason = "This is my free text";
				AssertHolderProperties(holder, category, Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectRating, Core.Constants.GenApprovalRequestReasonCode.Description.IncorrectRating, "This is my free text");
			}
			else
			{
				holder.Code = "IDE";
				AssertHolderProperties(holder, category, "IDE", "Incorrect Data Entry", "Incorrect Data Entry");

				holder.Code = "TXT";
				AssertHolderProperties(holder, category, "TXT", "Free Text", "Free Text");

				holder.Reason = "This is my free text";
				AssertHolderProperties(holder, category, "TXT", "Free Text", "This is my free text");
			}
			holder.Code = string.Empty;
			AssertHolderProperties(holder, category, string.Empty, string.Empty, string.Empty);
		}

		void AssertHolderProperties(TransactionReasonHolder holder, TransactionReasonCategory category, string code, string description, string reason)
		{
			AssertEquals("Category is incorrect", category, holder.Category);
			AssertEquals("Code is incorrect", code, holder.Code);
			AssertEquals("Description is incorrect", description, holder.Description);
			AssertEquals("Reason is incorrect", reason, holder.Reason);
		}

		public ReadOnlyCodeDescriptionPairList SetupCodeDescriptionPairList()
		{
			CodeDescriptionPairList newlist1 = new CodeDescriptionPairList();
			newlist1.Add(new CodeDescriptionPair("IDE", "Incorrect Data Entry"));
			newlist1.Add(new CodeDescriptionPair("XXX", "000000000-111111111-222222222-333333333-444444444-5555555555"));

			return newlist1;
		}
	}
}
