using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;
using OrgCusCode = Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.ES.Business.Testing.MessageWrappers.AES.ComplXAES
{
	public class ComplXAESSendMessageWrapperTest : WrapperHelperTest<ComplXAESSendMessageWrapper>
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
			var orgAddress = Factory.New<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;
			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
				AssertExceptionThrown<NullReferenceException>("Declaration with JE_DeclarantType = 1, result null", () => wrapper.Representative.ToString());

				declaration.JE_OA_Representative = orgAddress.PK;
				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
				wrapper = GetWrapper(entryHeader);
				AssertNoExceptionThrown("Declaration with JE_DeclarantType = 2 with representative, result not null", () => wrapper.Representative.ToString());

				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._3Indirect;
				wrapper = GetWrapper(entryHeader);
				AssertExceptionThrown<NullReferenceException>("Declaration with JE_DeclarantType = 3 with representative, result null", () => wrapper.Representative.ToString());

				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
				declaration.JE_OA_DeclarantAddress = orgAddress.PK;
				declaration.JE_OA_Representative = ZGuid.Empty;
				wrapper = GetWrapper(entryHeader);
				AssertNoExceptionThrown("JobDeclaration with representative is null and declarant not null and JE_DeclarantType = 2, result not null", () => wrapper.Representative.ToString());

				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._5IndirectATC;
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				wrapper = GetWrapper(entryHeader);
				AssertExceptionThrown<NullReferenceException>("JobDeclaration with representative and declarant is null and JE_DeclarantType = 5, result null", () => wrapper.Representative.ToString());

				declaration.JE_OA_Representative = orgAddress.PK;
				wrapper = GetWrapper(entryHeader);
				AssertNoExceptionThrown("JobDeclaration with representative is not null and declarant is null and JE_DeclarantType = 5, result not null", () => wrapper.Representative.ToString());

				declaration.JE_OA_DeclarantAddress = orgAddress.PK;
				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
				wrapper = GetWrapper(entryHeader);
				AssertNoExceptionThrown("JobDeclaration with representative is not null and declarant is not null and JE_DeclarantType = 2, result not null", () => wrapper.Representative.ToString());
			});
		}

		public void TestRepresentative()
		{
			CombineAssertions(() =>
			{
				var orgHeaderDeclarant = Factory.New<OrgHeader>();
				orgHeaderDeclarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF111111");
				var orgAddressDeclarant = Factory.New<OrgAddress>();
				orgAddressDeclarant.OA_OH = orgHeaderDeclarant.PK;

				declaration.JE_OA_DeclarantAddress = orgAddressDeclarant.PK;
				declaration.JE_GB = ZGuid.Empty;
				declaration.JE_OA_Representative = ZGuid.Empty;
				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;

				var representative = wrapper.Representative;

				AssertNotNull("Expected filled Representative (with declarant)", representative);
				AssertEquals("Only with declarant, the Id is the declarant's when JE_DeclarantType = 2 or 5", "ESNIF111111", representative.Id);
				AssertSame("Cached Representative", wrapper.Representative, representative);

				var orgHeaderRepresent = Factory.New<OrgHeader>();
				orgHeaderRepresent.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF222222");
				var orgAddressRepresent = Factory.New<OrgAddress>();
				orgAddressRepresent.OA_OH = orgHeaderRepresent.PK;

				declaration.JE_OA_Representative = orgAddressRepresent.PK;
				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;

				wrapper = GetWrapper(entryHeader);
				representative = wrapper.Representative;

				AssertNotNull("Expected filled Representative (with representative)", representative);
				AssertEquals("With declarant and representative, the Id is the representative's when JE_DeclarantType = 2 or 5", "ESNIF222222", representative.Id);
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

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader);
		}
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		ComplXAESSendMessageWrapper wrapper;

		ComplXAESSendMessageWrapper GetWrapper(CusEntryHeader entryheader) => new ComplXAESSendMessageWrapper(entryheader, Certificate);

		protected override ComplXAESSendMessageWrapper GetProvider() => wrapper;
	}
}
