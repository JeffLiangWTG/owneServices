using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class UnchanneledChannel : IVisualBoardChannel // This class name is really Zen.
	{
		public UnchanneledChannel(string channelType)
		{
			ChannelType = channelType;
		}

		public string ChannelType { get; }
		public string EntityType => ChannelType;
		public string Status => null;
		public string ToolTipStatus => null;
		public ZGuid RiskComponentPK => ZGuid.Invalid;
		public bool IsOvertime => false;
		public bool IsHighRisk => false;
		public ImageWithTooltip StatusImage => default;
		public Image DisplayImage => null;
		public Color BackgroundColor => Color.Transparent;
		public Color ForegroundColor => Color.Black;
		public ChannelNames Names => new ChannelNameBuilder(DisplayName, DisplayName, DisplayName).Build();
		protected virtual string DisplayName => BMConstants.UnchanneledDisplayName;
		public ZGuid EntityPK => ZGuid.Empty;
		public ZString ChannelEntityCode => ZString.Empty;

		public bool IsInChannel(IProcessTask task, bool showJobWorkflow)
		{
			var concreteTask = (ProcessTask)task;
			switch (ChannelType)
			{
				case ChannelTypeList.Codes.Resource:
					return IsInResourceChannel(concreteTask, showJobWorkflow);
				case ChannelTypeList.Codes.Group:
					return concreteTask.P9_GG_AssignedGroup.IsEmpty && (concreteTask.ProcessHeader == null || concreteTask.ProcessHeader.FH_GG_ReleaseGroup.IsEmpty);
				case ChannelTypeList.Codes.Capability:
					return concreteTask.P9_G4_RequiredCapability.IsEmpty;
				case ChannelTypeList.Codes.NotChanneled:
					return true;
			}

			return false;
		}

		public ZDBOnlySubQuery GetTasksInChannelQuery()
		{
			var query = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.PK);
			switch (ChannelType)
			{
				case ChannelTypeList.Codes.Resource:
					query.AddToFilter(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, string.Empty);
					break;

				case ChannelTypeList.Codes.Group:
					query.AddToFilter(ProcessTasksSchema.P9_GG_AssignedGroup, string.Empty);
					var subQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessTasksSchema.P9_FH_ProcessHeader);
					subQuery.AddToFilter(ProcessHeaderSchema.FH_GG_ReleaseGroup, string.Empty);
					query.AddSubQuery(subQuery, JoinCondition.And);
					break;

				case ChannelTypeList.Codes.Capability:
					query.AddToFilter(ProcessTasksSchema.P9_G4_RequiredCapability, Guid.Empty);
					break;

				case ChannelTypeList.Codes.NotChanneled:
					break;
			}
			return query;
		}

		protected virtual bool IsInResourceChannel(ProcessTask task, bool showJobWorkflow)
		{
			return task.P9_GS_NKAssignedStaffMember.IsEmpty;
		}

		public void ClearChannelCache()
		{
		}

		void ReloadChannelHeader()
		{
			if (Reloaded != null)
			{
				Reloaded.Invoke(this, EventArgs.Empty);
			}
		}

		public void ClearCacheAndReload()
		{
			ClearChannelCache();
			ReloadChannelHeader();
		}

		public virtual IVisualBoardChannel CreateCopy(BusinessObjectFactory factory)
		{
			return new UnchanneledChannel(ChannelType);
		}

		public event EventHandler Reloaded;

		#region Implementation

		public override bool Equals(object obj)
		{
			if (obj is UnchanneledChannel other)
			{
				return other.ChannelType == ChannelType && GetType() == other.GetType();
			}
			else
			{
				return base.Equals(obj);
			}
		}

		public override int GetHashCode()
		{
			return ChannelType.GetHashCode();
		}

		#endregion
	}
}
