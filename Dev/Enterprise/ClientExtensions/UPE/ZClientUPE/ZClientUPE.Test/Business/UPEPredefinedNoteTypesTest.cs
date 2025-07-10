using System.Collections;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class UPEPredefinedNoteTypesTest : TestCaseWithFactory
	{
		public void TestInstance()
		{
			AssertEquals("UPEPredefinedNoteTypes.Instance should return the correct type", typeof(UPEPredefinedNoteTypes), UPEPredefinedNoteTypes.Instance.GetType());
		}

		public void TestLevel1Record()
		{
			AssertEquals("Should exist in the 'All' array", true, ((IList)UPEPredefinedNoteTypes.Instance.All).Contains(UPEPredefinedNoteTypes.Instance.Level1Record));
			AssertEquals("Description", "Level1Record", UPEPredefinedNoteTypes.Instance.Level1Record.Description);
			AssertEquals("Should be IsTextOnly=true", true, UPEPredefinedNoteTypes.Instance.Level1Record.IsTextOnly);
			AssertEquals("Should be virtually no limit on TextOnlyMaxLength", 100000000, UPEPredefinedNoteTypes.Instance.Level1Record.TextOnlyMaxLength);
		}

		public void TestCRNote()
		{
			AssertEquals("Should exist in the 'All' array", true, ((IList)UPEPredefinedNoteTypes.Instance.All).Contains(UPEPredefinedNoteTypes.Instance.CRNote));
			AssertEquals("Description", "CRNote", UPEPredefinedNoteTypes.Instance.CRNote.Description);
			AssertEquals("Should be IsTextOnly=true", true, UPEPredefinedNoteTypes.Instance.CRNote.IsTextOnly);
			AssertEquals("Should be no limit on TextOnlyMaxLength", 100000000, UPEPredefinedNoteTypes.Instance.CRNote.TextOnlyMaxLength);
		}

		public void TestFinanceNote()
		{
			AssertEquals("Should exist in the 'All' array", true, ((IList)UPEPredefinedNoteTypes.Instance.All).Contains(UPEPredefinedNoteTypes.Instance.FinanceNote));
			AssertEquals("DesFinanceiption", "FinanceNote", UPEPredefinedNoteTypes.Instance.FinanceNote.Description);
			AssertEquals("Should be IsTextOnly=true", true, UPEPredefinedNoteTypes.Instance.FinanceNote.IsTextOnly);
			AssertEquals("Should be no limit on TextOnlyMaxLength", 100000000, UPEPredefinedNoteTypes.Instance.FinanceNote.TextOnlyMaxLength);
		}

		public void TestDeclarationNote()
		{
			AssertEquals("Should exist in the 'All' array", true, ((IList)UPEPredefinedNoteTypes.Instance.All).Contains(UPEPredefinedNoteTypes.Instance.DeclarationNote));
			AssertEquals("DesDeclarationiption", "DeclarationNote", UPEPredefinedNoteTypes.Instance.DeclarationNote.Description);
			AssertEquals("Should be IsTextOnly=true", true, UPEPredefinedNoteTypes.Instance.DeclarationNote.IsTextOnly);
			AssertEquals("Should be no limit on TextOnlyMaxLength", 100000000, UPEPredefinedNoteTypes.Instance.DeclarationNote.TextOnlyMaxLength);
		}

		public void TestPartPaymentNote()
		{
			AssertEquals(true, ((IList)UPEPredefinedNoteTypes.Instance.All).Contains(UPEPredefinedNoteTypes.Instance.PartPaymentNote));
			AssertEquals("PartPaymentNote", UPEPredefinedNoteTypes.Instance.PartPaymentNote.Description);
			AssertEquals(true, UPEPredefinedNoteTypes.Instance.PartPaymentNote.IsTextOnly);
			AssertEquals(100000000, UPEPredefinedNoteTypes.Instance.PartPaymentNote.TextOnlyMaxLength);
		}

		public void TestRefundNote()
		{
			AssertEquals(true, ((IList)UPEPredefinedNoteTypes.Instance.All).Contains(UPEPredefinedNoteTypes.Instance.RefundNote));
			AssertEquals("Refund", UPEPredefinedNoteTypes.Instance.RefundNote.Description);
			AssertEquals(true, UPEPredefinedNoteTypes.Instance.RefundNote.IsTextOnly);
			AssertEquals(100000000, UPEPredefinedNoteTypes.Instance.RefundNote.TextOnlyMaxLength);
		}

		public void TestManualBillNote()
		{
			AssertEquals(true, ((IList)UPEPredefinedNoteTypes.Instance.All).Contains(UPEPredefinedNoteTypes.Instance.ManualBillNote));
			AssertEquals("ManualBill", UPEPredefinedNoteTypes.Instance.ManualBillNote.Description);
			AssertEquals(true, UPEPredefinedNoteTypes.Instance.ManualBillNote.IsTextOnly);
			AssertEquals(100000000, UPEPredefinedNoteTypes.Instance.ManualBillNote.TextOnlyMaxLength);
		}

		public void TestMergeClearanceNote()
		{
			AssertEquals(true, ((IList)UPEPredefinedNoteTypes.Instance.All).Contains(UPEPredefinedNoteTypes.Instance.MergeClearanceNote));
			AssertEquals("MergeClearance", UPEPredefinedNoteTypes.Instance.MergeClearanceNote.Description);
			AssertEquals(true, UPEPredefinedNoteTypes.Instance.MergeClearanceNote.IsTextOnly);
			AssertEquals(100000000, UPEPredefinedNoteTypes.Instance.MergeClearanceNote.TextOnlyMaxLength);
		}

		public void TestPreReleaseNote()
		{
			AssertEquals(true, ((IList)UPEPredefinedNoteTypes.Instance.All).Contains(UPEPredefinedNoteTypes.Instance.PreReleaseNotification));
			AssertEquals("Pre-Release Notification", UPEPredefinedNoteTypes.Instance.PreReleaseNotification.Description);
			AssertEquals(true, UPEPredefinedNoteTypes.Instance.PreReleaseNotification.IsTextOnly);
			AssertEquals(true, UPEPredefinedNoteTypes.Instance.PreReleaseNotification.IsReadOnlyAfterAdd);
			AssertEquals(100000000, UPEPredefinedNoteTypes.Instance.PreReleaseNotification.TextOnlyMaxLength);
		}
	}
}
