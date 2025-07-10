using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	[XmlRoot("EmailSubjectFields")]
	public class EmailSubjectFieldCollection : EmailFieldCollection
	{
		public EmailSubjectFieldCollection()
		{
		}

		public new EmailSubjectField this[int i]
		{
			get { return (EmailSubjectField)Elements[i]; }
		}

		public new EmailSubjectField AddNew()
		{
			return (EmailSubjectField)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EmailSubjectFieldCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EmailSubjectField();
		}
	}
}
