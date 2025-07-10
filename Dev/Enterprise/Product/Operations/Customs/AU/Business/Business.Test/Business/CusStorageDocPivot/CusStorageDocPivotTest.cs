using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusStorageDocPivot))]
	sealed class CusStorageDocPivotTest : Customs.Business.Testing.BaseCusStorageDocPivotTest
	{
		public void TestConflictResolution_Invoice()
		{
			Factory.RefreshEnabled = false;
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;

			var declaration = Factory.New<JobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV 1";
			declaration.DocManagerInfo.Save();
			Factory.Save();

			var pivot = CreatePivot(invoice, "TY1", eDoc.UniqueKey);
			var declarationInAnotherFactory = anotherFactory.Load<JobDeclaration>(declaration.PK);
			var invoiceInAnotherFactory = declarationInAnotherFactory.Invoices[0];
			var pivotInAnotherFactory = CreatePivot(invoiceInAnotherFactory, "TY1", declarationInAnotherFactory.DocManagerInfo.AllEDocs.GetFromUniqueKey(eDoc.UniqueKey.ToGuid()).UniqueKey);

			Factory.Save();

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = false;

			CombineAssertions(() =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				try
				{
					anotherFactory.Save();
					Fail("First save should not have succeeded.");
				}
				catch (Exception ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}

				AssertEquals("User should have been notified", expected: false, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Message to notify user", "The type 'TY1' eDoc (Invoice.pdf) has already been linked to invoice INV 1 by another user. Your changes have been merged, please review your changes and save again.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Pivot 2 should be deleted", expected: true, pivotInAnotherFactory.IsDeleted);

				anotherFactory.Save();

				AssertEquals("Should be using the existing Pivot now", pivot.PK, invoiceInAnotherFactory.EDocPivotCollection.Single().PK);
			});
		}

		public void TestConflictResolution_InvoiceLine()
		{
			Factory.RefreshEnabled = false;
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;

			var declaration = Factory.New<JobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV 1";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.DocManagerInfo.Save();
			Factory.Save();

			var pivot = CreatePivot(invoiceLine, "TY1", eDoc.UniqueKey);
			var declarationInAnotherFactory = anotherFactory.Load<JobDeclaration>(declaration.PK);
			var invoiceLineInAnotherFactory = declarationInAnotherFactory.Invoices[0].InvoiceLines[0];
			var pivotInAnotherFactory = CreatePivot(invoiceLineInAnotherFactory, "TY1", declarationInAnotherFactory.DocManagerInfo.AllEDocs.GetFromUniqueKey(eDoc.UniqueKey.ToGuid()).UniqueKey);

			Factory.Save();

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = false;

			CombineAssertions(() =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				try
				{
					anotherFactory.Save();
					Fail("First save should not have succeeded.");
				}
				catch (Exception ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}

				AssertEquals("User should have been notified", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Message to notify user", "The type 'TY1' eDoc (Invoice.pdf) has already been linked to invoice INV 1 line 1 by another user. Your changes have been merged, please review your changes and save again.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Pivot 2 should be deleted", true, pivotInAnotherFactory.IsDeleted);

				anotherFactory.Save();

				AssertEquals("Should be using the existing Pivot now", pivot.PK, ((JobComInvoiceLine)invoiceLineInAnotherFactory).EDocPivotCollection.Single().PK);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var pivot = invoice.EDocPivotCollection.AddNew();
			pivot.CSD_DocType = "TY1";
			return pivot;
		}

		[TestDate(2019, 3, 6)]
		public void TestDocTypeDescription()
		{
			var date1 = new ZDateTime(2019, 3, 1);
			var date2 = new ZDateTime(2019, 3, 31);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			const string natyp = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSAttachmentType;
			helper.CreateNewOrGetExistingCusCodeType(natyp, "NATYP Desc.");
			helper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY1", "1", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY2", "2", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY3", "1", date1, date2);
			Factory.Save();

			var pivot = Factory.New<CusStorageDocPivot>();
			pivot.CSD_DocType = "TY1";
			AssertEquals("1", pivot.DocTypeDescription);
		}

		public void TestMessageStatusDescription()
		{
			var pivot = Factory.New<CusStorageDocPivot>();
			var quarantineColsHeader = Factory.New<QuarantineColsHeader>();
			pivot.Parent = quarantineColsHeader;

			pivot.CSD_MessageStatus = ZString.Empty;
			AssertEquals("Message Status Description should be empty when CSD_MessageStatus is empty", ZString.Empty, pivot.MessageStatusDescription);

			pivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse;
			AssertEquals("Message Status Description should match for CSD_MessageStatus = 'ADS'", COLSDocumentStatusList.Descriptions.AwaitingDocumentSentResponse, pivot.MessageStatusDescription.ToString());

			pivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingLastdocSentResponse;
			AssertEquals("Message Status Description should match for CSD_MessageStatus = 'ALS'", COLSDocumentStatusList.Descriptions.AwaitingLastdocSentResponse, pivot.MessageStatusDescription.ToString());
		}

		public override void TestParent() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var pivot1 = Factory.New<CusStorageDocPivot>();
			pivot1.CSD_ParentID = invoice.PK;
			pivot1.CSD_ParentTableCode = invoice.TablePrefix;
			AssertSame("Parent is JobComInvoiceHeader", invoice, pivot1.Parent);

			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var pivot2 = Factory.New<CusStorageDocPivot>();
			pivot2.CSD_ParentID = invoiceLine.PK;
			pivot2.CSD_ParentTableCode = invoiceLine.TablePrefix;
			AssertSame("Parent is JobComInvoiceLine", invoiceLine, pivot2.Parent);

			var quarantineColsHeader = Factory.New<QuarantineColsHeader>();
			var pivot3 = Factory.New<CusStorageDocPivot>();
			pivot3.CSD_ParentID = quarantineColsHeader.PK;
			pivot3.CSD_ParentTableCode = quarantineColsHeader.TablePrefix;
			AssertSame("Parent is QuarantineColsHeader", quarantineColsHeader, pivot3.Parent);
		});

		public void TestDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var invoice = declaration.Invoices.AddNew();
			var pivot = invoice.EDocPivotCollection.AddNew();
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;
			AssertEquals(eDoc, pivot.Document);
		}

		public void TestFileName()
		{
			var declaration = Factory.New<JobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var invoice = declaration.Invoices.AddNew();
			var pivot = invoice.EDocPivotCollection.AddNew();
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;
			AssertEquals("Invoice.pdf", pivot.FileName);
		}

		public void TestMimeType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var invoice = declaration.Invoices.AddNew();
			var pivot = invoice.EDocPivotCollection.AddNew();
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;
			AssertEquals("application/pdf", pivot.MimeType);
		}

		public void TestReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var invoice = declaration.Invoices.AddNew();
			var invoicePivot = invoice.EDocPivotCollection.AddNew();
			AssertEquals(false, invoicePivot.ReadOnly);

			invoicePivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse;
			AssertEquals("Invoice Pivot is not readonly", false, invoicePivot.ReadOnly);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var colsPivot = colsHeader.EDocPivotCollection.AddNew();
			AssertEquals(false, colsPivot.ReadOnly);

			colsPivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse;
			AssertEquals("COLS pivot is readonly when sent", true, colsPivot.ReadOnly);

			colsPivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingLastdocSentResponse;
			AssertEquals("COLS pivot is readonly when sent", true, colsPivot.ReadOnly);

			colsPivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.FailedDocumentSentResponse;
			AssertEquals("COLS pivot is editable when failed", false, colsPivot.ReadOnly);

			colsPivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.FailedLastdocSentResponse;
			AssertEquals("COLS pivot is editable when failed", false, colsPivot.ReadOnly);

			colsPivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.SuccessfulDocumentSent;
			AssertEquals("COLS pivot is editable when succeeded", false, colsPivot.ReadOnly);

			colsPivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.SucessfulLastdocSent;
			AssertEquals("COLS pivot is editable when succeeded", false, colsPivot.ReadOnly);

			colsPivot.CSD_MessageStatus = EDIMessage.Status.Discarded;
			AssertEquals("COLS pivot is editable when discarded", false, colsPivot.ReadOnly);
		}

		public void TestCanDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var colsPivot = colsHeader.EDocPivotCollection.AddNew();
			AssertEquals(true, colsPivot.CanDelete);

			foreach (var code in new COLSDocumentStatusList().GetAllCodes())
			{
				colsPivot.CSD_MessageStatus = code;
				AssertEquals("CSD_MessageStatus = " + code, false, colsPivot.CanDelete);
			}

			colsPivot.CSD_MessageStatus = EDIMessage.Status.Discarded;
			AssertEquals(false, colsPivot.CanDelete);
			AssertEquals("An attachment that has been sent cannot be deleted.", colsPivot.ReasonForNotAbleToDelete);
		}

		public void TestValidationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoicePivot = invoice.EDocPivotCollection.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var colsPivot = colsHeader.EDocPivotCollection.AddNew();

			AssertEquals(typeof(CusStorageDocPivotValidation), invoicePivot.Validation.GetType());
			AssertEquals(typeof(ColsCusStorageDocPivotValidation), colsPivot.Validation.GetType());
		}

		static CusStorageDocPivot CreatePivot(BusinessObject parent, string docType, ZGuid reference)
		{
			var result = ((ICusStorageDocPivotParent)parent).EDocPivotCollection.AddNew();
			result.CSD_DocType = docType;
			result.CSD_StorageDocReference = reference;
			return result;
		}
	}
}
