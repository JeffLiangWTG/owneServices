using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BMZoneCapacityMultiplierLookups : AutoBMZoneCapacityMultiplierLookups
	{
		public BMZoneCapacityMultiplierLookups(AutoBMZoneCapacityMultiplier parent)
			: base(parent)
		{
		}

		#region ReleaseGroups

		public virtual BMComponentCollection Components
		{
			get { return new BMComponentCollection(Factory); }
		}

		public ActiveBusinessObjectCollection<GlbGroup> SystemReleaseGroups
		{
			get
			{
				var parent = (BMZoneCapacityMultiplier)Parent;
				var component = parent.Component;
				if (component != null)
				{
					var system = component.System;

					if (system != null)
					{
						return system.ReleaseGroupLookups;
					}
				}

				return new GlbGroupActiveBusinessObjectCollection(Factory, ZQuery.NoResultQuery);
			}
		}

		public override GlbGroupCollection ReleaseGroups
		{
			get
			{
				var collection = new GlbGroupCollection(Factory);
				collection.Load();
				return collection;
			}
		}

		#endregion
	}
}
