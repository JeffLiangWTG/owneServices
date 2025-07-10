using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ConsolidatedBillingSetting : AutoConsolidatedBillingSetting
	{
		public ConsolidatedBillingSetting()
		{
		}
		public ConsolidatedBillingSetting(BusinessObjectFactory factory) : base(factory)
		{
		}
		public ConsolidatedBillingSetting(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}
		public ConsolidatedBillingSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ConsolidatedBillingSetting(fallbackLevel, factory);
		}

		[List("ProductList")]
		public override ZString ProductCode { get => base.ProductCode; set => base.ProductCode = value; }

		public ReadOnlyCodeDescriptionPairList ProductList { get; } = new ProductTypes(false);

		public override void ValidateProductCode()
		{
			base.ValidateProductCode();
			MandatoryValidation.CheckEntered(ProductCodeInfo);
			ListValidation.ErrorIfInvalidCode(ProductCodeInfo);
			if (!EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.OfType<UsageBillingPriceList>().Any(x => x.ProductCode == this.ProductCode))
			{
				ProductCodeInfo.AddWarning("Product is not enabled for Usage Billing.");
			}
		}
	}
}
