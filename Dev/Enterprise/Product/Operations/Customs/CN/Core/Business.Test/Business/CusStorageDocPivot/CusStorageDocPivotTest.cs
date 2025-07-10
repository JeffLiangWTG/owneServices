using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusStorageDocPivot))]
	class CusStorageDocPivotTest : Customs.Business.Testing.BaseCusStorageDocPivotTest
	{
		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, ((CusStorageDocPivot)BusinessObject).SupportsNotes);
		}

		public void TestConflictResolution_Entry()
		{
			Factory.RefreshEnabled = false;
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;
			var declaration = Factory.New<JobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			declaration.DocManagerInfo.Save();
			Factory.Save();
			CreatePivot(instruction, "TY1", eDoc.UniqueKey);
			var declarationInAnotherFactory = anotherFactory.Load<JobDeclaration>(declaration.PK);
			var instructionInAnotherFactory = declarationInAnotherFactory.CustomsEntryInstructions[0];
			var pivotInAnotherFactory = CreatePivot(instructionInAnotherFactory, "TY1", declarationInAnotherFactory.DocManagerInfo.AllEDocs.GetFromUniqueKey(eDoc.UniqueKey.ToGuid()).UniqueKey);
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
				AssertEquals("Message to notify user", $"The type 'TY1' eDoc (Invoice.pdf) has already been linked to Entry Instruction with CPC 11 by another user. Your changes have been merged, please review your changes and save again.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Pivot 2 should be deleted", true, pivotInAnotherFactory.IsDeleted);
				anotherFactory.Save();
				AssertEquals("Attachment reloaded", 1, instructionInAnotherFactory.Attachments.Count);
			}

			);
		}

		public void TestInvoiceLineLinks()
		{
			var anotherFactory = new BusinessObjectFactory();
			var storageDoc = GetNewBusinessObjectForDeleteTest(anotherFactory) as CusStorageDocPivot;
			var invoiceLine = storageDoc.EntryInstruction.JobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();

			var pivot = anotherFactory.New<GenPivot>();
			pivot.XX_RelationType = GenPivotTypeDecider.Types.AttachmentInvoiceLineLink;
			pivot.Relation1Object = storageDoc;
			pivot.Relation2Object = invoiceLine;
			anotherFactory.Save();

			storageDoc = Factory.Load<CusStorageDocPivot>(storageDoc.PK);
			AssertEquals(1, storageDoc.InvoiceLineLinks.Count);
			AssertEquals(storageDoc.PK, storageDoc.InvoiceLineLinks[0].XX_Relation1ID);
			AssertEquals(invoiceLine.PK, storageDoc.InvoiceLineLinks[0].XX_Relation2ID);
		}

		public void TestCanLinkToInvoiceLine()
		{
			var storagePivot = Factory.New<CusStorageDocPivot>();
			AssertEquals("CSD_DocType is empty", false, storagePivot.CanLinkToInvoiceLine);

			var canLinkToInvoiceLineTypes = new[] {
				CSDDocTypeList.Codes._80000001,
				CSDDocTypeList.Codes._80000002,
				CSDDocTypeList.Codes._80000003,
				CSDDocTypeList.Codes._80000004
			};

			foreach (var code in new CSDDocTypeList().GetAllCodes())
			{
				storagePivot.CSD_DocType = code;
				AssertEquals($"CSD_DocType={code}", canLinkToInvoiceLineTypes.Contains(code), storagePivot.CanLinkToInvoiceLine);
			}
		}

		public void TestReloadAttachmentLinksOnInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = entryInstruction.JobDeclaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._31);
			AssertEquals("No Attachments", 0, invoiceLine.AttachmentLinks.Count);

			var attachmentLinksReloaded = false;
			invoiceLine.AttachmentLinks.CountChanged += (o, e) => attachmentLinksReloaded = true;

			void AssertAttachmentLinksReloaded(bool reloaded)
			{
				AssertEquals($"AttachmentLinks should {(reloaded ? "" : "NOT ")}be reloaded", reloaded, attachmentLinksReloaded);
				attachmentLinksReloaded = false;
			}

			var attachment1 = entryInstruction.Attachments.AddNew();
			attachment1.AttachmentType = CSDDocTypeList.Codes._00000001;
			var storagePivot1 = attachment1.CusStorageDocPivot;
			AssertEquals("Attachment 00000001 cannot link to Invoice Line", 0, invoiceLine.AttachmentLinks.Count);
			AssertAttachmentLinksReloaded(false);

			storagePivot1.CSD_DocType = CSDDocTypeList.Codes._80000001;
			AssertEquals("Attachment 80000001 can link to Invoice Line", 1, invoiceLine.AttachmentLinks.Count);
			AssertAttachmentLinksReloaded(true);
			AssertEquals(storagePivot1, invoiceLine.AttachmentLinks[0].CusStorageDocPivot);
			AssertEquals(false, invoiceLine.AttachmentLinks[0].IsLinked);
			invoiceLine.AttachmentLinks[0].IsLinked = true;
			AssertEquals("Link to 80000001 added", 1, storagePivot1.InvoiceLineLinks.Count);
			var invoiceLineLink1 = storagePivot1.InvoiceLineLinks[0];

			var attachment2 = entryInstruction.Attachments.AddNew();
			attachment2.AttachmentType = CSDDocTypeList.Codes._00000001;
			var storagePivot2 = attachment2.CusStorageDocPivot;
			AssertEquals("Attachment 00000001 cannot link to Invoice Line", 1, invoiceLine.AttachmentLinks.Count);
			AssertAttachmentLinksReloaded(false);
			AssertEquals(true, invoiceLine.AttachmentLinks[0].IsLinked);

			storagePivot2.CSD_DocType = CSDDocTypeList.Codes._80000002;
			AssertEquals("Attachment 80000002 can link to Invoice Line", 2, invoiceLine.AttachmentLinks.Count);
			AssertAttachmentLinksReloaded(true);
			AssertEquals(storagePivot1, invoiceLine.AttachmentLinks[0].CusStorageDocPivot);
			AssertEquals(true, invoiceLine.AttachmentLinks[0].IsLinked);
			AssertEquals(storagePivot2, invoiceLine.AttachmentLinks[1].CusStorageDocPivot);
			AssertEquals(false, invoiceLine.AttachmentLinks[1].IsLinked);

			invoiceLine.AttachmentLinks[1].IsLinked = true;
			AssertEquals("Link to 80000002 added", 1, storagePivot2.InvoiceLineLinks.Count);
			var invoiceLineLink2 = storagePivot2.InvoiceLineLinks[0];

			storagePivot1.CSD_DocType = CSDDocTypeList.Codes._10000001;
			AssertEquals("Attachment 10000001 cannot link to Invoice Line", 1, invoiceLine.AttachmentLinks.Count);
			AssertAttachmentLinksReloaded(true);
			AssertEquals(storagePivot2, invoiceLine.AttachmentLinks[0].CusStorageDocPivot);
			AssertEquals(true, invoiceLine.AttachmentLinks[0].IsLinked);
			AssertEquals("Link to 10000001 deleted", true, invoiceLineLink1.IsDeleted);

			attachment2.AttachmentType = CSDDocTypeList.Codes._50000001;
			AssertEquals("Attachment 50000001 cannot link to Invoice Line", 0, invoiceLine.AttachmentLinks.Count);
			AssertAttachmentLinksReloaded(true);
			AssertEquals("Link to 50000001 deleted", true, invoiceLineLink2.IsDeleted);
		}

		public void TestDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = entryInstruction.JobDeclaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._31);

			var storageDoc1 = entryInstruction.CusStorageDocPivots.AddNew();
			storageDoc1.CSD_DocType = CSDDocTypeList.Codes._80000001;
			var storageDoc2 = entryInstruction.CusStorageDocPivots.AddNew();
			storageDoc2.CSD_DocType = CSDDocTypeList.Codes._80000002;
			var pivot1 = storageDoc1.InvoiceLineLinks.AddNew();
			pivot1.Relation2Object = invoiceLine;
			var pivot2 = storageDoc2.InvoiceLineLinks.AddNew();
			pivot2.Relation2Object = invoiceLine;
			AssertEquals("Invoice Line links to 2 Attachments", 2, invoiceLine.AttachmentLinks.Count);
			var link1 = invoiceLine.AttachmentLinks.Cast<AttachmentInvoiceLineLink>().Single(x => x.CusStorageDocPivot == storageDoc1);
			var link2 = invoiceLine.AttachmentLinks.Cast<AttachmentInvoiceLineLink>().Single(x => x.CusStorageDocPivot == storageDoc2);

			storageDoc1.Delete();
			AssertEquals("One AttachmentLink deleted", true, link1.IsDeleted);
			AssertEquals("One AttachmentLink removed", 1, invoiceLine.AttachmentLinks.Count);
		}

		public override void TestParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var pivot = new CusStorageDocPivotCollection(instruction).AddNew();
			AssertEquals(instruction, pivot.EntryInstruction);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var pivot = new CusStorageDocPivotCollection(instruction).AddNew();
			pivot.CSD_DocType = CSDDocTypeList.Codes._00000001;
			return pivot;
		}

		static CusStorageDocPivot CreatePivot(CusEntryInstruction parent, string docType, ZGuid reference)
		{
			var result = parent.Attachments.CreateNewCusStorageDocPivot();
			result.CSD_DocType = docType;
			result.CSD_StorageDocReference = reference;
			return result;
		}
	}
}
