using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class EmailFieldCollection : RegistryBusinessObjectCollectionTemplate
	{
		public EmailFieldCollection()
		{
		}

		public new EmailField this[int i]
		{
			get { return (EmailField)Elements[i]; }
		}

		public new EmailField AddNew()
		{
			return (EmailField)base.AddNew();
		}

		public int GetEmailFieldCountWithCode(ZString code)
		{
			int i = 0;
			foreach (EmailField emailField in this)
			{
				if (emailField.Code == code)
				{
					i++;
				}
			}
			return i;
		}

		public int GetEmailFieldCountWithIndex(ZString index)
		{
			int i = 0;
			foreach (EmailField emailField in this)
			{
				if (emailField.Index == index)
				{
					i++;
				}
			}
			return i;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EmailFieldCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EmailField();
		}
	}
}
