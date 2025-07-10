using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class GlowOpportunityScopePrioritySequenceCollection : RegistryBusinessObjectCollectionTemplate
	{
		public GlowOpportunityScopePrioritySequenceCollection()
		{
			SetReadOnlyIncludingChildren(true);
		}

		public new GlowOpportunityScopePrioritySequence this[int i]
		{
			get { return (GlowOpportunityScopePrioritySequence)Elements[i]; }
		}

		public new GlowOpportunityScopePrioritySequence AddNew()
		{
			return (GlowOpportunityScopePrioritySequence)base.AddNew();
		}

		public GlowOpportunityScopePrioritySequence AddNew(string description)
		{
			var priority = AddNew();
			priority.Description = description;
			return priority;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GlowOpportunityScopePrioritySequence();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GlowOpportunityScopePrioritySequenceCollection();
		}
	}
}
