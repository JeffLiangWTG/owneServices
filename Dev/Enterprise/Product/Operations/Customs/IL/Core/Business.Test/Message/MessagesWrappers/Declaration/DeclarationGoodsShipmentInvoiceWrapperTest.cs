using System;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentInvoiceWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentInvoice>
	{
		public void TestNewOrNull()
		{
			AssertNull("When invoiceHeader is null", DeclarationGoodsShipmentInvoiceWrapper.NewOrNull(null));
			AssertNotNull("When invoiceHeader is not null", DeclarationGoodsShipmentInvoiceWrapper.NewOrNull(invoiceHeader));
		}

		public void TestID()
		{
			var wrapper = GetProvider();
			AssertNullOrEmpty("When Invoice Number is empty", wrapper.ID?.Value);

			invoiceHeader.JZ_InvoiceNumber = "AR00123";
			wrapper = GetProvider();
			AssertEquals("When Invoice Number is not empty", "AR00123", wrapper.ID.Value);
		}

		public void TestIssueDateTime()
		{
			invoiceHeader.JZ_InvoiceDate = ZDateTime.Empty;
			var wrapper = GetProvider();
			AssertNullOrEmpty("When InvoiceDate is empty", wrapper.IssueDateTime);

			invoiceHeader.JZ_InvoiceDate = new DateTime(2024, 05, 16, 11, 47, 0, DateTimeKind.Utc);
			wrapper = GetProvider();
			AssertEquals("When Invoice Number is not empty", "2024-05-16T11:47:00", wrapper.IssueDateTime);
		}

		public void TestDmExtensions()
		{
			var wrapper = GetProvider();
			AssertNotNull("Even when Invoice amount and currency are empty", wrapper.DmExtensions);
		}

		public void TestTypeCode()
		{
			invoiceHeader.JZ_InvoiceType = "XYZ";
			AssertEquals("TypeCode value should be set as expected", "XYZ", Provider.TypeCode.Value);
		}

		protected override IDeclarationGoodsShipmentInvoice GetProvider() => DeclarationGoodsShipmentInvoiceWrapper.NewOrNull(invoiceHeader);

		protected override void SetUp()
		{
			base.SetUp();
			invoiceHeader = Factory.New<JobComInvoiceHeader>();
		}

		JobComInvoiceHeader invoiceHeader;
	}
}
