using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class EventVisibilityCollection : RegistryBusinessObjectCollectionTemplate
	{
		#region Constructors

		public EventVisibilityCollection()
			: base()
		{
		}

		#endregion

		#region Methods

		public EventVisibility AddNew(ZString eventCode)
		{
			var eventInfo = new EventVisibility(eventCode);
			Add(eventInfo);
			return eventInfo;
		}

		#endregion

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EventVisibility();
		}

		public new EventVisibility AddNew()
		{
			return (EventVisibility)base.AddNew();
		}

		public new EventVisibility this[int i]
		{
			get { return (EventVisibility)base[i]; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			if (bizOAdded is EventVisibility)
			{
				((EventVisibility)bizOAdded).Parent = this;
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (bizO is EventVisibility)
			{
				((EventVisibility)bizO).Parent = null;
			}
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override bool AllowRemoveCore
		{
			get { return true; }
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(Enterprise.ZArchitecture.Environment.FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EventVisibilityCollection() { CurrentFallbackLevel = fallbackLevel };
		}

		#endregion
	}
}
