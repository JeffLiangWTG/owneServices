using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolWithMandatoryDescriptionCollection : CodeDescriptionBoolCollection
	{
		public CodeDescriptionBoolWithMandatoryDescriptionCollection()
			: base(3)
		{
		}

		public CodeDescriptionBoolWithMandatoryDescriptionCollection(int codeMaxLength)
			: base(codeMaxLength)
		{
		}

		public new CodeDescriptionBoolWithMandatoryDescription this[int i]
		{
			get { return (CodeDescriptionBoolWithMandatoryDescription)base[i]; }
		}

		public new CodeDescriptionBoolWithMandatoryDescription AddNew()
		{
			return (CodeDescriptionBoolWithMandatoryDescription)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CodeDescriptionBoolWithMandatoryDescription();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new CodeDescriptionBoolWithMandatoryDescriptionCollection();
		}
	}
}
