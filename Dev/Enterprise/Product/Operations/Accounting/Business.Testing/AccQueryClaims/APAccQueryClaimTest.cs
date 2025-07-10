using System;
using System.Reflection;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	[TestedType(typeof(APAccQueryClaim))]
	class APAccQueryClaimTest : AccQueryClaimBaseTest
	{
		public void TestCheckIsRelatedCreditNoteAllowed()
		{
			var claim = (APAccQueryClaim)GetNewBusinessObject();

			foreach (var isAllowed in new[] { false, true })
			{
				AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, !isAllowed);
				foreach (var returnReason in new[] { false, true })
				{
					var result = claim.CheckIsRelatedCreditNoteAllowed(returnReason);
					AssertEquals("IsAllowed", isAllowed, result.IsAllowed);
					AssertEquals("ReasonWhyNoAllowed", !isAllowed && returnReason ? AccountingMasterFilesUtils.APCreditNoteDisallowedMessage : ZString.Empty, result.ReasonWhyNoAllowed);
				}
			}
		}

		public new void TestGetNewValidation()
		{
			Assert(((APAccQueryClaim)GetNewBusinessObject()).Validation is APAccQueryClaimValidation);
		}

		public override void TestIsHoldOptionVisible()
		{
			AssertEquals(true, ((APAccQueryClaim)GetNewBusinessObject()).IsHoldOptionVisible);
		}

		public void TestIsAllowMatch()
		{
			var claim = PrepareValidClaim();
			Factory.Save();

			AssertEquals(HoldOptionType.Codes.DNM, claim.AY_HoldOption);
			AssertEquals(false, claim.IsAllowMatch);

			claim.AY_HoldOption = HoldOptionType.Codes.ALM;
			Factory.Save();

			AssertEquals(HoldOptionType.Codes.ALM, claim.AY_HoldOption);
			AssertEquals(true, claim.IsAllowMatch);
		}

		public void TestNumberFountainNo()
		{
			APAccQueryClaim claim = Factory.New<APAccQueryClaim>();
			AssertEquals("Should be AP Claim number fountain", Env.NumberFountains.APQueryClaimNo.PeekPreliminaryFormatted(Db.Connection), claim.NumberFountainNo_ForTestOnly.PeekPreliminaryFormatted(Db.Connection));
			using (Db.Connection.BeginTransactionWithManager())
			{
				Env.NumberFountains.APQueryClaimNo.GetNextFormatted(Db.Connection);
			}

			AssertEquals("Should be AP Claim number fountain", Env.NumberFountains.APQueryClaimNo.PeekPreliminaryFormatted(Db.Connection), claim.NumberFountainNo_ForTestOnly.PeekPreliminaryFormatted(Db.Connection));
		}

		public void TestPrePopulateCreditNote()
		{
			APAccQueryClaim claim = Factory.New<APAccQueryClaim>();
			claim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			claim.AY_QueryClaimReference = "ref";
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			claim.AY_AH = invoice.PK;
			UACreditNote header = claim.CreateAndAttachRelatedCreditNote();
			AssertEquals("Related claim of credit note", claim.PK, header.RelatedClaim.PK);
			AssertEquals("Related credit note of a claim", header.PK, claim.RelatedUnapprovedCreditNote.PK);
			AssertEquals("AH_OH", TestObjectCreator.AALSHI.PK, header.AH_OH);
			AssertEquals("AH_TransactionNum", "ref", header.AH_TransactionNum);
			AssertEquals("AH_TransactionCategory", Core.Constants.TransactionCategory.Codes.ClaimRelated, header.AH_TransactionCategory);
		}

		public void TestPrePopulateCreditNoteBlankQueryClaimReference()
		{
			APAccQueryClaim claim = Factory.New<APAccQueryClaim>();
			claim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			claim.AY_QueryClaimReference = ZString.Empty;
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			claim.AY_AH = invoice.PK;
			UACreditNote header = claim.CreateAndAttachRelatedCreditNote();
			AssertEquals("AH_TransactionNum", ZString.Empty, header.AH_TransactionNum);
			AssertNoErrors("Please enter a Transaction Num..", header.AH_TransactionNumInfo);
		}

		public void TestCreateAndAttachRelatedCreditNote()
		{
			APAccQueryClaim claim = Factory.New<APAccQueryClaim>();
			claim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			claim.AY_QueryClaimReference = "ref";
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			claim.AY_AH = invoice.PK;
			UACreditNote header = claim.CreateAndAttachRelatedCreditNote();
			AssertEquals("AH_OH", TestObjectCreator.AALSHI.PK, header.AH_OH);
			AssertEquals("AH_TransactionNum", "ref", header.AH_TransactionNum);
			AssertEquals("AH_TransactionCategory", Core.Constants.TransactionCategory.Codes.ClaimRelated, header.AH_TransactionCategory);
		}

		public void TestLedger()
		{
			APAccQueryClaim claim = Factory.New<APAccQueryClaim>();
			AssertEquals("Details filled in", ZArchitecture.Core.LedgerTypes.AccountsPayable, claim.Ledger);
		}

		public void TestReadOnlinessOfPropertiesRelatedToCreditNote()
		{
			APAccQueryClaim claim = Factory.NewWithValidTestData<APAccQueryClaim>();
			AssertEquals("Invoice Number shouldn't be readonly as there's no related credit note", false, claim.AY_AHInfo.ReadOnly);
			AssertEquals("Debtor shouldn't be readonly as there's no related credit note", false, claim.AY_OH_DebtorInfo.ReadOnly);
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			claim.AY_AH = invoice.PK;
			claim.CreateAndAttachRelatedCreditNote();
			Factory.Save();
			AssertEquals("Invoice Number should be readonly as there's a related credit note now", true, claim.AY_AHInfo.ReadOnly);
			AssertEquals("Debtor should be readonly as there's a related credit note now", true, claim.AY_OH_DebtorInfo.ReadOnly);
		}

		public void TestHoldOptionEventWhenSaveClaim()
		{
			var claim1 = PrepareValidClaim();
			claim1.Debtor.CompanyData.APCreditorGroup.OG_IsAllowALM = true;

			Factory.Save();

			AssertEquals(HoldOptionType.Codes.DNM, claim1.AY_HoldOption);
			AssertEquals(HoldOptionType.Codes.DNM, claim1.Debtor.CompanyData.APCreditorGroup.OG_DefaultHoldOption);
			AssertEquals("Has default by group EDT event for DNM.", true, claim1.Logs.HasLogWith(StmALogSchema.SL_Reference, "Invoice Hold Option set to DNM based on creditor group."));

			var claim2 = PrepareValidClaim(TestObjectCreator.Creditor2);
			claim2.AY_HoldOption = HoldOptionType.Codes.ALM;

			Factory.Save();

			AssertEquals(HoldOptionType.Codes.ALM, claim2.AY_HoldOption);
			AssertEquals("Has default EDT event for ALM.", true, claim2.Logs.HasLogWith(StmALogSchema.SL_Reference, "Invoice Hold Option set to ALM."));

			claim2.AY_HoldOption = HoldOptionType.Codes.DNM;

			Factory.Save();

			AssertEquals("Has changed EDT event for ALM.", true, claim2.Logs.HasLogWith(StmALogSchema.SL_Reference, "Invoice Hold Option changed from ALM to DNM."));
		}

		public void TestDefaultHoldOptionSafe()
		{
			var claim = PrepareValidClaim();
			var propertyInfo = typeof(APAccQueryClaim).GetProperty("DefaultHoldOptionSafe", BindingFlags.NonPublic | BindingFlags.Instance);

			claim.Debtor.CompanyData.APCreditorGroup.OG_IsAllowALM = true;
			claim.Debtor.CompanyData.APCreditorGroup.OG_DefaultHoldOption = HoldOptionType.Codes.ALM;

			AssertEquals(HoldOptionType.Codes.ALM, propertyInfo.GetValue(claim));

			claim.AY_OH_Debtor = ZGuid.Empty;

			AssertNoExceptionThrown("Should not throw NullReferenceException even if relative property is null.", () => propertyInfo.GetValue(claim));
			AssertEquals(HoldOptionType.Codes.DNM, propertyInfo.GetValue(claim));
		}

		#region Implementation

		APAccQueryClaim PrepareValidClaim()
		{
			return PrepareValidClaim(TestObjectCreator.Creditor1);
		}

		APAccQueryClaim PrepareValidClaim(OrgHeader org)
		{
			var claim = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim.AY_OH_Debtor = org.PK;

			var contact = claim.Debtor.Contacts.AddNew();
			contact.OC_ContactName = "TST";
			claim.AY_OC = contact.PK;

			return claim;
		}

		#endregion
	}

	[TestedType(typeof(APAccQueryClaim))]
	public class APAccQueryClaimDocumentTest : AccQueryClaimBaseDocumentTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<APAccQueryClaim>();
		}
	}
}
