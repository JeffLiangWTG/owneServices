using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class IssueWorkItemCreationThresholdCollection : RegistryBusinessObjectCollectionTemplate<IssueWorkItemCreationThreshold>
	{
		public IssueWorkItemCreationThresholdCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public IssueWorkItemCreationThresholdCollection()
			: base(null, null)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new IssueWorkItemCreationThreshold(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IssueWorkItemCreationThresholdCollection(fallbackLevel, factory);
		}
	}
}

