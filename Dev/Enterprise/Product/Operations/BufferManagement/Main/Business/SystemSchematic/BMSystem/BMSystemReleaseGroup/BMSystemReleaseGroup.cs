using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BMSystemReleaseGroup : AutoBMSystemReleaseGroup,
		ICustomisedLayoutSupportable,
		IAuditParent
	{
		public BMSystemReleaseGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region FSG_FS_System

		[RelatedBusinessObject("BMSystem")]
		public override ZGuid FSG_FS_System
		{
			get { return base.FSG_FS_System; }
			set { base.FSG_FS_System = value; }
		}

		#endregion

		#region FSG_GG_Group

		public override ZGuid FSG_GG_Group
		{
			get { return base.FSG_GG_Group; }
			set
			{
				base.FSG_GG_Group = value;

				if (customisedLayoutLinks != null)
				{
					UnRegisterEditableChildObject(customisedLayoutLinks);
					customisedLayoutLinks = null;
				}
			}
		}

		#endregion

		#region ReleaseGroupDesc

		[ResourceStringData("BMSystemReleaseGroup|ReleaseGroupDesc", Caption = "Description")]
		public ZString ReleaseGroupDesc => Group?.GG_Desc ?? ZString.Empty;

		public ZPropertyInfo ReleaseGroupDescInfo
		{
			get { return GetZPropertyInfo(nameof(ReleaseGroupDesc)); }
		}

		#endregion

		#region FSG_ResourceCountdownTime

		[ZDateTimeDurationValue]
		public override ZDateTime FSG_ResourceCountdownTime
		{
			get => base.FSG_ResourceCountdownTime;
			set => base.FSG_ResourceCountdownTime = value.ConvertToDurationBasedDate(FSG_ResourceCountdownTimeInfo);
		}

		public TimeSpan CountdownTime
		{
			get { return FSG_ResourceCountdownTime.IsValid ? TimeSpan.FromMinutes(FSG_ResourceCountdownTime.GetMinutesFromDateTimeSpan()) : TimeSpan.Zero; }
		}

		#endregion

		#endregion

		#region ICustomisedLayoutSupportable Members

		[ChildEditable]
		public BMControlCustomisationLinkCollection CustomisedLayoutLinks
		{
			get
			{
				if (customisedLayoutLinks == null)
				{
					customisedLayoutLinks = Group != null ? new BMControlCustomisationLinkCollection(Group) : new BMControlCustomisationLinkCollection(Factory, ZQuery.NoResultQuery);
					RegisterEditableChildObject(customisedLayoutLinks);
				}

				return customisedLayoutLinks;
			}
		}
		BMControlCustomisationLinkCollection customisedLayoutLinks;

		IBMControlCustomisationLinkCollection ICustomisedLayoutSupportable.CustomisedLayoutLinks
		{
			get { return CustomisedLayoutLinks; }
		}

		bool ICustomisedLayoutSupportable.AreCustomisedLayoutFetchHintsAdded { get; set; }

		#endregion

		#region IAuditParent Mambers

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion

		#region Related Business Objects

		public BMSystem BMSystem => Factory.Load<BMSystem>(FSG_FS_System);

		public BMComponentReleaseGroupLinkCollection ComponentLinks
		{
			get
			{
				var group = Group;
				return group != null ? new BMComponentReleaseGroupLinkCollection(group) : new BMComponentReleaseGroupLinkCollection(Factory, ZQuery.NoResultQuery);
			}
		}

		#endregion

		#region BusinessObject Overrides

		public override void Delete()
		{
			ComponentLinks.DeleteAll();

			base.Delete();
		}

		#endregion
	}
}
