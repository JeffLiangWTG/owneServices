using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public abstract class AccQueryClaimBaseTest : AccQueryClaimTest
	{
		public void TestIsReassigned()
		{
			AccQueryClaimBase claim = (AccQueryClaimBase)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			claim.IsReassigned = true;
			Assert(claim.IsReassigned);
			claim.IsReassigned = false;
			Assert(!claim.IsReassigned);
		}

		public void TestEmailSentWhenReassignedAndSaving()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "DP";
			staff.GS_EmailAddress = "DP@TEST.COM";
			Factory.Save();
			var claim = (AccQueryClaimBase)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			claim.AY_GS_NKStaffAssignedTo = staff.GS_Code;
			claim.AY_QueryClaimReference = "111";
			claim.IsReassigned = true;
			AssertEquals("No email should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			Factory.Save();
			AssertEquals("email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Claim/Query 111 Reassignment", Env.OutgoingMailManager.EmailsCreated[0].Subject);
		}

		public void TestGetNewValidation()
		{
			Assert(((AccQueryClaimBase)GetNewBusinessObject()).Validation is AccQueryClaimBaseValidation);
		}

		public void TestAY_QueryClaimReference()
		{
			AccQueryClaimBase claim = (AccQueryClaimBase)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			claim.AY_QueryClaimReference = "111";
			ZString nextSelfBillingInvoiceNumber = claim.NumberFountainNo_ForTestOnly.PeekPreliminaryFormatted(Factory);
			Factory.Save();
			AssertEquals("AY_QueryClaimReference should be populated from appropriate Number Fountain.", nextSelfBillingInvoiceNumber, claim.AY_QueryClaimReference);
		}

		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();
			var reasonCollection = new SystemDefinableCodeDescriptionBoolCollection();
			reasonCollection.Add("RN1", (NoResString)"", false);
			reasonCollection.Add("RN2", (NoResString)"", true);
			AccountingConfigurationRegistry.Instance.ClaimReason.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reasonCollection);
			var statusCollection = new SystemDefinableCodeDescriptionBoolCollection();
			statusCollection.Add("ST1", (NoResString)"", true);
			statusCollection.Add("ST2", (NoResString)"", false);
			AccountingConfigurationRegistry.Instance.ClaimStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, statusCollection);
			var typeCollection = new SystemDefinableCodeDescriptionBoolCollection();
			typeCollection.Add("TP1", (NoResString)"", false);
			typeCollection.Add("TP2", (NoResString)"", true);
			AccountingConfigurationRegistry.Instance.QueryClaimType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, typeCollection);
			var claim = (AccQueryClaimBase)GetNewBusinessObject();
			AssertEquals("AY_QueryClaimReasonCode default value", "RN2", claim.AY_QueryClaimReasonCode);
			AssertEquals("AY_QueryClaimStatus default value", "ST1", claim.AY_QueryClaimStatus);
			AssertEquals("AY_QueryClaimType default value", "TP2", claim.AY_QueryClaimType);
		}

		public void TestRelatedUnapprovedCreditNoteSetter()
		{
			var claim = (AccQueryClaimBase)GetNewBusinessObject();
			claim.AY_AH = Factory.New<APInvoice>().PK;
			AssertNotNull("Precondition: claim.TransactionHeader should be set.", claim.TransactionHeader);
			var creditNote1 = Factory.New<UACreditNote>();
			var creditNote2 = Factory.New<UACreditNote>();
			claim.RelatedUnapprovedCreditNote = creditNote1;
			AssertEquals("RelatedUnapprovedCreditNote should return value we just set.", creditNote1, claim.RelatedUnapprovedCreditNote);
			var expectedTransactionGroupGuid = claim.TransactionHeader.AH_TransactionBelongsToGroup;
			AssertNotEquals("claim.TransactionHeader.AH_TransactionBelongsToGroup.", ZGuid.Empty, expectedTransactionGroupGuid);
			AssertEquals("AH_TransactionBelongsToGroup comparison for UA Credit Note and claim transaction.", expectedTransactionGroupGuid, creditNote1.AH_TransactionBelongsToGroup);
			AssertEquals("creditNote1 registration as editable child.", true, claim.IsRegisteredEditableChildObject(creditNote1));
			claim.RelatedUnapprovedCreditNote = creditNote1;
			AssertEquals("RelatedUnapprovedCreditNote should return value we just set.", creditNote1, claim.RelatedUnapprovedCreditNote);
			AssertEquals("claim.TransactionHeader.AH_TransactionBelongsToGroup.", expectedTransactionGroupGuid, claim.TransactionHeader.AH_TransactionBelongsToGroup);
			AssertEquals("AH_TransactionBelongsToGroup comparison for UA Credit Note and claim transaction.", expectedTransactionGroupGuid, creditNote1.AH_TransactionBelongsToGroup);
			AssertEquals("creditNote1 registration as editable child.", true, claim.IsRegisteredEditableChildObject(creditNote1));
			creditNote1.RelatedClaim = claim;
			claim.RelatedUnapprovedCreditNote = creditNote2;
			AssertEquals("RelatedUnapprovedCreditNote should return value we just set.", creditNote2, claim.RelatedUnapprovedCreditNote);
			expectedTransactionGroupGuid = claim.TransactionHeader.AH_TransactionBelongsToGroup;
			AssertNotEquals("claim.TransactionHeader.AH_TransactionBelongsToGroup.", ZGuid.Empty, expectedTransactionGroupGuid);
			AssertEquals("AH_TransactionBelongsToGroup comparison for UA Credit Note and claim transaction.", expectedTransactionGroupGuid, creditNote2.AH_TransactionBelongsToGroup);
			AssertEquals("creditNote2 registration as editable child.", true, claim.IsRegisteredEditableChildObject(creditNote2));
			AssertEquals("creditNote1.AH_TransactionBelongsToGroup.", ZGuid.Empty, creditNote1.AH_TransactionBelongsToGroup);
			AssertEquals("creditNote1 registration as editable child.", false, claim.IsRegisteredEditableChildObject(creditNote1));
			AssertNull("creditNote1.RelatedClaim should be reset.", creditNote1.RelatedClaim);
			creditNote2.RelatedClaim = claim;
			claim.RelatedUnapprovedCreditNote = null;
			AssertEquals("RelatedUnapprovedCreditNote should return value we just set.", null, claim.RelatedUnapprovedCreditNote);
			AssertEquals("claim.TransactionHeader.AH_TransactionBelongsToGroup.", ZGuid.Empty, claim.TransactionHeader.AH_TransactionBelongsToGroup);
			AssertEquals("creditNote2.AH_TransactionBelongsToGroup.", ZGuid.Empty, creditNote2.AH_TransactionBelongsToGroup);
			AssertEquals("creditNote2 registration as editable child.", false, claim.IsRegisteredEditableChildObject(creditNote2));
			AssertNull("creditNote2.RelatedClaim should be reset.", creditNote2.RelatedClaim);
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				fTestObjectCreator = fTestObjectCreator ?? new TestObjectCreator(Factory);
				fTestObjectCreator.Creditor1.CompanyData.OB_OG_APCreditorGroup = fTestObjectCreator.CreateCreditorGroup().PK;
				return fTestObjectCreator;
			}
		}
	}

	public abstract class AccQueryClaimBaseDocumentTest : DocumentSupporterTest
	{
	}
}
