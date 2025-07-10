using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public abstract class MilestoneEventUpdatesCollection : RegistryBusinessObjectCollectionTemplate
	{
		#region Constructors

		public MilestoneEventUpdatesCollection()
			: base()
		{
		}

		#endregion

		#region Methods

		public MilestoneEventUpdates AddNew(ZString eventType)
		{
			return AddNew(eventType, null);
		}

		public MilestoneEventUpdates AddNew(ZString eventType, params string[] allowedWebPartyTypes)
		{
			var result = base.AddNew() as MilestoneEventUpdates;
			if (result != null)
			{
				result.EventType = eventType;
				if (allowedWebPartyTypes != null)
				{
					foreach (string allowedWebPartyType in allowedWebPartyTypes)
					{
						result.SetValue(allowedWebPartyType, true);
					}
				}
			}
			return result;
		}

		public List<string> GetUpdateableEventCodes(string webPartyType)
		{
			var result = new List<string>();
			foreach (MilestoneEventUpdates item in this)
			{
				if (item.GetValue(webPartyType))
				{
					result.Add(item.EventType);
				}
			}
			return result;
		}

		#endregion

		#region List Caching

		public CodeDescriptionPairList EventTypeList
		{
			get { return eventTypeList ?? (eventTypeList = EventTypeListProvider.CreateMilestoneEventTypeList(Enterprise.Core.Constants.Workflow.MilestoneType, string.Empty, false)); }
		}

		CodeDescriptionPairList eventTypeList;

		#endregion

		#region Overrides

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (bizO is MilestoneEventUpdates)
			{
				((MilestoneEventUpdates)bizO).Parent = null;
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
			var clone = GetNewCollection();
			clone.CurrentFallbackLevel = fallbackLevel;
			return clone;
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);

			var milestoneEvent = child as MilestoneEventUpdates;
			if (milestoneEvent != null && milestoneEvent.Parent == null)
			{
				using (milestoneEvent.GetValidationSuspender())
				{
					milestoneEvent.Parent = this;
				}
			}
		}

		#endregion

		#region Implementation

		protected abstract MilestoneEventUpdatesCollection GetNewCollection();

		#endregion
	}
}
