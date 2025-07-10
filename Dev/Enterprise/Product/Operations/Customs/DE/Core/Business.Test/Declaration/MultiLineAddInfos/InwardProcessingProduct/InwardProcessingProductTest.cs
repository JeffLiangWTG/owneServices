using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(InwardProcessingProduct))]
	public class InwardProcessingProductTest : CusSupportingInfoTest<InwardProcessingProduct>
	{
		public void TestParent()
		{
			AssertType<JobComInvoiceLine>(product.Parent);
		}

		public void TestFormattedTariff_MaxLength()
		{
			AssertEquals(10, product.FormattedTariffInfo.MaxLength);
		}

		public void TestFormattedTariff_ReadOnly()
		{
			CombineAssertions(() =>
			{
				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
				AssertEquals("Grant Authorization N", true, product.FormattedTariffInfo.ReadOnly);

				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
				AssertEquals("Grant Authorization J", false, product.FormattedTariffInfo.ReadOnly);
			});
		}

		public void TestCSI_Tariff()
		{
			product.CSI_Tariff = "9873.22.11";
			AssertEquals("98732211", product.CSI_Tariff);
		}

		public void TestFormattedTariff()
		{
			product.FormattedTariff = "7635.44.55";
			AssertEquals("76354455", product.CSI_Tariff);
			product.CSI_Tariff = "98732211";
			AssertEquals("9873.22.11", product.FormattedTariff);
		}

		public void TestClearProductDetailsIfNeeded()
		{
			product.FormattedTariff = "7635.44.55";
			product.CSI_Description = "DESCRIPTION";
			product.CSI_SubType = "B";
			product.CSI_AdditionalDescription = "ADDTIONAL DESCRIPTION";
			product.FormattedTariff = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("Description", ZString.Empty, product.CSI_Description);
				AssertEquals("Sub Type", ZString.Empty, product.CSI_SubType);
				AssertEquals("Additional Description", ZString.Empty, product.CSI_AdditionalDescription);
			});
		}

		public void TestCSI_Description_ReadOnly()
		{
			AssertProductDetailReadOnly(product.CSI_DescriptionInfo);
		}

		public void TestCSI_SubTypeInfo_ReadOnly()
		{
			AssertProductDetailReadOnly(product.CSI_SubTypeInfo);
		}

		public void TestCSI_AdditionalDescriptionInfo_ReadOnly()
		{
			AssertProductDetailReadOnly(product.CSI_AdditionalDescriptionInfo);
		}

		public void TestLookups()
		{
			AssertType<InwardProcessingProductLookups>(product.Lookups);
		}

		public void TestValidation()
		{
			AssertType<InwardProcessingProductValidation>(product.Validation);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Product", product.HumanReadableName);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(InwardProcessingProduct.CusSupportingInfoType, product.CSI_Type);
		}

		public void TestITariffFormatProvider()
		{
			CombineAssertions(() =>
			{
				var provider = (ITariffFormatProvider)product;
				var tariffFormatter = provider.TariffFormatter;
				AssertType<EU.Business.TariffFormatterEleven>("Type", provider.TariffFormatter);
				AssertSame("Cached", tariffFormatter, provider.TariffFormatter);
			});
		}

		protected override IEnumerable<InwardProcessingProduct> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return CreateInwardProcessingProduct(factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			product = CreateInwardProcessingProduct(Factory);
			var invoiceLine = product.Parent;
			instruction = invoiceLine.EntryInstruction;
		}
		CusEntryInstruction instruction;
		InwardProcessingProduct product;

		InwardProcessingProduct CreateInwardProcessingProduct(BusinessObjectFactory factory)
		{
			var entryInstruction = factory.CreateInwardProcessingInstruction();
			entryInstruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
			var invoiceHeader = entryInstruction.JobDeclaration.Invoices.AddNew();
			var line = invoiceHeader.InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;
			return line.InwardProcessingProducts.AddNew();
		}

		void AssertProductDetailReadOnly(ZPropertyInfo propertyInfo)
		{
			CombineAssertions(() =>
			{
				product.FormattedTariff = "7635.44.55";
				AssertEquals("Read Write", false, propertyInfo.ReadOnly);
				product.FormattedTariff = ZString.Empty;
				AssertEquals("Read Only", true, propertyInfo.ReadOnly);
			});
		}
	}
}
