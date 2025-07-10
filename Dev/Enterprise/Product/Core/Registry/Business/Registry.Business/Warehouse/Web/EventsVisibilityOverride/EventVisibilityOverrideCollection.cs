using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class EventVisibilityOverrideCollection : RegistryBusinessObjectCollectionTemplate
	{
		#region Constructors

		public EventVisibilityOverrideCollection()
			: base()
		{
		}

		#endregion

		#region Methods

		public EventVisibilityOverride AddNew(ZString workflowCode)
		{
			var eventInfo = new EventVisibilityOverride(workflowCode);
			Add(eventInfo);
			return eventInfo;
		}

		#endregion

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EventVisibilityOverride();
		}

		public new EventVisibilityOverride AddNew()
		{
			return (EventVisibilityOverride)base.AddNew();
		}

		public new EventVisibilityOverride this[int i]
		{
			get { return (EventVisibilityOverride)base[i]; }
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override bool AllowRemoveCore
		{
			get { return true; }
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EventVisibilityOverrideCollection() { CurrentFallbackLevel = fallbackLevel };
		}

		#endregion
	}
}
