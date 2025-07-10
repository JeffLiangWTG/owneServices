using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AnnexAESSendMessageWrapperTest : WrapperHelperTest<AnnexAESSendMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if docPivots is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","docPivots"), () => new AnnexAESSendMessageWrapper(entryHeader, Certificate, null, "Y"));

				AssertExceptionThrown("Constructor Throws Exception if docPivots is empty", typeof(ArgumentOutOfRangeException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value '0' cannot be less than or equal to 0.","docPivots"), () => new AnnexAESSendMessageWrapper(entryHeader, Certificate, new List<CusStorageDocPivot>(), "Y"));

				AssertExceptionThrown("Constructor Throws Exception if requestDispatch is empty", typeof(ArgumentException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be empty string (\"\").","requestDispatch"), () => new AnnexAESSendMessageWrapper(entryHeader, Certificate, docPivotList, ZString.Empty));
			});
		}

		public void TestExportOperation()
		{
			var exportOperation = wrapper.ExportOperation;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled ExportOperation", exportOperation);
				AssertSame("Cached ExportOperation", wrapper.ExportOperation, exportOperation);
			});
		}

		public void TestDispatchRequestCode()
		{
			AssertEquals("Expected filled DispatchRequestCode", "S", wrapper.DispatchRequestCode);
		}

		public void TestDocuments()
		{
			var documents = wrapper.Documents;
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled Documents", 1, documents.Count);
				AssertSame("Cached Documents", wrapper.Documents, documents);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var pivot = entryHeader.EDocPivotCollection.AddNew();
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;

			docPivotList = new List<CusStorageDocPivot>() { pivot };

			wrapper = new AnnexAESSendMessageWrapper(entryHeader, Certificate, docPivotList, "Y");
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		List<CusStorageDocPivot> docPivotList;
		AnnexAESSendMessageWrapper wrapper;

		protected override AnnexAESSendMessageWrapper GetProvider() => wrapper;
	}
}
