using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CommonAnnexSendMessageWrapperTest : WrapperHelperTest<CommonAnnexSendMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if docPivot is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","docPivot"), () => GetWrapper(entryHeader, Certificate, null, "Y"));

				AssertExceptionThrown("Constructor Throws Exception if requestDispatch is empty", typeof(ArgumentException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be empty string (\"\").","requestDispatch"), () => GetWrapper(entryHeader, Certificate, pivot, ZString.Empty));
			});
		}

		public void TestOperation()
		{
			CombineAssertions(() =>
			{
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				invoiceLine.JI_CEI = entryInstruction.PK;

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Expected filled Operation with empty when declaration is not T2l or T2C", ZString.Empty, wrapper.Operation);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Expected filled Operation with 04 when declaration is T2L", "04", wrapper.Operation);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Expected filled Operation with 05 when declaration is T2C", "05", wrapper.Operation);
			});
		}

		public void TestReference()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
				entryHeader.MovementReferenceNumber = "TestReferenceMRN";
				AssertEquals("Expected filled Reference", "TestReferenceMRN", wrapper.Reference);

				declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
				var newEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Spain.T2CMovementReferenceNumber, Core.Constants.CountryCodes.Spain);
				newEntryNumber.CE_EntryNum = "TestT2CMovementReferenceNumber";
				newEntryNumber.CE_IssueDate = ZDateTime.Today;
				newEntryNumber.CE_EntryIsSystemGenerated = true;
				AssertEquals("Expected filled Reference", "TestT2CMovementReferenceNumber", wrapper.Reference);
			});
		}

		public void TestRequestDispatchTagName()
		{
			AssertEquals("Expected filled RequestDispatchTagName is SolicitudDespacho", "SolicitudDespacho", wrapper.RequestDispatchTagName);
		}

		public void TestDispatchRequest()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled DispatchRequest with S when argument requestDispatch is Y", "S", wrapper.DispatchRequest);

				wrapper = GetWrapper(entryHeader, Certificate, pivot, "N");
				AssertEquals("Expected filled DispatchRequest with N when argument requestDispatch is N", "N", wrapper.DispatchRequest);
			});
		}

		public void TestDocument()
		{
			CombineAssertions(() =>
			{
				var document = wrapper.Document;

				AssertNotNull("Expected not null Document", document);
				AssertSame("Cached Document", wrapper.Document, document);
			});
		}

		public void TestAdministrationCode()
		{
			Assert("Expected null AdministrationCode", string.IsNullOrEmpty(wrapper.AdministrationCode));
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			pivot = entryHeader.EDocPivotCollection.AddNew();
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;

			wrapper = GetWrapper(entryHeader, Certificate, pivot, "Y");
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusStorageDocPivot pivot;
		CommonAnnexSendMessageWrapper wrapper;

		CommonAnnexSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData, CusStorageDocPivot docPivot, ZString requestDispatch) => new CommonAnnexSendMessageWrapper(cusEntryHeader, certificateData, docPivot, requestDispatch);
		protected override CommonAnnexSendMessageWrapper GetProvider() => wrapper;
	}
}
