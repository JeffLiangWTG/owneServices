using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class GoodsNotificationAESSendMessageWrapperTest : WrapperHelperTest<GoodsNotificationAESSendMessageWrapper>
	{
		public void TestExportOperation()
		{
			var exportOperation = wrapper.ExportOperation;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled ExportOperation", exportOperation);
				AssertSame("Cached ExportOperation", wrapper.ExportOperation, exportOperation);
			});
		}

		public void TestCustomOfficeOfPresentation()
		{
			CombineAssertions(() =>
			{
				declaration.CustomsOffices.RemoveAndDeleteAll();
				declaration.JE_CustomsOffice = "ES009999";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty CustomOfficeOfPresentation when no OfficeOfPresentation is declared", ZString.Empty, wrapper.CustomOfficeOfPresentation);

				var customsOffice1 = declaration.CustomsOffices.AddNew();
				customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation;
				customsOffice1.CY_Data = "FR008889";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CustomOfficeOfPresentation with OfficeOfExport office code when declared in CustomsOffice list", "FR008889", wrapper.CustomOfficeOfPresentation);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty CustomOfficeOfPresentation when OfficeOfPresentation is declared but entryInstruction is C", ZString.Empty, wrapper.CustomOfficeOfPresentation);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CustomOfficeOfPresentation when OfficeOfPresentation is declared and entryInstruction is not C", "FR008889", wrapper.CustomOfficeOfPresentation);

				declaration.JE_CustomsOffice = "ES005500";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty CustomOfficeOfPresentation when OfficeOfPresentation is declared but CustomsOffice starts with ES0055 or ES0056", ZString.Empty, wrapper.CustomOfficeOfPresentation);

				declaration.JE_CustomsOffice = "ES009900";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CustomOfficeOfPresentation when OfficeOfPresentation is declared and CustomsOffice does not start with ES0055 nor ES0056", "FR008889", wrapper.CustomOfficeOfPresentation);

				var customsOffice2 = declaration.CustomsOffices.AddNew();
				customsOffice2.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
				customsOffice2.CY_Data = "ES005600";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty CustomOfficeOfPresentation when OfficeOfPresentation is declared but OfficeOfExport starts with ES0055 or ES0056", ZString.Empty, wrapper.CustomOfficeOfPresentation);

				customsOffice2.CY_Data = "ES002800";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CustomOfficeOfPresentation when OfficeOfPresentation is declared and OfficeOfExport does not start with ES0055 nor ES0056", "FR008889", wrapper.CustomOfficeOfPresentation);

				customsOffice1.CY_Data = ZString.Empty;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty CustomOfficeOfPresentation when OfficeOfPresentation is declared but empty", ZString.Empty, wrapper.CustomOfficeOfPresentation);
			});
		}

		public void TestCustomOfficeOfExport()
		{
			CombineAssertions(() =>
			{
				declaration.CustomsOffices.RemoveAndDeleteAll();
				declaration.JE_CustomsOffice = "ES009999";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CustomOfficeOfExport with CustomsOffice office code when no other customsOffice is declared", "ES009999", wrapper.CustomOfficeOfExport);

				var customsOffice1 = declaration.CustomsOffices.AddNew();
				customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
				customsOffice1.CY_Data = "FR008889";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CustomOfficeOfExport with OfficeOfExport office code when declared alone in CustomsOffice list", "FR008889", wrapper.CustomOfficeOfExport);

				customsOffice1.CY_Data = ZString.Empty;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled CustomOfficeOfExport with CustomsOffice office code when declared Empty OfficeOfExport", "ES009999", wrapper.CustomOfficeOfExport);
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

		public void TestGoodsShipment()
		{
			var goodsShipment = wrapper.GoodsShipment;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled GoodsShipment", goodsShipment);
				AssertSame("Cached GoodsShipment", wrapper.GoodsShipment, goodsShipment);
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
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader, Certificate);
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		GoodsNotificationAESSendMessageWrapper wrapper;

		GoodsNotificationAESSendMessageWrapper GetWrapper(CusEntryHeader entryheader, ICertificateProvider certificateData) => new GoodsNotificationAESSendMessageWrapper(entryheader, certificateData);

		protected override GoodsNotificationAESSendMessageWrapper GetProvider() => wrapper;
	}
}
