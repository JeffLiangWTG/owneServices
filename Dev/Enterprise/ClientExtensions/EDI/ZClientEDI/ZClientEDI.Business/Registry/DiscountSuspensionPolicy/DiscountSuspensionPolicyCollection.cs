using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class DiscountSuspensionPolicyCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new DiscountSuspensionPolicy this[int index]
		{
			get { return (DiscountSuspensionPolicy)Elements[index]; }
		}

		public new DiscountSuspensionPolicy AddNew()
		{
			return (DiscountSuspensionPolicy)base.AddNew();
		}

		public DiscountSuspensionPolicy AddNew(ZString productCode, ZString policyCode)
		{
			var result = AddNew();
			using (result.GetValidationSuspender())
			{
				result.ProductCode = productCode;
				result.PolicyCode = policyCode;
			}

			return result;
		}

		public DiscountSuspensionPolicy GetPolicyByProduct(ZString productCode) => this.OfType<DiscountSuspensionPolicy>().FirstOrDefault(x => x.ProductCode == productCode);

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DiscountSuspensionPolicy();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DiscountSuspensionPolicyCollection();
		}
	}
}

