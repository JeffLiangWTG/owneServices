using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.ZArchitecture.GUI.ZForm;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(ComplianceDocumentForm))]
	public class ARComplianceDocumentFormTest : ComplianceDocumentFormTestCase
	{
		protected override ComplianceDocumentForm GetFormByComplianceDocument(AccComplianceDocumentHeader complianceDocumentHeader)
		{
			return new TestComplianceDocumentForm(complianceDocumentHeader)
			{
				ControllerID = ControllerIDs.ARComplianceDocument
			};
		}

		protected override ComplianceDocumentForm GetFormByVoidComplianceDocument(AccComplianceDocumentHeader complianceDocumentHeader)
		{
			return new ComplianceDocumentForm(complianceDocumentHeader)
			{
				ControllerID = ControllerIDs.ARVoidComplianceDocument,
				VoidInsteadOfDelete = true
			};
		}

		protected override AccComplianceDocumentHeader GetComplianceDocumentWithValidTestData(bool fillTestData = true)
		{
			var header = Factory.New<ARComplianceDocumentHeader>();
			if (fillTestData)
			{
				header.FillWithValidTestData();
			}

			return header;
		}

		[TestDate(2018, 7, 5)]
		public override void TestShowPreDeleteDialogsForVoid()
		{
			base.TestShowPreDeleteDialogsForVoid();

			var testObjectCreator = new TestObjectCreator(Factory);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var complianceDocument = GetComplianceDocumentWithValidTestData();
				complianceDocument.ADH_DocumentDate = ZDateTime.Today;
				complianceDocument.ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
				complianceDocument.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
				complianceDocument.ADH_DocumentNumber = "001";
				complianceDocument.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
				complianceDocument.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
				complianceDocument.ADH_Description = "Description";
				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, complianceDocument.PK));
				pivot.AIP_Status = EInvoicingPivotState.BatchedWithError;
				Factory.Save();

				using (var form = GetFormByVoidComplianceDocument(complianceDocument))
				{
					Assert(!complianceDocument.IsFinalised);
					Assert(!complianceDocument.IsVoided);
					AssertNotEquals(EInvoicingPivotState.Succeed, complianceDocument.EInvoicingStatus);
					AssertEquals(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, complianceDocument.ADH_ComplianceSubType);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddOKAnswer();
					var result = form.ShowPreDeleteDialogs_ForTestOnly();

					AssertEquals(ContinueWithDelete.No, result);
					AssertEquals("Compliance Document with sub type TXE and TCE cannot be voided until the original document has been successfully uploaded.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (var form = GetFormByVoidComplianceDocument(complianceDocument))
				{
					complianceDocument.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE;

					Assert(!complianceDocument.IsFinalised);
					Assert(!complianceDocument.IsVoided);
					AssertNotEquals(EInvoicingPivotState.Succeed, complianceDocument.EInvoicingStatus);
					AssertEquals(TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE, complianceDocument.ADH_ComplianceSubType);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddOKAnswer();
					var result = form.ShowPreDeleteDialogs_ForTestOnly();

					AssertEquals(ContinueWithDelete.No, result);
					AssertEquals("Compliance Document with sub type TXE and TCE cannot be voided until the original document has been successfully uploaded.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (var form = GetFormByVoidComplianceDocument(complianceDocument))
				{
					pivot.AIP_Status = EInvoicingPivotState.Succeed;

					Assert(!complianceDocument.IsFinalised);
					Assert(!complianceDocument.IsVoided);
					AssertEquals(EInvoicingPivotState.Succeed, complianceDocument.EInvoicingStatus);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddOKAnswer();
					var result = form.ShowPreDeleteDialogs_ForTestOnly();

					AssertEquals(ContinueWithDelete.Yes, result);
					AssertEquals("The compliance document will be voided. Are you sure to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (var form = GetFormByVoidComplianceDocument(complianceDocument))
				{
					complianceDocument.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
					pivot.AIP_Status = EInvoicingPivotState.Batched;

					Assert(!complianceDocument.IsFinalised);
					Assert(!complianceDocument.IsVoided);
					AssertNotEquals(EInvoicingPivotState.Succeed, complianceDocument.EInvoicingStatus);
					AssertEquals(TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC, complianceDocument.ADH_ComplianceSubType);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddOKAnswer();
					var result = form.ShowPreDeleteDialogs_ForTestOnly();

					AssertEquals(ContinueWithDelete.Yes, result);
					AssertEquals("The compliance document will be voided. Are you sure to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}
	}
}
