using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	class ComplianceDocumentBatchExporterTest : TestCaseWithFactory
	{
		[TestDate(2019, 05, 22)]
		public void TestCreateComplianceDocumentBatch()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				TestObjectCreator.CreateTestPeriodsForEntireYear(2019);

				SetupComplianceDocumentDebtor();

				TestObjectCreator.Debtor.OH_RL_NKClosestPort = "TWTPE";
				var address1 = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor, OrgAddressType.Receivables, true);
				address1.OA_CompanyNameOverride = "测试公司1";

				TestObjectCreator.Debtor1.OH_RL_NKClosestPort = "AUSYD";
				var address2 = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor1, OrgAddressType.Receivables, true);
				address2.OA_CompanyNameOverride = "测试公司2";
				Factory.Save();

				var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.TWD, 1M, TestObjectCreator.Debtor);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.TWD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
				line1.AL_AT = TestObjectCreator.GST1.PK;

				var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV002", TestObjectCreator.TWD, 1M, TestObjectCreator.Debtor1);
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.TWD, 1M, 200M, 20M, 0M, TestObjectCreator.CC1.PK);
				line2.AL_AT = TestObjectCreator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice1 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				new ComplianceDocumentCreator(new[] { invoice2 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				Factory.Save();

				var arComplianceDocument1 = Factory.LoadTop1<ARComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Debtor.PK));
				AssertNotNull($"compliance document with org {TestObjectCreator.Debtor.OH_Code} is generated", arComplianceDocument1);
				var arComplianceDocument2 = Factory.LoadTop1<ARComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.Debtor1.PK));
				AssertNotNull($"compliance document with org {TestObjectCreator.Debtor1.OH_Code} is generated", arComplianceDocument2);

				arComplianceDocument1.ADH_DocumentNumber = "AA00000001";
				arComplianceDocument2.ADH_DocumentNumber = "AA00000002";

				arComplianceDocument1.ADH_DocumentDate = new ZDate(2019, 5, 21);
				arComplianceDocument2.ADH_DocumentDate = new ZDate(2019, 5, 21);

				arComplianceDocument1.ADH_OA_AddressOverride = address1.PK;
				arComplianceDocument2.ADH_OA_AddressOverride = address2.PK;

				arComplianceDocument1.ADH_VoidingReason = "VoidingReason1";
				arComplianceDocument2.ADH_ApprovalNumber = "ApprovalNumber1";

				arComplianceDocument1.ADH_VoidingReason = "VoidingReason2";
				arComplianceDocument2.ADH_ApprovalNumber = "ApprovalNumber2";

				Factory.Save();

				arComplianceDocument2.Void();
				Factory.Save();

				var invoicingBatch = Factory.New<AccEInvoicingBatch>();
				invoicingBatch.AIB_BatchNumber = 1;
				invoicingBatch.AIB_GC = GlbCompany.CurrentCompany.PK;
				invoicingBatch.AIB_Status = Core.Constants.EInvoicingBatchState.Ready;
				invoicingBatch.AIB_SystemCreateTimeUtc = ZDateTime.UtcNow.ToDateTime();
				invoicingBatch.AIB_SystemCreateUser = GlbStaff.CurrentUser.GS_Code.ToString();

				LinkTransactionPivot(Factory, arComplianceDocument1.PK, invoicingBatch.PK, GlbCompany.CurrentCompany, Core.Constants.EInvoicingPivotState.Batched);
				LinkTransactionPivot(Factory, arComplianceDocument2.PK, invoicingBatch.PK, GlbCompany.CurrentCompany, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				var exporter = new ComplianceDocumentBatchExporter();
				var complianceDocumentBatch = exporter.CreateComplianceDocumentBatch(invoicingBatch);
				AssertEquals("BatchNumber", invoicingBatch.AIB_BatchNumber, complianceDocumentBatch.BatchNumber);
				AssertEquals("CompanyPK", invoicingBatch.AIB_GC, complianceDocumentBatch.CompanyPK);
				AssertEquals("ComplianceDocumentHeaderDetails Count", 2, complianceDocumentBatch.ComplianceDocumentHeaderDetails.Length);

				var complianceDocumentHeaderDetail1 = complianceDocumentBatch.ComplianceDocumentHeaderDetails.FirstOrDefault(x => x.HeaderPK == arComplianceDocument1.PK);
				AssertNotNull(complianceDocumentHeaderDetail1);
				AssertComplianceDocumentHeaderDetail(complianceDocumentHeaderDetail1, arComplianceDocument1, 100M, 10M);
				AssertOrgHeaderDetail(complianceDocumentHeaderDetail1.OrgHeaderDetail, "BUS", "测试公司1", "22222222", "xxyu/123", countryCode: "TW");

				var complianceDocumentHeaderDetail2 = complianceDocumentBatch.ComplianceDocumentHeaderDetails.FirstOrDefault(x => x.HeaderPK == arComplianceDocument2.PK);
				AssertNotNull(complianceDocumentHeaderDetail2);
				AssertComplianceDocumentHeaderDetail(complianceDocumentHeaderDetail2, arComplianceDocument2, 200M, 20M);
				AssertOrgHeaderDetail(complianceDocumentHeaderDetail2.OrgHeaderDetail, "NAT", "测试公司2", "33333333", pigRegistrationNumber: "45678", countryCode: "AU");
			}
		}

		void SetupComplianceDocumentDebtor()
		{
			TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.Business;
			TestObjectCreator.Debtor1.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

			var cusCode1 = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = "VAT";
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusCode1.OK_CustomsRegNo = "12345675";
			GlbCompany.CurrentCompany.Factory.Save();

			var cusCode2 = TestObjectCreator.Debtor.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = "VAT";
			cusCode2.OK_RN_NKCodeCountry = CountryCodes.Taiwan;
			cusCode2.OK_CustomsRegNo = "22222222";

			var cusCode3 = TestObjectCreator.Debtor1.CustomsCodes.AddNew();
			cusCode3.OK_CodeType = "VAT";
			cusCode3.OK_RN_NKCodeCountry = CountryCodes.Taiwan;
			cusCode3.OK_CustomsRegNo = "33333333";

			var cusCode4 = TestObjectCreator.Debtor.CustomsCodes.AddNew();
			cusCode4.OK_CodeType = "MCI";
			cusCode4.OK_RN_NKCodeCountry = CountryCodes.Taiwan;
			cusCode4.OK_CustomsRegNo = "xxyu/123";

			var cusCode5 = TestObjectCreator.Debtor1.CustomsCodes.AddNew();
			cusCode5.OK_CodeType = "PIG";
			cusCode5.OK_RN_NKCodeCountry = CountryCodes.Taiwan;
			cusCode5.OK_CustomsRegNo = "45678";
		}

		void AssertComplianceDocumentHeaderDetail(ComplianceDocumentHeaderDetail complianceDocumentHeaderDetail, AccComplianceDocumentHeader accComplianceDocumentHeader, ZDecimal amount, ZDecimal taxAmount)
		{
			AssertEquals("HeaderPK", accComplianceDocumentHeader.PK, complianceDocumentHeaderDetail.HeaderPK);
			AssertEquals("TransactionType", accComplianceDocumentHeader.ADH_TransactionType, complianceDocumentHeaderDetail.TransactionType);
			AssertEquals("DocumentNumber", accComplianceDocumentHeader.ADH_DocumentNumber, complianceDocumentHeaderDetail.DocumentNumber);
			AssertEquals("DocumentDate", new ZDate(2019, 5, 21), complianceDocumentHeaderDetail.DocumentDate);
			AssertEquals("OriginalDocumentDate", accComplianceDocumentHeader.INVComplianceDocumentHeaderForCRD?.ADH_DocumentDate, complianceDocumentHeaderDetail.OriginalDocumentDate);
			if (accComplianceDocumentHeader.IsVoided)
			{
				AssertEquals("VoidDate", new ZDate(2019, 5, 22), complianceDocumentHeaderDetail.VoidDate.Value);
			}
			else
			{
				AssertNull("VoidDate", complianceDocumentHeaderDetail.VoidDate);
			}
			AssertEquals("SystemVATRegistrationNum", "12345675", complianceDocumentHeaderDetail.SystemVATRegistrationNum);
			AssertEquals("BarCode", accComplianceDocumentHeader.ADH_BarCode, complianceDocumentHeaderDetail.BarCode);
			AssertEquals("Description", accComplianceDocumentHeader.ADH_Description, complianceDocumentHeaderDetail.Description);
			AssertEquals("InternalReference", accComplianceDocumentHeader.ADH_InternalReference, complianceDocumentHeaderDetail.InternalReference);
			AssertEquals("Amount", amount, complianceDocumentHeaderDetail.Amount);
			AssertEquals("TaxAmount", taxAmount, complianceDocumentHeaderDetail.TaxAmount);
			AssertEquals("VoidingReason", accComplianceDocumentHeader.ADH_VoidingReason, complianceDocumentHeaderDetail.VoidingReason);
			AssertEquals("ApprovalNumber", accComplianceDocumentHeader.ADH_ApprovalNumber, complianceDocumentHeaderDetail.ApprovalNumber);

			AssertEquals("ComplianceDocumentLineDetails Length", 1, complianceDocumentHeaderDetail.ComplianceDocumentLineDetails.Length);
			AssertEquals("ComplianceDocumentLines Length", 1, accComplianceDocumentHeader.OriginalComplianceDocumentLines.Count);
			AssertComplianceDocumentLineDetail(complianceDocumentHeaderDetail.ComplianceDocumentLineDetails[0], accComplianceDocumentHeader.OriginalComplianceDocumentLines[0], amount, taxAmount);
		}

		void AssertComplianceDocumentLineDetail(ComplianceDocumentLineDetail complianceDocumentLineDetail, AccComplianceDocumentLine accComplianceDocumentLine, ZDecimal amount, ZDecimal taxAmount)
		{
			AssertEquals("LinePK", accComplianceDocumentLine.PK, complianceDocumentLineDetail.LinePK);
			AssertEquals("LineDescription", accComplianceDocumentLine.ADL_Description, complianceDocumentLineDetail.LineDescription);
			AssertEquals("TaxCode", TestObjectCreator.GST1.AT_Code, complianceDocumentLineDetail.TaxCode);
			AssertEquals("Rate", 10M, complianceDocumentLineDetail.Rate);
			AssertEquals("Amount", amount, complianceDocumentLineDetail.Amount);
			AssertEquals("TaxAmount", taxAmount, complianceDocumentLineDetail.TaxAmount);
		}

		void AssertOrgHeaderDetail(OrgHeaderDetail orgHeaderDetail, ZString category, ZString companyName, ZString vatRegistrationNumber, string mciRegistrationNumber = "", string pigRegistrationNumber = "", string countryCode = "")
		{
			AssertEquals("Category", category, orgHeaderDetail.Category);
			AssertEquals("CompanyName", companyName, orgHeaderDetail.CompanyName);
			AssertEquals("VATRegistrationNum", vatRegistrationNumber, orgHeaderDetail.VATRegistrationNum);
			AssertEquals("MCIRegistrationNum", mciRegistrationNumber, orgHeaderDetail.MCIRegistrationNum);
			AssertEquals("PIGRegistrationNum", pigRegistrationNumber, orgHeaderDetail.PIGRegistrationNum);
			AssertEquals("CountryCode", countryCode, orgHeaderDetail.CountryCode);
		}

		void LinkTransactionPivot(BusinessObjectFactory factory, ZGuid transactionPK, ZGuid invoicingBatchPK, GlbCompany company, ZString status, string errorDescription = null)
		{
			var pivot = factory.New<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentID = transactionPK;
			pivot.AIP_ParentTableCode = AccComplianceDocumentHeaderSchema.Constants.Prefix;
			pivot.AIP_AIB = invoicingBatchPK;
			pivot.AIP_Status = status;
			pivot.AIP_ErrorDescription = errorDescription;
			pivot.AIP_LastSentTimeUtc = ZDateTime.UtcNow.AddHours(-2).ToDateTime();
			pivot.AIP_LastResponseReceivedUtc = ZDateTime.UtcNow.ToDateTime();
			pivot.SetCompanyAndCountryCode(company);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator TestObjectCreator;
	}
}
