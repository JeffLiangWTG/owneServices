using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.CA;
using Enterprise.DocumentEngineIntegration;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B2AdjustmentsDocumentWrapper))]
	sealed class B2AdjustmentsDocumentWrapperTest : AdjustmentsDocumentWrapperTest
	{
		#region TestSourceIdentifierProvider

		public void TestISourceIdentifierProvider()
		{
			var b2 = Factory.NewWithValidTestData<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "1";
			subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "11";
			subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "2";
			subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "NS";
			Factory.Save();
			var wrapper = CreateNewWrapper(b2);

			var supporter = wrapper as ISourceIdentifierProvider;
			AssertNotNull("B2AdjustmentsDocumentWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", b2.PK, supporter?.SourceIdentifier);
		}

		#endregion

		public override void TestDocumentPagesOrder()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "1";
			subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "11";
			subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "2";
			subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "NS";

			var wrapper = CreateNewWrapper(b2);
			AssertEquals(4, wrapper.DocumentPages.Count);
			var docPage = wrapper.DocumentPages[0];
			AssertEquals("1", docPage.SubHeaderNo);
			docPage = wrapper.DocumentPages[1];
			AssertEquals("2", docPage.SubHeaderNo);
			docPage = wrapper.DocumentPages[2];
			AssertEquals("11", docPage.SubHeaderNo);
			docPage = wrapper.DocumentPages[3];
			AssertEquals("NS", docPage.SubHeaderNo);
		}

		public override void TestSimpleLineChangeScenario()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var accountLine1 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine1.CA_OriginalLineNo = "1";

			var wrapper = CreateNewWrapper(b2);
			AssertEquals(1, wrapper.DocumentPages.Count);
			var docPage = wrapper.DocumentPages[0];
			Assert(!docPage.AsAccountForDocLine1.IsEmpty);
			Assert(!docPage.AsClaimForDocLine1.IsEmpty);
			Assert(docPage.AsAccountForDocLine2.IsEmpty);
			Assert(docPage.AsClaimForDocLine2.IsEmpty);
		}

		public override void TestDeleteLineScenario()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b2.Invoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var accountLine1 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine1.CA_OriginalLineNo = "1";
			accountLine1.Delete();

			var wrapper = CreateNewWrapper(b2);
			AssertEquals(1, wrapper.DocumentPages.Count);
			var docPage = wrapper.DocumentPages[0];
			Assert(docPage.AsAccountForDocLine1.IsEmpty);
			Assert(docPage.AsClaimForDocLine1.IsEmpty);
			Assert(docPage.AsAccountForDocLine2.IsEmpty);
			Assert(docPage.AsClaimForDocLine2.IsEmpty);
		}

		public override void TestSplitALineOnSameSubHeader()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var asClaimedInvoice = subHeader.CorrespondingAsClaimedForInvoice;
			var accountLine = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine.CA_OriginalLineNo = "1";
			var claimLine1 = asClaimedInvoice.AsClaimForFilteredInvoiceLines.AddNew(accountLine);

			var wrapper = CreateNewWrapper(b2);
			AssertEquals(1, wrapper.DocumentPages.Count);
			var docPage = wrapper.DocumentPages[0];
			Assert(!docPage.AsAccountForDocLine1.IsEmpty);
			Assert(!docPage.AsClaimForDocLine1.IsEmpty);
			Assert(docPage.AsAccountForDocLine2.IsEmpty);
			Assert(!docPage.AsClaimForDocLine2.IsEmpty);
		}

		public override void TestLineWithMultipleDuties()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b2.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var accountLine1 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine1.CA_OriginalLineNo = "1";
			//TODO
			//var sima = accountLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SIMADuty);
			//sima.C1_Override = true;
			var duty = accountLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty.C1_Rate = 1m;
			duty.C1_RateType = RateTypes.Codes.AdValorem;
			duty.C1_Amount = 1000m;
			duty = accountLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Override = true;
			duty.C1_Rate = 2m;
			duty.C1_RateType = RateTypes.Codes.Specific;
			duty.C1_Amount = 400m;
			var claimLine = accountLine1.CorrespondingAsClaimedForInvoiceLine;
			claimLine.DutiesAndTaxes.DeleteAll();
			var wrapper = CreateNewWrapper(b2);
			AssertEquals(1, wrapper.DocumentPages.Count);
			var docPage = wrapper.DocumentPages[0];
			Assert(!docPage.AsAccountForDocLine1.IsEmpty);
			Assert(!docPage.AsClaimForDocLine1.IsEmpty);
			Assert(!docPage.AsAccountForDocLine2.IsEmpty);
			Assert(docPage.AsClaimForDocLine2.IsEmpty);
		}

		public override void TestAdjustmentsDocumentWrapperMembers()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			b2.CA_B2Type = B2TypeList.Codes.Specific;
			b2.TransactionNumber.AccountSecurityCode = "10207";
			b2.TransactionNumber.SequentialNumber = "00000294";

			var wrapper = CreateNewWrapper(b2);
			AssertEquals("10207 - 000002945", wrapper.TransactionNumberFormated);
		}

		public override void TestThrowExceptionWhenMessageTypeIsInvalid()
		{
			var notB2 = Factory.New<JobDeclaration>();
			notB2.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			var exception = AssertExceptionThrown<InvalidOperationException>(() =>
			{
				var wrapper = new B2AdjustmentsDocumentWrapper(notB2);
			});
			AssertEquals("Declaration must be a Manual B2 Adjustments", exception.Message);
		}

		protected override AdjustmentsDocumentWrapper CreateNewWrapper(JobDeclaration declaration)
		{
			return new B2AdjustmentsDocumentWrapper(declaration);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			return new B2AdjustmentsDocumentWrapper(b2);
		}
	}
}
