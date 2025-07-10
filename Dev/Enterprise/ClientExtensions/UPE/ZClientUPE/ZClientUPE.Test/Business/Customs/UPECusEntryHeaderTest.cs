using System;
using CargoWise.Common;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPECusEntryHeader))]
	sealed class UPECusEntryHeaderTest : CusEntryHeaderTest
	{
		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.New<UPECusEntryHeader>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestEmailSentOnStatusChangingToAmendment()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			var staff = CreditNotificationGroup.Staff.AddNew();
			staff.GS_EmailAddress = "sirko@sobaka.com";
			staff.GS_Code = "ZAC";
			var declaration = Factory.NewWithValidTestData<UPEJobDeclaration>();
			var callout = Factory.NewWithValidTestData<Callout>();
			callout.CS_JE_CustomsFormalEntry = declaration.PK;
			declaration.ResetRelatedCusHAWBs();
			var entryHeader = Factory.New<UPECusEntryHeader>();
			declaration.CustomsEntryHeaders.Add(entryHeader);
			entryHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			Factory.Save();
			AssertEquals("no email should be sent", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			entryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			Factory.Save();
			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();
			AssertEquals("2 emails should be sent", 2, printJobs.Count);
			printJobs.Sort("SP_DocumentName", System.ComponentModel.ListSortDirection.Descending);
			AssertEquals("printJobs[0].SP_DocumentName", "UPE Credit Note" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[0].SP_DocumentName);
			AssertEquals("printJobs[0].SP_JobTypeDisplayName", "Email", printJobs[0].SP_JobTypeDisplayName);
			AssertEquals("printJobs[0].SP_EmailAttachmentFormat", OrgConstants.AttachmentType.PDF, printJobs[0].SP_EmailAttachmentFormat);
			AssertEquals("printJobs[1].SP_DocumentName", "TaxInvoice" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobs[1].SP_DocumentName);
			AssertEquals("printJobs[1].SP_JobTypeDisplayName", "Email", printJobs[1].SP_JobTypeDisplayName);
			AssertEquals("printJobs[1].SP_EmailAttachmentFormat", OrgConstants.AttachmentType.PDF, printJobs[1].SP_EmailAttachmentFormat);
		}

		protected override void DoMerge(Customs.Business.BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			base.DoMerge(declaration);
		}

		protected override Type ExpectedChargeCollectionType => typeof(Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		GlbGroup CreditNotificationGroup
		{
			get
			{
				GlbGroup result = Factory.Load<GlbGroup>(UPEDataRegistry.Instance.CreditNotificationGroup);
				if (result == null)
				{
					result = Factory.NewWithValidTestData<GlbGroup>();
					UPEDataRegistry.Instance.CreditNotificationGroup = result.PK.ToGuid();
				}

				return result;
			}
		}
	}
}
