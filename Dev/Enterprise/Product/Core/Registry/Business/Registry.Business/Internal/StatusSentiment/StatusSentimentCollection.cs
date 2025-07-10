using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class StatusSentimentCollection : RegistryBusinessObjectCollectionTemplate
	{
		public StatusSentimentCollection()
		{
		}

		public StatusSentimentCollection(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		public new StatusSentiment this[int index]
		{
			get { return (StatusSentiment)Elements[index]; }
		}

		public new StatusSentiment AddNew()
		{
			return (StatusSentiment)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StatusSentiment(CurrentFallbackLevel);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new StatusSentimentCollection(fallbackLevel);
		}
	}
}
