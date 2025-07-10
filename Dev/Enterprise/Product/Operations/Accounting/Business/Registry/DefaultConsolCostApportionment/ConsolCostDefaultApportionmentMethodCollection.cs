using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ConsolCostDefaultApportionmentMethodCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new ConsolCostDefaultApportionmentMethod this[int i]
		{
			get { return SetParent((ConsolCostDefaultApportionmentMethod)Elements[i]); }
		}

		public new ConsolCostDefaultApportionmentMethod AddNew()
		{
			return SetParent((ConsolCostDefaultApportionmentMethod)base.AddNew());
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ConsolCostDefaultApportionmentMethodCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return SetParent(new ConsolCostDefaultApportionmentMethod());
		}

		ConsolCostDefaultApportionmentMethod SetParent(ConsolCostDefaultApportionmentMethod method)
		{
			method.ParentCollection = this;
			return method;
		}
	}
}