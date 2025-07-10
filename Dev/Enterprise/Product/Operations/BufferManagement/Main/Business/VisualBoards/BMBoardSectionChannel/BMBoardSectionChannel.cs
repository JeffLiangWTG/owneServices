using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[System.Diagnostics.DebuggerDisplay("Name: {BizoDescription} Sequence: {MSC_Sequence}")]
	public class BMBoardSectionChannel : AutoBMBoardSectionChannel, IChannel, IAuditParent
	{
		public BMBoardSectionChannel(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZGuid EntityPK => MSC_ParentID;

		#region BusinessObject Overrides

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (value)
				{
					if (!IsPurged && !IsAdded)
					{
						var board = Section?.Board;

						if (board != null)
						{
							board.MB_SystemLastEditTimeUtc = ZDateTime.UtcNow;
						}
					}
				}
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			isUnChanneled = MSC_ParentID.IsEmpty && !IsCurrentUser;
		}

		#endregion

		#region Persisted Properties

		#region MSC_MS_Section

		[RelatedBusinessObject(nameof(Section))]
		public override ZGuid MSC_MS_Section
		{
			get { return base.MSC_MS_Section; }
			set { base.MSC_MS_Section = value; }
		}

		#endregion

		#region MSC_ChannelType

		[List("Lookups.ChannelTypes")]
		[ReadOnlyMember(nameof(UseDefaultChannels))]
		public override ZString MSC_ChannelType
		{
			get { return base.MSC_ChannelType; }
			set
			{
				var oldValue = MSC_ChannelType;
				base.MSC_ChannelType = value;

				if (oldValue != value)
				{
					MSC_ParentID = ZGuid.Empty;
					MSC_ParentTableCode = ZString.Empty;

					Factory.ClearCachedValue<IBusinessObjectCollection>(Lookups.ParentListCacheKey);
				}
			}
		}

		#endregion

		#region MSC_ParentID

		[List("Lookups.ParentList")]
		[ReadOnlyMember(nameof(MSC_ParentID_Readonly))]
		public override ZGuid MSC_ParentID
		{
			get { return base.MSC_ParentID; }
			set
			{
				base.MSC_ParentID = value;

				BizoDescriptionInfo.RefreshBinding();

				if (MSC_ParentID.IsValid)
				{
					IsUnChanneled = ZBool.False;

					switch (MSC_ChannelType)
					{
						case ChannelTypeList.Codes.Resource:
							MSC_ParentTableCode = GlbStaffSchema.Constants.Prefix;
							break;

						case ChannelTypeList.Codes.Capability:
							MSC_ParentTableCode = GlbCapabilitySchema.Constants.Prefix;
							break;

						case ChannelTypeList.Codes.Group:
							MSC_ParentTableCode = GlbGroupSchema.Constants.Prefix;
							break;

						case ChannelTypeList.Codes.Tag:
							MSC_ParentTableCode = TagMagnitudeSchema.Constants.Prefix;
							break;
					}
				}
				else
				{
					MSC_ParentTableCode = ZString.Empty;
				}
			}
		}

		protected bool MSC_ParentID_Readonly => UseDefaultChannels || IsUnChanneled || MSC_ChannelType == ChannelTypeList.Codes.CurrentUser;

		#endregion

		#endregion

		#region New Properties

		public bool IsPurged { get; set; }
		public bool IsAdded { get; set; }

		#region IsUnChanneled

		public ZBool IsUnChanneled
		{
			get { return isUnChanneled; }
			set
			{
				SetNonPersistentPropertyValue(IsUnChanneledInfo, ref isUnChanneled, value);

				if (value)
				{
					MSC_ParentID = ZGuid.Empty;
					MSC_ParentTableCode = ZString.Empty;
					BizoDescriptionInfo.RefreshBinding();
				}
			}
		}

		ZBool isUnChanneled;

		public ZPropertyInfo IsUnChanneledInfo
		{
			get { return GetZPropertyInfo(nameof(IsUnChanneled)); }
		}

		#endregion

		#region Current User
		public bool IsCurrentUser => MSC_ChannelType == ChannelTypeList.Codes.CurrentUser;

		public static BMBoardSectionChannel GetCurrentUserChannel(BMBoardSectionChannel channelToReplace)
		{
			Argument.NotNull(channelToReplace, nameof(channelToReplace));

			if (Env.CurrentUserPK.Equals(Guid.Empty))
			{
				throw new UserNotFoundException(nameof(Env.CurrentUser));
			}

			var currentUserFactory = new BusinessObjectFactory();
			var currentUserChannel = currentUserFactory.New<BMBoardSectionChannel>();
			currentUserChannel.MSC_MS_Section = channelToReplace.MSC_MS_Section;
			currentUserChannel.MSC_Axis = channelToReplace.MSC_Axis;
			currentUserChannel.MSC_Sequence = channelToReplace.MSC_Sequence;
			currentUserChannel.MSC_ChannelType = ChannelTypeList.Codes.Resource;
			currentUserChannel.MSC_ParentID = Env.CurrentUserPK;

			return currentUserChannel;
		}

		#endregion

		#region BizoDescription

		public ZString BizoDescription
		{
			get
			{
				if (IsUnChanneled)
				{
					return BMConstants.UnchanneledDisplayName;
				}
				else if (IsCurrentUser)
				{
					return ChannelTypeList.Descriptions.CurrentUser;
				}
				else
				{
					var parentBizo = GetChannelBusinessObject(Factory);
					return parentBizo != null ? ((ICodeDescription)parentBizo).Description : string.Empty;
				}
			}
		}

		public ZPropertyInfo BizoDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(BizoDescription)); }
		}

		#endregion

		public bool UseDefaultChannels
		{
			get
			{
				var sectionConfiguration = Section?.SectionConfiguration;

				if (sectionConfiguration == null)
				{
					return true;
				}

				return (MSC_Axis == ChannelAxisCodeList.Codes.Primary) ?
					!sectionConfiguration.OverrideChannels :
					!sectionConfiguration.OverrideSecondaryChannels;
			}
		}

		public bool IsPrimaryAxis => MSC_Axis == ChannelAxisCodeList.Codes.Primary;

		#endregion

		#region Related Business Objects

		public BMBoardSection Section => Factory.Load<BMBoardSection>(MSC_MS_Section);

		public BusinessObject GetChannelBusinessObject(BusinessObjectFactory factory)
		{
			if (MSC_ParentTableCode.IsValid && MSC_ParentID.IsValid)
			{
				return factory.Load(MSC_ParentTableCode, MSC_ParentID);
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region IAuditParent Mambers

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion
	}
}
