using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B2AdjustmentsDocFooter))]
	sealed class B2AdjustmentsDocFooterTest : AdjustmentsDocFooterTest
	{
		public override void TestTotalDuty()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;

			var subHeahder = b2.B2AsAccountedForInvoices.AddNew();
			subHeahder.JZ_InvoiceNumber = "INV1";
			subHeahder.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var accountLine = subHeahder.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine.CA_OriginalLineNo = "1";
			accountLine.CA_CVforCurrConv = 1000m;
			var accountLine2 = subHeahder.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine2.CA_OriginalLineNo = "2";
			accountLine2.CA_CVforCurrConv = 2000m;

			var gst1 = accountLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst1.C1_Override = true;
			gst1.C1_Amount = 10m;
			var gst2 = accountLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst2.C1_Override = true;
			gst2.C1_Amount = 20m;

			var claimLine = accountLine.CorrespondingAsClaimedForInvoiceLine;
			claimLine.DutiesAndTaxes.DeleteAll();
			claimLine.CA_CVforCurrConv = 1000m;
			var claimLine2 = accountLine2.CorrespondingAsClaimedForInvoiceLine;
			claimLine2.DutiesAndTaxes.DeleteAll();
			claimLine2.CA_CVforCurrConv = 2000m;

			var gst3 = claimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst3.C1_Override = true;
			gst3.C1_Amount = 15m;
			var gst4 = claimLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst4.C1_Override = true;
			gst4.C1_Amount = 10m;

			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			var footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(15-10) + (10-20)", "-5.00", footer.TotalGST);
			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(15-10=5>0 result:5) + (10-20=-10<0 result:0)", "5.00", footer.TotalGST);

			var claimInvoice = subHeahder.CorrespondingAsClaimedForInvoice;
			var claimSLLine = claimInvoice.AsClaimForFilteredInvoiceLines.AddNew();
			claimSLLine.JI_ParentID = accountLine.PK;
			claimSLLine.JI_ParentTableCode = accountLine.TablePrefix;
			AssertEquals("1/SL", claimSLLine.CA_OriginalLineNo);

			var claimSLLine2 = claimInvoice.AsClaimForFilteredInvoiceLines.AddNew();
			claimSLLine2.JI_ParentID = accountLine2.PK;
			claimSLLine2.JI_ParentTableCode = accountLine2.TablePrefix;
			AssertEquals("2/SL", claimSLLine2.CA_OriginalLineNo);

			var gst5 = claimSLLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst5.C1_Override = true;
			gst5.C1_Amount = 22m;

			var gst6 = claimSLLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst6.C1_Override = true;
			gst6.C1_Amount = 6m;

			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(15+22-10) + (10+6-20)", "23.00", footer.TotalGST);
			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(15+22-10=27>0 result:27) + (10+6-20=-4<0 result:0)", "27.00", footer.TotalGST);

			gst6.C1_Amount = 11m;
			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(15+22-10) + (10+11-20)", "28.00", footer.TotalGST);
			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(15+22-10=27>0 result:27) + (10+11-20=1>0 result:1)", "28.00", footer.TotalGST);
		}

		public override void TestTotalDutyForNoAsAccount()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;

			var subHeahder = b2.B2AsAccountedForInvoices.AddNew();
			subHeahder.JZ_InvoiceNumber = "INV1";
			subHeahder.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var accountLine = subHeahder.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine.CA_OriginalLineNo = "1";
			accountLine.CA_CVforCurrConv = 1000m;

			var gst1 = accountLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst1.C1_Override = true;
			gst1.C1_Amount = 10m;

			var claimLine = accountLine.CorrespondingAsClaimedForInvoiceLine;
			claimLine.DutiesAndTaxes.DeleteAll();
			claimLine.CA_CVforCurrConv = 1000m;

			var gst2 = claimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst2.C1_Override = true;
			gst2.C1_Amount = 8m;

			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			var footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(8-10)", "-2.00", footer.TotalGST);
			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(8-10=-2<0 result:0)", ZString.Empty, footer.TotalGST);

			var claimInvoice = subHeahder.CorrespondingAsClaimedForInvoice;
			var claimLine2 = claimInvoice.AsClaimForFilteredInvoiceLines.AddNew();
			claimLine2.CA_OriginalLineNo = "2";
			AssertEquals("2", claimLine2.CA_OriginalLineNo);

			var gst3 = claimLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst3.C1_Override = true;
			gst3.C1_Amount = 5m;

			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(8-10) + 5", "3.00", footer.TotalGST);
			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(8-10=-2<0 result:0) + 5", "5.00", footer.TotalGST);
		}

		public override void TestAmountDue()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;

			var subHeahder = b2.B2AsAccountedForInvoices.AddNew();
			subHeahder.JZ_InvoiceNumber = "INV1";
			subHeahder.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var accountLine = subHeahder.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine.CA_OriginalLineNo = "1";
			accountLine.CA_CVforCurrConv = 1000m;

			// Duty is refundable, GST as future credit
			var sima = accountLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			sima.C1_Override = true;

			var duty1 = accountLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty1.C1_Override = true;
			duty1.C1_Amount = 12m;

			var gst1 = accountLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst1.C1_Override = true;
			gst1.C1_Amount = 10m;

			var claimLine = accountLine.CorrespondingAsClaimedForInvoiceLine;
			claimLine.DutiesAndTaxes.DeleteAll();
			claimLine.CA_CVforCurrConv = 1000m;
			sima = claimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CVD);
			sima.C1_Override = true;

			var duty2 = claimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty2.C1_Override = true;
			duty2.C1_Amount = 25m;

			var gst2 = claimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst2.C1_Override = true;
			gst2.C1_Amount = 15.5m;

			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			var footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(15.5-10)", "5.50", footer.TotalGST);
			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(15.5-10>0 result:5.50)", "5.50", footer.TotalGST);

			AssertEquals("18.50", footer.AmountDueReceiverGeneralCanada);
			Assert(footer.AmountDueClaimant.IsEmpty);

			duty1.C1_Amount = 20.50m;
			gst1.C1_Amount = 19.25m;
			duty2.C1_Amount = 10m;
			gst2.C1_Amount = 19.25m;

			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			footer = new B2AdjustmentsDocFooter(b2);
			Assert("(19.25-19.25) = 0", footer.TotalGST.IsEmpty);
			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			footer = new B2AdjustmentsDocFooter(b2);
			Assert("(19.25-19.25) = 0", footer.TotalGST.IsEmpty);

			Assert(footer.AmountDueReceiverGeneralCanada.IsEmpty);
			AssertEquals("10.50", footer.AmountDueClaimant);

			// Duty is owing, and GST is owing
			duty1.C1_Amount = 20.50m;
			gst1.C1_Amount = 19.25m;
			duty2.C1_Amount = 10m;
			gst2.C1_Amount = 6.5m;

			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(6.5-19.25) = -12.75", "-12.75", footer.TotalGST);
			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(6.5-19.25<0 result:0)", ZString.Empty, footer.TotalGST);

			Assert(footer.AmountDueReceiverGeneralCanada.IsEmpty);
			AssertEquals("10.50", footer.AmountDueClaimant);

			// Offset payable GST against refundable GST but not vice versa.
			duty1.C1_Amount = 10m;
			duty2.C1_Amount = 13m;
			gst1.C1_Amount = 35m;
			gst2.C1_Amount = 20m;

			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(20-35)", "-15.00", footer.TotalGST);
			CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("(20-35<0 result:0)", ZString.Empty, footer.TotalGST);

			AssertEquals("3.00", footer.AmountDueReceiverGeneralCanada);
			Assert(footer.AmountDueClaimant.IsEmpty);
			AssertEquals("3.00", footer.SubTotal);
		}

		public override void TestAdjustmentsDocumentFooterMembers()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;

			b2.CA_IsDocAttached = true;
			b2.CA_JustificationForRequest = "JUSTIFICATION FOR REQUEST";
			b2.CA_Under = "UNDER";
			b2.CA_B2Explanation = "EXPLANATION";
			b2.CA_ClaimedInterestAmount = 11m;
			b2.CA_AnySightDepositAmount = 12m;

			var company = Factory.New<GlbCompany>();
			company.GC_Name = "MY COMPANY";
			var branch = company.Branches.AddNew();
			branch.GB_BranchName = "MY BRANCH";
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XXX";
			staff.GS_WorkPhone = "MY PHONE";
			staff.GS_FullName = "MY NAME";
			staff.GS_GB_HomeBranch = branch.PK;
			b2.JE_GS_NKCusAgent = staff.GS_Code;

			var subHeader1 = b2.Invoices.AddNew();
			subHeader1.JZ_InvoiceNumber = "INV1";
			subHeader1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var accountLine1 = subHeader1.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine1.CA_OriginalLineNo = "1";
			accountLine1.CA_CVforCurrConv = 1000m;
			var sima = accountLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			sima.C1_ExemptCode = SIMACodes.Codes.C51;
			sima.C1_Override = true;
			sima.C1_Amount = 490m;
			var duty = accountLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty.C1_Amount = 390m;
			var excise = accountLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			excise.C1_Override = true;
			excise.C1_Amount = 40m;

			var claimLine1 = accountLine1.CorrespondingAsClaimedForInvoiceLine;
			claimLine1.DutiesAndTaxes.DeleteAll();
			claimLine1.CA_CVforCurrConv = 1000m;
			sima = claimLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CVD);
			sima.C1_ExemptCode = SIMACodes.Codes.C51;
			sima.C1_Override = true;
			sima.C1_Amount = 500m;
			duty = claimLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty.C1_Amount = 400m;
			excise = claimLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			excise.C1_Override = true;
			excise.C1_Amount = 50m;

			var subHeader2 = b2.B2AsAccountedForInvoices.AddNew();
			subHeader2.JZ_InvoiceNumber = "INV2";
			subHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var accountLine2 = subHeader2.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine2.CA_OriginalLineNo = "1";
			accountLine2.CA_CVforCurrConv = 1000m;
			sima = accountLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SUR);
			sima.C1_ExemptCode = SIMACodes.Codes.C51;
			sima.C1_Override = true;
			sima.C1_Amount = 20m;
			duty = accountLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty.C1_Amount = 100m;
			excise = accountLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			excise.C1_Override = true;
			excise.C1_Amount = 35m;

			var claimLine2 = accountLine2.CorrespondingAsClaimedForInvoiceLine;
			claimLine2.DutiesAndTaxes.DeleteAll();
			claimLine2.CA_CVforCurrConv = 1000m;
			sima = claimLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			sima.C1_ExemptCode = SIMACodes.Codes.C51;
			sima.C1_Override = true;
			sima.C1_Amount = 200m;
			duty = claimLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty.C1_Amount = 700m;
			excise = claimLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			excise.C1_Override = true;
			excise.C1_Amount = 100m;

			CACustomsDataRegistry.Instance.DeclarantOnEntryDocsBrokerOnB2.SetValue(Guid.Empty, b2.RegistryBranchPK, Guid.Empty, Guid.Empty);
			CACustomsDataRegistry.Instance.ShouldPrintBrokerSignatureOnEntryDocs.SetValue(Guid.Empty, b2.RegistryBranchPK, Guid.Empty, false);
			var footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("X", footer.DocAttached);
			AssertEquals("JUSTIFICATION FOR REQUEST", footer.JustificationForRequest);
			AssertEquals("UNDER", footer.Under);
			AssertEquals("EXPLANATION", footer.Explanation);
			AssertEquals("MY NAME", footer.BrokerName);
			AssertEquals("MY PHONE", footer.BrokerPhone);
			AssertEquals(@"MY COMPANY
MY BRANCH", footer.BrokerAgent);
			AssertEquals("622.00", footer.TotalCustomDuties);
			AssertEquals("190.00", footer.TotalSIMAAssessment);
			AssertEquals("75.00", footer.TotalExciseTax);
			AssertEquals("11.00", footer.Interest);
			AssertNull("Broker Signature Image", footer.BrokerSignatureImage);

			var declarant = Factory.New<GlbStaff>();
			declarant.GS_Code = "OOO";
			declarant.GS_WorkPhone = "Declarant PHONE";
			declarant.GS_FullName = "Declarant NAME";
			declarant.SignatureImage = new Bitmap(1, 2);
			var declarantCompany = Factory.New<GlbCompany>();
			declarantCompany.GC_Name = "Declarant COMPANY";
			var declarantBranch = declarantCompany.Branches.AddNew();
			declarantBranch.GB_BranchName = "Declarant BRANCH";
			declarant.GS_GB_HomeBranch = declarantBranch.PK;
			b2.JE_GS_NKCusAgent = declarant.GS_Code;
			CACustomsDataRegistry.Instance.DeclarantOnEntryDocsBrokerOnB2.SetValue(Guid.Empty, b2.RegistryBranchPK, Guid.Empty, declarant.PK.ToGuid());
			CACustomsDataRegistry.Instance.ShouldPrintBrokerSignatureOnEntryDocs.SetValue(Guid.Empty, b2.RegistryBranchPK, Guid.Empty, true);
			footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("Declarant NAME", footer.BrokerName);
			AssertEquals("Declarant PHONE", footer.BrokerPhone);
			AssertEquals(@"Declarant COMPANY
Declarant BRANCH", footer.BrokerAgent);
			AssertNotNull("Broker Signature Image", footer.BrokerSignatureImage);
		}

		public override void TestExplanation_ShouldShowInFull()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Explanation = "0123456789abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			var footer = new B2AdjustmentsDocFooter(b2);
			AssertEquals("Explanation should show in full", "0123456789abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", footer.Explanation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			return new B2AdjustmentsDocFooter(b2);
		}
	}
}
