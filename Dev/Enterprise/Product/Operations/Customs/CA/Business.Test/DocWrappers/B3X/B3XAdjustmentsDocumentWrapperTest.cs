using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.CA;
using Enterprise.DocumentEngineIntegration;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B3XAdjustmentsDocumentWrapper))]
	sealed class B3XAdjustmentsDocumentWrapperTest : AdjustmentsDocumentWrapperTest
	{
		#region TestSourceIdentifierProvider

		public void TestISourceIdentifierProvider()
		{
			var b3x = Factory.NewWithValidTestData<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			b3x.CA_B2Type = B2TypeList.Codes.Specific;
			b3x.TransactionNumber.AccountSecurityCode = "10207";
			b3x.TransactionNumber.SequentialNumber = "00000294";

			Factory.Save();
			var wrapper = CreateNewWrapper(b3x);

			var supporter = wrapper as ISourceIdentifierProvider;
			AssertNotNull("B3XAdjustmentsDocumentWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", b3x.PK, supporter?.SourceIdentifier);
		}

		#endregion

		public override void TestAdjustmentsDocumentWrapperMembers()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			b3x.CA_B2Type = B2TypeList.Codes.Specific;
			b3x.TransactionNumber.AccountSecurityCode = "10207";
			b3x.TransactionNumber.SequentialNumber = "00000294";

			var wrapper = CreateNewWrapper(b3x);
			AssertEquals("10207 - 000002945", wrapper.TransactionNumberFormated);
		}

		public override void TestDeleteLineScenario()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			b3x.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b3x.Invoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var accountLine1 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine1.CA_OriginalLineNo = "1";
			accountLine1.Delete();

			var wrapper = CreateNewWrapper(b3x);
			AssertEquals(1, wrapper.DocumentPages.Count);
			var docPage = wrapper.DocumentPages[0];
			Assert(docPage.AsAccountForDocLine1.IsEmpty);
			Assert(docPage.AsClaimForDocLine1.IsEmpty);
			Assert(docPage.AsAccountForDocLine2.IsEmpty);
			Assert(docPage.AsClaimForDocLine2.IsEmpty);
		}

		public override void TestDocumentPagesOrder()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			b3x.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b3x.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "1";
			subHeader = b3x.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "11";
			subHeader = b3x.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "2";
			subHeader = b3x.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "NS";

			var wrapper = CreateNewWrapper(b3x);
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

		public override void TestLineWithMultipleDuties()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			b3x.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b3x.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var accountLine1 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine1.CA_OriginalLineNo = "1";
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
			var wrapper = CreateNewWrapper(b3x);
			AssertEquals(1, wrapper.DocumentPages.Count);
			var docPage = wrapper.DocumentPages[0];
			Assert(!docPage.AsAccountForDocLine1.IsEmpty);
			Assert(!docPage.AsClaimForDocLine1.IsEmpty);
			Assert(!docPage.AsAccountForDocLine2.IsEmpty);
			Assert(docPage.AsClaimForDocLine2.IsEmpty);
		}

		public override void TestSimpleLineChangeScenario()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			b3x.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b3x.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var accountLine1 = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine1.CA_OriginalLineNo = "1";

			var wrapper = CreateNewWrapper(b3x);
			AssertEquals(1, wrapper.DocumentPages.Count);
			var docPage = wrapper.DocumentPages[0];
			Assert(!docPage.AsAccountForDocLine1.IsEmpty);
			Assert(!docPage.AsClaimForDocLine1.IsEmpty);
			Assert(docPage.AsAccountForDocLine2.IsEmpty);
			Assert(docPage.AsClaimForDocLine2.IsEmpty);
		}

		public override void TestSplitALineOnSameSubHeader()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			b3x.CA_B2Type = B2TypeList.Codes.Specific;
			var subHeader = b3x.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var asClaimedInvoice = subHeader.CorrespondingAsClaimedForInvoice;
			var accountLine = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine.CA_OriginalLineNo = "1";
			var claimLine1 = asClaimedInvoice.AsClaimForFilteredInvoiceLines.AddNew(accountLine);

			var wrapper = CreateNewWrapper(b3x);
			AssertEquals(1, wrapper.DocumentPages.Count);
			var docPage = wrapper.DocumentPages[0];
			Assert(!docPage.AsAccountForDocLine1.IsEmpty);
			Assert(!docPage.AsClaimForDocLine1.IsEmpty);
			Assert(docPage.AsAccountForDocLine2.IsEmpty);
			Assert(!docPage.AsClaimForDocLine2.IsEmpty);
		}

		public override void TestThrowExceptionWhenMessageTypeIsInvalid()
		{
			var notB3X = Factory.New<JobDeclaration>();
			notB3X.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var exception = AssertExceptionThrown<InvalidOperationException>(() =>
			{
				var wrapper = new B3XAdjustmentsDocumentWrapper(notB3X);
			});
			AssertEquals("Declaration must be an X Type Entry", exception.Message);
		}

		protected override AdjustmentsDocumentWrapper CreateNewWrapper(JobDeclaration declaration)
		{
			return new B3XAdjustmentsDocumentWrapper(declaration);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var b3x = Factory.New<JobDeclaration>();
			b3x.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			return new B3XAdjustmentsDocumentWrapper(b3x);
		}
	}
}
