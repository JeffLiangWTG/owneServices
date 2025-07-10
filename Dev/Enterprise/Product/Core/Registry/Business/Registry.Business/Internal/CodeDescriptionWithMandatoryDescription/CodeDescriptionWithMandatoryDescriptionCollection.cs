using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionWithMandatoryDescriptionCollection : RegistryBusinessObjectCollection
	{
		public CodeDescriptionWithMandatoryDescriptionCollection()
		{
		}

		public CodeDescriptionWithMandatoryDescriptionCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public CodeDescriptionWithMandatoryDescription Add(ZString code, MultilingualString description)
		{
			var result = AddNew();
			result.Code = code;
			result.Description = description;

			return result;
		}

		public new CodeDescriptionWithMandatoryDescription this[int i]
		{
			get { return (CodeDescriptionWithMandatoryDescription)base[i]; }
		}

		public new CodeDescriptionWithMandatoryDescription AddNew()
		{
			return (CodeDescriptionWithMandatoryDescription)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CodeDescriptionWithMandatoryDescription();
		}

		#region clone

		protected virtual CodeDescriptionWithMandatoryDescriptionCollection GetNewCollection()
		{
			return new CodeDescriptionWithMandatoryDescriptionCollection(CurrentFallbackLevel);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = GetNewCollection();
			clone.CurrentFallbackLevel = fallbackLevel;
			return clone;
		}

		#endregion
	}
}
