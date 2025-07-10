using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class RatingTokenAuthenticationCollection : RegistryBusinessObjectCollectionTemplate
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RatingTokenAuthentication();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RatingTokenAuthenticationCollection();
		}

		public new RatingTokenAuthentication this[int i]
		{
			get { return (RatingTokenAuthentication)Elements[i]; }
		}

		public new RatingTokenAuthentication AddNew()
		{
			return (RatingTokenAuthentication)base.AddNew();
		}

		public IEnumerator<RatingTokenAuthentication> GetEnumerator()
		{
			foreach (var item in Elements)
			{
				yield return (RatingTokenAuthentication)item;
			}
		}

		protected override bool AllowSort => false;
	}
}
