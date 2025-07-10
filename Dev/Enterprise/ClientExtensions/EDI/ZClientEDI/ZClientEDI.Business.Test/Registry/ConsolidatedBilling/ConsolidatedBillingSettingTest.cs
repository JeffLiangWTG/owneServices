using System;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ConsolidatedBillingSetting))]
	public class ConsolidatedBillingSettingTest : RegistryBusinessObjectTemplateTestCase<ConsolidatedBillingSetting>
	{
		public void TestValidateProductCode()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var setting = NewSetting();
			setting.ProductCode = "";
			Assert(setting.ProductCodeInfo.HasErrors());

			var usageBillingSettings = new UsageBillingSettings();
			var product = usageBillingSettings.PriceLists.AddNew();
			product.ProductCode = "ABC";
			product.RawUsageCategory = "SAT";
			product.Description = "Description";
			product.PriceListCode = "111";

			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, usageBillingSettings);

			setting.ProductCode = "ENT";
			Assert(setting.ProductCodeInfo.GetWarnings().Any(x => x.Message.Equals("Product is not enabled for Usage Billing.")));

			setting.ProductCode = "ABC";
			Assert(!setting.ProductCodeInfo.HasWarnings());
			Assert(!setting.ProductCodeInfo.HasErrors());
		}

		ConsolidatedBillingSetting NewSetting()
		{
			return new ConsolidatedBillingSetting(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override ConsolidatedBillingSetting GetBusinessObjectToClone()
		{
			var product = NewSetting();
			product.ProductCode = "GLW";
			product.Description = "Description";

			return product;
		}

		protected override ConsolidatedBillingSetting GetBusinessObjectToSerialise()
		{
			var product = NewSetting();
			product.ProductCode = "GLW";
			product.Description = "Description";

			return product;
		}
	}
}
