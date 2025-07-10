using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OverrideImmuneCodeDescriptionBoolWithMandatoryDescriptionCollection : CodeDescriptionBoolCollection
	{
		public OverrideImmuneCodeDescriptionBoolWithMandatoryDescriptionCollection()
			: base(3)
		{
		}

		public OverrideImmuneCodeDescriptionBoolWithMandatoryDescriptionCollection(int codeMaxLength)
			: base(codeMaxLength)
		{
		}

		public new OverrideImmuneCodeDescriptionBoolWithMandatoryDescription this[int i]
		{
			get { return (OverrideImmuneCodeDescriptionBoolWithMandatoryDescription)base[i]; }
		}

		public new OverrideImmuneCodeDescriptionBoolWithMandatoryDescription AddNew()
		{
			return (OverrideImmuneCodeDescriptionBoolWithMandatoryDescription)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OverrideImmuneCodeDescriptionBoolWithMandatoryDescription();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new OverrideImmuneCodeDescriptionBoolWithMandatoryDescriptionCollection();
		}
	}
}
