using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class InvoiceRollupAndGroupDescriptionCollection : RegistryBusinessObjectCollectionTemplate
	{
		public InvoiceRollupAndGroupDescriptionCollection()
		{
		}

		public InvoiceRollupAndGroupDescriptionCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public new InvoiceRollupAndGroupDescription this[int index]
		{
			get { return (InvoiceRollupAndGroupDescription)Elements[index]; }
		}

		public new InvoiceRollupAndGroupDescription AddNew()
		{
			return (InvoiceRollupAndGroupDescription)base.AddNew();
		}

		public void AddDefaultValue(ZString style, ZString group, MultilingualString description)
		{
			var newValue = new InvoiceRollupAndGroupDescription();
			newValue.Style = style;
			newValue.Group = group;
			newValue.Description = description;
			Add(newValue);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InvoiceRollupAndGroupDescriptionCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new InvoiceRollupAndGroupDescription(CurrentFallbackLevel, CurrentFactory);
		}
	}
}