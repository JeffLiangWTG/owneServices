using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Tasks.ScheduledUpgrader
{
	public class ScheduledUpgraderConfig : AutoScheduledUpgraderConfig
	{
		public ScheduledUpgraderConfig(IServiceTaskSchedule parent)
			: base(((BusinessObject)parent).Factory)
		{
			this.parent = parent;
			((BusinessObject)parent).RegisterEditableChildObject(this);
			Parse(parent.ConfigString);
		}

		public ScheduledUpgraderConfig(string configString)
			: base(new BusinessObjectFactory())
		{
			Parse(configString);
		}

		public virtual GlbGroup NotificationGroup
		{
			get
			{
				return Factory.Load<GlbGroup>(NotificationGroup_PK);
			}
		}

		public virtual GlbGroupCollection GlbGroups
		{
			get
			{
				return new GlbGroupCollection(Factory);
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (parent != null)
			{
				parent.ConfigString = ConfigString;
			}
		}

		internal string ConfigString
		{
			get { return (AutoDownload ? "Y" : "N") + "," + (PatchOnly ? "Y" : "N") + "," + (NotifyOnSuccess ? "Y" : "N") + "," + NotificationGroup_PK; }
		}

		#region Sealed override properties, to prevent CA2214

		public sealed override ZBool NotifyOnSuccess
		{
			get
			{
				return base.NotifyOnSuccess;
			}
			set
			{
				base.NotifyOnSuccess = value;
			}
		}

		public sealed override ZBool AutoDownload
		{
			get
			{
				return base.AutoDownload;
			}
			set
			{
				base.AutoDownload = value;
			}
		}

		public sealed override ZBool PatchOnly
		{
			get
			{
				return base.PatchOnly;
			}
			set
			{
				base.PatchOnly = value;
			}
		}

		public sealed override ZGuid NotificationGroup_PK
		{
			get
			{
				return base.NotificationGroup_PK;
			}
			set
			{
				base.NotificationGroup_PK = value;
			}
		}

		#endregion

		void Parse(string configString)
		{
			NotificationGroup_PK = Constants.Groups.PostMastersGroupPK;

			if (configString != null)
			{
				string[] parts = configString.Split(new char[] { ',' });
				if (parts.Length > 0)
				{
					AutoDownload = parts[0] == "Y";
				}
				if (parts.Length > 1)
				{
					PatchOnly = parts[1] == "Y";
				}
				if (parts.Length > 2)
				{
					NotifyOnSuccess = parts[2] == "Y";
				}
				if (parts.Length > 3)
				{
					ZGuid pK = ZGuid.Empty;
					if (ZGuid.TryParse(parts[3], out pK))
					{
						NotificationGroup_PK = pK;
					}
				}
			}
		}

		readonly IServiceTaskSchedule parent;
	}
}
