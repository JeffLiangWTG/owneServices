using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using JobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CancelAESSendMessageWrapperTest : WrapperHelperTest<CancelAESSendMessageWrapper>
	{
		public void TestConstructor()
		{
			reasonForCancellation = null;
			AssertExceptionThrown("Constructor Throws Exception if entryHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","reasonForCancellation"), () => GetWrapper(entryHeader, Certificate));
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

		public void TestCustomsOfficeOfExport()
		{
			CombineAssertions(() =>
			{
				declaration.CustomsOffices.RemoveAndDeleteAll();
				declaration.JE_CustomsOffice = "ES009999";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CustomOfficeOfExport with CustomsOffice office code when no other customsOffice is declared", "ES009999", wrapper.CustomsOfficeOfExport);

				var customsOffice1 = declaration.CustomsOffices.AddNew();
				customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
				customsOffice1.CY_Data = "FR008889";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CustomOfficeOfExport with OfficeOfExport office code when declared alone in CustomsOffice list", "FR008889", wrapper.CustomsOfficeOfExport);

				customsOffice1.CY_Data = ZString.Empty;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CustomOfficeOfExport with CustomsOffice office code when declared Empty OfficeOfExport", "ES009999", wrapper.CustomsOfficeOfExport);
			});
		}

		public void TestNullExporter()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Exporter.ToString());
		}

		public void TestExporter()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.Addresses.AddNew();
				declaration.JE_OH_Supplier = orgHeader.PK;

				var exporter = wrapper.Exporter;

				AssertNotNull("Expected filled Exporter", exporter);
				AssertSame("Cached Exporter", wrapper.Exporter, exporter);
			});
		}

		public void TestNullDeclarant()
		{
			declaration.Declarant.OA_OH = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Declarant.ToString());
		}

		public void TestDeclarant()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.Addresses.AddNew();
				declaration.Declarant.OA_OH = orgHeader.PK;

				var declarant = wrapper.Declarant;

				AssertNotNull("Expected filled Declarant", declarant);
				AssertSame("Cached Declarant", wrapper.Declarant, declarant);
			});
		}

		public void TestNullRepresentative()
		{
			declaration.JE_OA_Representative = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Representative.ToString());
		}

		public void TestRepresentative()
		{
			CombineAssertions(() =>
			{
				var orgAddress = Factory.New<OrgAddress>();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgAddress.OA_OH = orgHeader.PK;
				declaration.JE_OA_Representative = orgAddress.PK;
				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;

				var representative = wrapper.Representative;

				AssertNotNull("Expected filled Representative", representative);
				AssertSame("Cached Representative", wrapper.Representative, representative);
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
			declaration.CustomsEntryInstructions.AddNew();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			reasonForCancellation = new ReasonForCancellation(Factory);

			wrapper = GetWrapper(entryHeader, Certificate);
		}
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		CancelAESSendMessageWrapper wrapper;
		ReasonForCancellation reasonForCancellation;

		CancelAESSendMessageWrapper GetWrapper(CusEntryHeader entryheader, ICertificateProvider certificateData) => new CancelAESSendMessageWrapper(entryheader, certificateData, reasonForCancellation);

		protected override CancelAESSendMessageWrapper GetProvider() => wrapper;
	}
}
