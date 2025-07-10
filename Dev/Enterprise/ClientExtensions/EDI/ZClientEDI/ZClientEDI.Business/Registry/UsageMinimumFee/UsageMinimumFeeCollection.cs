using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class UsageMinimumFeeCollection : RegistryBusinessObjectCollectionTemplate<UsageMinimumFee>
	{
		public UsageMinimumFeeCollection() : this(null, null)
		{
		}

		public UsageMinimumFeeCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new UsageMinimumFeeCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UsageMinimumFee(CurrentFallbackLevel, CurrentFactory);
		}

		public IEnumerable<ZString> GetProductCodes() => this.OfType<UsageMinimumFee>().Select(x => x.ProductCode).Distinct().ToArray();
		public bool ContainsProductCode(string productCode) => this.OfType<UsageMinimumFee>().Any(x => x.ProductCode.EqualsIgnoringCase(productCode));
	}
}
