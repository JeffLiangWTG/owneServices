using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Taiwan.Testing
{
	public class EInvoicingDataValidatorForTaiwanTest : BaseEInvoicingDataValidatorTest
	{
		public override BaseEInvoicingDataValidator GetEInvoicingDataValidatorForTest()
		{
			return new EInvoicingDataValidatorForTaiwan(GlbCompany.CurrentCompany);
		}

		public void TestClearBatchOfPivotWithVaildationError()
		{
			AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var pivot = CreateINVComplianceDocument();
			var batchID = pivot.AIP_AIB;
			var complianceDocument = Factory.LoadTop1<AccComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_DocumentNumber, "AA00000001"));
			complianceDocument.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().First().ADL_Description = "<desc";
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals("Compliance Document Line's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			AssertEquals(batchID, pivot.AIP_AIB);

			AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			complianceDocument.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().First().ADL_Description = ">desc";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals("Compliance Document Line's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			AssertEquals(ZGuid.Empty, pivot.AIP_AIB);
		}

		public void TestValidateComplianceDocumentDescription()
		{
			var pivot = CreateINVComplianceDocument();

			var complianceDocument = Factory.LoadTop1<AccComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_DocumentNumber, "AA00000001"));
			complianceDocument.ADH_Description = "<desc";
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Compliance Document's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ADH_Description = ">desc";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Compliance Document's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ADH_Description = "desc&";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Compliance Document's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ADH_Description = "'desc";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Compliance Document's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ADH_Description = ":desc";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Compliance Document's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ADH_Description = "\"desc";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Compliance Document's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ADH_Description = "|desc";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Compliance Document's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ADH_Description = "desc";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Validation should succeed", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.Batched, pivot.AIP_Status);
		}

		public void TestValidComplianceDocumentLineDescription()
		{
			var pivot = CreateINVComplianceDocument();

			var complianceDocument = Factory.LoadTop1<AccComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_DocumentNumber, "AA00000001"));
			complianceDocument.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().First().ADL_Description = "<desc";
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Compliance Document Line's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().First().ADL_Description = ">desc";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Compliance Document Line's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().First().ADL_Description = "desc&";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Compliance Document Line's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().First().ADL_Description = "'desc";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Compliance Document Line's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().First().ADL_Description = ":desc";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Compliance Document Line's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().First().ADL_Description = "\"desc";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Compliance Document Line's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().First().ADL_Description = "|desc";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Compliance Document Line's description cannot contains any invalid characters.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().First().ADL_Description = "desc";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Validation should succeed", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.Batched, pivot.AIP_Status);
		}

		public void TestValidateAmount()
		{
			var pivot = CreateCRDComplianceDocument();

			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals(@"Compliance Document Line's amount cannot less than 0.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
		}

		public void TestValidateComplianceDocumentVATRegistrationNum()
		{
			var pivot = CreateINVComplianceDocument();
			var complianceDocument = Factory.LoadTop1<AccComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_DocumentNumber, "AA00000001"));
			complianceDocument.ADH_VATRegistrationNumberOverride = "1234567";
			Factory.Save();
			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals(@"The Taiwan VAT number must be an 8-digit number.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ADH_VATRegistrationNumberOverride = "123456ab";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();
			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals(@"The Taiwan VAT number must be an 8-digit number.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ADH_VATRegistrationNumberOverride = "12345678";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();
			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals(@"The Taiwan VAT number is invalid.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			complianceDocument.ADH_VATRegistrationNumberOverride = "87654321";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();
			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals(@"The Taiwan VAT number is invalid.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
		}

		public void TestValidateComplianceDocumentVATRegistrationNum_NotTWBUSDebotr()
		{
			var pivot = CreateINVComplianceDocument();
			var complianceDocument = Factory.LoadTop1<AccComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_DocumentNumber, "AA00000001"));
			complianceDocument.ADH_VATRegistrationNumberOverride = ZString.Empty;

			var org = complianceDocument.Organisation;
			org.OH_Category = OrgConstants.Category.Business;

			Factory.Save();
			AssertEquals("Category of organization of compliance document is BUS.", OrgConstants.Category.Business, org.OH_Category);
			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals(@"The Taiwan VAT number must be an 8-digit number.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);

			org.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();
			AssertEquals("Category of organization of compliance document is NAT.", OrgConstants.Category.NaturalPersonIndividual, org.OH_Category);
			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Validation should succeed", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Status is not changed after data validation", Core.Constants.EInvoicingPivotState.Batched, pivot.AIP_Status);

			org.OH_Category = OrgConstants.Category.Business;
			org.OH_RL_NKClosestPort = "AUSYD";
			pivot.AIP_ErrorDescription = string.Empty;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			Factory.Save();
			AssertEquals("Category of organization of compliance document is BUS.", OrgConstants.Category.Business, org.OH_Category);
			AssertNotEquals("Country of organization of compliance document is not TW.", CountryCodes.Taiwan, org.CountryCode);
			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Validation should succeed", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Status is not changed after data validation", Core.Constants.EInvoicingPivotState.Batched, pivot.AIP_Status);
		}

		public void TestValidate_Success()
		{
			var pivot = CreateINVComplianceDocument();

			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Validation should succeed", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Status is not changed after data validation", Core.Constants.EInvoicingPivotState.Batched, pivot.AIP_Status);
		}

		public void TestValidateComplianceDocumentCompanyVATRegistrationNum()
		{
			var pivot = CreateINVComplianceDocument();

			var complianceDocument = Factory.LoadTop1<AccComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_DocumentNumber, "AA00000001"));
			var cusCode = complianceDocument.Company.OrgProxy?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.VATCode && x.OK_RN_NKCodeCountry == GlbCompany.CurrentCompany.GC_RN_NKCountryCode && x.OK_CustomsRegNo == "32323329");
			cusCode.OK_CustomsRegNo = "6234567";

			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals(@"Please check System Company VAT number. The Taiwan VAT number must be an 8-digit number.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
		}

		ARInvoice Invoice;
		ARCreditNote CreditNote;
		ARInvoiceLine ARInvoiceLine;
		ARCreditNoteLine CreditNoteLine;

		AccEInvoicingTransactionPivot CreateINVComplianceDocument()
		{
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "TWTPE";
			Invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.TWD, 1m, TestObjectCreator.Debtor);
			ARInvoiceLine = TestObjectCreator.CreateARInvoiceLine(Invoice, null, TestObjectCreator.FRT, TestObjectCreator.TWD, 1m, "desc", 100m);
			ARInvoiceLine.AL_AT = TestObjectCreator.GST1.PK;
			Factory.Save();

			TestObjectCreator.AALSHI.OH_RL_NKClosestPort = "TWTPE";
			TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "Desc", "AA00000001", "TXE", "Desc", ARInvoiceLine, TestObjectCreator.AALSHI);
			Factory.Save();

			var complianceDocument = Factory.LoadTop1<AccComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_DocumentNumber, "AA00000001"));
			complianceDocument.ADH_VATRegistrationNumberOverride = "96944490";

			SetCompanyOrgProxyVATRegistrationNum(complianceDocument);

			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument, Core.Constants.EInvoicingPivotState.Batched);

			return pivot;
		}

		AccEInvoicingTransactionPivot CreateCRDComplianceDocument()
		{
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "TWTPE";
			CreditNote = TestObjectCreator.CreateARCreditNote("CRD001", TestObjectCreator.Debtor, TestObjectCreator.TWD, 1m, "");
			CreditNoteLine = TestObjectCreator.CreateARCreditNoteLine(CreditNote, null, TestObjectCreator.FRT, -100m);
			CreditNoteLine.AL_AT = TestObjectCreator.GST1.PK;
			Factory.Save();

			TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "Desc", "AA00000001", "TXE", "Desc", CreditNoteLine);
			Factory.Save();

			var complianceDocument = Factory.LoadTop1<AccComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_DocumentNumber, "AA00000001"));
			complianceDocument.ADH_VATRegistrationNumberOverride = "96944490";

			SetCompanyOrgProxyVATRegistrationNum(complianceDocument);

			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument, Core.Constants.EInvoicingPivotState.Batched);

			return pivot;
		}

		void SetCompanyOrgProxyVATRegistrationNum(AccComplianceDocumentHeader complianceDocument)
		{
			var cusCode = complianceDocument.Company.OrgProxy?.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = complianceDocument.Company.GC_RN_NKCountryCode;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = "32323329";

			Factory.Save();
		}
	}
}
