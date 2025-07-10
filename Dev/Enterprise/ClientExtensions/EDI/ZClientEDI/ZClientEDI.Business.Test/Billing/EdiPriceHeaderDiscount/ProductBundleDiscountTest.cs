using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ProductBundleDiscount))]
	public class ProductBundleDiscountTest : NonPersistentBusinessObjectTestCase
	{
		public void TestXmlSerialization()
		{
			var discount = new ProductBundleDiscount();
			var line1 = discount.Lines.AddNew();
			line1.ProductCode = "CW1";
			var line2 = discount.Lines.AddNew();
			line2.ProductCode = "BOR";

			var serializer = ZXmlSerializer.New(typeof(ProductBundleDiscount));
			string xml = "";
			using (var writer = new StringWriter())
			{
				serializer.Serialize(writer, discount);
				xml = writer.ToString();
			}

			var reader = new StringReader(xml);
			var discount2 = (ProductBundleDiscount)serializer.Deserialize(reader);
			AssertEquals(2, discount2.Lines.Count);
			AssertNotNull(discount2.Lines.OfType<ProductBundleDiscountLine>().Single(x => x.ProductCode == "CW1"));
			AssertNotNull(discount2.Lines.OfType<ProductBundleDiscountLine>().Single(x => x.ProductCode == "BOR"));
		}

		public void TestCodeAlive()
		{
			AssertNotNull("CodeAlive", typeof(AutoProductBundleDiscount.Schema));
		}
	}

	[TestedType(typeof(ProductBundleDiscountLine))]
	public class ProductBundleDiscountLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			mappings.AddNew("AB1", "AB1 Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var discount = Factory.New<EdiPriceHeaderDiscount>();
			discount.PHD_Name = "PRODUCTBUNDLE";
			discount.PHD_Version = "STL1";
			discount.PHD_Type = BillingConstants.DiscountCalculator.ProductBundle;
			var bundleDiscount = (ProductBundleDiscount)discount.Config;
			var line1 = bundleDiscount.Lines.AddNew();

			line1.RunPreSaveValidation();
			AssertHasError(line1.ProductCodeInfo, "Please enter a value.");
			line1.ProductCode = "CW1";
			AssertNoErrors(line1.ProductCodeInfo);
			line1.ProductCode = "--";
			AssertHasError(line1.ProductCodeInfo, "Enter a valid selection.");
			line1.ProductCode = "CW1";
			AssertNoErrors(line1.ProductCodeInfo);

			var priceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			priceHeader.L6_DiscountCode = "STL1";
			priceHeader.L6_SystemCode = "ABC";

			Factory.Save();

			var line2 = bundleDiscount.Lines.AddNew();
			line2.ProductCode = "CW1";
			AssertHasError(line2.ProductCodeInfo, "Duplicate product not allowed.");

			line2.ProductCode = "ABC";
			AssertHasError(line2.ProductCodeInfo, "System pricelist product cannot be added in this grid.");

			line2.ProductCode = "AB1";
			AssertNoErrors(line2.ProductCodeInfo);
		}

		public void TestXmlSerialization()
		{
			var line = new ProductBundleDiscountLine();
			line.ProductCode = "CW1";

			var serializer = ZXmlSerializer.New(typeof(ProductBundleDiscountLine));
			string xml = "";
			using (var writer = new StringWriter())
			{
				serializer.Serialize(writer, line);
				xml = writer.ToString();
			}

			var reader = new StringReader(xml);
			var line2 = (ProductBundleDiscountLine)serializer.Deserialize(reader);
			AssertEquals("CW1", line2.ProductCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ProductBundleDiscountLine();
		}
	}

	[TestedType(typeof(ProductBundleDiscountLineCollection))]
	internal class ProductBundleDiscountLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ProductBundleDiscountLineCollection>
	{
		protected override ProductBundleDiscountLineCollection GetCollectionToTest()
		{
			return new ProductBundleDiscountLineCollection(null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ProductBundleDiscountLine();
		}
	}
}
