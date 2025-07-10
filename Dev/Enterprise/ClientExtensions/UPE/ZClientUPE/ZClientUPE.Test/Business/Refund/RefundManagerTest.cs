using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class RefundManagerTest : TestCaseWithFactory
	{
		public void TestRefundWith2Owners()
		{
			RefundEnquiryDummy owner1 = Factory.NewWithValidTestData<RefundEnquiryDummy>();
			RefundEnquiryDummy owner2 = Factory.NewWithValidTestData<RefundEnquiryDummy>();
			owner1.OwnerColumn = ClientRefundSchema.T10_JE;
			owner2.OwnerColumn = ClientRefundSchema.T10_CS;
			owner1.RelatedOwner = owner2;
			owner2.RelatedOwner = owner1;
			AssertNull("owner1.Refund", owner1.Refund);
			owner1.RefundManager.CreateClientRefund();
			AssertNotNull(owner1.Refund);
			AssertEquals("Refund has Owner", owner1.PK, owner1.Refund.T10_JE);
			AssertEquals("Refund has Related owner", owner2.PK, owner1.Refund.T10_CS);
			Factory.Save();
			AssertNotNull(owner2.Refund);
			AssertEquals("Refund has Owner", owner2.PK, owner2.Refund.T10_CS);
			AssertEquals("Refund has Related owner", owner1.PK, owner1.Refund.T10_JE);
		}

		public void TestRefundWith1Owner()
		{
			RefundEnquiryDummy owner = Factory.NewWithValidTestData<RefundEnquiryDummy>();
			owner.OwnerColumn = ClientRefundSchema.T10_JE;
			owner.RefundManager.CreateClientRefund();
			AssertNotNull("owner.Refund", owner.Refund);
			AssertEquals("Refund has Owner", owner.PK, owner.Refund.T10_JE);
			Assert("Refund hasn't Related owner", owner.Refund.T10_CS.IsEmpty);
		}

		public void TestMissingControlNumer()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			UPEJobDeclaration owner = Factory.NewWithValidTestData<UPEJobDeclaration>();
			owner.RefundManager.CreateClientRefund();
			TestHelperForRefund.PopulateRefund(owner.Refund);
			Assert("Control Number exists.", !owner.RefundManager.Refund.T10_ControlNumber.IsEmpty);
			ZString noteText = ZString.Format(refundNoteFormat, "0223573", GlbStaff.CurrentUser.GS_FullName, ZDateTime.UtcNow.ToLongTimeString(), "TICKED");
			owner.Notes.AddNew(false, UPEPredefinedNoteTypes.Instance.RefundNote.Description, noteText);
			ClientRefund refund = Factory.LoadTop1<ClientRefund>(new ZQuery(ClientRefundSchema.PK, owner.Refund.PK));
			refund.T10_ControlNumber = ZString.Empty;
			Factory.Save();
			Assert("Simulate missing Control Number", refund.T10_ControlNumber.IsEmpty);
			Assert("And now it magically reappears...", !owner.RefundManager.Refund.T10_ControlNumber.IsEmpty);
		}

		readonly string refundNoteFormat = System.Environment.NewLine + "CONTROL NUMBER        : {0}" + System.Environment.NewLine + "USER NAME             : {1}" + System.Environment.NewLine + "DATE                  : {2}" + System.Environment.NewLine + "REMARKS               : {3}";
		public void TestRefundIsTheSameOwner()
		{
			RefundEnquiryDummy owner = Factory.NewWithValidTestData<RefundEnquiryDummy>();
			owner.OwnerColumn = ClientRefundSchema.T10_JE;
			owner.RelatedOwner = owner;
			owner.RefundManager.CreateClientRefund();
			AssertNotNull(owner.Refund);
			AssertEquals("Refund has Owner", owner.PK, owner.Refund.T10_JE);
			Assert("Refund hasn't Related owner", owner.Refund.T10_CS.IsEmpty);
		}

		public void TestOnClientRefundCreated()
		{
			RefundEnquiryDummy owner = Factory.NewWithValidTestData<RefundEnquiryDummy>();
			owner.OwnerColumn = ClientRefundSchema.T10_CS;
			bool refundCreated = false;
			owner.RefundManager.OnRefundCreated += delegate
			{
				refundCreated = true;
			};
			ClientRefund refund = owner.Refund;
			AssertNull("owner.Refund", owner.Refund);
			Assert("refundCreated", !refundCreated);
			owner.RefundManager.CreateClientRefund();
			AssertNotNull("owner.Refund", owner.Refund);
			Assert("refundCreated", refundCreated);
		}

		public void TestRefundExistForRelatedOwner()
		{
			RefundEnquiryDummy owner1 = Factory.NewWithValidTestData<RefundEnquiryDummy>();
			owner1.OwnerColumn = ClientRefundSchema.T10_CS;
			owner1.RefundManager.CreateClientRefund();
			Factory.Save();
			RefundEnquiryDummy owner2 = Factory.NewWithValidTestData<RefundEnquiryDummy>();
			owner2.OwnerColumn = ClientRefundSchema.T10_JE;
			owner2.RelatedOwner = owner1;
			AssertNotNull(owner2.Refund);
			AssertEquals("Should find Refund by Related Owner", owner1.PK, owner2.Refund.T10_CS);
			AssertEquals("Should find Refund by Related Owner", owner2.PK, owner2.Refund.T10_JE);
		}

		public void TestDeleteRefund()
		{
			RefundEnquiryDummy owner = Factory.NewWithValidTestData<RefundEnquiryDummy>();
			owner.OwnerColumn = ClientRefundSchema.T10_JE;
			owner.RefundManager.CreateClientRefund();
			ClientRefund refund = owner.Refund;
			Factory.Save();
			Assert("owner.IsInDatabase", owner.IsInDatabase);
			AssertNotNull("owner.Refund", owner.Refund);
			Assert("owner.Refund.IsInDatabase", owner.Refund.IsInDatabase);
			owner.Delete();
			Factory.Save();
			Assert("!owner.IsInDatabase", !owner.IsInDatabase);
			Assert("Refund Deleted because doesn't have Related Owner", !refund.IsInDatabase);
			owner = Factory.NewWithValidTestData<RefundEnquiryDummy>();
			RefundEnquiryDummy owner2 = Factory.NewWithValidTestData<RefundEnquiryDummy>();
			owner.OwnerColumn = ClientRefundSchema.T10_JE;
			owner2.OwnerColumn = ClientRefundSchema.T10_CS;
			owner.RelatedOwner = owner2;
			owner.RefundManager.CreateClientRefund();
			AssertNotNull("owner.Refund", owner.Refund);
			Factory.Save();
			refund = owner.Refund;
			Assert("owner.IsInDatabase", owner.IsInDatabase);
			Assert("refund.IsInDatabase", refund.IsInDatabase);
			owner.Delete();
			Factory.Save();
			Assert("!owner.IsInDatabase", !owner.IsInDatabase);
			Assert("Shouldn't be deleted, because has Related Owner", refund.IsInDatabase);
		}
	}
}
