using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class UsageBillingPriceListCollection : RegistryBusinessObjectCollectionTemplate<UsageBillingPriceList>
	{
		public UsageBillingPriceListCollection() : this(null, null)
		{
		}

		public UsageBillingPriceListCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new UsageBillingPriceListCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UsageBillingPriceList(CurrentFallbackLevel, CurrentFactory);
		}

		public CodeDescriptionPairList GetPriceListCodeDescriptionPairList()
		{
			var list = new CodeDescriptionPairList();
			foreach (var obj in this)
			{
				list.AddPairIfNotExist(obj.PriceListCode, obj.Description);
			}

			return list;
		}

		public IEnumerable<ZString> GetProductCodes() => this.OfType<UsageBillingPriceList>().Select(x => x.ProductCode).Distinct().ToArray();
		public bool ContainsProductCode(string productCode) => this.OfType<UsageBillingPriceList>().Any(x => x.ProductCode.EqualsIgnoringCase(productCode));
		public bool ContainsRawUsageCategoryCode(string rawUsageCategoryCode) => this.OfType<UsageBillingPriceList>().Any(x => x.RawUsageCategory.EqualsIgnoringCase(rawUsageCategoryCode));
		public bool ContainsPriceListCode(string priceListCode) => this.OfType<UsageBillingPriceList>().Any(x => x.PriceListCode.EqualsIgnoringCase(priceListCode));
	}
}
