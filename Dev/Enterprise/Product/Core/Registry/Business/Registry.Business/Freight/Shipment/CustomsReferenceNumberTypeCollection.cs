using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot(ElementName = "CustomsReferenceNumberTypes")]
	public class CustomsReferenceNumberTypeCollection : RegistryBusinessObjectCollection
	{
		#region Overrides

		public new CustomsReferenceNumberType this[int index]
		{
			get { return (CustomsReferenceNumberType)Elements[index]; }
		}

		public new CustomsReferenceNumberType AddNew()
		{
			return (CustomsReferenceNumberType)base.AddNew();
		}

		public CustomsReferenceNumberType Add(ZString code, MultilingualString description, bool isUnique = true, bool isAutomation = false)
		{
			var result = AddNew();
			result.Code = code;
			result.Description = description;
			result.IsUnique = isUnique;
			result.IsAutomation = isAutomation;
			return result;
		}

		public CustomsReferenceNumberType AddSystemDefined(ZString code, MultilingualString description, bool isUnique = true, bool isAutomation = false)
		{
			var result = Add(code, description, isUnique, isAutomation);
			result.SystemDefined = true;
			return result;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CustomsReferenceNumberTypeCollection();
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);

			CustomsReferenceNumberType customsReferenceNumberType = businessObject as CustomsReferenceNumberType;

			if (customsReferenceNumberType != null)
			{
				customsReferenceNumberType.Parent = this;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CustomsReferenceNumberType(this);
		}

		protected override BusinessObject AddNewCore()
		{
			CustomsReferenceNumberType customsReferenceNumberType = (CustomsReferenceNumberType)base.AddNewCore();
			customsReferenceNumberType.Parent = this;
			return customsReferenceNumberType;
		}

		#endregion
	}
}
